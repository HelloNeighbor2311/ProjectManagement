using Microsoft.EntityFrameworkCore;
using ProjectManagement.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectManagement.Statuses
{
    public class EfCoreStatusReporitory : EfCoreRepository<ProjectManagementDbContext, Status, Guid>, IStatusRepository
    {
        public EfCoreStatusReporitory(IDbContextProvider<ProjectManagementDbContext> dbContextProvider): base(dbContextProvider)
        {
        }
        public async  Task<Status> FindStatusByTitleAsync(string title)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.FirstOrDefaultAsync(t => t.Title == title);
        }

        public async Task<List<Status>> GetListStatusAsync(int skipCount, int MaxResultCount, string sorting, string filter = null)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.WhereIf(!filter.IsNullOrWhiteSpace(), t => t.Title.Contains(filter))
                .OrderBy(sorting).Skip(skipCount).Take(MaxResultCount).ToListAsync();
        }
    }
}
