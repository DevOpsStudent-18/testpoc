namespace EDF.Api.DTOs;

public record DeviceDto(Guid Id, string Name, string Type, string Location, DateTime CreatedAt);
