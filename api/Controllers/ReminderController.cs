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

    public class GetListRequest : PageRequest
    {
        public long? ProjectId { get; set; }
    }

    [HttpGet("")]
    public async Task<PageResponse<ReminderDto>> GetList([FromQuery] GetListRequest request)
    {
        var queryable = (await _repository.GetQueryableAsync()).Include(p => p.Project).Where(p => p.UserId == CurrentUserId);

        if (request.ProjectId is not null)
            queryable = queryable.Where(p => p.ProjectId == request.ProjectId);

        queryable = queryable.OrderByDescending(p => p.CreationTime);

        var count = queryable.Count();
        var items = queryable.Skip(request.Skip).Take(request.Count).ToList();

        return new PageResponse<ReminderDto>
        {
            Items = items.ToDto(),
            TotalCount = count
        };
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

        await _ipLimitService.CheckAsync(dto.ProjectId.ToString());

        var entity = new Reminder
        {
            UserId = project.UserId,
            ProjectId = project.Id,
            Ip = GetIpAddress(),
            IpLocation = string.Empty,
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
        };

        await _repository.InsertAsync(entity, true);

        return entity.ToDto();
    }

    string GetIpAddress()
    {
        if (HttpContext.Request.Headers.TryGetValue("X-Real-IP", out var realIp) && !string.IsNullOrEmpty(realIp))
            return realIp.ToString().Split(',')[0].Trim();
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}