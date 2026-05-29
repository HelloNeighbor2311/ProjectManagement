using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace ProjectManagement.WorkTasks
{
    public class WorkTaskPagedAndSortedResultRequestDto: PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? StatusId { get; set; }
        public Guid? PriorityId { get; set; }
        public Guid? AssigneeId { get; set; }
    }
}
