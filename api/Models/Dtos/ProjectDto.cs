using System.Text.Json;
using GuguEveryday.Enums;

namespace GuguEveryday.Models.Dtos;

public class ProjectDto
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long TypeId { get; set; }

    public ProjectTypeDto Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Order { get; set; }

    public string Color { get; set; } = string.Empty;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool IsPublic { get; set; }

    public bool IsArchived { get; set; }

    public JsonElement ExtraData { get; set; }

    public ICollection<ProjectProgressDto> Progresses { get; set; } = [];
}
