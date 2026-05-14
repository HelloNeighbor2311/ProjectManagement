using Microsoft.EntityFrameworkCore;
using ProjectManagement.Board;
using ProjectManagement.Enums;
using ProjectManagement.Priorities;
using ProjectManagement.Projects;
using ProjectManagement.TeamMembers;
using ProjectManagement.WorkTask;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace ProjectManagement.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class ProjectManagementDbContext :
    AbpDbContext<ProjectManagementDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */
    //public DbSet<Tasks> Tasks { get; set; }
    //public DbSet<Boards> Boards { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<Priority> Priorities { get; set; }
    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public ProjectManagementDbContext(DbContextOptions<ProjectManagementDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(ProjectManagementConsts.DbTablePrefix + "YourEntities", ProjectManagementConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
        //builder.Entity<Boards>(b =>
        //{
        //    b.ToTable(ProjectManagementConsts.DbTablePrefix + "Boards", ProjectManagementConsts.DbSchema);
        //    b.ConfigureByConvention();
        //    b.Property(x => x.Title).IsRequired().HasMaxLength(128);
        //    b.Property(x => x.Description).IsRequired().HasMaxLength(500);
        //});
        //builder.Entity<Tasks>(b =>
        //{
        //    b.ToTable(ProjectManagementConsts.DbTablePrefix + "Tasks", ProjectManagementConsts.DbSchema);
        //    b.ConfigureByConvention();
        //    b.Property(x => x.Title).IsRequired();
        //    b.Property(x => x.Status).IsRequired();
        //    b.Property(x => x.Priority).IsRequired();
        //});
        builder.Entity<Project>(b =>
        {
            b.ToTable(ProjectManagementConsts.DbTablePrefix + "Projects", ProjectManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).IsRequired();
        });
        builder.Entity<TeamMember>(b =>
        {
            b.ToTable(ProjectManagementConsts.DbTablePrefix + "TeamMembers", ProjectManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Email).IsRequired();
            b.Property(x => x.Role).IsRequired();
            b.Property(x => x.WeeklyCapacity).IsRequired();
        });
        builder.Entity<Priority>(b =>
        {
            b.ToTable(ProjectManagementConsts.DbTablePrefix + "Priorities", ProjectManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired();
        });
    }
}
