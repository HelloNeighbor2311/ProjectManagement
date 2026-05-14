using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectManagement.Priorities;
using System;
using System.Threading.Tasks;

namespace ProjectManagement.Web.Pages.Priorities
{
    public class EditModalModel : ProjectManagementPageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CreateUpdatePriorityDto Priority { get; set; }
        private readonly IPriorityAppService _priorityAppService;
        public EditModalModel(IPriorityAppService priorityAppService)
        {
            _priorityAppService = priorityAppService;
        }
        public async Task OnGetAsync()
        {
            var priorityDto = await _priorityAppService.GetAsync(Id);
            Priority = ObjectMapper.Map<PriorityDto, CreateUpdatePriorityDto>(priorityDto);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _priorityAppService.UpdateAsync(Id, Priority);
            return NoContent();
        }
    }
}
