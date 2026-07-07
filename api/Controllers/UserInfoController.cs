using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Json;
using GuguEveryday.Models;
using GuguEveryday.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace GuguEveryday.Controllers;

[Authorize]
public class UserInfoController : BaseController
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private readonly IRepository<UserInfo> _userInfoRepository;

    private readonly IRepository<ProjectType> _projectTypeRepository;

    private readonly IRepository<Project> _projectRepository;

    private readonly IRepository<Reminder> _reminderRepository;

    public UserInfoController(IRepository<UserInfo> userInfoRepository, IRepository<ProjectType> projectTypeRepository, IRepository<Project> projectRepository, IRepository<Reminder> reminderRepository)
    {
        _userInfoRepository = userInfoRepository;
        _projectTypeRepository = projectTypeRepository;
        _projectRepository = projectRepository;
        _reminderRepository = reminderRepository;
    }

    [HttpPut("{id}")]
    public async Task<UserInfoDto> Update(long id, [FromBody] CreateUpdateUserInfoDto dto)
    {
        var entity = await _userInfoRepository.FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);
        if (entity is null)
            throw new EntityNotFoundException("用户信息不存在");

        if (!string.IsNullOrEmpty(dto.Mask) && dto.Mask.Length > 2)
            throw new StatusCodeException("项目名称掩码长度不能大于2");

        entity.Update(dto);

        return entity.ToDto();
    }

    [HttpGet("export-json")]
    public async Task<IActionResult> ExportJson()
    {
        var projectTypes = await _projectTypeRepository.GetListAsync(p => p.UserId == CurrentUserId && !p.IsDeleted);
        var projects = await (await _projectRepository.WithDetailsAsync(p => p.Progresses)).Where(p => p.UserId == CurrentUserId && !p.IsDeleted).ToListAsync();
        var reminders = await _reminderRepository.GetListAsync(p => p.UserId == CurrentUserId && !p.IsDeleted);
        var json = JsonSerializer.Serialize(new
        {
            projectTypes,
            projects,
            reminders
        }, _jsonOptions);

        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", "gugu-export.json");
    }

    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv()
    {
        var projectTypes = await _projectTypeRepository.GetListAsync(p => p.UserId == CurrentUserId && !p.IsDeleted);
        var projects = await (await _projectRepository.WithDetailsAsync(p => p.Progresses)).Where(p => p.UserId == CurrentUserId && !p.IsDeleted).ToListAsync();
        var reminders = await _reminderRepository.GetListAsync(p => p.UserId == CurrentUserId && !p.IsDeleted);

        using var memoryStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteCsvEntry(archive, "projectTypes.csv", new CsvColumn<ProjectType>[]
            {
                new CsvColumn<ProjectType>("Id", p => p.Id.ToString()),
                new CsvColumn<ProjectType>("UserId", p => p.UserId.ToString()),
                new CsvColumn<ProjectType>("Name", p => EscapeCsv(p.Name)),
                new CsvColumn<ProjectType>("ProgressType", p => p.ProgressType.ToString()),
                new CsvColumn<ProjectType>("ExtraData", p => EscapeCsv(p.ExtraData)),
                new CsvColumn<ProjectType>("CreationTime", p => p.CreationTime.ToString("o")),
                new CsvColumn<ProjectType>("LastModificationTime", p => p.LastModificationTime?.ToString("o") ?? ""),
            }, projectTypes);

            var projectsEntry = archive.CreateEntry("projects.csv");
            using (var writer = new StreamWriter(projectsEntry.Open(), Encoding.UTF8))
            {
                var isFirst = true;
                foreach (var p in projects)
                {
                    if (!isFirst) writer.WriteLine();
                    isFirst = false;

                    // 项目表头 + 项目行
                    writer.WriteLine("Id,UserId,TypeId,Name,Description,Order,Color,StartTime,EndTime,IsPublic,IsMask,IsArchived,ExtraData,CreatedBy,CreationTime,LastModificationTime");
                    writer.WriteLine(string.Join(",", new[]
                    {
                        p.Id.ToString(), p.UserId.ToString(), p.TypeId?.ToString() ?? "",
                        EscapeCsv(p.Name), EscapeCsv(p.Description), p.Order.ToString(), EscapeCsv(p.Color),
                        p.StartTime?.ToString("o") ?? "", p.EndTime?.ToString("o") ?? "",
                        p.IsPublic.ToString(), p.IsMask.ToString(), p.IsArchived.ToString(),
                        EscapeCsv(p.ExtraData), p.CreatedBy.ToString(), p.CreationTime.ToString("o"),
                        p.LastModificationTime?.ToString("o") ?? "",
                    }));

                    // 进度表头 + 进度行
                    if (p.Progresses.Count > 0)
                    {
                        writer.WriteLine("Id,ProjectId,CurrentProgress,NextReportProgress,CreatedBy,CreationTime");
                        foreach (var pp in p.Progresses.OrderBy(pp => pp.CreationTime))
                            writer.WriteLine(string.Join(",", new[]
                            {
                                pp.Id.ToString(), p.Id.ToString(),
                                EscapeCsv(pp.CurrentProgress), EscapeCsv(pp.NextReportProgress ?? ""),
                                pp.CreatedBy.ToString(), pp.CreationTime.ToString("o"),
                            }));
                    }
                }
            }

            WriteCsvEntry(archive, "reminders.csv", new CsvColumn<Reminder>[]
            {
                new CsvColumn<Reminder>("Id", r => r.Id.ToString()),
                new CsvColumn<Reminder>("UserId", r => r.UserId.ToString()),
                new CsvColumn<Reminder>("ProjectId", r => r.ProjectId.ToString()),
                new CsvColumn<Reminder>("Ip", r => EscapeCsv(r.Ip)),
                new CsvColumn<Reminder>("IpLocation", r => EscapeCsv(r.IpLocation)),
                new CsvColumn<Reminder>("UserAgent", r => EscapeCsv(r.UserAgent)),
                new CsvColumn<Reminder>("CreationTime", r => r.CreationTime.ToString("o")),
            }, reminders);
        }

        memoryStream.Position = 0;
        return File(memoryStream.ToArray(), "application/zip", "gugu-export.zip");
    }

    private record CsvColumn<T>(string Header, Func<T, string> ValueSelector);

    private static void WriteCsvEntry<T>(System.IO.Compression.ZipArchive archive, string fileName, CsvColumn<T>[] columns, IReadOnlyCollection<T> rows)
    {
        var entry = archive.CreateEntry(fileName);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.WriteLine(string.Join(",", columns.Select(c => EscapeCsv(c.Header))));
        foreach (var row in rows)
            writer.WriteLine(string.Join(",", columns.Select(c => c.ValueSelector(row))));
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

}