using BLL_ConstruccionAPI.DTOs.Materiales;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/materiales")]
public class MaterialController : ControllerBase
{
    private readonly IMaterialesService _service;

    public MaterialController(IMaterialesService service)
    {
        _service = service;
    }

    // GET api/materiales
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var materiales = await _service.GetAllAsync();
        return Ok(materiales);
    }

    // GET api/materiales/bajo-stock
    [HttpGet("bajo-stock")]
    public async Task<IActionResult> GetBajoStock()
    {
        var materiales = await _service.GetBajoStockAsync();
        return Ok(materiales);
    }

    // GET api/materiales/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var material = await _service.GetByIdAsync(id);
        if (material is null) return NotFound(new { message = "Material no encontrado." });
        return Ok(material);
    }

    // GET api/materiales/almacen-central
    [HttpGet("almacen-central")]
    public async Task<IActionResult> GetAlmacenCentral()
    {
        var data = await _service.GetAlmacenCentralAsync();
        return Ok(data);
    }

    // GET api/materiales/{id}/stock
    [HttpGet("{id:int}/stock")]
    public async Task<IActionResult> GetStock(int id)
    {
        var stock = await _service.GetStockAsync(id);
        if (stock is null) return NotFound(new { message = "Registro de stock no encontrado para este material." });
        return Ok(stock);
    }

    // POST api/materiales
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MaterialRequestDto dto)
    {
        var (success, message, data) = await _service.CreateAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }

    // PUT api/materiales/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MaterialRequestDto dto)
    {
        var (success, message) = await _service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/materiales/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}
