using GuguEveryday.Models;
using GuguEveryday.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace GuguEveryday.Controllers;

[Authorize]
public class UserInfoController : BaseController
{
    private readonly IRepository<UserInfo> _userInfoRepository;

    public UserInfoController(IRepository<UserInfo> userInfoRepository)
    {
        _userInfoRepository = userInfoRepository;
    }

    [HttpPut("{id}")]
    public async Task<UserInfoDto> Update(long id, [FromBody] CreateUpdateUserInfoDto dto)
    {
        var entity = await _userInfoRepository.FirstOrDefaultAsync(p => p.Id == id && p.UserId == CurrentUserId);
        if (entity is null)
            throw new EntityNotFoundException("用户信息不存在");

        if(!string.IsNullOrEmpty(dto.Mask) && dto.Mask.Length > 2)
            throw new StatusCodeException("项目名称掩码长度不能大于2");

        entity.Update(dto);

        return entity.ToDto();
    }

    public class SetUserNameRequest
    {
        public string UserName { get; set; }
    }

    [HttpPatch("{id}/set-user-name")]
    public async Task<UserInfoDto> SetUserName(long id, [FromBody] SetUserNameRequest request)
    {
        var entity = await _userInfoRepository.FirstOrDefaultAsync(u => u.Id == id && u.UserId == CurrentUserId);

        if (entity is null)
            throw new EntityNotFoundException("用户信息不存在");

        entity.UserName = request.UserName;

        return entity.ToDto();
    }

    public class SetBulletinRequest
    {
        public string Bulletin { get; set; }
    }

    [HttpPatch("{id}/set-bulletin")]
    public async Task<UserInfoDto> UpdateBulletin(long id, [FromBody] SetBulletinRequest request)
    {
        var entity = await _userInfoRepository.FirstOrDefaultAsync(u => u.Id == id && u.UserId == CurrentUserId);

        if (entity is null)
            throw new EntityNotFoundException("用户信息不存在");

        entity.Bulletin = request.Bulletin;

        return entity.ToDto();
    }
}