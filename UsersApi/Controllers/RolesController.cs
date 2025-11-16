using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersApi.Services.Interfaces;

namespace UsersApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController(IRolService rolService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await rolService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rol = await rolService.GetByIdAsync(id);
            return Ok(rol);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateRolRequest request)
        {
            var rol = await rolService.CreateAsync(request.Nombre, request.Descripcion);
            return CreatedAtAction(nameof(GetById), new { id = rol.Id }, rol);
        }
    }

    public class CreateRolRequest
    {
        public required string Nombre { get; set; }
        public string? Descripcion { get; set; }
    }
}
