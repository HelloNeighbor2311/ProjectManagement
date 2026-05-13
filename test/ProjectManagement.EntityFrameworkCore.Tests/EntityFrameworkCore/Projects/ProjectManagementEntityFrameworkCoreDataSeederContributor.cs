using System;
using System.Threading.Tasks;
using ProjectManagement.Projects;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.EntityFrameworkCore
{
    public class ProjectManagementEntityFrameworkCoreDataSeederContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<Project, Guid> _projectRepository;

        public ProjectManagementEntityFrameworkCoreDataSeederContributor(IRepository<Project, Guid> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (await _projectRepository.GetCountAsync() > 0)
            {
                return;
            }

            await _projectRepository.InsertAsync(
                new Project("Project 1", "Project 1 description", "#2495bd"),
                autoSave: true
            );

            await _projectRepository.InsertAsync(
                new Project("Project 2", "Project 2 description", "#808080"),
                autoSave: true
            );
        }
    }
}
