using BLL_ConstruccionAPI.DTOs.ProveedoresClientes;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[ApiController]
[Route("api/proveedores")]
public class ProveedorController : ControllerBase
{
    private readonly IProveedoresClientesService _service;

    public ProveedorController(IProveedoresClientesService service)
    {
        _service = service;
    }

    // GET api/proveedores
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var proveedores = await _service.GetAllProveedoresAsync();
        return Ok(proveedores);
    }

    // GET api/proveedores/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var proveedor = await _service.GetProveedorByIdAsync(id);
        if (proveedor is null) return NotFound(new { message = "Proveedor no encontrado." });
        return Ok(proveedor);
    }

    // POST api/proveedores
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProveedorRequestDto dto)
    {
        var (success, message, data) = await _service.CreateProveedorAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }

    // PUT api/proveedores/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProveedorRequestDto dto)
    {
        var (success, message) = await _service.UpdateProveedorAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/proveedores/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteProveedorAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}
