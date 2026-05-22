using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Projects;
using ProjectManagement.WorkTasks;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ProjectManagement.Configurations
{
    public class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
    {
        public void Configure(EntityTypeBuilder<WorkTask> builder)
        {
            builder.ToTable(ProjectManagementConsts.DbTablePrefix + "Tasks", ProjectManagementConsts.DbSchema);
            builder.ConfigureByConvention();
            builder.Property(b => b.Title).IsRequired();
            builder.Property(b => b.ProjectId).IsRequired();
            builder.Property(b => b.StatusId).IsRequired();
            builder.Property(b => b.PriorityId).IsRequired();
            builder.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ProjectId).HasDatabaseName("IX_Tasks_ProjectId");
            builder.HasIndex(x => x.StatusId).HasDatabaseName("IX_Tasks_StatusId");
            builder.HasIndex(x => x.PriorityId).HasDatabaseName("IX_Tasks_PriorityId");
            builder.HasIndex(x => x.AssigneeId).HasDatabaseName("IX_Tasks_AssigneeId");
            builder.HasIndex(x => new
            {
                x.ProjectId,
                x.StatusId
            }).HasDatabaseName("IX_Tasks_ProjectId_With_StatusId");

            builder.HasIndex(x => new
            {
                x.AssigneeId,
                x.StatusId
            }).HasDatabaseName("IX_Tasks_AssigneeId_With_StatusId");
        }
    }
}
