using ProjectManagement.TeamMembers;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Mapperly;

namespace ProjectManagement.Mappers.TeamMembers
{
    public class TeamMemberToTeamMemberDtoMapper : MapperBase<TeamMember, TeamMemberDto>
    {
        public override TeamMemberDto Map(TeamMember source)
        {
            var destination = new TeamMemberDto();
            Map(source, destination);
            return destination;
        }

        public override void Map(TeamMember source, TeamMemberDto destination)
        {
            destination.Id = source.Id;
            destination.Name = source.Name;
            destination.Role = source.Role;
            destination.Email = source.Email;
            destination.CurrentCapacity = source.CurrentCapacity;
            destination.WeeklyCapacity = source.WeeklyCapacity;
            destination.CreationTime = source.CreationTime;
            destination.CreatorId = source.CreatorId;
            destination.LastModificationTime = source.LastModificationTime;
            destination.LastModifierId = source.LastModifierId;
        }
    }
}
