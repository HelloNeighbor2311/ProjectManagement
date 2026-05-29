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
                throw new StatusAlreadyExistedException(title);      
            }
            return new Status(GuidGenerator.Create(), title);
        }
        public async Task ChangeStatusColorAsync(Status status, string color)
        {
            Check.NotNull(status, nameof(status));
            Check.NotNullOrWhiteSpace(color, nameof(color));

            status.ChangeColor(color);
        }

        public async Task ChangeStatusTitleAsync(Status status, string title)
        {
            Check.NotNull(status, nameof(status));
            Check.NotNullOrWhiteSpace(title, nameof(title));

            var existedStatus = await _statusRepository.FindStatusByTitleAsync(title);
            if (existedStatus != null && existedStatus.Id != status.Id)
            {
                throw new StatusAlreadyExistedException(title);
            }

            status.ChangeTitle(title);
        }
    }
}
