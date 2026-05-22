using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace ProjectManagement.Statuses
{
    public class StatusPagedAndSortedResultRequestDto: PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}
