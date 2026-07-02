using GuguEveryday.Data;
using GuguEveryday.Filters;
using GuguEveryday.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Auditing;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.MySQL;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;


namespace GuguEveryday;

[DependsOn(typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAutofacModule),
    typeof(AbpMapperlyModule),
    typeof(AbpEntityFrameworkCoreMySQLModule),
    typeof(AbpAuditingModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule))]
public class GuguEverydayModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext builder)
    {
        Console.WriteLine("[启动] 开始注册服务...");

        // 添加服务到容器
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<GuguEveryday.Filters.GlobalExceptionFilter>();
        });

        builder.Services.AddOpenApi();

        builder.Services.AddAbpDbContext<ApplicationDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });
        builder.Services.Configure<AbpDbContextOptions>(options =>
        {
            options.UseMySQL();
        });

        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabled = true;
            options.IsEnabledForGetRequests = false;
            options.IsEnabledForAnonymousUsers = true;
            options.EntityHistorySelectors.AddAllEntities();
            options.Contributors.Add(new CustomAuditLogContributor());
        });

        // 配置 Redis 缓存
        Console.WriteLine("[启动] 连接 Redis...");
        var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "GuguEveryday:";
        });
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
        Console.WriteLine("[启动] Redis 已配置");

        // 注册自定义服务
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<PasswordService>();
        builder.Services.AddScoped<JwtService>();
        builder.Services.AddScoped<PoWService>();
        builder.Services.AddSingleton<Snowflake16>();
        builder.Services.AddScoped<CurrentUser>();
        builder.Services.AddScoped<IpLimitService>();
        builder.Services.AddScoped<EmailService>();

        // 构建敏感词
        Console.WriteLine("[启动] 构建敏感词库...");
        var trie = new Trie();
        foreach (var fileName in Directory.GetFiles("Vocabulary"))
        {
            using var reader = new StreamReader(fileName);
            string? line;
            if (Path.GetFileName(fileName) == "COVID-19词库.txt")
            {
                while ((line = reader.ReadLine()) != null)
                    trie.Add(line.Split("+", StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                while ((line = reader.ReadLine()) != null)
                    trie.Add(line);
            }
        }
        trie.Build();
        builder.Services.AddSingleton(trie);
        Console.WriteLine("[启动] 敏感词库构建完成");

        // 配置JWT认证
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
                        var email = context.Principal?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

                        var cacheKey = $"jwt:{email}:{context.SecurityToken.UnsafeToString()}";
                        var exists = await cache.GetStringAsync(cacheKey);
                        if (string.IsNullOrEmpty(exists))
                        {
                            context.Fail("登录信息已过期");
                        }
                    }
                };
            });

        builder.Services.AddAuthorization();
        Console.WriteLine("[启动] JWT 认证已配置");

        // 配置CORS
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        builder.Services.AddHealthChecks();
        Console.WriteLine("[启动] 服务注册完成");
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        Console.WriteLine("[启动] 配置中间件...");
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseRouting();

        app.UseCors();
        app.UseAuditing();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            if (env.IsDevelopment())
                endpoints.MapOpenApi();
            endpoints.MapHealthChecks("/healthz");
            endpoints.MapControllers();
        });
        Console.WriteLine("[启动] 中间件配置完成");
    }

    public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<GuguEverydayModule>>();

        // 验证数据库连接
        var dbContext = context.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            await dbContext.Database.CanConnectAsync();
            logger.LogInformation("数据库连接成功");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "数据库连接失败，请检查 MySQL 服务是否启动以及连接字符串是否正确");
        }
    }
}