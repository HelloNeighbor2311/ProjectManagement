using Microsoft.Extensions.Localization;
using ProjectManagement.Localization;
using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace ProjectManagement.Web;

[Dependency(ReplaceServices = true)]
public class ProjectManagementBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<ProjectManagementResource> _localizer;

    public ProjectManagementBrandingProvider(IStringLocalizer<ProjectManagementResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
