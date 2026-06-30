using System.ComponentModel.DataAnnotations;
using GuguEveryday.Attributes;

namespace GuguEveryday.Models;

public class User : BaseModel
{
    [StringLength(100)]
    [SensitiveLexicon]
    public string Email { get; set; } = string.Empty;

    [StringLength(64)]
    public string Salt { get; set; } = string.Empty;

    [StringLength(64)]
    public string PasswordHash { get; set; } = string.Empty;
    
    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;
    
    [StringLength(64)]
    public string IdSalt { get; set; } = string.Empty;

    [StringLength(64)]
    public string IdHash { get; set; } = string.Empty;
}