using ProjectManagement.Projects;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace ProjectManagement
{
    public abstract class ProjectAppService_Tests<TStartUpModule>: ProjectManagementApplicationTestBase<TStartUpModule> where TStartUpModule : IAbpModule
    {
        private readonly IProjectAppService _projectAppService;

        protected ProjectAppService_Tests()
        {
            _projectAppService = GetRequiredService<IProjectAppService>();
        }
        [Fact]
        public async Task Should_Get_List_Projects()
        {
            var result = await _projectAppService.GetListAsync(new ProjectPagedAndSortedResultRequestDto());
            result.TotalCount.ShouldBeGreaterThan(1);
            result.Items.ShouldContain(b => b.Name == "Project 1");
        }

        [Fact]
        public async Task Should_Create_A_Valid_Project()
        {
            var result = await _projectAppService.CreateAsync(
                new CreateUpdateProjectDto
                {
                    Name = "Test project",
                    Description = "aaaaaaa",
                    Color = "#2495bd"
                }
            );
            result.Id.ShouldNotBe(Guid.Empty);
            result.Name.ShouldBe("Test project");
        }

        [Fact]
        public async Task Should_Not_Create_A_Project_Without_Name()
        {
            var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
            {
                await _projectAppService.CreateAsync(
                    new CreateUpdateProjectDto
                    {
                        Name = "",
                        Description = "asdasdasdad",
                        Color = "#2495bd"
                    }
                );
            });

            exception.ValidationErrors
                .ShouldContain(err => err.MemberNames.Any(mem => mem == "Name"));
        }
    }
}
