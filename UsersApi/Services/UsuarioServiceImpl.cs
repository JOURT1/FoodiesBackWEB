using CommonApi.Helpers;
using Isopoh.Cryptography.Argon2;
using UsersApi.Data.Repositories;
using UsersApi.Dtos.Request;
using UsersApi.Dtos.Response;
using UsersApi.Models;

namespace UsersApi.Services
{
    public class UsuarioServiceImpl(UsuarioRepository repository, UsuarioRolRepository usuarioRolRepository) : IUsuarioService
    {
        private readonly IUsuarioRepository repository = repository;
        private readonly UsuarioRolRepository usuarioRolRepository = usuarioRolRepository;

        private UsuarioResponseDto MapToResponseDto(Usuario usuario)
        {
            return new UsuarioResponseDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                CodigoUsuario = usuario.CodigoUsuario,
                EstaActivo = usuario.EstaActivo,
                FechaUltimoAcceso = usuario.FechaUltimoAcceso,
                FechaBloqueo = usuario.FechaBloqueo,
                FechaCreacion = usuario.FechaCreacion,
                IntentosFallidos = usuario.IntentosFallidos,
                Roles = usuario.UsuarioRoles?.Select(ur => ur.Rol?.Codigo ?? "").Where(r => !string.IsNullOrEmpty(r)).ToList() ?? new List<string>()
            };
        }

