using ProjectManagement.BackgroundJobs;
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
using Xunit;

namespace ProjectManagement
{
    public class WeeklyMaintenanceJob_Tests : ProjectManagementApplicationTestBase<ProjectManagementApplicationEfCoreTestModule>
    {
        [Fact]
        public async Task Should_Reset_TeamMemberCapacity_And_Reject_Overdue_WorkTasks()
        {
            var statusRepository = GetRequiredService<IStatusRepository>();
            var projectRepository = GetRequiredService<IRepository<Project, Guid>>();
            var priorityRepository = GetRequiredService<IRepository<Priority, Guid>>();
            var teamMemberRepository = GetRequiredService<IRepository<TeamMember, Guid>>();
            var workTaskRepository = GetRequiredService<IWorkTaskRepository>();
            var statusManager = GetRequiredService<StatusManager>();
            var workTaskManager = GetRequiredService<WorkTaskManager>();
            var teamMemberManager = GetRequiredService<TeamMemberManager>();
            var job = GetRequiredService<WeeklyMaintenanceJob>();

            await WithUnitOfWorkAsync(async () =>
            {
                var doneStatus = await statusRepository.FindStatusByTitleAsync("Done");
                if (doneStatus == null)
                {
                    doneStatus = await statusManager.CreateStatusAsync("Done");
                    await statusRepository.InsertAsync(doneStatus, autoSave: true);
                }

                var rejectStatus = await statusRepository.FindStatusByTitleAsync("Reject");
                if (rejectStatus == null)
                {
                    rejectStatus = await statusManager.CreateStatusAsync("Reject");
                    await statusRepository.InsertAsync(rejectStatus, autoSave: true);
                }

                var todoStatus = await statusRepository.FindStatusByTitleAsync("Todo");
                if (todoStatus == null)
                {
                    todoStatus = await statusManager.CreateStatusAsync("Todo");
                    await statusRepository.InsertAsync(todoStatus, autoSave: true);
                }

                var project = (await projectRepository.GetListAsync()).FirstOrDefault();
                if (project == null)
                {
                    project = new Project("Integration Project", "Integration test project", "#123456");
                    await projectRepository.InsertAsync(project, autoSave: true);
                }

                var priority = (await priorityRepository.GetListAsync()).FirstOrDefault();
                if (priority == null)
                {
                    priority = new Priority { Title = "High" };
                    await priorityRepository.InsertAsync(priority, autoSave: true);
                }

                var teamMember = (await teamMemberRepository.GetListAsync()).FirstOrDefault();
                if (teamMember == null)
                {
                    teamMember = await teamMemberManager.CreateTeamMemberAsync("Integration User", "integration@test.com", "Developer", 40);
                    await teamMemberRepository.InsertAsync(teamMember, autoSave: true);
                }

                teamMember.CurrentCapacity = 20;
                await teamMemberRepository.UpdateAsync(teamMember, autoSave: true);

                var overdueWorkTask = await workTaskManager.CreateWorkTaskAsync(
                    title: $"Overdue Task {Guid.NewGuid()}",
                    startedTime: DateTime.UtcNow.AddDays(-5),
                    endedTime: DateTime.UtcNow.AddDays(-1),
                    projectId: project.Id,
                    statusId: todoStatus.Id,
                    priorityId: priority.Id,
                    assigneeId: teamMember.Id
                );

                await workTaskRepository.InsertAsync(overdueWorkTask, autoSave: true);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                await job.ExecuteAsync(new WeeklyMaintenanceJobArgs());
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var rejectStatus = await statusRepository.FindStatusByTitleAsync("Reject");
                rejectStatus.ShouldNotBeNull();

                var teamMember = (await teamMemberRepository.GetListAsync()).First();
                teamMember.CurrentCapacity.ShouldBe(0);

                var overdueTasks = await workTaskRepository.GetListAsync(x => x.EndedTime < DateTime.UtcNow && x.StatusId == rejectStatus.Id);
                overdueTasks.Count.ShouldBeGreaterThan(0);
            });
        }
    }
}
