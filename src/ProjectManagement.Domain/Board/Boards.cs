using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;
using ProjectManagement.WorkTask;
namespace ProjectManagement.Board
{
    public class Boards: AuditedAggregateRoot<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
    }
}
