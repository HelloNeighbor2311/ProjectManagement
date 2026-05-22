using ProjectManagement.Priorities;
using ProjectManagement.Projects;
using ProjectManagement.Statuses;
using ProjectManagement.TeamMembers;
using ProjectManagement.WorkTasks;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace ProjectManagement
{
    public abstract class WorkTaskAppService_Tests<TStartupModule> : ProjectManagementApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly IWorkTaskAppService _workTaskAppService;
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly IRepository<Project, Guid> _projectRepository;
        private readonly IRepository<Status, Guid> _statusRepository;
        private readonly IRepository<Priority, Guid> _priorityRepository;
        private readonly IRepository<TeamMember, Guid> _teamMemberRepository;

        protected WorkTaskAppService_Tests()
        {
            _workTaskAppService = GetRequiredService<IWorkTaskAppService>();
            _workTaskRepository = GetRequiredService<IWorkTaskRepository>();
            _projectRepository = GetRequiredService<IRepository<Project, Guid>>();
            _statusRepository = GetRequiredService<IRepository<Status, Guid>>();
            _priorityRepository = GetRequiredService<IRepository<Priority, Guid>>();
            _teamMemberRepository = GetRequiredService<IRepository<TeamMember, Guid>>();
        }

        [Fact]
        public async Task Should_Create_A_New_WorkTask()
        {
            var project = new Project("Test project", "Project for work task", "#2495bd");
            await _projectRepository.InsertAsync(project, autoSave: true);

            var status = (await _statusRepository.GetListAsync()).FirstOrDefault();
            status.ShouldNotBeNull();

            var priority = (await _priorityRepository.GetListAsync()).FirstOrDefault();
            priority.ShouldNotBeNull();

            var assignee = (await _teamMemberRepository.GetListAsync()).FirstOrDefault();
            assignee.ShouldNotBeNull();

            var startedTime = DateTime.UtcNow;
            var endedTime = startedTime.AddDays(1);
            var title = $"Task {Guid.NewGuid():N}";

            var result = await _workTaskAppService.CreateWorkTaskAsync(
                new CreateWorkTaskDto
                {
                    Title = title,
                    StartedDate = startedTime,
                    EndedDate = endedTime,
                    ProjectId = project.Id,
                    StatusId = status!.Id,
                    PriorityId = priority!.Id,
                    AssigneeId = assignee!.Id
                });

            result.ShouldNotBeNull();
            result.Title.ShouldBe(title);
            result.ProjectId.ShouldBe(project.Id);
            result.StatusId.ShouldBe(status.Id);
            result.PriorityId.ShouldBe(priority.Id);
            result.AssigneeId.ShouldBe(assignee.Id);

            var stored = await _workTaskRepository.GetWorkTaskByName(title);
            stored.ShouldNotBeNull();
            stored!.ProjectId.ShouldBe(project.Id);
        }
    }
}
