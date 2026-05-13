using ProjectManagement.Projects;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Mapperly;

namespace ProjectManagement.Mappers
{
    public class ProjectToProjectDtoMapper : MapperBase<Project, ProjectDto>
    {
        public override ProjectDto Map(Project source)
        {
            var destination = new ProjectDto();
            Map(source, destination);
            return destination;
        }

        public override void Map(Project source, ProjectDto destination)
        {
            destination.Id = source.Id;
            destination.Name = source.Name;
            destination.Color = source.Color;
            destination.Description = source.Description;
            destination.CreationTime = source.CreationTime;
            destination.CreatorId = source.CreatorId;
            destination.LastModificationTime = source.LastModificationTime;
            destination.LastModifierId = source.LastModifierId;
        }
    }
}
