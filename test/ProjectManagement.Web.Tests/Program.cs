using Microsoft.AspNetCore.Builder;
using ProjectManagement;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();

builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("ProjectManagement.Web.csproj");
await builder.RunAbpModuleAsync<ProjectManagementWebTestModule>(applicationName: "ProjectManagement.Web" );

public partial class Program
{
}
