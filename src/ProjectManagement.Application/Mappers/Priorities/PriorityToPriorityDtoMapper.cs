using ProjectManagement.Priorities;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Mapperly;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ProjectManagement.Mappers.Priorities
{
    internal class PriorityToPriorityDtoMapper : MapperBase<Priority, PriorityDto>
    {
        public override PriorityDto Map(Priority source)
        {
            var des = new PriorityDto();
            Map(source, des);
            return des;
        }

        public override void Map(Priority source, PriorityDto des)
        {
            des.Id = source.Id;
            des.Title = source.Title;
            des.CreationTime = source.CreationTime;
            des.CreatorId = source.CreatorId;
            des.LastModificationTime = source.LastModificationTime;
            des.LastModifierId = source.LastModifierId;
        }
    }
}
