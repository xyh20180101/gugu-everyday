using System.Text.Json;
using GuguEveryday.Models;
using GuguEveryday.Models.Dtos;
using GuguEveryday.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace GuguEveryday.Controllers;

[Authorize]
public class ReminderController : BaseController
{
    private readonly IRepository<Reminder> _repository;

    private readonly IRepository<Project> _projectRepository;

    private readonly IRepository<UserInfo> _userInfoRepository;

    private readonly IpLimitService _ipLimitService;

    public ReminderController(IRepository<Reminder> repository, IRepository<Project> projectRepository, IpLimitService ipLimitService, IRepository<UserInfo> userInfoRepository)
    {
        _repository = repository;
        _projectRepository = projectRepository;
        _ipLimitService = ipLimitService;
        _userInfoRepository = userInfoRepository;
    }

    public class GetSummaryResponse
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public DateTime LatestReminderTime { get; set; }
        public int TotalCount { get; set; }
        public int IncrementCount { get; set; }
        public ICollection<ReminderDto> Details { get; set; } = [];
    }

    [HttpGet("summary")]
    public async Task<List<GetSummaryResponse>> GetSummary()
    {
        var queryable = (await _repository.GetQueryableAsync())
            .Include(r => r.Project)
            .Where(r => r.UserId == CurrentUserId);

        var allReminders = queryable.OrderByDescending(r => r.CreationTime).ToList();

        var grouped = allReminders
            .GroupBy(r => r.ProjectId)
            .Select(g =>
            {
                var ordered = g.ToList();
                var latest = ordered[0];
                var totalCount = ordered.Count;
                var incrementCount = CalcIncrement(ordered);

                return new GetSummaryResponse
                {
                    ProjectId = g.Key,
                    ProjectName = latest.Project?.Name ?? string.Empty,
                    LatestReminderTime = latest.CreationTime,
                    TotalCount = totalCount,
                    IncrementCount = incrementCount,
                    Details = ordered.Take(20).ToList().ToDto()
                };
            })
            .OrderByDescending(x => x.LatestReminderTime)
            .ToList();

        return grouped;

        int CalcIncrement(List<Reminder> reminders)
        {
            var today = DateTime.Today;
            return reminders.Count(r => r.CreationTime >= today.AddDays(-1));
        }
    }

    [HttpPost("")]
    [AllowAnonymous]
    public async Task<ReminderDto> Create([FromBody] CreateUpdateReminderDto dto)
    {
        var project = await _projectRepository.FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
        if (project is null)
            throw new EntityNotFoundException("项目不存在");

        var userInfo = await _userInfoRepository.FirstOrDefaultAsync(p => p.UserId == project.UserId);
        if (userInfo is null || !userInfo.IsAllowReminder)
            throw new StatusCodeException("不准催更", 403);

        await _ipLimitService.CheckAsync("create-reminder", dto.ProjectId.ToString());

        var entity = new Reminder
        {
            UserId = project.UserId,
            ProjectId = project.Id,
            Ip =  _ipLimitService.GetIpAddress(),
            IpLocation = string.Empty,
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
        };

        await _repository.InsertAsync(entity, true);

        await _ipLimitService.RecordAsync("create-reminder", dto.ProjectId.ToString(), TimeSpan.FromMinutes(5), 3);

        return entity.ToDto();
    }
}