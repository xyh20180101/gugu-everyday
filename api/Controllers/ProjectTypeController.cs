using System.Text.Json;
using GuguEveryday.Data;
using GuguEveryday.Enums;
using GuguEveryday.Models;
using GuguEveryday.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;

namespace GuguEveryday.Controllers;

[Authorize]
public class ProjectTypeController : BaseController
{
    private readonly IRepository<ProjectType> _repository;

    public ProjectTypeController(IRepository<ProjectType> repository)
    {
        _repository = repository;
    }

    [HttpGet("")]
    public async Task<PageResponse<ProjectTypeDto>> GetList([FromQuery] PageRequest request)
    {
        var queryable = (await _repository.GetQueryableAsync()).Where(p => p.UserId == CurrentUserId);

        var count = await queryable.CountAsync();
        var items = await queryable.Skip(request.Skip).Take(request.Count).ToListAsync();

        return new PageResponse<ProjectTypeDto>
        {
            Items = items.ToDto(),
            TotalCount = count
        };
    }

    [HttpPost("")]
    public async Task<ProjectTypeDto> Create([FromBody] CreateUpdateProjectTypeDto dto)
    {
        CheckExtraData(dto);
        var entity = dto.ToModel();

        entity.UserId = CurrentUserId!.Value;

        await _repository.InsertAsync(entity, true);

        return entity.ToDto();
    }

    [HttpPut("{id}")]
    public async Task<ProjectTypeDto> Update(long id, [FromBody] CreateUpdateProjectTypeDto dto)
    {
        CheckExtraData(dto);
        var entity = await _repository.FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);
        if (entity is null)
            throw new EntityNotFoundException("项目不存在");

        entity.Update(dto);
        await _repository.UpdateAsync(entity, true);

        return entity.ToDto();
    }

    [HttpDelete("{id}")]
    public async Task Delete(long id)
    {
        var entity = await _repository.FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);
        if (entity is null)
            throw new EntityNotFoundException("项目不存在");

        await _repository.DeleteAsync(entity, true);
    }

    #region 内部方法

    void CheckExtraData(CreateUpdateProjectTypeDto dto)
    {
        try
        {
            switch (dto.ProgressType)
            {
                case ProgressType.Progress:
                    JsonSerializer.Deserialize<ProgressExtraData>(dto.ExtraData, JsonSerializerOptions.Web);
                    break;
                case ProgressType.Step:
                    JsonSerializer.Deserialize<StepExtraData>(dto.ExtraData, JsonSerializerOptions.Web);
                    break;
                default:
                    throw new StatusCodeException("进度类型不正确");
            }
        }
        catch (JsonException e)
        {
            throw new StatusCodeException("extraData格式错误");
        }
    }

    public class ProgressExtraData
    {
        public string? ProgressUnit { get; set; } = string.Empty;

        public string? TotalProgress { get; set; } = string.Empty;
    }

    public class StepExtraData
    {
        public List<string>? Steps { get; set; } = [];
    }

    #endregion
}