using ProjectManagement.Priorities;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Mapperly;

namespace ProjectManagement.Mappers.Priorities
{
    public class CreateUpdatePriorityDtoToPriorityMapper : MapperBase<CreateUpdatePriorityDto, Priority>
    {
        public override Priority Map(CreateUpdatePriorityDto source)
        {
            var des = new Priority();
            Map(source, des);
            return des;
        }

        public override void Map(CreateUpdatePriorityDto source, Priority destination)
        {
            destination.Title = source.Title;
        }
    }
}
