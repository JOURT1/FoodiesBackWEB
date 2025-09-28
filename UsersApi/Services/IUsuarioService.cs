using UsersApi.Dtos.Request;
using UsersApi.Dtos.Response;

namespace UsersApi.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioResponseDto> GestionarUsuarioAsync(GestionUsuarioRequestDto request);
        Task<List<UsuarioResponseDto>> ConsultarUsuariosAsync(ConsultaUsuarioRequestDto request);
        Task<UsuarioResponseDto> AutenticarUsuarioAsync(AutenticacionRequestDto request);
    }
}
