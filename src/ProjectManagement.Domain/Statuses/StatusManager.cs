using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace ProjectManagement.Statuses
{
    public class StatusManager(IStatusRepository _statusRepository): DomainService
    {
        public async Task<Status> CreateStatusAsync(string title)
        {
            Check.NotNullOrWhiteSpace(title, nameof(title));
            var existedStatus = await _statusRepository.FindStatusByTitleAsync(title);
            if(existedStatus != null)
            {

            }
        }
    }
}
