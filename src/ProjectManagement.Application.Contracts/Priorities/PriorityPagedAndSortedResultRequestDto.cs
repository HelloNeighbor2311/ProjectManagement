using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace ProjectManagement.Priorities
{
    public class PriorityPagedAndSortedResultRequestDto: PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}
