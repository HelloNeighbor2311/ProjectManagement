using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ProjectManagement.Caching;
using ProjectManagement.Permissions;
using ProjectManagement.Priorities;
using ProjectManagement.Projects;
using ProjectManagement.Statuses;
using ProjectManagement.TeamMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.WorkTasks
{
    [Authorize(ProjectManagementPermissions.Tasks.Default)]
    public class WorkTaskAppService : ProjectManagementAppService, IWorkTaskAppService
    {
        private const string CacheVersionKey = "ProjectManagement:WorkTasks:Version";
        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(20),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        private readonly IDistributedCache _distributeCache;
        private readonly ILogger<WorkTaskAppService> _logger;
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly IRepository<Project, Guid> _projectRepository;
        private readonly IRepository<Status, Guid> _statusRepository;
        private readonly IRepository<Priority, Guid> _priorityRepository;
        private readonly IRepository<TeamMember, Guid> _teamMemberRepository;
        private readonly WorkTaskMapper _workTaskMapper;
        private readonly WorkTaskManager _workTaskManager;

        public WorkTaskAppService(
            IDistributedCache distributeCache,
            ILogger<WorkTaskAppService> logger,
            IWorkTaskRepository workTaskRepository,
            IRepository<Project, Guid> projectRepository,
            IRepository<Status, Guid> statusRepository,
            IRepository<Priority, Guid> priorityRepository,
            IRepository<TeamMember, Guid> teamMemberRepository,
            WorkTaskMapper workTaskMapper,
            WorkTaskManager workTaskManager)
        {
            _distributeCache = distributeCache;
            _logger = logger;
            _workTaskRepository = workTaskRepository;
            _projectRepository = projectRepository;
            _statusRepository = statusRepository;
            _priorityRepository = priorityRepository;
            _teamMemberRepository = teamMemberRepository;
            _workTaskMapper = workTaskMapper;
            _workTaskManager = workTaskManager;
        }
        [Authorize(ProjectManagementPermissions.Tasks.Create)]
        [Audited]
        public async Task<WorkTaskDetailDto> CreateWorkTaskAsync(CreateWorkTaskDto input)
        {
            var workTask = await _workTaskManager.CreateWorkTaskAsync(input.Title, input.StartedDate, input.EndedDate, input.ProjectId, input.StatusId, input.PriorityId, input.AssigneeId);
            await _workTaskRepository.InsertAsync(workTask);
            await InvalidateWorkTaskCacheAsync();
            var cacheKey = await BuildWorkTaskByIdCacheKeyAsync(workTask.Id);
            var result = _workTaskMapper.MapWithDates(workTask);
            await SetCacheWorkTaskAsync<WorkTaskDetailDto>(cacheKey, result);
            _logger.LogInformation("Created work task {WorkTaskId} with title {Title}", workTask.Id, workTask.Title);
            return result;
        }
        [Authorize(ProjectManagementPermissions.Tasks.Default)]
        public async Task<PagedResultDto<WorkTaskDetailDto>> GetListWorkTaskAsync(WorkTaskPagedAndSortedResultRequestDto input)
        {
            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = nameof(WorkTask.CreationTime);
            }
            var cacheKey = await BuildWorkTaskListCacheKeyAsync(input);
            var cacheItems = await GetCacheWorkTaskListAsync(cacheKey);
            if (cacheItems != null) return cacheItems;
            
            var query = await _workTaskRepository.GetQueryableAsync();

            var filter = input.Filter?.Trim();
            if (!filter.IsNullOrWhiteSpace())
            {
                query = query.Where(x => x.Title.Contains(filter));
            }

            if (input.ProjectId.HasValue)
            {
                query = query.Where(x => x.ProjectId == input.ProjectId.Value);
            }

            if (input.StatusId.HasValue)
            {
                query = query.Where(x => x.StatusId == input.StatusId.Value);
            }

            if (input.PriorityId.HasValue)
            {
                query = query.Where(x => x.PriorityId == input.PriorityId.Value);
            }

            if (input.AssigneeId.HasValue)
            {
                query = query.Where(x => x.AssigneeId == input.AssigneeId.Value);
            }

            var totalCount = await AsyncExecuter.CountAsync(query);
            query = query.OrderBy(input.Sorting);
            query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
            var workTasks = await AsyncExecuter.ToListAsync(query);

            var projectIds = workTasks.Select(x => x.ProjectId).Distinct().ToList();
            var statusIds = workTasks.Select(x => x.StatusId).Distinct().ToList();
            var priorityIds = workTasks.Select(x => x.PriorityId).Distinct().ToList();
            var assigneeIds = workTasks
                .Where(x => x.AssigneeId.HasValue)
                .Select(x => x.AssigneeId!.Value)
                .Distinct()
                .ToList();

            var projectQuery = await _projectRepository.GetQueryableAsync();
            var statusQuery = await _statusRepository.GetQueryableAsync();
            var priorityQuery = await _priorityRepository.GetQueryableAsync();
            var assigneeQuery = await _teamMemberRepository.GetQueryableAsync();

            var projects = await AsyncExecuter.ToListAsync(projectQuery.Where(x => projectIds.Contains(x.Id)));
            var statuses = await AsyncExecuter.ToListAsync(statusQuery.Where(x => statusIds.Contains(x.Id)));
            var priorities = await AsyncExecuter.ToListAsync(priorityQuery.Where(x => priorityIds.Contains(x.Id)));
            var assignees = await AsyncExecuter.ToListAsync(assigneeQuery.Where(x => assigneeIds.Contains(x.Id)));

            var projectLookup = projects.ToDictionary(x => x.Id, x => x.Name);
            var statusLookup = statuses.ToDictionary(x => x.Id, x => x.Title);
            var priorityLookup = priorities.ToDictionary(x => x.Id, x => x.Title);
            var assigneeLookup = assignees.ToDictionary(x => x.Id, x => x.Name);

            var results = new List<WorkTaskDetailDto>(workTasks.Count);
            foreach (var workTask in workTasks)
            {
                projectLookup.TryGetValue(workTask.ProjectId, out var projectName);
                statusLookup.TryGetValue(workTask.StatusId, out var statusName);
                priorityLookup.TryGetValue(workTask.PriorityId, out var priorityName);

                var assigneeName = string.Empty;
                if (workTask.AssigneeId.HasValue)
                {
                    assigneeLookup.TryGetValue(workTask.AssigneeId.Value, out assigneeName);
                }

                results.Add(_workTaskMapper.ToDetailDtoWithNames(
                    workTask,
                    projectName ?? string.Empty,
                    statusName ?? string.Empty,
                    priorityName ?? string.Empty,
                    assigneeName ?? string.Empty));
            }
            var pagedResults = new PagedResultDto<WorkTaskDetailDto>(totalCount, results);
            await SetCacheWorkTaskListAsync(cacheKey, pagedResults);
            return pagedResults;
        }


        [Authorize(ProjectManagementPermissions.Tasks.Default)]
        public async Task<WorkTaskDetailDto> GetWorkTaskDetailAsync(Guid id)
        {
            var cacheKey = await BuildWorkTaskByIdCacheKeyAsync(id);
            var cacheItems = await GetCacheWorkTaskAsync(cacheKey);
            if (cacheItems != null) return cacheItems;
            var workTask = await _workTaskRepository.GetAsync(id);
            var projectQuery = await _projectRepository.GetQueryableAsync();
            var statusQuery = await _statusRepository.GetQueryableAsync();
            var priorityQuery = await _priorityRepository.GetQueryableAsync();
            var assigneeQuery = await _teamMemberRepository.GetQueryableAsync();

                var projectName = await AsyncExecuter.FirstOrDefaultAsync(projectQuery.Where(x => x.Id == workTask.ProjectId))
                    .ContinueWith(t => t.Result?.Name ?? string.Empty);

                var statusName = await AsyncExecuter.FirstOrDefaultAsync(statusQuery.Where(x => x.Id == workTask.StatusId))
                    .ContinueWith(t => t.Result?.Title ?? string.Empty);

                var priorityName = await AsyncExecuter.FirstOrDefaultAsync(priorityQuery.Where(x => x.Id == workTask.PriorityId))
                    .ContinueWith(t => t.Result?.Title ?? string.Empty);

            var assigneeName = string.Empty;
            if (workTask.AssigneeId.HasValue)
            {
                    assigneeName = await AsyncExecuter.FirstOrDefaultAsync(assigneeQuery.Where(x => x.Id == workTask.AssigneeId.Value))
                        .ContinueWith(t => t.Result?.Name ?? string.Empty);
            }
            var result = _workTaskMapper.ToDetailDtoWithNames(workTask, projectName, statusName, priorityName, assigneeName);
            await SetCacheWorkTaskAsync(cacheKey, result);
            return result;
        }
        [Authorize(ProjectManagementPermissions.Tasks.Edit)]
        [Audited]
        public async Task UpdateWorkTaskAsync(Guid id, UpdateWorkTaskDto input)
        {
            var workTask = await _workTaskRepository.GetAsync(id);
            await _workTaskManager.UpdateWorkTaskAsync(workTask, input.Title, input.StartedTime, input.EndedTime, input.ProjectId, input.StatusId, input.PriorityId, input.AssigneeId);
            await _workTaskRepository.UpdateAsync(workTask);
            await InvalidateWorkTaskCacheAsync();
            _logger.LogInformation("Updated work task {WorkTaskId} to title {Title}", workTask.Id, workTask.Title);
        }
        [Authorize(ProjectManagementPermissions.Tasks.Delete)]
        [Audited]
        public async Task DeleteWorkTaskAsync(Guid id)
        {
            var workTask = await _workTaskRepository.GetAsync(id);
            await _workTaskRepository.DeleteAsync(workTask);
            await InvalidateWorkTaskCacheAsync();
            _logger.LogInformation("Deleted work task {WorkTaskId}", workTask.Id);
        }

        [Authorize(ProjectManagementPermissions.Tasks.Default)]
        public async Task<List<WorkTaskShortDetailDto>> GetListWorkTaskByTeamMemberAsync(Guid teamMemberId)
        {
            var cacheKey = await BuildWorkTaskByTeamMemberIdCacheAsync(teamMemberId);
            var cacheItems = await GetCacheWorkTaskListByTeamMemberAsync(cacheKey);
            if (cacheItems != null) return cacheItems;
            var workTaskQuery = await _workTaskRepository.GetQueryableAsync();
            var statusQuery = await _statusRepository.GetQueryableAsync();
            var projectQuery = await _projectRepository.GetQueryableAsync();

            var workTasks = await AsyncExecuter.ToListAsync(
                workTaskQuery
                    .Where(x => x.AssigneeId == teamMemberId)
                    .OrderByDescending(x => x.StartedTime)
                    .ThenByDescending(x => x.CreationTime));
            var projectIds = workTasks.Select(x => x.ProjectId).Distinct().ToList();
            var statusIds = workTasks.Select(x => x.StatusId).Distinct().ToList();

            var projects = await AsyncExecuter.ToListAsync(projectQuery.Where(x => projectIds.Contains(x.Id)));
            var statuses = await AsyncExecuter.ToListAsync(statusQuery.Where(x => statusIds.Contains(x.Id)));

            var projectLookup = projects.ToDictionary(x => x.Id, x => x.Name);
            var statusLookup = statuses.ToDictionary(x => x.Id, x => x.Title);

            var results = new List<WorkTaskShortDetailDto>(workTasks.Count);
            foreach (var workTask in workTasks)
            {
                projectLookup.TryGetValue(workTask.ProjectId, out var projectName);
                statusLookup.TryGetValue(workTask.StatusId, out var statusName);

                results.Add(_workTaskMapper.ToShortDetailDtoWithNames(
                    workTask,
                    projectName ?? string.Empty,
                    statusName ?? string.Empty
                ));
            }
            await SetCacheWorkTaskListByTeamMemberAsync(cacheKey, results);
            return results;
        }


        //Redis caching helper methods
        private async Task<WorkTaskDetailDto?> GetCacheWorkTaskAsync(string cacheKey)
        {
            return await _distributeCache.GetJsonAsync<WorkTaskDetailDto>(cacheKey);
        }
        private async Task SetCacheWorkTaskAsync<T>(string cacheKey, T dto)
        {
            await _distributeCache.SetJsonAsync(cacheKey, dto, CacheOptions);
        }
        private async Task<PagedResultDto<WorkTaskDetailDto>?> GetCacheWorkTaskListAsync(string cacheKey)
        {
            return await _distributeCache.GetJsonAsync<PagedResultDto<WorkTaskDetailDto>?>(cacheKey);
        }
        private async Task<List<WorkTaskShortDetailDto>?> GetCacheWorkTaskListByTeamMemberAsync(string cacheKey)
        {
            return await _distributeCache.GetJsonAsync<List<WorkTaskShortDetailDto>?>(cacheKey);
        }
        private async Task SetCacheWorkTaskListAsync(string cacheKey, PagedResultDto<WorkTaskDetailDto> dtos)
        {
            await _distributeCache.SetJsonAsync(cacheKey, dtos, CacheOptions);
        }
        private async Task SetCacheWorkTaskListByTeamMemberAsync(string cacheKey, List<WorkTaskShortDetailDto> dtos)
        {
            await _distributeCache.SetJsonAsync(cacheKey, dtos, CacheOptions);
        }
        private async Task<string> GetCacheKeyVersion()
        {
            var version = await _distributeCache.GetStringAsync(CacheVersionKey);
            if (!string.IsNullOrWhiteSpace(version)) return version;
            version = Guid.NewGuid().ToString("N");
            await _distributeCache.SetStringAsync(CacheVersionKey, version, CacheOptions);
            return version;
        }
        private async Task<string> BuildWorkTaskByTeamMemberIdCacheAsync(Guid teamMemberId)
        {
            var version = await GetCacheKeyVersion();
            return $"ProjectManagement:WorkTasks:{version}:ByTeamMemberId:{teamMemberId:N}";
        }
        private async Task<string> BuildWorkTaskByIdCacheKeyAsync(Guid id)
        {
            var version = await GetCacheKeyVersion();
            return $"ProjectManagement:WorkTasks:{version}:ById:{id:N}";
        }
        private async Task<string> BuildWorkTaskListCacheKeyAsync(WorkTaskPagedAndSortedResultRequestDto input)
        {
            var version = await GetCacheKeyVersion();
            var normalizeFilter = input.Filter?.Trim() ?? string.Empty;
            var normalizeSorting = input.Sorting?.Trim() ?? string.Empty;
            var rawKey = $"{input.SkipCount}:{input.MaxResultCount}:{normalizeSorting}:{normalizeFilter}:{input.ProjectId?.ToString("N") ?? string.Empty}:{input.StatusId?.ToString("N") ?? string.Empty}:{input.PriorityId?.ToString("N") ?? string.Empty}:{input.AssigneeId?.ToString("N") ?? string.Empty}";
            var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            return $"ProjectManagement:WorkTasks:{version}:List:{keyHash}";
        }
        private async Task InvalidateWorkTaskCacheAsync()
        {
            await _distributeCache.SetStringAsync(CacheVersionKey, Guid.NewGuid().ToString("N"), CacheOptions);
        }
    }
}
