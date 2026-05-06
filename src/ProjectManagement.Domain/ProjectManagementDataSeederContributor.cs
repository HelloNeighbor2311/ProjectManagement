using ProjectManagement.Projects;
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
        private readonly IRepository<Project, Guid> _projectRepository;

        public ProjectManagementDataSeederContributor(IRepository<Project, Guid> projectRepository)
        {
            _projectRepository = projectRepository;
        }
        public async Task SeedAsync(DataSeedContext context)
        {
            if(await _projectRepository.GetCountAsync() <= 0)
            {
                await _projectRepository.InsertAsync(
                    new Project
                    {
                        Name = "Project 1",
                        Description = "This is the project 1",
                        Color = "#ff6f5c"
                    },
                    autoSave:true);
                await _projectRepository.InsertAsync(
                    new Project
                    {
                        Name = "Project 2",
                        Description = "This is the project 2",
                        Color = "#5eff4f"
                    },
                    autoSave:true);
            }
        }
    }
}
