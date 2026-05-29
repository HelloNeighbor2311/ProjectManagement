using ProjectManagement.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Auditing;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using ProjectManagement.Caching;

namespace ProjectManagement.Projects
{
    public class ProjectAppService : CrudAppService<Project, ProjectDto, Guid, ProjectPagedAndSortedResultRequestDto, CreateUpdateProjectDto>, IProjectAppService
    {
        private const string CacheVersionKey = "ProjectManagement:Projects:Version";
        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(20),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        private readonly ILogger<ProjectAppService> _logger;
        private readonly IDistributedCache _distributedCache;


        public ProjectAppService(IRepository<Project, Guid> repository, ILogger<ProjectAppService> logger, IDistributedCache distributedCache) : base(repository)
        {
            _logger = logger;
            _distributedCache = distributedCache;
            GetPolicyName = ProjectManagementPermissions.Projects.Default;
            GetListPolicyName = ProjectManagementPermissions.Projects.Default;
            CreatePolicyName = ProjectManagementPermissions.Projects.Create;
            UpdatePolicyName = ProjectManagementPermissions.Projects.Edit;
            DeletePolicyName = ProjectManagementPermissions.Projects.Delete;
        }

        public override async Task<ProjectDto> GetAsync(Guid id)
        {
            await CheckGetPolicyAsync();

            // Chiến lược read-through: thử Redis trước, nếu miss thì đọc DB rồi ghi lại vào cache.
            var cacheKey = await BuildProjectByIdCacheKeyAsync(id);
            var cachedItem = await GetCachedProjectAsync(cacheKey);
            if (cachedItem != null)
            {
                return cachedItem;
            }

            var result = await base.GetAsync(id);
            await SetCachedProjectAsync(cacheKey, result);
            return result;
        }

        public override async Task<PagedResultDto<ProjectDto>> GetListAsync(ProjectPagedAndSortedResultRequestDto input)
        {
            await CheckGetListPolicyAsync();

            input ??= new ProjectPagedAndSortedResultRequestDto();

            // Cache cả truy vấn danh sách vì trang Projects thường gọi lặp lại cùng bộ lọc/sort.
            var cacheKey = await BuildProjectListCacheKeyAsync(input);
            var cachedResult = await GetCachedProjectListAsync(cacheKey);
            if (cachedResult != null)
            {
                return cachedResult;
            }

            var query = await Repository.GetQueryableAsync();

            // FE gửi `filter`; BE sẽ trim và tìm theo keyword trên Name/Description.
            var filter = input.Filter?.Trim();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(
                    x => (x.Name != null && x.Name.Contains(filter)) ||
                         (x.Description != null && x.Description.Contains(filter))
                );
            }

            // Giữ nguyên flow paging/sorting của ABP sau khi đã lọc dữ liệu.
            var totalCount = await AsyncExecuter.CountAsync(query);

            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);

            var entities = await AsyncExecuter.ToListAsync(query);
            var dtos = await MapToGetListOutputDtosAsync(entities);

            var result = new PagedResultDto<ProjectDto>(totalCount, dtos);
            await SetCachedProjectListAsync(cacheKey, result);
            return result;
        }

        [Audited]
        public override async Task<ProjectDto> CreateAsync(CreateUpdateProjectDto input)
        {
            var result = await base.CreateAsync(input);
            await InvalidateProjectCachesAsync();
            _logger.LogInformation("Created project {ProjectId} named {ProjectName}", result.Id, result.Name);
            return result;
        }

        [Audited]
        public override async Task<ProjectDto> UpdateAsync(Guid id, CreateUpdateProjectDto input)
        {
            var result = await base.UpdateAsync(id, input);
            // Update sẽ làm invalid cả cache chi tiết lẫn cache danh sách.
            await InvalidateProjectCachesAsync();
            _logger.LogInformation("Updated project {ProjectId} to name {ProjectName}", result.Id, result.Name);
            return result;
        }

        [Audited]
        public override async Task DeleteAsync(Guid id)
        {
            var project = await Repository.GetAsync(id);
            await base.DeleteAsync(id);
            // Delete cũng đổi version key để các giá trị cũ bị xem là không hợp lệ.
            await InvalidateProjectCachesAsync();
            _logger.LogInformation("Deleted project {ProjectId} named {ProjectName}", project.Id, project.Name);
        }


        //Redis caching helper methods
        private async Task<ProjectDto?> GetCachedProjectAsync(string cacheKey)
        {
            return await _distributedCache.GetJsonAsync<ProjectDto>(cacheKey);
        }

        private async Task SetCachedProjectAsync(string cacheKey, ProjectDto project)
        {
            await _distributedCache.SetJsonAsync(cacheKey, project, CacheOptions);
        }

        private async Task<PagedResultDto<ProjectDto>?> GetCachedProjectListAsync(string cacheKey)
        {
            return await _distributedCache.GetJsonAsync<PagedResultDto<ProjectDto>>(cacheKey);
        }

        private async Task SetCachedProjectListAsync(string cacheKey, PagedResultDto<ProjectDto> projects)
        {
            await _distributedCache.SetJsonAsync(cacheKey, projects, CacheOptions);
        }
    
        private async Task<string> GetCacheVersionAsync()
        {
            var version = await _distributedCache.GetStringAsync(CacheVersionKey);
            if (!string.IsNullOrWhiteSpace(version))
            {
                return version;
            }

            version = Guid.NewGuid().ToString("N");
            await _distributedCache.SetStringAsync(CacheVersionKey, version, CacheOptions);
            return version;
        }
        private async Task<string> BuildProjectByIdCacheKeyAsync(Guid id)
        {
            var version = await GetCacheVersionAsync();
            return $"ProjectManagement:Projects:{version}:ById:{id:N}";
        }

        private async Task<string> BuildProjectListCacheKeyAsync(ProjectPagedAndSortedResultRequestDto input)
        {
            var version = await GetCacheVersionAsync();
            var normalizedFilter = input.Filter?.Trim() ?? string.Empty;
            var normalizedSorting = input.Sorting?.Trim() ?? string.Empty;
            var rawKey = $"{input.SkipCount}:{input.MaxResultCount}:{normalizedSorting}:{normalizedFilter}";
            var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            return $"ProjectManagement:Projects:{version}:List:{keyHash}";
        }


        private async Task InvalidateProjectCachesAsync()
        {
            await _distributedCache.SetStringAsync(CacheVersionKey, Guid.NewGuid().ToString("N"), CacheOptions);
        }
    }
}
