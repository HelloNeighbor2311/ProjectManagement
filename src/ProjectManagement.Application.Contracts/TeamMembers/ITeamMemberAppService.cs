using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ProjectManagement.TeamMembers
{
    public interface ITeamMemberAppService : IApplicationService
    {
        Task<TeamMemberDto> GetTeamMemberAsync(Guid id);
        Task<PagedResultDto<TeamMemberDto>> GetListTeamMemberDto(GetTeamMemberListDto input);
        Task<TeamMemberDto> CreateTeamMemberAsync(CreateTeamMemberDto input);
        Task UpdateTeamMemberAsync(Guid id, UpdateTeamMemberDto input);
        Task DeleteTeamMemberAsync(Guid id);
    }
}
