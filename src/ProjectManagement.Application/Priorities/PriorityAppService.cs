using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ProjectManagement.Caching;
using ProjectManagement.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.Priorities
{
    public class PriorityAppService: CrudAppService<Priority, PriorityDto, Guid, PriorityPagedAndSortedResultRequestDto,CreateUpdatePriorityDto>, IPriorityAppService
    {
        private const string CacheVersionKey = "ProjectManagement:Priorities:Version";
        private static readonly DistributedCacheEntryOptions cacheOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(20),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        private readonly IDistributedCache _distributedCache;

        private readonly ILogger<PriorityAppService> _logger;
        public PriorityAppService(IRepository<Priority,Guid> _priorityRepository, ILogger<PriorityAppService> logger, IDistributedCache distributedCache) : base(_priorityRepository)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            GetPolicyName = ProjectManagementPermissions.Priorities.Default;
            GetListPolicyName = ProjectManagementPermissions.Priorities.Default;
            CreatePolicyName = ProjectManagementPermissions.Priorities.Create;
            UpdatePolicyName = ProjectManagementPermissions.Priorities.Edit;
            DeletePolicyName = ProjectManagementPermissions.Priorities.Delete;
        }

        public override async Task<PagedResultDto<PriorityDto>> GetListAsync(PriorityPagedAndSortedResultRequestDto input)
        {
            await CheckGetListPolicyAsync();
            input ??= new PriorityPagedAndSortedResultRequestDto();
            var cacheKey = await BuildPriorityListCacheKeyAsync(input);
            var cacheItems = await GetCachedPriorityListAsync(cacheKey);
            if (cacheItems != null) return cacheItems;
            
            var query = await Repository.GetQueryableAsync();
            var filter = input.Filter?.Trim();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(x => x.Title != null && x.Title.Contains(filter));
            }

            var totalCount = await AsyncExecuter.CountAsync(query);

            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);
            var entities = await AsyncExecuter.ToListAsync(query);
            var dtos = await MapToGetListOutputDtosAsync(entities);
            var pageResult = new PagedResultDto<PriorityDto>(totalCount, dtos);
            await SetCachedPriorityListAsync(cacheKey, pageResult);

            return pageResult;
        }
        public override async Task<PriorityDto> CreateAsync(CreateUpdatePriorityDto input)
        {
            var result = await base.CreateAsync(input);
            var cacheKey = await BuildPriorityByIdCacheKeyAsync(result.Id);
            await InvalidatePriorityCacheAsync();
            await SetCachedPriorityAsync(cacheKey, result);
            _logger.LogInformation("Created priority {PriorityId} with title {Title}", result.Id, result.Title);
            return result;
        }

        public override async Task<PriorityDto> UpdateAsync(Guid id, CreateUpdatePriorityDto input)
        {
            var result = await base.UpdateAsync(id, input);
            var cacheKey = await BuildPriorityByIdCacheKeyAsync(result.Id);
            await InvalidatePriorityCacheAsync();
            await SetCachedPriorityAsync(cacheKey, result);
            _logger.LogInformation("Updated priority {PriorityId} to title {Title}", result.Id, result.Title);
            return result;
        }

        public override async Task DeleteAsync(Guid id)
        {
            await base.DeleteAsync(id);
            await InvalidatePriorityCacheAsync();
            _logger.LogInformation("Deleted priority {PriorityId}", id);
        }

        //Redis caching helper methods
        private async Task<PriorityDto?> GetCachedPriorityAsync(string cacheKey)
        {
            return await _distributedCache.GetJsonAsync<PriorityDto>(cacheKey);
        }
        private async Task SetCachedPriorityAsync(string cacheKey, PriorityDto dto)
        {
            await _distributedCache.SetJsonAsync(cacheKey, dto, cacheOptions);
        }
        private async Task<PagedResultDto<PriorityDto>?> GetCachedPriorityListAsync(string cacheKey)
        {
            return await _distributedCache.GetJsonAsync<PagedResultDto<PriorityDto>?>(cacheKey);
        }
        private async Task SetCachedPriorityListAsync(string cacheKey, PagedResultDto<PriorityDto> dtos)
        {
            await _distributedCache.SetJsonAsync(cacheKey, dtos, cacheOptions);
        }
        private async Task<string> GetCacheVersion()
        {
            var version = await _distributedCache.GetStringAsync(CacheVersionKey);
            if (!string.IsNullOrWhiteSpace(version)) return version;

            version = Guid.NewGuid().ToString("N");
            await _distributedCache.SetStringAsync(CacheVersionKey, version, cacheOptions);
            return version;
        }
        private async Task<string> BuildPriorityByIdCacheKeyAsync(Guid id)
        {
            var version = await GetCacheVersion();
            return $"ProjectManagement:Priority:{version}:ById:{id:N}";
        }
        private async Task<string> BuildPriorityListCacheKeyAsync(PriorityPagedAndSortedResultRequestDto input)
        {
            var normalizeFilter = input.Filter?.Trim() ?? string.Empty;
            var normalizeSorting = input.Sorting?.Trim() ?? string.Empty;
            var version = await GetCacheVersion();
            var rawKey = $"{input.SkipCount}:{input.MaxResultCount}:{normalizeSorting}:{normalizeFilter}";
            var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            return $"ProjectManagement:Priorities:{version}:List:{keyHash}";
        }
        private async Task InvalidatePriorityCacheAsync()
        {
            await _distributedCache.SetStringAsync(CacheVersionKey, Guid.NewGuid().ToString("N"), cacheOptions);
        }
    }
}
