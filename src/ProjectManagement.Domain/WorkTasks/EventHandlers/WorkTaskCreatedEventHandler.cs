using ProjectManagement.Projects;
using ProjectManagement.WorkTasks.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;

namespace ProjectManagement.WorkTasks.EventHandlers
{
    public class WorkTaskCreatedEventHandler: ILocalEventHandler<WorkTaskCreatedEvent>, ITransientDependency
    {
        private readonly IRepository<Project, Guid> _projectRepository;

        public WorkTaskCreatedEventHandler(
            IRepository<Project, Guid> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task HandleEventAsync(WorkTaskCreatedEvent eventData)
        {
            var project = await _projectRepository.GetAsync(eventData.ProjectId);
            await _projectRepository.UpdateAsync(project);
        }
    }
}
