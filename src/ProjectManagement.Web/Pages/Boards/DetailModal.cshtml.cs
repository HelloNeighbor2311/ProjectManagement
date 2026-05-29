using Microsoft.AspNetCore.Mvc;
using ProjectManagement.WorkTasks;
using System;
using System.Threading.Tasks;

namespace ProjectManagement.Web.Pages.WorkTasks;

public class DetailModalModel : ProjectManagementPageModel
{
    public WorkTaskDetailDto WorkTask { get; set; } = new();

    private readonly IWorkTaskAppService _workTaskAppService;

    public DetailModalModel(IWorkTaskAppService workTaskAppService)
    {
        _workTaskAppService = workTaskAppService;
    }

    public async Task OnGetAsync(Guid id)
    {
        WorkTask = await _workTaskAppService.GetWorkTaskDetailAsync(id);
    }
}