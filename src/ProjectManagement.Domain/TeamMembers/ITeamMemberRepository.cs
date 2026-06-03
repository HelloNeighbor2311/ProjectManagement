using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.TeamMembers
{
    public interface ITeamMemberRepository: IRepository<TeamMember, Guid>
    {
        Task<TeamMember> FindTeamMemberByNameAsync(string name);
        Task<TeamMember> FindTeamMemberByEmailAsync(string email);
        Task<List<TeamMember>> GetListTeamMemberAsync(int skipCount, int MaxResultCount, string sorting, string filter = null);
    }
}
