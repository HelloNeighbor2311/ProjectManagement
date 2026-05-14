using Volo.Abp.Application.Dtos;

namespace ProjectManagement.Projects
{
    public class ProjectPagedAndSortedResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}