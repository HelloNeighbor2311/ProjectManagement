namespace ProjectManagement;

public static class ProjectManagementDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */
    public const string TeamMemberAlreadyExisted = "ProjectManagement:00001";
    public const string TeamMemberEmailAlreadyExisted = "ProjectManagement:00005";
    public const string TeamMemberInvalidEmail = "ProjectManagement:00006";
    public const string StatusAlreadyExisted = "ProjectManagement:00002";
    public const string WorkTaskTimeConflict = "ProjectManagement:00003";
    public const string WorkTaskAlreadyExisted = "ProjectManagement:00004";
}
