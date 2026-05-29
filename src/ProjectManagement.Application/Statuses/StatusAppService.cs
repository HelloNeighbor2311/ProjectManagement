using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ProjectManagement.Caching;
using ProjectManagement.Permissions;
using ProjectManagement.WorkTasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.Statuses
{
    [Authorize(ProjectManagementPermissions.Statuses.Default)]
    public class StatusAppService (IDistributedCache _distributedCache , ILogger<StatusAppService> _logger, IStatusRepository _statusRepository, StatusManager _statusManager, IRepository<WorkTask, Guid> _workTaskRepository, WorkTaskManager _workTaskManager): ProjectManagementAppService, IStatusAppService
    {

        private const string CacheVersionKey = "ProjectManagement:Statuses:Version";
        private static readonly DistributedCacheEntryOptions cacheOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(20),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };

        [Authorize(ProjectManagementPermissions.Statuses.Create)]
        public async Task<StatusDto> CreateStatusAsync(CreateStatusDto input)
        {
            var status = await _statusManager.CreateStatusAsync(input.Title);
            await _statusRepository.InsertAsync(status);
            await InvalidateStatusesCacheKeyAsync();
            var dto = ObjectMapper.Map<Status, StatusDto>(status);
            var cachedKey = await BuildStatusByIdCachedKeyAsync(status.Id);
            await SetCachedStatusAsync(cachedKey, dto);
            _logger.LogInformation("Created status {StatusId} with title {Title}", status.Id, status.Title);
            return dto;
        }

        [Authorize(ProjectManagementPermissions.Statuses.Edit)]
        public async Task UpdateStatusAsync(Guid id, UpdateStatusDto input)
        {
            var status = await _statusRepository.GetAsync(id);
            await _statusManager.ChangeStatusTitleAsync(status, input.Title);
            await _statusRepository.UpdateAsync(status);
            var dto = ObjectMapper.Map<Status, StatusDto>(status);
            await InvalidateStatusesCacheKeyAsync();
            var cachedKey = await BuildStatusByIdCachedKeyAsync(id);
            await SetCachedStatusAsync(cachedKey, dto);
            _logger.LogInformation("Updated status {StatusId} to title {Title}", status.Id, status.Title);
        }

        [Authorize(ProjectManagementPermissions.Statuses.Delete)]
        public async Task DeleteStatusAsync(Guid id)
        {
            var replacementStatus = await _statusRepository.GetFirstStatusAsync(id);
            if (replacementStatus == null)
            {
                throw new UserFriendlyException("Cannot delete the last remaining status.");
            }

            var tasksToReassign = await _workTaskRepository.GetListAsync(x => x.StatusId == id);
            foreach (var workTask in tasksToReassign)
            {
                _workTaskManager.ChangeWorkTaskStatus(workTask, replacementStatus.Id);
                await _workTaskRepository.UpdateAsync(workTask);
            }
            _logger.LogInformation("Deleted status {StatusId}", id);
            await _statusRepository.DeleteAsync(id);
            await InvalidateStatusesCacheKeyAsync();
        }

        public async Task<PagedResultDto<StatusDto>> GetListStatusAsync(StatusPagedAndSortedResultRequestDto input)
        {
            var filter = input.Filter ?? string.Empty;

            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = nameof(Status.Title);
            }
            var cacheKey = await BuildStatusListCacheKeyAsync(input);
            var cacheResult = await GetCachedStatusListAsync(cacheKey);
            if (cacheResult != null) return cacheResult;

            var statuses = await _statusRepository.GetListStatusAsync(input.SkipCount, input.MaxResultCount, input.Sorting, filter);
            var totalCount = string.IsNullOrWhiteSpace(filter) ? await _statusRepository.CountAsync() : await _statusRepository.CountAsync(s => s.Title.Contains(filter));
            var statusDtos = new PagedResultDto<StatusDto>(totalCount, ObjectMapper.Map<List<Status>, List<StatusDto>>(statuses));
            await SetCachedStatusListAsync(cacheKey, statusDtos);
            return statusDtos;
        }

        public async Task<StatusDto> GetStatusAsync(Guid id)
        {
            var cacheKey = await BuildStatusByIdCachedKeyAsync(id);
            var cacheItem = await GetCachedStatusAsync(cacheKey);
            if (cacheItem != null) return cacheItem;
            
            var status = await _statusRepository.GetAsync(id);
            var dto = ObjectMapper.Map<Status, StatusDto>(status);
            await SetCachedStatusAsync(cacheKey, dto);
            return dto;
        }


        //Redis caching helper methods
        private async Task<StatusDto?> GetCachedStatusAsync(string cacheKey)
        {
            return await _distributedCache.GetJsonAsync<StatusDto>(cacheKey);
        } 
        private async Task SetCachedStatusAsync(string cacheKey, StatusDto dto)
        {
            await _distributedCache.SetJsonAsync(cacheKey, dto, cacheOptions);
        }
        private async Task<PagedResultDto<StatusDto>?> GetCachedStatusListAsync(string cacheKey)
        {
            return await _distributedCache.GetJsonAsync<PagedResultDto<StatusDto>>(cacheKey);
        }
        private async Task SetCachedStatusListAsync(string cacheKey, PagedResultDto<StatusDto> dtos)
        {
            await _distributedCache.SetJsonAsync(cacheKey, dtos, cacheOptions);
        }
        private async Task<string> GetCacheVersionAsync()
        {
            var version = await _distributedCache.GetStringAsync(CacheVersionKey);
            if (!string.IsNullOrWhiteSpace(version))
            {
                return version;
            }
            version = Guid.NewGuid().ToString("N");
            await _distributedCache.SetStringAsync(CacheVersionKey, version, cacheOptions);
            return version;
        }
        private async Task<string> BuildStatusByIdCachedKeyAsync(Guid id)
        {
            var version = await GetCacheVersionAsync();
            return $"ProjectManagement:Statuses:{version}:ById:{id:N}";
        }
        private async Task<string> BuildStatusListCacheKeyAsync(StatusPagedAndSortedResultRequestDto input)
        {
            var version = await GetCacheVersionAsync();
            var normalizeFilter = input.Filter?.Trim() ?? string.Empty;
            var normalizeSorting = input.Sorting?.Trim() ?? string.Empty;
            var rawKey = $"{input.SkipCount}:{input.MaxResultCount}:{normalizeSorting}: {normalizeFilter}";
            var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            return $"ProjectManagement:Statuses:{version}:List:{keyHash}";
        }
        private async Task InvalidateStatusesCacheKeyAsync()
        {
            await _distributedCache.SetStringAsync(CacheVersionKey, Guid.NewGuid().ToString("N"), cacheOptions);
        }



    }
}
