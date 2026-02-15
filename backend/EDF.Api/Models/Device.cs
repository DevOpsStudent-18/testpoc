namespace EDF.Api.Models;

public class Device
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Location { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
