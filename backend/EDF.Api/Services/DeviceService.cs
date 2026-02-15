using EDF.Api.DTOs;
using EDF.Api.Models;
using EDF.Api.Repositories;

namespace EDF.Api.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _repo;

    public DeviceService(IDeviceRepository repo)
    {
        _repo = repo;
    }

    public IEnumerable<DeviceDto> GetAll()
    {
        return _repo.GetAll().Select(MapToDto);
    }

    public DeviceDto? Get(Guid id)
    {
        var d = _repo.Get(id);
        return d is null ? null : MapToDto(d);
    }

    public DeviceDto Create(CreateDeviceRequest request)
    {
        var device = new Device
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            Location = request.Location,
            CreatedAt = DateTime.UtcNow
        };
        _repo.Add(device);
        return MapToDto(device);
    }

    public bool Update(Guid id, UpdateDeviceRequest request)
    {
        var existing = _repo.Get(id);
        if (existing == null) return false;
        existing.Name = request.Name;
        existing.Type = request.Type;
        existing.Location = request.Location;
        _repo.Update(existing);
        return true;
    }

    public bool Delete(Guid id)
    {
        var existing = _repo.Get(id);
        if (existing == null) return false;
        _repo.Delete(id);
        return true;
    }

    private static DeviceDto MapToDto(Device d) => new(d.Id, d.Name, d.Type, d.Location, d.CreatedAt);
}
