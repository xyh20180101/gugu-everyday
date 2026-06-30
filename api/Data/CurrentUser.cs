using System.Security.Claims;

namespace GuguEveryday.Data;

public class CurrentUser
{
    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        Id = long.TryParse(httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
    }

    public long? Id { get; private set; }
}