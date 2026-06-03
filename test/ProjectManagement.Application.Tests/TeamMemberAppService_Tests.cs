using ProjectManagement.TeamMembers;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace ProjectManagement
{
    public abstract class TeamMemberAppService_Tests<TStartupModule>: ProjectManagementTestBase<TStartupModule> where TStartupModule : IAbpModule
    {
        private readonly ITeamMemberAppService _teamMemberAppService;
        protected TeamMemberAppService_Tests()
        {
            _teamMemberAppService = GetRequiredService<ITeamMemberAppService>();
        }
        [Fact]
        public async Task Should_Get_All_TeamMembers_Without_Any_Filter()
        {
            var result = await _teamMemberAppService.GetListTeamMemberDto(new TeamMemberPagedAndSortedResultRequestDto());

            result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
            result.Items.ShouldContain(author => author.Name == "Richard Olstand");
            result.Items.ShouldContain(author => author.Name == ("Michael Oliver"));
        }

        [Fact]
        public async Task Should_Get_Filtered_TeamMembers()
        {
            var result = await _teamMemberAppService.GetListTeamMemberDto(
                new TeamMemberPagedAndSortedResultRequestDto { Filter = "Olstand" });

            result.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
            result.Items.ShouldContain(author => author.Name == "Richard Olstand");
            result.Items.ShouldNotContain(author => author.Name == "Michael Oliver");
        }
        [Fact]
        public async Task Should_Create_A_New_TeamMember()
        {
            var authorDto = await _teamMemberAppService.CreateTeamMemberAsync(
                new CreateTeamMemberDto
                {
                    Name = "Edward Bellamy",
                    Email = "BEward@gmail.com",
                    Role = "Senior Frontend Developer",
                    WeeklyCapacity = 40
                }
            );

            authorDto.Id.ShouldNotBe(Guid.Empty);
            authorDto.Name.ShouldBe("Edward Bellamy");
        }
        

    }
}
