using GuguEveryday.Models;
using GuguEveryday.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Domain.Repositories;

namespace GuguEveryday.Controllers;

[Authorize]
public class HomeController : BaseController
{
    private readonly IRepository<Project> _projectRepository;

    public HomeController(IRepository<Project> projectRepository)
    {
        _projectRepository = projectRepository;
    }

    [HttpGet("waterfall")]
    public async Task<List<List<WaterfallData>>> Waterfall([FromQuery] int year, [FromQuery] int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var items = (await _projectRepository.WithDetailsAsync(p => p.Progresses, p => p.Type))
        .Where(p => p.UserId == CurrentUserId && (p.StartTime != null && p.StartTime >= start && p.StartTime < end || p.EndTime != null && p.EndTime >= start && p.EndTime < end))
        .ToList();

        var projects = items.OrderBy(p => p.Order).ThenBy(p => p.StartTime);
        var startDays = projects.Select(p => new WaterfallData
        {
            Name = p.Name,
            Value = p.StartTime is null || p.StartTime.Value.Year != year || p.StartTime.Value.Month != month ? start.Day : p.StartTime.Value.Day
        }).ToList();
        var endDays = projects.Select((p, i) => new WaterfallData
        {
            Name = p.Name,
            Project = p.ToDto(),
            Value = p.EndTime is null || p.EndTime.Value.Year != year || p.EndTime.Value.Month != month ? (end.AddDays(-1).Day - startDays[i].Value) : (p.EndTime.Value.Day - startDays[i].Value),
            ItemStyle = new WaterfallDataItemStyle { Color = p.Color }
        }).ToList();

        return new List<List<WaterfallData>> { startDays, endDays };
    }

    public class WaterfallData
    {
        public string Name { get; set; }

        public ProjectDto? Project { get; set; }

        public int Value { get; set; }

        public WaterfallDataItemStyle? ItemStyle { get; set; }
    }

    public class WaterfallDataItemStyle
    {
        public string Color { get; set; }
    }
}