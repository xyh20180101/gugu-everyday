using System.Reflection;
using GuguEveryday.Attributes;
using GuguEveryday.Models;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace GuguEveryday.Data;

public class ApplicationDbContext : AbpDbContext<ApplicationDbContext>
{
    private readonly Snowflake16 _snowflake16;
    private readonly CurrentUser _currentUser;
    private readonly Trie _trie;
    private readonly ILogger<ApplicationDbContext> _logger;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, Snowflake16 snowflake16, CurrentUser currentUser, Trie trie, ILogger<ApplicationDbContext> logger) : base(options)
    {
        _snowflake16 = snowflake16;
        _currentUser = currentUser;
        _trie = trie;
        _logger = logger;
    }

    public DbSet<User> Users { get; set; }

    public DbSet<UserInfo> UserInfos { get; set; }

    public DbSet<ProjectType> ProjectTypes { get; set; }

    public DbSet<Project> Projects { get; set; }

    public DbSet<ProjectProgress> ProjectProgresses { get; set; }

    public DbSet<Reminder> Reminders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureAuditLogging();

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    public override int SaveChanges()
    {
        CheckSensitiveLexicon();
        AutoFill();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        CheckSensitiveLexicon();
        AutoFill();
        return await base.SaveChangesAsync(cancellationToken);
    }

    void AutoFill()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is BaseModel)
            {
                if (entry.State == EntityState.Added)
                {
                    {
                        var property = GetPropertyInfo(entry.Entity, nameof(BaseModel.Id));
                        property.SetValue(entry.Entity, _snowflake16.NextId());
                    }
                    {
                        var property = GetPropertyInfo(entry.Entity, nameof(BaseModel.CreatedBy));
                        property.SetValue(entry.Entity, _currentUser.Id);
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    if (GetPropertyInfo(entry.Entity, nameof(BaseModel.IsDeleted)).GetValue(entry.Entity) as bool? == true)
                    {
                        var property = GetPropertyInfo(entry.Entity, nameof(BaseModel.DeletedBy));
                        property.SetValue(entry.Entity, _currentUser.Id);
                    }
                    else
                    {
                        var property = GetPropertyInfo(entry.Entity, nameof(BaseModel.UpdatedBy));
                        property.SetValue(entry.Entity, _currentUser.Id);
                    }
                }
            }
        }
    }

    PropertyInfo GetPropertyInfo(object entity, string propertyName)
    {
        return entity.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
    }

    void CheckSensitiveLexicon()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            var entity = entry.Entity;
            var type = entity.GetType();

            foreach (var prop in type.GetProperties())
            {
                if (prop.GetCustomAttribute<SensitiveLexiconAttribute>() is null)
                    continue;

                if (prop.PropertyType != typeof(string))
                    continue;

                var value = prop.GetValue(entity) as string;
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                foreach (var hit in _trie.Find(value))
                {
                    _logger.LogWarning("包含敏感词汇: {Hit}", hit);
                    throw new StatusCodeException("包含敏感词汇，请修改后再提交");
                }
            }
        }
    }
}