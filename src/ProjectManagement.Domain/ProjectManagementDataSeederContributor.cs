using ProjectManagement.Priorities;
using ProjectManagement.Projects;
using ProjectManagement.Statuses;
using ProjectManagement.TeamMembers;
using ProjectManagement.WorkTasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement
{
    public class ProjectManagementDataSeederContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<TeamMember, Guid> _teamMemberRepository;
        private readonly IRepository<Priority, Guid> _priorityRepository;
        private readonly IRepository<Status, Guid> _statusRepository;
        private readonly IRepository<Project, Guid> _projectRepository;
        private readonly IRepository<WorkTask, Guid> _workTaskRepository;

        public ProjectManagementDataSeederContributor(
            IRepository<TeamMember, Guid> teamMemberRepository,
            IRepository<Priority, Guid> priorityRepository,
            IRepository<Status, Guid> statusRepository,
            IRepository<Project, Guid> projectRepository,
            IRepository<WorkTask, Guid> workTaskRepository)
        {
            _teamMemberRepository = teamMemberRepository;
            _priorityRepository = priorityRepository;
            _statusRepository = statusRepository;
            _projectRepository = projectRepository;
            _workTaskRepository = workTaskRepository;
        }
        public async Task SeedAsync(DataSeedContext context)
        {
            var teamMembers = new Dictionary<string, Guid>();
            var priorities = new Dictionary<string, Guid>();
            var statuses = new Dictionary<string, Guid>();
            var projects = new Dictionary<string, Guid>();

            if (await _teamMemberRepository.GetCountAsync() == 0)
            {
                var richard = new TeamMember(Guid.NewGuid(), "Richard Olstand", "ORichard@gmail.com", "Frontend Developer", 40);
                var michael = new TeamMember(Guid.NewGuid(), "Michael Oliver", "MOliver@gmail.com", "Backend Developer", 40);

                await _teamMemberRepository.InsertAsync(richard, autoSave: true);
                await _teamMemberRepository.InsertAsync(michael, autoSave: true);

                teamMembers["Richard"] = richard.Id;
                teamMembers["Michael"] = michael.Id;
            }
            else
            {
                var allMembers = await _teamMemberRepository.GetListAsync();
                foreach (var member in allMembers)
                {
                    teamMembers[member.Name] = member.Id;
                }
            }

            if (await _priorityRepository.GetCountAsync() == 0)
            {
                var low = new Priority { Title = "Low" };
                var medium = new Priority { Title = "Medium" };
                var high = new Priority { Title = "High" };

                await _priorityRepository.InsertAsync(low, autoSave: true);
                await _priorityRepository.InsertAsync(medium, autoSave: true);
                await _priorityRepository.InsertAsync(high, autoSave: true);

                priorities["Low"] = low.Id;
                priorities["Medium"] = medium.Id;
                priorities["High"] = high.Id;
            }
            else
            {
                var allPriorities = await _priorityRepository.GetListAsync();
                foreach (var priority in allPriorities)
                {
                    priorities[priority.Title] = priority.Id;
                }
            }

            if (await _statusRepository.GetCountAsync() == 0)
            {
                var todo = new Status(Guid.NewGuid(), "Todo");
                var inProgress = new Status(Guid.NewGuid(), "On Progress");
                var done = new Status(Guid.NewGuid(), "Done");
                var failed = new Status(Guid.NewGuid(), "Failed");

                await _statusRepository.InsertAsync(todo, autoSave: true);
                await _statusRepository.InsertAsync(inProgress, autoSave: true);
                await _statusRepository.InsertAsync(done, autoSave: true);
                await _statusRepository.InsertAsync(failed, autoSave: true);

                statuses["Todo"] = todo.Id;
                statuses["On Progress"] = inProgress.Id;
                statuses["Done"] = done.Id;
                statuses["Failed"] = failed.Id;
            }
            else
            {
                var allStatuses = await _statusRepository.GetListAsync();
                foreach (var status in allStatuses)
                {
                    statuses[status.Title] = status.Id;
                }

                // Ensure "Failed" status exists
                if (!statuses.ContainsKey("Failed"))
                {
                    var failed = new Status(Guid.NewGuid(), "Failed");
                    await _statusRepository.InsertAsync(failed, autoSave: true);
                    statuses["Failed"] = failed.Id;
                }
            }

            if (await _projectRepository.GetCountAsync() == 0)
            {
                var webProject = new Project("Web Application", "Build modern web application", "#3498db");
                var mobileProject = new Project("Mobile App", "Develop cross-platform mobile app", "#e74c3c");
                var desktopProject = new Project("Desktop Client", "Create desktop application", "#27ae60");

                await _projectRepository.InsertAsync(webProject, autoSave: true);
                await _projectRepository.InsertAsync(mobileProject, autoSave: true);
                await _projectRepository.InsertAsync(desktopProject, autoSave: true);

                projects["Web"] = webProject.Id;
                projects["Mobile"] = mobileProject.Id;
                projects["Desktop"] = desktopProject.Id;
            }
            else
            {
                var allProjects = await _projectRepository.GetListAsync();
                foreach (var project in allProjects)
                {
                    projects[project.Name] = project.Id;
                }
            }

            // Seed WorkTasks
            if (await _workTaskRepository.GetCountAsync() == 0)
            {
                var now = DateTime.UtcNow;

                // Web Project Tasks
                var task1 = new WorkTask(
                    Guid.NewGuid(),
                    "Design UI Layout",
                    now.AddDays(-5),
                    now.AddDays(2),
                    projects["Web"],
                    statuses["Todo"],
                    priorities["High"],
                    teamMembers["Richard"]
                );

                var task2 = new WorkTask(
                    Guid.NewGuid(),
                    "Setup Backend API",
                    now.AddDays(-2),
                    now.AddDays(5),
                    projects["Web"],
                    statuses["On Progress"],
                    priorities["High"],
                    teamMembers["Michael"]
                );

                var task3 = new WorkTask(
                    Guid.NewGuid(),
                    "Database Configuration",
                    now.AddDays(1),
                    now.AddDays(7),
                    projects["Web"],
                    statuses["Todo"],
                    priorities["Medium"],
                    null
                );

                // Mobile Project Tasks
                var task4 = new WorkTask(
                    Guid.NewGuid(),
                    "Create App Navigation",
                    now.AddDays(-3),
                    now.AddDays(3),
                    projects["Mobile"],
                    statuses["On Progress"],
                    priorities["Medium"],
                    teamMembers["Richard"]
                );

                var task5 = new WorkTask(
                    Guid.NewGuid(),
                    "Implement User Auth",
                    now.AddDays(2),
                    now.AddDays(10),
                    projects["Mobile"],
                    statuses["Todo"],
                    priorities["High"],
                    teamMembers["Michael"]
                );

                // Desktop Project Tasks
                var task6 = new WorkTask(
                    Guid.NewGuid(),
                    "Setup Project Structure",
                    now.AddDays(-7),
                    now.AddDays(-1),
                    projects["Desktop"],
                    statuses["Done"],
                    priorities["Low"],
                    null
                );

                await _workTaskRepository.InsertAsync(task1, autoSave: true);
                await _workTaskRepository.InsertAsync(task2, autoSave: true);
                await _workTaskRepository.InsertAsync(task3, autoSave: true);
                await _workTaskRepository.InsertAsync(task4, autoSave: true);
                await _workTaskRepository.InsertAsync(task5, autoSave: true);
                await _workTaskRepository.InsertAsync(task6, autoSave: true);
            }
        }
    }
}
