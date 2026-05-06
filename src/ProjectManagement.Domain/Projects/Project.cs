using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace ProjectManagement.Projects
{
    public class Project: AuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#808080";

        public Project()
        { 
        }
        public Project(string name, string description, string color)
        {
            Name = name;
            Description = description;
            Color = color;
        }
    }
}
