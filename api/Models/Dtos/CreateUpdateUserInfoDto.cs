using GuguEveryday.Enums;

namespace GuguEveryday.Models.Dtos;

public class CreateUpdateUserInfoDto
{
    public string UserName { get; set; }

    public string Bulletin { get; set; }

    public bool IsShowPageEnabled { get; set; }

    public string ShowPageTitle { get; set; }

    public bool IsAllowReminder { get; set; }

    public string Mask { get; set; }
}
