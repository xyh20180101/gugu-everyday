using System.ComponentModel.DataAnnotations;
using GuguEveryday.Attributes;
using Volo.Abp.Domain.Entities.Auditing;

namespace GuguEveryday.Models;

public class UserInfo : BaseModel
{
    public long UserId { get; set; }

    [StringLength(50)]
    [SensitiveLexicon]
    public string UserName { get; set; } = string.Empty;

    [StringLength(1000)]
    [SensitiveLexicon]
    public string Bulletin { get; set; } = string.Empty;

    public bool IsShowPageEnabled { get; set; }

    [StringLength(50)]
    [SensitiveLexicon]
    public string ShowPageTitle { get; set; } = string.Empty;

    public bool IsAllowReminder { get; set; }

    [StringLength(2)]
    [SensitiveLexicon]
    public string Mask { get; set; } = string.Empty;
}