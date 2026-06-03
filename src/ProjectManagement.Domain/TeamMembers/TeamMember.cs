using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ProjectManagement.TeamMembers
{
    public class TeamMember: AuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int WeeklyCapacity { get; set; }
        public int CurrentCapacity { get; set; } = 0;
        private TeamMember() { }
        internal TeamMember(Guid id ,string name, string email, string role, int weeklyCapacity): base(id)
        {
            Name = name;
            Email = email;
            Role = role;
            WeeklyCapacity = weeklyCapacity;
        }

        internal TeamMember ChangeName(string name)
        {
            SetName(name);
            return this;
        }

        internal TeamMember ChangeEmail(string email)
        {
            SetEmail(email);
            return this;
        }

        private void SetEmail(string email)
        {
            Email = Check.NotNullOrWhiteSpace(
                email,
                nameof(email)
            );
        }

        private void SetName(string name)
        {
            Name = Check.NotNullOrWhiteSpace(
                name,
                nameof(name)
            );
        }
    }

}
