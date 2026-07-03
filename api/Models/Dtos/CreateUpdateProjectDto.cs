using System.Text.Json;

namespace GuguEveryday.Models.Dtos;

public class CreateUpdateProjectDto
{
    public long TypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Order { get; set; }

    public string Color { get; set; } = string.Empty;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool IsPublic { get; set; }

    public bool IsMask { get; set; }

    public bool IsArchived { get; set; }

    public JsonElement ExtraData { get; set; }
}
