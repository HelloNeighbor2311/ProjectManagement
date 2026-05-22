using Microsoft.EntityFrameworkCore;
using ProjectManagement.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectManagement.WorkTasks
{
    public class EfcoreWorkTaskRepository : EfCoreRepository<ProjectManagementDbContext, WorkTask, Guid>, IWorkTaskRepository
    {
        public EfcoreWorkTaskRepository(IDbContextProvider<ProjectManagementDbContext> dbContextProvider): base(dbContextProvider)
        {}
        public async Task<List<WorkTask>> GetListWorkTaskAsync(int skipCount, int MaxResultCount, string sorting, string filter = null)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.WhereIf(!filter.IsNullOrWhiteSpace(), t => t.Title.Contains(filter)).OrderBy(sorting).Skip(skipCount).Take(MaxResultCount).ToListAsync();    
        }

        public async Task<WorkTask?> GetWorkTaskByName(string name)
        {
            var dbSet = await GetDbSetAsync();
            var workTask = await dbSet.FirstOrDefaultAsync(x=>x.Title == name);
            return workTask;
        }
    }
}
