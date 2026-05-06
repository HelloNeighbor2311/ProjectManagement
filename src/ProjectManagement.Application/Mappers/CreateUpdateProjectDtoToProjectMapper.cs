using ProjectManagement.Projects;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Mapperly;

namespace ProjectManagement.Mappers
{
    internal class CreateUpdateProjectDtoToProjectMapper : MapperBase<CreateUpdateProjectDto, Project>
    {
        public override Project Map(CreateUpdateProjectDto source)
        {
            var destination = new Project();
            Map(source, destination);
            return destination;
        }

        public override void Map(CreateUpdateProjectDto source, Project destination)
        {
            destination.Name = source.Name;
            destination.Color = source.Color;
            destination.Description = source.Description;
        }
    }
}
