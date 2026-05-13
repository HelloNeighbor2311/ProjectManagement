using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Projects;
using System.Threading.Tasks;

namespace ProjectManagement.Web.Pages.Projects
{
    public class CreateModalModel : ProjectManagementPageModel
    {
        [BindProperty]
        public CreateUpdateProjectDto Project { get; set; }

        private readonly IProjectAppService _projectAppService;

        public CreateModalModel(IProjectAppService projectAppService)
        {
            _projectAppService = projectAppService;
        }

        public void OnGet()
        {
            Project = new CreateUpdateProjectDto
            {
                Name = "",
                Description = "",
                Color = "#808080"
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _projectAppService.CreateAsync(Project);
            return NoContent();
        }
    }
}
