using EDF.Api.Models;

namespace EDF.Api.Repositories;

public class InMemoryDeviceRepository : IDeviceRepository
{
    private readonly List<Device> _devices = new();

    public InMemoryDeviceRepository()
    {
        // Seed with simple mock data
        _devices.AddRange(new[] {
            new Device { Id = Guid.NewGuid(), Name = "Transformer A1", Type = "Transformer", Location = "Substation 12", CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new Device { Id = Guid.NewGuid(), Name = "Meter X4", Type = "SmartMeter", Location = "Building 3", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new Device { Id = Guid.NewGuid(), Name = "Sensor T9", Type = "TemperatureSensor", Location = "Line 7", CreatedAt = DateTime.UtcNow.AddDays(-1) }
        });
    }

    public IEnumerable<Device> GetAll() => _devices;

    public Device? Get(Guid id) => _devices.FirstOrDefault(d => d.Id == id);

    public void Add(Device device)
    {
        _devices.Add(device);
    }

    public void Update(Device device)
    {
        var idx = _devices.FindIndex(d => d.Id == device.Id);
        if (idx >= 0) _devices[idx] = device;
    }

    public void Delete(Guid id)
    {
        var existing = _devices.FirstOrDefault(d => d.Id == id);
        if (existing != null) _devices.Remove(existing);
    }
}
