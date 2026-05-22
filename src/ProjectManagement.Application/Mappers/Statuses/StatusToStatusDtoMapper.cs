using ProjectManagement.Statuses;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Mapperly;

namespace ProjectManagement.Mappers.Statuses
{
    public class StatusToStatusDtoMapper : MapperBase<Status, StatusDto>
    {
        public override StatusDto Map(Status source)
        {
            var des = new StatusDto();
            Map(source, des);
            return des;
        }

        public override void Map(Status source, StatusDto destination)
        {
            destination.Id = source.Id;
            destination.Title = source.Title;
            destination.Color = source.Color;
            destination.CreationTime = source.CreationTime;
            destination.CreatorId = source.CreatorId;
            destination.LastModificationTime = source.LastModificationTime;
            destination.LastModifierId = source.LastModifierId;
        }
    }
}
