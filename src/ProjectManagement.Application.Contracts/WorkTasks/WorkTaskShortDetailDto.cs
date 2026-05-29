using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace ProjectManagement.WorkTasks
{
    public class WorkTaskShortDetailDto: AuditedEntityDto<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public DateTime StartedDate { get; set; }
        public DateTime EndedDate { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;


        public Guid ProjectId { get; set; }
        public Guid StatusId { get; set; }
        public Guid? AssigneeId { get; set; }
    }
}
