using ProjectManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
namespace ProjectManagement.Permissions;

public class ProjectManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var projectManagementGroup = context.AddGroup(ProjectManagementPermissions.GroupName, L("Permission:ProjectManagement"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(ProjectManagementPermissions.MyPermission1, L("Permission:MyPermission1"));

        //Dashboard permission
        projectManagementGroup.AddPermission(ProjectManagementPermissions.Dashboard.Host, L("Permission:Dashboard"), MultiTenancySides.Host);
        projectManagementGroup.AddPermission(ProjectManagementPermissions.Dashboard.Tenant, L("Permission:Dashboard"), MultiTenancySides.Tenant);


        //Project permission
        var projectPermission = projectManagementGroup.AddPermission(ProjectManagementPermissions.Projects.Default, L("Permission:Projects"));
        projectPermission.AddChild(ProjectManagementPermissions.Projects.Create, L("Permission:Projects.Create"));
        projectPermission.AddChild(ProjectManagementPermissions.Projects.Edit, L("Permission:Projects.Edit"));
        projectPermission.AddChild(ProjectManagementPermissions.Projects.Delete, L("Permission:Projects.Delete"));

        //Team member permission
        var teamMemberPermission = projectManagementGroup.AddPermission(ProjectManagementPermissions.TeamMembers.Default, L("Permission:TeamMembers"));
        teamMemberPermission.AddChild(ProjectManagementPermissions.TeamMembers.Create, L("Permission:TeamMembers.Create"));
        teamMemberPermission.AddChild(ProjectManagementPermissions.TeamMembers.Edit, L("Permission:TeamMembers.Edit"));
        teamMemberPermission.AddChild(ProjectManagementPermissions.TeamMembers.Delete, L("Permission:TeamMembers.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProjectManagementResource>(name);
    }
}
