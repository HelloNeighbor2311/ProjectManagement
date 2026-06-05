using Microsoft.AspNetCore.Authorization;
using ProjectManagement.Permissions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Volo.Abp.Caching;
using System.Security.Cryptography;
using ProjectManagement.Caching;

namespace ProjectManagement.TeamMembers
{
    [Authorize(ProjectManagementPermissions.TeamMembers.Default)]
    public class TeamMemberAppService(IDistributedCache distributedCache,ILogger<TeamMemberAppService> _logger, ITeamMemberRepository teamMemberRepository, TeamMemberManager teamMemberManager) : ProjectManagementAppService, ITeamMemberAppService
    {

        private const string CacheVersionKey = "ProjectManagement:TeamMembers:Version";
        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(20),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };

        [Authorize(ProjectManagementPermissions.TeamMembers.Create)]
        [Audited]
        public async Task<TeamMemberDto> CreateTeamMemberAsync(CreateTeamMemberDto input)
        {
            var teamMember = await teamMemberManager.CreateTeamMemberAsync(input.Name, input.Email, input.Role, input.WeeklyCapacity);
            await teamMemberRepository.InsertAsync(teamMember);
            // Invalidate list cache namespace and then populate the single-item cache
            await InvalidateTeamMemberCachesAsync();
            var dto = ObjectMapper.Map<TeamMember, TeamMemberDto>(teamMember);
            var cacheKey = await BuildTeamMemberByIdCacheKeyAsync(teamMember.Id);
            await SetCachedTeamMemberAsync(cacheKey, dto);
            _logger.LogInformation("Created team member {TeamMemberId} with name {Name}", teamMember.Id, teamMember.Name);
            return dto;
        }
        
        [Authorize(ProjectManagementPermissions.TeamMembers.Delete)]
        [Audited]
        public async Task DeleteTeamMemberAsync(Guid id)
        {
            await teamMemberRepository.DeleteAsync(id);
            await InvalidateTeamMemberCachesAsync();
            _logger.LogInformation("Deleted team member {TeamMemberId}", id);
        }
        
        [Authorize(ProjectManagementPermissions.TeamMembers.Default)]
        public async Task<PagedResultDto<TeamMemberDto>> GetListTeamMemberDto(TeamMemberPagedAndSortedResultRequestDto input)
        {
            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = nameof(TeamMember.Name);
            }
            var cacheKey = await BuildTeamMemberListCacheKeyAsync(input);
            var cachedResult = await GetCachedTeamMemberListAsync(cacheKey);
            if (cachedResult != null)
            {
                return cachedResult;
            }

            var filter = input.Filter ?? string.Empty;
            var teamMembers = await teamMemberRepository.GetListTeamMemberAsync(input.SkipCount, input.MaxResultCount, input.Sorting, filter);
            var totalCount = string.IsNullOrWhiteSpace(filter) ? await teamMemberRepository.CountAsync() : await teamMemberRepository.CountAsync(t => t.Name.Contains(filter));
            var teamMemberDtos =  new PagedResultDto<TeamMemberDto>(totalCount, ObjectMapper.Map<List<TeamMember>, List<TeamMemberDto>>(teamMembers));
            await SetCachedListTeamMemberAsync(cacheKey, teamMemberDtos);
            return teamMemberDtos;
        }
       
        [Authorize(ProjectManagementPermissions.TeamMembers.Default)]
        public async Task<TeamMemberDto> GetTeamMemberAsync(Guid id)
        {
            var cachedKey = await BuildTeamMemberByIdCacheKeyAsync(id);
            var cachedItem = await GetCachedTeamMemberAsync(cachedKey);
            if (cachedItem != null) return cachedItem;

            var teamMember = await teamMemberRepository.GetAsync(id);
            var dto = ObjectMapper.Map<TeamMember, TeamMemberDto>(teamMember);
            await SetCachedTeamMemberAsync(cachedKey, dto);
            return dto;
        }
        
        [Authorize(ProjectManagementPermissions.TeamMembers.Edit)]
        [Audited]
        public async Task UpdateTeamMemberAsync(Guid id, UpdateTeamMemberDto input)
        {
            var teamMember = await teamMemberRepository.GetAsync(id);
            if(teamMember.Name != input.Name)
            {
                await teamMemberManager.ChangeTeamMemberNameAsync(teamMember, input.Name);
            }
            teamMember.Role = input.Role;
            if (teamMember.Email != input.Email)
            {
                await teamMemberManager.ChangeTeamMemberEmailAsync(teamMember, input.Email);
            }
            teamMember.WeeklyCapacity = input.WeeklyCapacity;
            await teamMemberRepository.UpdateAsync(teamMember);
            await InvalidateTeamMemberCachesAsync();
            _logger.LogInformation("Updated team member {TeamMemberId} to name {Name}", teamMember.Id, teamMember.Name);
        }

        //Redis cache helpers
        private async Task<TeamMemberDto?> GetCachedTeamMemberAsync(string cachedKey)
        {
            return await distributedCache.GetJsonAsync<TeamMemberDto>(cachedKey);
        } 
        private async Task SetCachedTeamMemberAsync(string cacheKey, TeamMemberDto dto)
        {
            await distributedCache.SetJsonAsync(cacheKey, dto, CacheOptions);
        }
        private async Task<PagedResultDto<TeamMemberDto>?> GetCachedTeamMemberListAsync(string cacheKey)
        {
            return await distributedCache.GetJsonAsync<PagedResultDto<TeamMemberDto>>(cacheKey);
        }
        private async Task SetCachedListTeamMemberAsync(string cachedKey, PagedResultDto<TeamMemberDto> dtos)
        {
            await distributedCache.SetJsonAsync(cachedKey, dtos, CacheOptions);
        }
        private async Task<string> GetCacheVersionAsync()
        {
            var version = await distributedCache.GetStringAsync(CacheVersionKey);
            if (!string.IsNullOrWhiteSpace(version)) return version;
            version = Guid.NewGuid().ToString("N");
            await distributedCache.SetStringAsync(CacheVersionKey, version, CacheOptions);
            return version;
        }

        private async Task<string> BuildTeamMemberByIdCacheKeyAsync(Guid id)
        {
            var version = await GetCacheVersionAsync();
            return $"ProjectManagement:TeamMembers:{version}:ById:{id:N}";
        }
        private async Task<string> BuildTeamMemberListCacheKeyAsync(TeamMemberPagedAndSortedResultRequestDto input)
        {
            var version = await GetCacheVersionAsync();
            var normalizedFilter = input.Filter?.Trim() ?? string.Empty;
            var normalizedSorting = input.Sorting?.Trim() ?? string.Empty;

            var rawKey = $"{input.SkipCount}:{input.MaxResultCount}:{normalizedSorting}:{normalizedFilter}";
            var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            return $"ProjectManagement:TeamMembers:{version}:List:{keyHash}";
        }
        private async Task InvalidateTeamMemberCachesAsync()
        {
            await distributedCache.SetStringAsync(CacheVersionKey, Guid.NewGuid().ToString("N"), CacheOptions);
        }
        public async Task UpdateTeamMemberCapacityAsync(List<TeamMember> teamMembers)
        {
            await teamMemberRepository.UpdateManyAsync(teamMembers);
            await InvalidateTeamMemberCachesAsync();
        }
    }
}
