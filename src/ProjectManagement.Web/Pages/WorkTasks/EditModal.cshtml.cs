using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectManagement.Permissions;
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
    [Authorize(ProjectManagementPermissions.Tasks.Edit)]
    public class EditModalModel : ProjectManagementPageModel
    {
        public List<SelectListItem> Projects { get; set; } = new();
        public List<SelectListItem> Priorities { get; set; } = new();
        public List<SelectListItem> Statuses { get; set; } = new();
        public List<SelectListItem> Assignees { get; set; } = new();

        [BindProperty]
        public WorkTaskDetailDto WorkTask { get; set; } = new();
        public IWorkTaskAppService workTaskAppService;
        public IProjectAppService projectAppService;
        public IPriorityAppService priorityAppService;
        public IStatusAppService statusAppService;
        public ITeamMemberAppService teamMemberAppService;

        public EditModalModel(IWorkTaskAppService _workAppService, IProjectAppService _projectAppService, IPriorityAppService _priorityAppService, IStatusAppService _statusAppService, ITeamMemberAppService _teamMemberAppService)
        {
            workTaskAppService = _workAppService;
            projectAppService = _projectAppService;
            priorityAppService = _priorityAppService;
            statusAppService = _statusAppService;
            teamMemberAppService = _teamMemberAppService;
        }

        public async Task OnGetAsync(Guid id)
        {
            WorkTask = await workTaskAppService.GetWorkTaskDetailAsync(id);
            Projects = await LoadProjectsAsync();
            Priorities = await LoadPrioritiesAsync();
            Statuses = await LoadStatusesAsync();
            Assignees = await LoadAsigneesAsync();
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

            //var startedUtc = DateTime.SpecifyKind(WorkTask.StartedDate.Date, DateTimeKind.Utc);
            //var endedUtc = DateTime.SpecifyKind(WorkTask.EndedDate.Date, DateTimeKind.Utc);

            var input = new UpdateWorkTaskDto
            {
                Title = WorkTask.Title,
                StartedTime = WorkTask.StartedDate,
                EndedTime = WorkTask.EndedDate,
                ProjectId = WorkTask.ProjectId,
                StatusId = WorkTask.StatusId,
                PriorityId = WorkTask.PriorityId,
                AssigneeId = WorkTask.AssigneeId
            };

            await workTaskAppService.UpdateWorkTaskAsync(WorkTask.Id, input);
            return NoContent();
        }
        public async Task<List<SelectListItem>> LoadProjectsAsync()
        {
            var results = await projectAppService.GetListAsync(new ProjectPagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 100,
                Sorting = "name"
            });
            return results.Items.Select(item => new SelectListItem(item.Name, item.Id.ToString())).ToList();
        }
        public async Task<List<SelectListItem>> LoadPrioritiesAsync()
        {
            var results = await priorityAppService.GetListAsync(new PriorityPagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 100,
                Sorting = "title"
            });
            return results.Items.Select(u => new SelectListItem(u.Title, u.Id.ToString())).ToList();
        }
        public async Task<List<SelectListItem>> LoadStatusesAsync()
        {
            var results = await statusAppService.GetListStatusAsync(new StatusPagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 1000,
                Sorting = "title"
            });
            return results.Items.Select(u => new SelectListItem(u.Title, u.Id.ToString())).ToList();
        }
        public async Task<List<SelectListItem>> LoadAsigneesAsync()
        {
            var results = await teamMemberAppService.GetListTeamMemberDto(new TeamMemberPagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 1000,
                Sorting = "name"
            });
            return results.Items.Select(u => new SelectListItem(u.Name, u.Id.ToString())).ToList();
        }
    }
}
