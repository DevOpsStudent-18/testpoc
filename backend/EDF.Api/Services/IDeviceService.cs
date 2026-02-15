using EDF.Api.DTOs;

namespace EDF.Api.Services;

public interface IDeviceService
{
    IEnumerable<DeviceDto> GetAll();
    DeviceDto? Get(Guid id);
    DeviceDto Create(CreateDeviceRequest request);
    bool Update(Guid id, UpdateDeviceRequest request);
    bool Delete(Guid id);
}

public record CreateDeviceRequest(string Name, string Type, string Location);
public record UpdateDeviceRequest(string Name, string Type, string Location);
