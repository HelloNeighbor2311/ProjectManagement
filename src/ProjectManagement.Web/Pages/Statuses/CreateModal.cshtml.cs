using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Statuses;
using System.Threading.Tasks;

namespace ProjectManagement.Web.Pages.Statuses;

public class CreateModalModel : ProjectManagementPageModel
{
    [BindProperty]
    public CreateStatusDto Status { get; set; } = new();

    private readonly IStatusAppService _statusAppService;

    public CreateModalModel(IStatusAppService statusAppService)
    {
        _statusAppService = statusAppService;
    }

    public void OnGet()
    {
        Status = new CreateStatusDto();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _statusAppService.CreateStatusAsync(Status);
        return NoContent();
    }
}