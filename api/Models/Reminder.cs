using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuguEveryday.Models;

public class Reminder : BaseModel
{
    public long UserId { get; set; }

    public long ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; }

    [StringLength(50)]
    public string Ip { get; set; }

    [StringLength(50)]
    public string IpLocation { get; set; }

    [StringLength(1000)]
    public string UserAgent { get; set; }
}