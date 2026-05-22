using Microsoft.AspNetCore.Authorization;
using ProjectManagement.Permissions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.TeamMembers
{
    [Authorize(ProjectManagementPermissions.TeamMembers.Default)]
    public class TeamMemberAppService(ITeamMemberRepository teamMemberRepository, TeamMemberManager teamMemberManager) : ProjectManagementAppService, ITeamMemberAppService
    {
        [Authorize(ProjectManagementPermissions.TeamMembers.Create)]
        public async Task<TeamMemberDto> CreateTeamMemberAsync(CreateTeamMemberDto input)
        {
            var teamMember = await teamMemberManager.CreateTeamMemberAsync(input.Name, input.Email, input.Role, input.WeeklyCapacity);
            await teamMemberRepository.InsertAsync(teamMember);
            return ObjectMapper.Map<TeamMember, TeamMemberDto>(teamMember);
        }
        [Authorize(ProjectManagementPermissions.TeamMembers.Delete)]
        public async Task DeleteTeamMemberAsync(Guid id)
        {
            await teamMemberRepository.DeleteAsync(id);
        }
        [Authorize(ProjectManagementPermissions.TeamMembers.Default)]
        public async Task<PagedResultDto<TeamMemberDto>> GetListTeamMemberDto(TeamMemberPagedAndSortedResultRequestDto input)
        {
            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = nameof(TeamMember.Name);
            }
            var teamMembers = await teamMemberRepository.GetListTeamMemberAsync(input.SkipCount, input.MaxResultCount, input.Sorting, input.Filter);
            var totalCount = input.Filter == null ? await teamMemberRepository.CountAsync() : await teamMemberRepository.CountAsync(t => t.Name.Contains(input.Filter));
            return new PagedResultDto<TeamMemberDto>(totalCount, ObjectMapper.Map<List<TeamMember>, List<TeamMemberDto>>(teamMembers));
        }
        [Authorize(ProjectManagementPermissions.TeamMembers.Default)]
        public async Task<TeamMemberDto> GetTeamMemberAsync(Guid id)
        {
            var teamMember = await teamMemberRepository.GetAsync(id);
            return ObjectMapper.Map<TeamMember, TeamMemberDto>(teamMember);
        }
        [Authorize(ProjectManagementPermissions.TeamMembers.Edit)]
        public async Task UpdateTeamMemberAsync(Guid id, UpdateTeamMemberDto input)
        {
            var teamMember = await teamMemberRepository.GetAsync(id);
            if(teamMember.Name != input.Name)
            {
                await teamMemberManager.ChangeTeamMemberNameAsync(teamMember, input.Name);
            }
            teamMember.Role = input.Role;
            teamMember.Email = input.Email;
            teamMember.WeeklyCapacity = input.WeeklyCapacity;

            await teamMemberRepository.UpdateAsync(teamMember);
        }
    }
}
