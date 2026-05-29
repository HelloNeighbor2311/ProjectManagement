using Microsoft.AspNetCore.Mvc;
using ProjectManagement.TeamMembers;
using ProjectManagement.WorkTasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagement.Web.Pages.TeamMembers;

public class DetailModalModel : ProjectManagementPageModel
{
    public TeamMemberDto TeamMember { get; set; } = new();

    public List<WorkTaskShortDetailDto> WorkTasks { get; set; } = new();

    private readonly ITeamMemberAppService _teamMemberAppService;
    private readonly IWorkTaskAppService _workTaskAppService;

    public DetailModalModel(
        ITeamMemberAppService teamMemberAppService,
        IWorkTaskAppService workTaskAppService)
    {
        _teamMemberAppService = teamMemberAppService;
        _workTaskAppService = workTaskAppService;
    }

    public async Task OnGetAsync(Guid id)
    {
        TeamMember = await _teamMemberAppService.GetTeamMemberAsync(id);
        WorkTasks = await _workTaskAppService.GetListWorkTaskByTeamMemberAsync(id);
    }
}