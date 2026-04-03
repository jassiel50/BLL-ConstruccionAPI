using BLL_ConstruccionAPI.DTOs.Catalogos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/catalogos/categorias-material")]
public class CategoriaMaterialController : ControllerBase
{
    private readonly ICatalogosService _catalogosService;

    public CategoriaMaterialController(ICatalogosService catalogosService)
    {
        _catalogosService = catalogosService;
    }

    // GET api/catalogos/categorias-material
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categorias = await _catalogosService.GetAllCategoriasAsync();
        return Ok(categorias);
    }

    // GET api/catalogos/categorias-material/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var categoria = await _catalogosService.GetCategoriaByIdAsync(id);
        if (categoria is null) return NotFound(new { message = "Categoría no encontrada." });
        return Ok(categoria);
    }

    // POST api/catalogos/categorias-material
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoriaMaterialRequestDto dto)
    {
        var (success, message, data) = await _catalogosService.CreateCategoriaAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }

    // PUT api/catalogos/categorias-material/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoriaMaterialRequestDto dto)
    {
        var (success, message) = await _catalogosService.UpdateCategoriaAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/catalogos/categorias-material/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _catalogosService.DeleteCategoriaAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}
