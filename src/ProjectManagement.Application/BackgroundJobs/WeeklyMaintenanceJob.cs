using Microsoft.Extensions.Logging;
using ProjectManagement.Statuses;
using ProjectManagement.TeamMembers;
using ProjectManagement.WorkTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace ProjectManagement.BackgroundJobs
{
    [BackgroundJobName("WeeklyMaintenance")]
    public class WeeklyMaintenanceJob : AsyncBackgroundJob<WeeklyMaintenanceJobArgs>, ITransientDependency
    {

        private readonly ITeamMemberRepository teamMemberRepository;
        private readonly IWorkTaskRepository workTaskRepository;
        private readonly IStatusRepository statusRepository;
        private readonly ILogger<WeeklyMaintenanceJob> logger;
        private readonly WorkTaskManager workTaskManager;
        private readonly TeamMemberAppService teamMemberAppService;
        private readonly WorkTaskAppService workTaskAppService;

        public WeeklyMaintenanceJob(ITeamMemberRepository _teamMemberRepository, IWorkTaskRepository _workTaskRepository, 
            IStatusRepository _statusRepository, ILogger<WeeklyMaintenanceJob> _logger, 
            WorkTaskManager _workTaskManager, TeamMemberAppService _teamMemberAppService, WorkTaskAppService _workTaskAppService)
        {
            teamMemberRepository = _teamMemberRepository;
            workTaskManager = _workTaskManager;
            workTaskRepository = _workTaskRepository;
            statusRepository = _statusRepository;
            teamMemberAppService = _teamMemberAppService;
            workTaskAppService = _workTaskAppService;
            logger = _logger;
        }

        [UnitOfWork]
        public override async Task ExecuteAsync(WeeklyMaintenanceJobArgs args)
        {
            try
            {
                logger.LogInformation("Starting weekly maintenance");
                await AutoRejectOverdueTaskAsync();
                await ResetTeamMemberCapacityAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Executing error!");
                throw;
            }
        }

        private async Task ResetTeamMemberCapacityAsync()
        {
            var teamMembers = await teamMemberRepository.GetListAsync();
            if (!teamMembers.Any()) return;
            foreach (var i in teamMembers)
            {
                i.GetType().GetProperty("CurrentCapacity")?.SetValue(i, 0);
            }
            await teamMemberAppService.UpdateTeamMemberCapacityAsync(teamMembers);
        }

        private async Task AutoRejectOverdueTaskAsync()
        {
            var currentTime = DateTime.UtcNow;
            var overdueTasks = await workTaskRepository.GetListAsync(x => !x.IsDeleted && x.EndedTime < currentTime);
            if (!overdueTasks.Any()) return;

            var statuses = await statusRepository.GetListAsync();
            var failedStatus = statuses.FirstOrDefault(x => x.Title.Equals("Failed", StringComparison.OrdinalIgnoreCase));

            if (failedStatus == null)
            {
                logger.LogWarning("Failed status not found!");
                return;
            }

            foreach (var task in overdueTasks)
            {
                try
                {
                    workTaskManager.ChangeWorkTaskStatus(task, failedStatus.Id);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Failed to reject overdue task '{task.Title}'");
                }
            }
            await workTaskAppService.UpdateWorkTaskStatusAsync(overdueTasks);
        }
    }
}
