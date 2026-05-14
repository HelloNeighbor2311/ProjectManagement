using ProjectManagement.TeamMembers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.Statuses
{
    public interface IStatusRepository: IRepository<Status, Guid>
    {
        Task<Status> FindStatusByTitleAsync(string title);
        Task<List<Status>> GetListStatusAsync(int skipCount, int MaxResultCount, string sorting, string filter = null);
    }
}
