using ProjectManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace ProjectManagement.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class ProjectManagementController : AbpControllerBase
{
    protected ProjectManagementController()
    {
        LocalizationResource = typeof(ProjectManagementResource);
    }
}
