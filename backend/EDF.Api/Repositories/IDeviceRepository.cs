using EDF.Api.Models;

namespace EDF.Api.Repositories;

public interface IDeviceRepository
{
    IEnumerable<Device> GetAll();
    Device? Get(Guid id);
    void Add(Device device);
    void Update(Device device);
    void Delete(Guid id);
}
