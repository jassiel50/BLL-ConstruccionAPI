using BLL_ConstruccionAPI.DTOs.ProveedoresClientes;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/clientes")]
public class ClienteController : ControllerBase
{
    private readonly IProveedoresClientesService _service;

    public ClienteController(IProveedoresClientesService service)
    {
        _service = service;
    }

    // GET api/clientes
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clientes = await _service.GetAllClientesAsync();
        return Ok(clientes);
    }

    // GET api/clientes/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cliente = await _service.GetClienteByIdAsync(id);
        if (cliente is null) return NotFound(new { message = "Cliente no encontrado." });
        return Ok(cliente);
    }

    // POST api/clientes
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClienteRequestDto dto)
    {
        var (success, message, data) = await _service.CreateClienteAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }

    // PUT api/clientes/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ClienteRequestDto dto)
    {
        var (success, message) = await _service.UpdateClienteAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/clientes/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteClienteAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    // POST api/clientes/{id}/contactos
    [HttpPost("{id:int}/contactos")]
    public async Task<IActionResult> AddContacto(int id, [FromBody] ContactoRequestDto dto)
    {
        var (success, message, data) = await _service.AddContactoClienteAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // PUT api/clientes/{clienteId}/contactos/{contactoId}
    [HttpPut("{clienteId:int}/contactos/{contactoId:int}")]
    public async Task<IActionResult> UpdateContacto(int clienteId, int contactoId, [FromBody] ContactoRequestDto dto)
    {
        var (success, message) = await _service.UpdateContactoClienteAsync(clienteId, contactoId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/clientes/{clienteId}/contactos/{contactoId}
    [HttpDelete("{clienteId:int}/contactos/{contactoId:int}")]
    public async Task<IActionResult> DeleteContacto(int clienteId, int contactoId)
    {
        var (success, message) = await _service.DeleteContactoClienteAsync(clienteId, contactoId);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}
