using BLL_ConstruccionAPI.DTOs.Catalogos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/catalogos/categorias-proveedor")]
public class CategoriaProveedorController : ControllerBase
{
    private readonly ICatalogosService _catalogosService;

    public CategoriaProveedorController(ICatalogosService catalogosService)
    {
        _catalogosService = catalogosService;
    }

    // GET api/catalogos/categorias-proveedor
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categorias = await _catalogosService.GetAllCategoriasProveedorAsync();
        return Ok(categorias);
    }

    // GET api/catalogos/categorias-proveedor/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var categoria = await _catalogosService.GetCategoriaProveedorByIdAsync(id);
        if (categoria is null) return NotFound(new { message = "Categoría de proveedor no encontrada." });
        return Ok(categoria);
    }

    // POST api/catalogos/categorias-proveedor
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoriaProveedorRequestDto dto)
    {
        var (success, message, data) = await _catalogosService.CreateCategoriaProveedorAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }

    // PUT api/catalogos/categorias-proveedor/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoriaProveedorRequestDto dto)
    {
        var (success, message) = await _catalogosService.UpdateCategoriaProveedorAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/catalogos/categorias-proveedor/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _catalogosService.DeleteCategoriaProveedorAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}
