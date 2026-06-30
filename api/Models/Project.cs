using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GuguEveryday.Attributes;

namespace GuguEveryday.Models;

public class Project : BaseModel
{
    public long UserId { get; set; }

    public long? TypeId { get; set; }

    [ForeignKey(nameof(TypeId))]
    public ProjectType? Type { get; set; }

    [StringLength(50)]
    [SensitiveLexicon]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    [SensitiveLexicon]
    public string Description { get; set; } = string.Empty;

    public int Order { get; set; }

    [StringLength(9)]
    [SensitiveLexicon]
    public string Color { get; set; } = string.Empty;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool IsPublic { get; set; }

    public bool IsArchived { get; set; }

    [StringLength(1000)]
    [SensitiveLexicon]
    public string ExtraData { get; set; } = "{}";

    public ICollection<ProjectProgress> Progresses { get; set; } = [];
}