using System.ComponentModel.DataAnnotations;
using GuguEveryday.Attributes;
using GuguEveryday.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace GuguEveryday.Models;

public class ProjectType : BaseModel
{
    public long UserId { get; set; }

    [StringLength(50)]
    [SensitiveLexicon]
    public string Name { get; set; } = string.Empty;

    public ProgressType ProgressType { get; set; }

    [StringLength(1000)]
    [SensitiveLexicon]
    public string ExtraData { get; set; } = "{}";
}