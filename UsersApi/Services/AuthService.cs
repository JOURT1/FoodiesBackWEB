using UsersApi.Dtos.Request;
using UsersApi.Services.Interfaces;
using UsersApi.Data.Repositories.Interfaces;
using UsersApi.Helpers;

namespace UsersApi.Services
{
    public class AuthService(IUsuarioRepository usuarioRepository, IJwtService jwtService) : IAuthService
    {
        public async Task<object> LoginAsync(LoginRequest request)
        {
            var usuario = await usuarioRepository.GetByCorreoAsync(request.Email);
            if (usuario == null || !usuario.Activo)
            {
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            if (!PasswordHelper.VerifyPassword(request.Password, usuario.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            var token = jwtService.GenerateToken(usuario);
            
            return new
            {
                access_token = token,
                token_type = "Bearer",
                expires_in = 3600
            };
        }

        public async Task<object> GetUserInfoAsync(string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                throw new UnauthorizedAccessException("Token requerido");
            }

            var token = authorizationHeader.Substring("Bearer ".Length);
            var claimsPrincipal = jwtService.ValidateToken(token);
            
            if (claimsPrincipal == null)
            {
                throw new UnauthorizedAccessException("Token inválido");
            }

            var userId = claimsPrincipal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            {
                throw new UnauthorizedAccessException("Token inválido");
            }

            var usuario = await usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
            {
                throw new UnauthorizedAccessException("Usuario no encontrado");
            }

            return new
            {
                id = usuario.Id,
                name = $"{usuario.Nombre} {usuario.Apellido}",
                email = usuario.Correo,
                roles = usuario.Roles.Select(r => r.Nombre).ToArray()
            };
        }
    }
}