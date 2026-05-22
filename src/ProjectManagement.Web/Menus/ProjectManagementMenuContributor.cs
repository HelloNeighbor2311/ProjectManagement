using System.Threading.Tasks;
using ProjectManagement.Localization;
using ProjectManagement.MultiTenancy;
using ProjectManagement.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;
using Volo.Abp.UI.Navigation;

namespace ProjectManagement.Web.Menus;

public class ProjectManagementMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();
        var l = context.GetLocalizer<ProjectManagementResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                ProjectManagementMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fas fa-home",
                order: 0
            )
        );

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);
        context.Menu.AddItem(
            new ApplicationMenuItem(
                "Project",
                l["Menu:ProjectManagement"],
                icon: "fa fa-book"
            ).AddItem(
                new ApplicationMenuItem(
                    "Project.Project",
                    l["Menu:Project"],
                    url: "/Projects"
                ).RequirePermissions(ProjectManagementPermissions.Projects.Default)
            ).AddItem(
                new ApplicationMenuItem(
                    ProjectManagementMenus.TeamMembers,
                    l["Menu:TeamMembers"],
                    url: "/TeamMembers"
                ).RequirePermissions(ProjectManagementPermissions.TeamMembers.Default)
            ).AddItem(
                new ApplicationMenuItem(
                    ProjectManagementMenus.Priorities,
                    l["Menu:Priorities"],
                    url: "/Priorities"
                ).RequirePermissions(ProjectManagementPermissions.Priorities.Default)
            ).AddItem(
                new ApplicationMenuItem(
                    ProjectManagementMenus.Tasks,
                    l["Menu:Tasks"],
                    url: "/Tasks"
                ).RequirePermissions(ProjectManagementPermissions.Tasks.Default)
            )
        );
        return Task.CompletedTask;
    }
}
