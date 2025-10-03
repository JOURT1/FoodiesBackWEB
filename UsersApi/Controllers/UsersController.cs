using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersApi.Dtos.Request;
using UsersApi.Services.Interfaces;
using System.Security.Claims;

namespace UsersApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController(IUsuarioService usuarioService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await usuarioService.GetAllAsync();
            return Ok(usuarios);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var usuario = await usuarioService.GetByIdAsync(id);
            return Ok(usuario);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] UsuarioCreateRequestDto requestDto)
        {
            var nuevoUsuario = await usuarioService.CreateAsync(requestDto);
            return CreatedAtAction(nameof(GetById), new { id = nuevoUsuario.Id }, nuevoUsuario);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdateRequestDto requestDto)
        {
            var usuarioActualizado = await usuarioService.UpdateAsync(id, requestDto);
            return Ok(usuarioActualizado);
        }

        [HttpPost("{id:int}/roles")]
        [AllowAnonymous] // Permitir acceso sin autenticación para comunicación entre servicios
        public async Task<IActionResult> AddRoleToUser(int id, [FromBody] AddRoleRequest request)
        {
            var resultado = await usuarioService.AddRoleToUserAsync(id, request.RoleName);
            return Ok(new { success = resultado, message = resultado ? "Rol agregado exitosamente" : "No se pudo agregar el rol" });
        }
    }

    public class AddRoleRequest
    {
        public required string RoleName { get; set; }
    }
}
