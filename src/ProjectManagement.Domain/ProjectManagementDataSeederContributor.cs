using ProjectManagement.Priorities;
using ProjectManagement.Projects;
using ProjectManagement.TeamMembers;
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

        public ProjectManagementDataSeederContributor(IRepository<TeamMember, Guid> teamMemberRepository, IRepository<Priority, Guid> priorityRepository)
        {
            _teamMemberRepository = teamMemberRepository;
            _priorityRepository = priorityRepository;
        }
        public async Task SeedAsync(DataSeedContext context)
        {
            if (await _teamMemberRepository.GetCountAsync() == 0)
            {
                await _teamMemberRepository.InsertAsync(
                    new TeamMember(Guid.NewGuid(), "Richard Olstand", "ORichard@gmail.com", "Frontend Developer", 40),
                    autoSave: true);

                await _teamMemberRepository.InsertAsync(
                    new TeamMember(Guid.NewGuid(), "Michael Oliver", "MOliver@gmail.com", "Backend Developer", 40),
                    autoSave: true);
            }

            if(await _priorityRepository.GetCountAsync() == 0)
            {
                await _priorityRepository.InsertAsync(new Priority
                {
                    Title = "Low"
                },autoSave:true);

                await _priorityRepository.InsertAsync(new Priority
                {
                    Title = "Medium"
                }, autoSave: true);
            }
        }
    }
}
