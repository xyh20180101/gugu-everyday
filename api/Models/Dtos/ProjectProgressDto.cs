namespace GuguEveryday.Models.Dtos;

public class ProjectProgressDto
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long ProjectId { get; set; }

    public string CurrentProgress { get; set; } = string.Empty;

    public string? NextReportProgress { get; set; }

    public DateTime CreationTime { get; set; }

    public double CurrentProgressValue { get; set; }
}
