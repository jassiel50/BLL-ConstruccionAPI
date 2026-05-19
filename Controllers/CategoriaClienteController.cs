using BLL_ConstruccionAPI.DTOs.Catalogos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/catalogos/categorias-cliente")]
public class CategoriaClienteController : ControllerBase
{
    private readonly ICatalogosService _catalogosService;

    public CategoriaClienteController(ICatalogosService catalogosService)
    {
        _catalogosService = catalogosService;
    }

    // GET api/catalogos/categorias-cliente
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categorias = await _catalogosService.GetAllCategoriasClienteAsync();
        return Ok(categorias);
    }

    // GET api/catalogos/categorias-cliente/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var categoria = await _catalogosService.GetCategoriaClienteByIdAsync(id);
        if (categoria is null) return NotFound(new { message = "Categoría de cliente no encontrada." });
        return Ok(categoria);
    }

    // POST api/catalogos/categorias-cliente
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoriaClienteRequestDto dto)
    {
        var (success, message, data) = await _catalogosService.CreateCategoriaClienteAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }

    // PUT api/catalogos/categorias-cliente/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoriaClienteRequestDto dto)
    {
        var (success, message) = await _catalogosService.UpdateCategoriaClienteAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/catalogos/categorias-cliente/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _catalogosService.DeleteCategoriaClienteAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}
