using Volo.Abp.Application.Dtos;

namespace ProjectManagement.Projects
{
    // Request DTO used by Project list endpoint; `Filter` carries keyword from FE.
    public class ProjectPagedAndSortedResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}