namespace GuguEveryday.Models.Dtos;

public class ReminderDto
{
    public long UserId { get; set; }

    public long ProjectId { get; set; }

    public DateTime CreationTime { get; set; }
}