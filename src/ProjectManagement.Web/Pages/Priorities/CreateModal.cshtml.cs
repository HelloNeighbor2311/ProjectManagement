using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectManagement.Priorities;
using System.Threading.Tasks;

namespace ProjectManagement.Web.Pages.Priorities
{
    public class CreateModalModel : ProjectManagementPageModel
    {
        [BindProperty]
        public CreateUpdatePriorityDto Priority { get; set; }
        private readonly IPriorityAppService _priorityAppService;
        public CreateModalModel(IPriorityAppService priorityAppService)
        {
            _priorityAppService = priorityAppService;
        }
        public void OnGet()
        {
            Priority = new CreateUpdatePriorityDto();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            await _priorityAppService.CreateAsync(Priority);
            return NoContent();
        }
    }
}
