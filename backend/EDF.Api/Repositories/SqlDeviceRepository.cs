using EDF.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EDF.Api.Repositories;

public class SqlDeviceRepository : IDeviceRepository
{
    private readonly DeviceContext _context;

    public SqlDeviceRepository(DeviceContext context)
    {
        _context = context;
    }

    public IEnumerable<Device> GetAll()
    {
        return _context.Devices.AsNoTracking().ToList();
    }

    public Device? Get(Guid id)
    {
        return _context.Devices.Find(id);
    }

    public void Add(Device device)
    {
        _context.Devices.Add(device);
        _context.SaveChanges();
    }

    public void Update(Device device)
    {
        _context.Devices.Update(device);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var existing = _context.Devices.Find(id);
        if (existing != null)
        {
            _context.Devices.Remove(existing);
            _context.SaveChanges();
        }
    }
}