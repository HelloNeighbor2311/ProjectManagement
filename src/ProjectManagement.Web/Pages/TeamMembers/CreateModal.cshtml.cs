using Microsoft.AspNetCore.Mvc;
using ProjectManagement.TeamMembers;
using System.Threading.Tasks;

namespace ProjectManagement.Web.Pages.TeamMembers
{
    public class CreateModalModel : ProjectManagementPageModel
    {
        [BindProperty]
        public CreateTeamMemberDto TeamMember { get; set; } = new();

        private readonly ITeamMemberAppService _teamMemberAppService;

        public CreateModalModel(ITeamMemberAppService teamMemberAppService)
        {
            _teamMemberAppService = teamMemberAppService;
        }

        public void OnGet()
        {
            TeamMember = new CreateTeamMemberDto
            {
                Name = "",
                Email = "",
                Role = "",
                WeeklyCapacity = 0
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _teamMemberAppService.CreateTeamMemberAsync(TeamMember);
            return NoContent();
        }
    }
}
