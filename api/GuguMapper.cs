using System.Text.Json;
using GuguEveryday.Enums;
using GuguEveryday.Models;
using GuguEveryday.Models.Dtos;
using Riok.Mapperly.Abstractions;

namespace GuguEveryday;

[Mapper]
public static partial class GuguMapper
{
    //UserInfo
    public static partial UserInfoDto ToDto(this UserInfo obj);
    public static partial UserInfo ToModel(this CreateUpdateUserInfoDto obj);
    public static partial void Update([MappingTarget] this UserInfo obj1, CreateUpdateUserInfoDto obj2);

    //Project
    [MapProperty(nameof(Project.ExtraData), nameof(ProjectDto.ExtraData), Use = nameof(MapExtraDataToDto))]
    public static partial ProjectDto ToDto(this Project obj);
    public static partial ICollection<ProjectDto> ToDto(this ICollection<Project> obj);

    [UserMapping(Default = true)]
    public static ProjectDto MapCarToCarDto(Project project)
    {
        var dto = ToDto(project);
        foreach (var item in dto.Progresses)
        {
            var progress = 0d;
            if (dto.Type.ProgressType == ProgressType.Progress && dto.ExtraData.TryGetProperty("totalProgress", out var tp))
            {
                var totalProgress = tp.GetString();
                if (!string.IsNullOrEmpty(totalProgress))
                {
                    progress = Divide(item.CurrentProgress, tp.ToString());
                }
            }
            else if (dto.Type.ProgressType == ProgressType.Step && dto.ExtraData.TryGetProperty("steps", out var s))
            {
                var steps = s.Deserialize<List<string>>()!;
                if (steps.Count > 0)
                {
                    var index = steps.IndexOf(item.CurrentProgress);
                    progress = (double)(index + 1) / steps.Count;
                }
            }
            item.CurrentProgressValue = progress;
        }
        return dto;

        double Divide(string a, string b)
        {
            try
            {
                if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;

                var isTime = b.Contains(":");

                if (isTime)
                {
                    var formatA = a.Split(':').Length == 2 ? "00:" + a : a;
                    var formatB = b.Split(':').Length == 2 ? "00:" + b : b;

                    if (TimeSpan.TryParse(formatA, out TimeSpan tsA) && TimeSpan.TryParse(formatB, out TimeSpan tsB))
                    {
                        var secA = tsA.TotalSeconds;
                        var secB = tsB.TotalSeconds;
                        return secB == 0 ? 0 : secA / secB;
                    }
                    return 0;
                }
                else
                {
                    if (double.TryParse(a, out var numA) && double.TryParse(b, out var numB))
                    {
                        if (numB == 0) return 0;
                        return numA / numB;
                    }
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
    }

    [MapProperty(nameof(CreateUpdateProjectDto.ExtraData), nameof(Project.ExtraData), Use = nameof(MapExtraDataToModel))]
    public static partial Project ToModel(this CreateUpdateProjectDto obj);
    public static partial void Update([MappingTarget] this Project obj1, CreateUpdateProjectDto obj2);

    //ProjectProgress
    public static partial ProjectProgressDto ToDto(this ProjectProgress obj);
    public static partial ICollection<ProjectProgressDto> ToDto(this ICollection<ProjectProgress> obj);
    public static partial ProjectProgress ToModel(this CreateUpdateProjectProgressDto obj);

    //ProjectType
    [MapProperty(nameof(ProjectType.ExtraData), nameof(ProjectTypeDto.ExtraData), Use = nameof(MapExtraDataToDto))]
    public static partial ProjectTypeDto ToDto(this ProjectType obj);
    public static partial ICollection<ProjectTypeDto> ToDto(this ICollection<ProjectType> obj);

    [MapProperty(nameof(CreateUpdateProjectTypeDto.ExtraData), nameof(ProjectType.ExtraData), Use = nameof(MapExtraDataToModel))]
    public static partial ProjectType ToModel(this CreateUpdateProjectTypeDto obj);
    public static partial void Update([MappingTarget] this ProjectType obj1, CreateUpdateProjectTypeDto obj2);

    //Reminder
    public static partial ReminderDto ToDto(this Reminder obj);
    public static partial ICollection<ReminderDto> ToDto(this ICollection<Reminder> obj);
    public static partial Reminder ToModel(this CreateUpdateReminderDto obj);

    #region 内部方法

    private static JsonElement MapExtraDataToDto(string? json)
        => string.IsNullOrEmpty(json)
            ? JsonDocument.Parse("{}").RootElement
            : JsonDocument.Parse(json!).RootElement;

    private static string MapExtraDataToModel(JsonElement json)
        => json.ValueKind == JsonValueKind.Undefined ? null : json.GetRawText();

    #endregion
}