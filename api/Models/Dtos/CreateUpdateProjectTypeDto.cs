using System.Text.Json;
using System.Text.Json.Serialization;
using GuguEveryday.Enums;

namespace GuguEveryday.Models.Dtos;

public class CreateUpdateProjectTypeDto
{
    public string Name { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProgressType ProgressType { get; set; }

    public JsonElement ExtraData { get; set; }
}