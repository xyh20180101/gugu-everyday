using System.ComponentModel.DataAnnotations;
using GuguEveryday.Attributes;

namespace GuguEveryday.Models;

public class ProjectProgress : BaseModel
{
    public long ProjectId { get; set; }

    [StringLength(20)]
    [SensitiveLexicon]
    public string CurrentProgress { get; set; } = string.Empty;

    [StringLength(20)]
    [SensitiveLexicon]
    public string? NextReportProgress { get; set; }
}