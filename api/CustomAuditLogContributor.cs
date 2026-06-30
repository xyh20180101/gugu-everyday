using System.Security.Claims;
using GuguEveryday.Services;
using Volo.Abp.Auditing;

namespace GuguEveryday;

public class CustomAuditLogContributor : AuditLogContributor
{
    public override void PreContribute(AuditLogContributionContext context)
    {
        context.AuditInfo.UserName = long.TryParse(context.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id.ToString() : null;
        context.AuditInfo.ClientIpAddress = IpLimitService.GetIpAddress(context.ServiceProvider.GetRequiredService<IHttpContextAccessor>());
        base.PreContribute(context);
    }
}