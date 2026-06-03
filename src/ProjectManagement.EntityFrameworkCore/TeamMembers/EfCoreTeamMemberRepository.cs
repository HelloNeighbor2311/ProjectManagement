using Microsoft.EntityFrameworkCore;
using ProjectManagement.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectManagement.TeamMembers
{
    public class EfCoreTeamMemberRepository : EfCoreRepository<ProjectManagementDbContext, TeamMember, Guid>, ITeamMemberRepository
    {
        public EfCoreTeamMemberRepository(IDbContextProvider<ProjectManagementDbContext> dbContextProvider): base(dbContextProvider)
        { }
        public async Task<TeamMember> FindTeamMemberByNameAsync(string name)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<TeamMember> FindTeamMemberByEmailAsync(string email)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.FirstOrDefaultAsync(t => t.Email == email);
        }

        public async Task<List<TeamMember>> GetListTeamMemberAsync(int skipCount, int MaxResultCount, string sorting, string filter = null)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.WhereIf(!filter.IsNullOrWhiteSpace(), t => t.Name.Contains(filter))
                .OrderBy(sorting).Skip(skipCount).Take(MaxResultCount).ToListAsync();
        }
    }
}
