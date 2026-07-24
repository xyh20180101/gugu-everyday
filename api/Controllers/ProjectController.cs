using GuguEveryday.Models;
using GuguEveryday.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace GuguEveryday.Controllers;

[Authorize]
public class ProjectController : BaseController
{
    private readonly IRepository<Project> _repository;

    private readonly IRepository<ProjectProgress> _progressRepository;

    private readonly IRepository<User> _userRepository;

    private readonly IRepository<UserInfo> _userInfoRepository;

    public ProjectController(IRepository<Project> repository, IRepository<ProjectProgress> progressRepository, IRepository<User> userRepository, IRepository<UserInfo> userInfoRepository)
    {
        _repository = repository;
        _progressRepository = progressRepository;
        _userRepository = userRepository;
        _userInfoRepository = userInfoRepository;
    }

    #region project

    public class GetListRequest : PageRequest
    {
        public bool? IsArchived { get; set; }
    }

    [HttpGet("")]
    public async Task<PageResponse<ProjectDto>> GetList([FromQuery] GetListRequest request)
    {
        var queryable = (await _repository.WithDetailsAsync(p => p.Type)).Where(p => p.UserId == CurrentUserId);

        if (request.IsArchived is not null)
            queryable = queryable.Where(p => p.IsArchived == request.IsArchived);

        queryable = queryable.OrderBy(p => p.Order).ThenBy(p => p.CreationTime);

        var count = queryable.Count();
        var items = queryable.Skip(request.Skip).Take(request.Count).ToList();

        return new PageResponse<ProjectDto>
        {
            Items = items.ToDto(),
            TotalCount = count
        };
    }

    [HttpPost("")]
    public async Task<ProjectDto> Create([FromBody] CreateUpdateProjectDto dto)
    {
        var entity = dto.ToModel();

        entity.UserId = CurrentUserId!.Value;

        await _repository.InsertAsync(entity, true);

        return entity.ToDto();
    }

    [HttpPut("{id}")]
    public async Task<ProjectDto> Update(long id, [FromBody] CreateUpdateProjectDto dto)
    {
        var entity = await _repository.FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);
        if (entity is null)
            throw new EntityNotFoundException("项目不存在");

        entity.Update(dto);
        await _repository.UpdateAsync(entity, true);

        return entity.ToDto();
    }

    public class SetIsArchivedRequest
    {
        public bool IsArchived { get; set; }
    }

    [HttpPatch("{id}/set-is-archived")]
    public async Task<ProjectDto> SetIsArchived(long id, [FromBody] SetIsArchivedRequest request)
    {
        var entity = await _repository.FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);
        if (entity is null)
            throw new EntityNotFoundException("项目不存在");

        entity.IsArchived = request.IsArchived;

        return entity.ToDto();
    }

    [HttpDelete("{id}")]
    public async Task Delete(long id)
    {
        var entity = await _repository.FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);
        if (entity is null)
            throw new EntityNotFoundException("项目不存在");

        await _repository.DeleteAsync(entity);
    }

    #endregion

    #region progress

    [HttpGet("{id}/progresses")]
    public async Task<PageResponse<ProjectProgressDto>> GetProgressList(long id, [FromQuery] PageRequest request)
    {
        var project = (await _repository.WithDetailsAsync(p => p.Progresses))
            .FirstOrDefault(p => p.Id == id && p.UserId == CurrentUserId);
        if (project is null)
            throw new EntityNotFoundException("项目不存在");

        var queryable = project.Progresses;

        var count = queryable.Count();
        var items = queryable.Skip(request.Skip).Take(request.Count).ToList();

        return new PageResponse<ProjectProgressDto>
        {
            Items = items.ToDto(),
            TotalCount = count
        };
    }

    [HttpPost("{id}/progress")]
    public async Task<ProjectProgressDto> CreateProgress(long id, [FromBody] CreateUpdateProjectProgressDto dto)
    {
        var project = (await _repository.WithDetailsAsync(p => p.Progresses))
            .FirstOrDefault(p => p.Id == id && p.UserId == CurrentUserId);
        if (project is null)
            throw new EntityNotFoundException("项目不存在");

        var entity = dto.ToModel();
        entity.ProjectId = project.Id;

        project.Progresses.Add(entity);

        return entity.ToDto();
    }

    [HttpDelete("{id}/progress/{id2}")]
    public async Task DeleteProgress(long id, long id2)
    {
        var project = (await _repository.WithDetailsAsync(p => p.Progresses))
            .FirstOrDefault(p => p.Id == id && p.UserId == CurrentUserId);
        if (project is null)
            throw new EntityNotFoundException("项目不存在");

        var entity = project.Progresses.FirstOrDefault(p => p.Id == id2);
        if (entity is null)
            throw new EntityNotFoundException("项目进度不存在");

        await _progressRepository.DeleteAsync(entity, true);
    }

    #endregion

    #region public

    public class PublicGetListRequest : PageRequest
    {
        public string IdHash { get; set; }
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<PageResponse<ProjectDto>> PublicGetList([FromQuery] PublicGetListRequest request)
    {
        var user = await _userRepository.FirstOrDefaultAsync(p => p.IdHash == request.IdHash);
        if (user is null)
            throw new EntityNotFoundException("展示页不存在");

        var userInfo = await _userInfoRepository.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (userInfo is null || !userInfo.IsShowPageEnabled)
            throw new EntityNotFoundException("展示页不存在");

        var queryable = (await _repository.WithDetailsAsync(p => p.Progresses, p => p.Type)).Where(p => p.UserId == user.Id && p.IsPublic && !p.IsArchived);

        queryable = queryable.OrderBy(p => p.Order).ThenBy(p => p.StartTime);

        var count = queryable.Count();
        var items = queryable.Skip(request.Skip).Take(request.Count).ToList().ToDto();

        foreach (var item in items)
        {
            item.Progresses = [.. item.Progresses.OrderBy(p => p.CreationTime)];
            if (item.IsMask && !string.IsNullOrEmpty(userInfo.Mask))
                item.Name = string.Join(string.Empty, item.Name.Select(_ => userInfo.Mask));
        }

        return new PublicGetListResponse
        {
            Items = items,
            TotalCount = count,
            UserName = userInfo.UserName,
            Bulletin = userInfo.Bulletin,
            IsShowPageEnabled = userInfo.IsShowPageEnabled,
            ShowPageTitle = userInfo.ShowPageTitle,
            IsAllowReminder = userInfo.IsAllowReminder
        };
    }

    public class PublicGetListResponse : PageResponse<ProjectDto>
    {
        public string UserName { get; set; }

        public string Bulletin { get; set; }

        public bool IsShowPageEnabled { get; set; }

        public string ShowPageTitle { get; set; } = string.Empty;

        public bool IsAllowReminder { get; set; }
    }

    #endregion
}