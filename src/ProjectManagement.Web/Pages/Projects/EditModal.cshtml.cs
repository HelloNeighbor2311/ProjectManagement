using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Projects;
using System;
using System.Threading.Tasks;

namespace ProjectManagement.Web.Pages.Projects
{
    public class EditModalModel : ProjectManagementPageModel
    {
        [BindProperty]
        public ProjectDto Project { get; set; }

        private readonly IProjectAppService _projectAppService;

        public EditModalModel(IProjectAppService projectAppService)
        {
            _projectAppService = projectAppService;
        }

        public async Task OnGetAsync(Guid id)
        {
            Project = await _projectAppService.GetAsync(id);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _projectAppService.UpdateAsync(Project.Id, new CreateUpdateProjectDto
            {
                Name = Project.Name,
                Description = Project.Description,
                Color = Project.Color
            });

            return NoContent();
        }
    }
}
