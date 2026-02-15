using EDF.Api.DTOs;
using EDF.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EDF.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _service;

    public DevicesController(IDeviceService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<IEnumerable<DeviceDto>> GetAll()
    {
        return Ok(_service.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<DeviceDto> Get(Guid id)
    {
        var d = _service.Get(id);
        return d is null ? NotFound() : Ok(d);
    }

    [HttpPost]
    public ActionResult<DeviceDto> Create([FromBody] CreateDeviceRequest request)
    {
        var created = _service.Create(request);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(Guid id, [FromBody] UpdateDeviceRequest req)
    {
        var ok = _service.Update(id, req);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var ok = _service.Delete(id);
        return ok ? NoContent() : NotFound();
    }
}
