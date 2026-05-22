namespace ProjectManagement.Permissions;

public static class ProjectManagementPermissions
{
    public const string GroupName = "ProjectManagement";

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
    public static class Dashboard
    {
        public const string Host = GroupName + ".Dashboard.Host";
        public const string Tenant = GroupName + ".Dashboard.Tenant";
    }
    public static class Projects
    {
        public const string Default = GroupName + ".Projects";
        public const string Create = GroupName + ".Projects.Create";
        public const string Edit = GroupName + ".Projects.Edit";
        public const string Delete = GroupName + ".Projects.Delete";
    }
    public static class TeamMembers
    {
        public const string Default = GroupName + ".TeamMembers";
        public const string Create = GroupName + ".TeamMembers.Create";
        public const string Edit = GroupName + ".TeamMembers.Edit";
        public const string Delete = GroupName + ".TeamMembers.Delete";
    }
    public static class Priorities
    {
        public const string Default = GroupName + ".Priorities";
        public const string Create = GroupName + ".Priorities.Create";
        public const string Edit = GroupName + ".Priorities.Edit";
        public const string Delete = GroupName + ".Priorities.Delete";
    }
    public static class Statuses
    {
        public const string Default = GroupName + ".Statuses";
        public const string Create = GroupName + ".Statuses.Create";
        public const string Delete = GroupName + ".Statuses.Delete";
    }
    public static class Tasks
    {
        public const string Default = GroupName + ".Tasks";
        public const string Create = GroupName + ".Tasks.Create";
        public const string Edit = GroupName + ".Tasks.Edit";
        public const string Delete = GroupName + ".Tasks.Delete";
    }
}
