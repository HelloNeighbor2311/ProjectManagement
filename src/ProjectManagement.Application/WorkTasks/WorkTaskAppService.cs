using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Priorities;
using ProjectManagement.Projects;
using ProjectManagement.Statuses;
using ProjectManagement.TeamMembers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp; 

namespace ProjectManagement.WorkTasks
{
    public class WorkTaskAppService : ProjectManagementAppService, IWorkTaskAppService
    {
        private readonly IRepository<WorkTask, Guid> _workTaskRepository;
        private readonly IRepository<Project, Guid> _projectRepository;
        private readonly IRepository<Status, Guid> _statusRepository;
        private readonly IRepository<Priority, Guid> _priorityRepository;
        private readonly IRepository<TeamMember, Guid> _teamMemberRepository;
        private readonly WorkTaskMapper _workTaskMapper;
        private readonly WorkTaskManager _workTaskManager;

        public WorkTaskAppService(
            IRepository<WorkTask, Guid> workTaskRepository,
            IRepository<Project, Guid> projectRepository,
            IRepository<Status, Guid> statusRepository,
            IRepository<Priority, Guid> priorityRepository,
            IRepository<TeamMember, Guid> teamMemberRepository,
            WorkTaskMapper workTaskMapper,
            WorkTaskManager workTaskManager)
        {
            _workTaskRepository = workTaskRepository;
            _projectRepository = projectRepository;
            _statusRepository = statusRepository;
            _priorityRepository = priorityRepository;
            _teamMemberRepository = teamMemberRepository;
            _workTaskMapper = workTaskMapper;
            _workTaskManager = workTaskManager;
        }

        public async Task<WorkTaskDetailDto> CreateWorkTaskAsync(CreateWorkTaskDto input)
        {
            var workTask = await _workTaskManager.CreateWorkTaskAsync(input.Title, input.StartedDate, input.EndedDate, input.ProjectId, input.StatusId, input.PriorityId, input.AssigneeId);
            await _workTaskRepository.InsertAsync(workTask);
            return _workTaskMapper.MapWithDates(workTask);
        }

        public async Task<List<WorkTaskDetailDto>> GetListWorkTaskAsync()
        {
            var workTasks = await _workTaskRepository.GetListAsync();

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

            return results;
        }

        public async Task<WorkTaskDetailDto> GetWorkTaskDetailAsync(Guid id)
        {
            var workTask = await _workTaskRepository.GetAsync(id);
            // Ensure date fields are populated from entity
            return _workTaskMapper.MapWithDates(workTask);
        }

        public async Task UpdateWorkTaskAsync(Guid id, UpdateWorkTaskDto input)
        {
            var workTask = await _workTaskRepository.GetAsync(id);
            await _workTaskManager.UpdateWorkTaskAsync(workTask, input.Title, input.StartedTime, input.EndedTime, input.ProjectId, input.StatusId, input.PriorityId, input.AssigneeId);
            await _workTaskRepository.UpdateAsync(workTask);
        }

        public async Task DeleteWorkTaskAsync(Guid id)
        {
            var workTask = await _workTaskRepository.GetAsync(id);
            await _workTaskRepository.DeleteAsync(workTask);
        }
    }
}
