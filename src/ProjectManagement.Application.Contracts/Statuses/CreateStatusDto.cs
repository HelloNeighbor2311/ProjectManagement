using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ProjectManagement.Statuses
{
    public class CreateStatusDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
    }
}
