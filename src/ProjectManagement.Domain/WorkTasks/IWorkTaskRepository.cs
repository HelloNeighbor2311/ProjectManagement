using ProjectManagement.Statuses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.WorkTasks
{
    public interface IWorkTaskRepository: IRepository<WorkTask, Guid>
    {
        Task<List<WorkTask>> GetListWorkTaskAsync(int skipCount, int MaxResultCount, string sorting, string filter = null);
        Task<WorkTask?> GetWorkTaskByName(string name);
    }
}
