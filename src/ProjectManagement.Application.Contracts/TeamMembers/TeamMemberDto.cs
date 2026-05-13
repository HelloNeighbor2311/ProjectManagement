using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace ProjectManagement.TeamMembers
{
    public class TeamMemberDto: AuditedEntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int WeeklyCapacity { get; set; }
        public int CurrentCapacity { get; set; }
    }
}
