using BLL_ConstruccionAPI.DTOs.Catalogos;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLL_ConstruccionAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/catalogos/categorias-herramienta")]
public class CategoriaHerramientaController : ControllerBase
{
    private readonly ICatalogosService _catalogosService;

    public CategoriaHerramientaController(ICatalogosService catalogosService)
    {
        _catalogosService = catalogosService;
    }

    // GET api/catalogos/categorias-herramienta
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categorias = await _catalogosService.GetAllCategoriasHerramientaAsync();
        return Ok(categorias);
    }

    // GET api/catalogos/categorias-herramienta/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var categoria = await _catalogosService.GetCategoriaHerramientaByIdAsync(id);
        if (categoria is null) return NotFound(new { message = "Categoría de herramienta no encontrada." });
        return Ok(categoria);
    }

    // POST api/catalogos/categorias-herramienta
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoriaHerramientaRequestDto dto)
    {
        var (success, message, data) = await _catalogosService.CreateCategoriaHerramientaAsync(dto);
        if (!success) return BadRequest(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, new { message, data });
    }

    // PUT api/catalogos/categorias-herramienta/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoriaHerramientaRequestDto dto)
    {
        var (success, message) = await _catalogosService.UpdateCategoriaHerramientaAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    // DELETE api/catalogos/categorias-herramienta/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _catalogosService.DeleteCategoriaHerramientaAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}
