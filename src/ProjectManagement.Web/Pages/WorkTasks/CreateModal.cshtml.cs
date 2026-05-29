using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectManagement.Priorities;
using ProjectManagement.Projects;
using ProjectManagement.Statuses;
using ProjectManagement.TeamMembers;
using ProjectManagement.WorkTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Web.Pages.WorkTasks
{
    public class CreateModalModel : ProjectManagementPageModel
    {
        public List<SelectListItem> Projects { get; set; } = new();
        public List<SelectListItem> Priorities { get; set; } = new();
        public List<SelectListItem> Statuses { get; set; } = new();
        public List<SelectListItem> Assignees { get; set; } = new();

        [BindProperty]
        public CreateWorkTaskDto WorkTask { get; set; } = new();

        private readonly IWorkTaskAppService _workTaskAppService;
        private readonly IProjectAppService _projectAppService;
        private readonly IPriorityAppService _priorityAppService;
        private readonly IStatusAppService _statusAppService;
        private readonly ITeamMemberAppService _teamMemberAppService;

        public CreateModalModel(
            IWorkTaskAppService workTaskAppService,
            IProjectAppService projectAppService,
            IPriorityAppService priorityAppService,
            IStatusAppService statusAppService,
            ITeamMemberAppService teamMemberAppService)
        {
            _workTaskAppService = workTaskAppService;
            _projectAppService = projectAppService;
            _priorityAppService = priorityAppService;
            _statusAppService = statusAppService;
            _teamMemberAppService = teamMemberAppService;
        }

        public async Task OnGetAsync(Guid? projectId = null)
        {
            WorkTask.StartedDate = DateTime.UtcNow;
            WorkTask.EndedDate = DateTime.UtcNow.AddDays(1);

            Projects = await LoadProjectsAsync();
            Priorities = await LoadPrioritiesAsync();
            Statuses = await LoadStatusesAsync();
            Assignees = await LoadAsigneesAsync();

            if (projectId.HasValue && projectId.Value != Guid.Empty)
            {
                WorkTask.ProjectId = projectId.Value;
            }
            else if (Projects.Any())
            {
                WorkTask.ProjectId = Guid.Parse(Projects.First().Value);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (WorkTask.StartedDate.Date > WorkTask.EndedDate.Date)
            {
                ModelState.AddModelError("WorkTask.EndedDate", "End date must be after start date.");
                return BadRequest(ModelState);
            }

            var input = new CreateWorkTaskDto
            {
                Title = WorkTask.Title,
                StartedDate = WorkTask.StartedDate,
                EndedDate = WorkTask.EndedDate,
                ProjectId = WorkTask.ProjectId,
                StatusId = WorkTask.StatusId,
                PriorityId = WorkTask.PriorityId,
                AssigneeId = WorkTask.AssigneeId
            };

            await _workTaskAppService.CreateWorkTaskAsync(input);
            return NoContent();
        }

        private async Task<List<SelectListItem>> LoadProjectsAsync()
        {
            var results = await _projectAppService.GetListAsync(new ProjectPagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 100,
                Sorting = "name"
            });

            return results.Items.Select(item => new SelectListItem(item.Name, item.Id.ToString())).ToList();
        }

        private async Task<List<SelectListItem>> LoadPrioritiesAsync()
        {
            var results = await _priorityAppService.GetListAsync(new PriorityPagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 100,
                Sorting = "title"
            });

            return results.Items.Select(item => new SelectListItem(item.Title, item.Id.ToString())).ToList();
        }

        private async Task<List<SelectListItem>> LoadStatusesAsync()
        {
            var results = await _statusAppService.GetListStatusAsync(new StatusPagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 1000,
                Sorting = "title"
            });

            return results.Items.Select(item => new SelectListItem(item.Title, item.Id.ToString())).ToList();
        }

        private async Task<List<SelectListItem>> LoadAsigneesAsync()
        {
            var results = await _teamMemberAppService.GetListTeamMemberDto(new TeamMemberPagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 1000,
                Sorting = "name"
            });

            return results.Items.Select(item => new SelectListItem(item.Name, item.Id.ToString())).ToList();
        }
    }
}