        public async Task<UsuarioResponseDto> GestionarUsuarioAsync(GestionUsuarioRequestDto request)
        {
            // TODO: Implementar transacción - begin transaction antes del switch y commit al final
            switch (request.Operacion?.ToLower())
            {
                case "crear":
                    var usuario = new Usuario
                    {
                        Nombre = request.Nombre,
                        CodigoUsuario = !string.IsNullOrEmpty(request.CodigoUsuario) ? request.CodigoUsuario : request.Nombre,
                        Contrasenia = HashPassword(request.Contrasenia),
                        IntentosFallidos = 0,
                        EstaActivo = true,
                        Roles = "cliente" // Valor por defecto
                    };
                    await repository.CreateAsync(usuario);

                    // Asignar roles por códigos si se especificaron
                    if (request.RolesCodigos != null && request.RolesCodigos.Any())
                    {
                        await usuarioRolRepository.AsignarRolesPorCodigosAsync(usuario.IdUsuario, request.RolesCodigos);
                    }

                    // Recargar el usuario con roles para devolverlo completo
                    var usuarioCompleto = await repository.GetByIdAsync(usuario.IdUsuario);
                    return MapToResponseDto(usuarioCompleto!);

                case "actualizar":
                    var usuarioExistente = await repository.GetByIdAsync(request.IdUsuario ?? 0);
                    if (usuarioExistente is null)
                        throw new ExcepcionPersonalizada("Error", "Usuario no encontrado");

                    usuarioExistente.Nombre = request.Nombre ?? usuarioExistente.Nombre;
                    if (!string.IsNullOrEmpty(request.Contrasenia))
                    {
                        usuarioExistente.Contrasenia = HashPassword(request.Contrasenia);
                    }

                    await repository.UpdateAsync(usuarioExistente);

                    // Actualizar roles por códigos si se especificaron
                    if (request.RolesCodigos != null && request.RolesCodigos.Any())
                    {
                        await usuarioRolRepository.AsignarRolesPorCodigosAsync(usuarioExistente.IdUsuario, request.RolesCodigos);
                    }

                    // Recargar el usuario con roles para devolverlo completo
                    var usuarioCompletoActualizado = await repository.GetByIdAsync(usuarioExistente.IdUsuario);
                    return MapToResponseDto(usuarioCompletoActualizado!);

                case "activar":
                    var usuarioActivar = await repository.GetByIdAsync(request.IdUsuario ?? 0);
                    if (usuarioActivar is null)
                        throw new ExcepcionPersonalizada("Error", "Usuario no encontrado");

                    usuarioActivar.EstaActivo = true;
                    usuarioActivar.FechaBloqueo = null;

                    await repository.UpdateAsync(usuarioActivar);
                    return MapToResponseDto(usuarioActivar);

                case "desactivar":
                    var usuarioDesactivar = await repository.GetByIdAsync(request.IdUsuario ?? 0);
                    if (usuarioDesactivar is null)
                        throw new ExcepcionPersonalizada("Error", "Usuario no encontrado");

                    usuarioDesactivar.EstaActivo = false;

                    await repository.UpdateAsync(usuarioDesactivar);
                    return MapToResponseDto(usuarioDesactivar);

                case "bloquear":
                    var usuarioBloquear = await repository.GetByIdAsync(request.IdUsuario ?? 0);
                    if (usuarioBloquear is null)
                        throw new ExcepcionPersonalizada("Error", "Usuario no encontrado");

                    usuarioBloquear.EstaActivo = false;
                    usuarioBloquear.FechaBloqueo = DateTime.UtcNow;

                    await repository.UpdateAsync(usuarioBloquear);
                    return MapToResponseDto(usuarioBloquear);

                case "desbloquear":
                    var usuarioDesbloquear = await repository.GetByIdAsync(request.IdUsuario ?? 0);
                    if (usuarioDesbloquear is null)
                        throw new ExcepcionPersonalizada("Error", "Usuario no encontrado");

                    usuarioDesbloquear.EstaActivo = true;
                    usuarioDesbloquear.FechaBloqueo = null;
                    usuarioDesbloquear.IntentosFallidos = 0;

                    await repository.UpdateAsync(usuarioDesbloquear);
                    return MapToResponseDto(usuarioDesbloquear);

                case "desbloquearcuenta":
                    var usuarioDesbloquearCuenta = await repository.GetByIdAsync(request.IdUsuario ?? 0);
                    if (usuarioDesbloquearCuenta is null)
                        throw new ExcepcionPersonalizada("Error", "Usuario no encontrado");

                    usuarioDesbloquearCuenta.EstaActivo = true;
                    usuarioDesbloquearCuenta.FechaBloqueo = null;
                    usuarioDesbloquearCuenta.IntentosFallidos = 0;

                    await repository.UpdateAsync(usuarioDesbloquearCuenta);
                    return MapToResponseDto(usuarioDesbloquearCuenta);

                case "cambiar_contrasenia":
                case "cambio_contrasenia":
                    var usuarioCambioPass = await repository.GetByIdAsync(request.IdUsuario ?? 0);
                    if (usuarioCambioPass is null)
                        throw new ExcepcionPersonalizada("Error", "Usuario no encontrado");

                    if (string.IsNullOrEmpty(request.ContraseniaActual))
                        throw new ExcepcionPersonalizada("Error", "Se requiere la contraseña actual para cambiar la contraseña");

                    if (!VerifyPassword(request.ContraseniaActual, usuarioCambioPass.Contrasenia))
                        throw new ExcepcionPersonalizada("Error", "La contraseña actual es incorrecta");

                    if (string.IsNullOrEmpty(request.Contrasenia))
                        throw new ExcepcionPersonalizada("Error", "Se requiere una nueva contraseña");

                    usuarioCambioPass.Contrasenia = HashPassword(request.Contrasenia);

                    await repository.UpdateAsync(usuarioCambioPass);
                    return MapToResponseDto(usuarioCambioPass);

                default:
                    throw new ExcepcionPersonalizada("Error", "Operación no válida");
            }
        }

        public async Task<List<UsuarioResponseDto>> ConsultarUsuariosAsync(ConsultaUsuarioRequestDto request)
        {
            var usuarios = await repository.GetAllAsync();

            // Aplicar filtros si es necesario
            if (!string.IsNullOrEmpty(request.Nombre))
            {
                usuarios = usuarios.Where(u => u.Nombre.Contains(request.Nombre, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Mapeo usando el método helper
            var usuariosDto = usuarios.Select(MapToResponseDto).ToList();

            return usuariosDto;
        }

        public async Task<UsuarioResponseDto> AutenticarUsuarioAsync(AutenticacionRequestDto request)
        {
            var usuario = await repository.GetByNombreAsync(request.Nombre);

            if (usuario == null)
                throw new ExcepcionPersonalizada("Error", "Usuario no encontrado");

            if (!VerifyPassword(request.Contrasenia, usuario.Contrasenia))
                throw new ExcepcionPersonalizada("Error", "Contraseña incorrecta");

            if (!usuario.EstaActivo)
                throw new ExcepcionPersonalizada("Error", "Usuario inactivo");

            return MapToResponseDto(usuario);
        }

        private string HashPassword(string password)
        {
            return Argon2.Hash(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return Argon2.Verify(hash, password);
        }
    }
}