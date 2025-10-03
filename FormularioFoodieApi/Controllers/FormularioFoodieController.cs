using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FormularioFoodieApi.Dtos.Request;
using FormularioFoodieApi.Services.Interfaces;
using System.Security.Claims;

namespace FormularioFoodieApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FormularioFoodieController(IFormularioFoodieService formularioService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var formularios = await formularioService.GetAllAsync();
            return Ok(formularios);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var formulario = await formularioService.GetByIdAsync(id);
            return Ok(formulario);
        }

        [HttpGet("usuario/{usuarioId:int}")]
        public async Task<IActionResult> GetByUsuarioId(int usuarioId)
        {
            var formulario = await formularioService.GetByUsuarioIdAsync(usuarioId);
            return Ok(formulario);
        }

        [HttpGet("mi-formulario")]
        public async Task<IActionResult> GetMyFormulario()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                return BadRequest("No se pudo identificar al usuario");
            }

            var formulario = await formularioService.GetByUsuarioIdAsync(usuarioId);
            return Ok(formulario);
        }

        [HttpGet("estado/{estado}")]
        public async Task<IActionResult> GetByEstado(string estado)
        {
            var formularios = await formularioService.GetByEstadoAsync(estado);
            return Ok(formularios);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FormularioFoodieCreateRequestDto requestDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                return BadRequest("No se pudo identificar al usuario");
            }

            var nuevoFormulario = await formularioService.CreateAsync(usuarioId, requestDto);
            return CreatedAtAction(nameof(GetById), new { id = nuevoFormulario.Id }, nuevoFormulario);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] FormularioFoodieUpdateRequestDto requestDto)
        {
            var formularioActualizado = await formularioService.UpdateAsync(id, requestDto);
            return Ok(formularioActualizado);
        }

        [HttpPut("mi-formulario")]
        public async Task<IActionResult> UpdateMyFormulario([FromBody] FormularioFoodieUpdateRequestDto requestDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                return BadRequest("No se pudo identificar al usuario");
            }

            var formularioActualizado = await formularioService.UpdateMyFormularioAsync(usuarioId, requestDto);
            return Ok(formularioActualizado);
        }
    }
}