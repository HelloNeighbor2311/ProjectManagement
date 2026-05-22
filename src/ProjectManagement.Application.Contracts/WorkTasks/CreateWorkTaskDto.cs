using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ProjectManagement.WorkTasks
{
    public class CreateWorkTaskDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime StartedDate { get; set; }

        [Required]
        public DateTime EndedDate { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public Guid StatusId { get; set; }

        [Required]
        public Guid PriorityId { get; set; }

        public Guid? AssigneeId { get; set; }
    }
}
