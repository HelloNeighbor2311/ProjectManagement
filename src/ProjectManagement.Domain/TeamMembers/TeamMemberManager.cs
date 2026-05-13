using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace ProjectManagement.TeamMembers
{
    public class TeamMemberManager(ITeamMemberRepository _teamMemberRepository): DomainService
    {
        public async Task<TeamMember> CreateTeamMemberAsync(string name, string email, string role, int weeklyCapacity)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            var existedTeamMember = await _teamMemberRepository.FindTeamMemberByNameAsync(name);
            if(existedTeamMember != null)
            {
                throw new TeamMemberAlreadyExistedException(name);
            }
            return new TeamMember(GuidGenerator.Create(), name, email, role, weeklyCapacity);
        }
        public async Task ChangeTeamMemberNameAsync(TeamMember teamMember, string name)
        {
            Check.NotNull(teamMember, nameof(teamMember));
            Check.NotNullOrWhiteSpace(name, nameof(name));

            var existedTeamMember = await _teamMemberRepository.FindTeamMemberByNameAsync(name);
            if (existedTeamMember != null)
            {
                throw new TeamMemberAlreadyExistedException(name);
            }
            teamMember.ChangeName(name);
        }
    }
}
