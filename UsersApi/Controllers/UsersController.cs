using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersApi.Dtos.Request;
using UsersApi.Dtos.Response;
using UsersApi.Services;

namespace UsersApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsersController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("gestionar")]
        [Authorize]
        public async Task<ActionResult<UsuarioResponseDto>> GestionarUsuario([FromBody] GestionUsuarioRequestDto request)
        {
            var resultado = await _usuarioService.GestionarUsuarioAsync(request);
            return Ok(resultado);
        }

        [HttpGet("consultar/{id?}")]
        [Authorize]
        public async Task<ActionResult<List<UsuarioResponseDto>>> ConsultarUsuarios(int? id = null)
        {
            var usuarios = await _usuarioService.ConsultarUsuariosAsync(new ConsultaUsuarioRequestDto { Id = id, Nombre = null });
            return Ok(usuarios);
        }

        [HttpPost("consultar")]
        [Authorize]
        public async Task<ActionResult<List<UsuarioResponseDto>>> ConsultarUsuariosPost([FromBody] ConsultaUsuarioRequestDto request)
        {
            var usuarios = await _usuarioService.ConsultarUsuariosAsync(request);
            return Ok(usuarios);
        }

        [HttpPost("autenticar")]
        public async Task<ActionResult<UsuarioResponseDto>> AutenticarUsuario([FromBody] AutenticacionRequestDto request)
        {
            var usuario = await _usuarioService.AutenticarUsuarioAsync(request);
            return Ok(usuario);
        }
    }
}
