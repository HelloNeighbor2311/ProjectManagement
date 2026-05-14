using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ProjectManagement.Priorities
{
    public class CreateUpdatePriorityDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
    }
}
