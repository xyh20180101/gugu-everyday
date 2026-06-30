using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace GuguEveryday.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected long? CurrentUserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    public class PageRequest
    {
        public int Skip { get; set; }

        public int Count { get; set; } = 10;
    }

    public class PageResponse<T>
    {
        public ICollection<T> Items { get; set; }
        
        public int TotalCount{ get; set; }
    }
}