using ProjectManagement.Priorities;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Mapperly;

namespace ProjectManagement.Mappers.Priorities
{
    internal class PriorityDtoToCreateUpdatePriorityDtoMapper : MapperBase<PriorityDto, CreateUpdatePriorityDto>
    {
        public override CreateUpdatePriorityDto Map(PriorityDto source)
        {
            var des = new CreateUpdatePriorityDto();
            Map(source, des);
            return des;
        }

        public override void Map(PriorityDto source, CreateUpdatePriorityDto destination)
        {
            destination.Title = source.Title;
        }
    }
}
