using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace ProjectManagement.Statuses
{
    public class Status: AuditedAggregateRoot<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string Color { get; private set; } = string.Empty;

        private Status()
        {
        }
        internal Status(Guid id, string title): base(id)
        {
            Title = title;
            Color = RandomHexColor();
        }
        internal void ChangeColor(string color)
        {
            Color = color;
        }
        private static string RandomHexColor()
        {
            return $"#{Random.Shared.Next(0x1000000):X6}";
        }
    }
}
