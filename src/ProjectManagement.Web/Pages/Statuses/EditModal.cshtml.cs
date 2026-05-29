using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Permissions;
using ProjectManagement.Statuses;
using System;
using System.Threading.Tasks;

namespace ProjectManagement.Web.Pages.Statuses;

[Authorize(ProjectManagementPermissions.Statuses.Edit)]
public class EditModalModel : ProjectManagementPageModel
{
    [BindProperty]
    public UpdateStatusDto Status { get; set; } = new();

    private readonly IStatusAppService _statusAppService;

    public EditModalModel(IStatusAppService statusAppService)
    {
        _statusAppService = statusAppService;
    }

    public async Task OnGetAsync(Guid id)
    {
        var status = await _statusAppService.GetStatusAsync(id);
        Status = new UpdateStatusDto
        {
            Id = status.Id,
            Title = status.Title
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _statusAppService.UpdateStatusAsync(Status.Id, Status);
        return NoContent();
    }
}
