using Microsoft.AspNetCore.Mvc;
using ProjectManagement.TeamMembers;
using System;
using System.Threading.Tasks;

namespace ProjectManagement.Web.Pages.TeamMembers
{
    public class EditModalModel : ProjectManagementPageModel
    {
        [BindProperty]
        public TeamMemberDto TeamMember { get; set; } = new();

        private readonly ITeamMemberAppService _teamMemberAppService;

        public EditModalModel(ITeamMemberAppService teamMemberAppService)
        {
            _teamMemberAppService = teamMemberAppService;
        }

        public async Task OnGetAsync(Guid id)
        {
            TeamMember = await _teamMemberAppService.GetTeamMemberAsync(id);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _teamMemberAppService.UpdateTeamMemberAsync(TeamMember.Id, new UpdateTeamMemberDto
            {
                Name = TeamMember.Name,
                Email = TeamMember.Email,
                Role = TeamMember.Role,
                WeeklyCapacity = TeamMember.WeeklyCapacity
            });

            return NoContent();
        }
    }
}
