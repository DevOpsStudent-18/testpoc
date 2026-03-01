using EDF.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EDF.Api.Repositories;

public class DeviceContext : DbContext
{
    public DeviceContext(DbContextOptions<DeviceContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices { get; set; } = null!;
}