using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace ProjectManagement.TeamMembers
{
    public class GetTeamMemberListDto: PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}
