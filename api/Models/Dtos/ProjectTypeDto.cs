using System.Text.Json;
using GuguEveryday.Enums;

namespace GuguEveryday.Models.Dtos;

public class ProjectTypeDto
{
    public long Id { get; set; }
    
    public long UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ProgressType ProgressType { get; set; }

    public JsonElement ExtraData { get; set; }
}
