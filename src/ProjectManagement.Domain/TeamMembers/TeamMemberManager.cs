using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.TeamMembers
{
    public class TeamMemberManager(ITeamMemberRepository _teamMemberRepository): DomainService
    {
        public async Task<TeamMember> CreateTeamMemberAsync(string name, string email, string role, int weeklyCapacity)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            Check.NotNullOrWhiteSpace(email, nameof(email));
            Check.NotNullOrWhiteSpace(role, nameof(role));
            var existedTeamMember = await _teamMemberRepository.FindTeamMemberByNameAsync(name);    
            if(existedTeamMember != null)
            {
                throw new BusinessException(ProjectManagementDomainErrorCodes.TeamMemberAlreadyExisted)
                    .WithData("name", name);
            }
            // Email format validation
            var emailAttr = new EmailAddressAttribute();
            if (!emailAttr.IsValid(email))
            {
                throw new BusinessException(ProjectManagementDomainErrorCodes.TeamMemberInvalidEmail)
                    .WithData("email", email);
            }
            var existedByEmail = await _teamMemberRepository.FindTeamMemberByEmailAsync(email);
            if (existedByEmail != null)
            {
                throw new BusinessException(ProjectManagementDomainErrorCodes.TeamMemberEmailAlreadyExisted)
                    .WithData("email", email);
            }
            return new TeamMember(GuidGenerator.Create(), name, email, role, weeklyCapacity);
        }
        public async Task ChangeTeamMemberEmailAsync(TeamMember teamMember, string email)
        {
            Check.NotNull(teamMember, nameof(teamMember));
            Check.NotNullOrWhiteSpace(email, nameof(email));

            var emailAttr = new EmailAddressAttribute();
            if (!emailAttr.IsValid(email))
            {
                throw new BusinessException(ProjectManagementDomainErrorCodes.TeamMemberInvalidEmail)
                    .WithData("email", email);
            }

            var existedByEmail = await _teamMemberRepository.FindTeamMemberByEmailAsync(email);
            if (existedByEmail != null && existedByEmail.Id != teamMember.Id)
            {
                throw new BusinessException(ProjectManagementDomainErrorCodes.TeamMemberEmailAlreadyExisted)
                    .WithData("email", email);
            }

            teamMember.ChangeEmail(email);
        }
        public async Task ChangeTeamMemberNameAsync(TeamMember teamMember, string name)
        {
            Check.NotNull(teamMember, nameof(teamMember));
            Check.NotNullOrWhiteSpace(name, nameof(name));

            var existedTeamMember = await _teamMemberRepository.FindTeamMemberByNameAsync(name);
            if (existedTeamMember != null)
            {
                throw new BusinessException(ProjectManagementDomainErrorCodes.TeamMemberAlreadyExisted)
                    .WithData("name", name);
            }
            teamMember.ChangeName(name);
        }
    }
}
