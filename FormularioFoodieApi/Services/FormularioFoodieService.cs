using FormularioFoodieApi.Data.Repositories.Interfaces;
using FormularioFoodieApi.Dtos.Request;
using FormularioFoodieApi.Dtos.Response;
using FormularioFoodieApi.Models;
using FormularioFoodieApi.Services.Interfaces;
using System.Security.Claims;

namespace FormularioFoodieApi.Services
{
    public class FormularioFoodieService : IFormularioFoodieService
    {
        private readonly IFormularioFoodieRepository _formularioRepository;
        private readonly IUsersApiService _usersApiService;
        private readonly ILogger<FormularioFoodieService> _logger;

        public FormularioFoodieService(
            IFormularioFoodieRepository formularioRepository,
            IUsersApiService usersApiService,
            ILogger<FormularioFoodieService> logger)
        {
            _formularioRepository = formularioRepository;
            _usersApiService = usersApiService;
            _logger = logger;
        }

        public async Task<FormularioFoodieResponseDto> CreateAsync(ClaimsPrincipal user, FormularioFoodieCreateRequestDto requestDto)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                throw new UnauthorizedAccessException("No se pudo identificar al usuario");
            }

            return await CreateAsync(usuarioId, requestDto);
        }

        private async Task<FormularioFoodieResponseDto> CreateAsync(int usuarioId, FormularioFoodieCreateRequestDto requestDto)
        {
            // Validar que el usuario no tenga ya un formulario
            if (await _formularioRepository.ExistsForUserAsync(usuarioId))
            {
                throw new InvalidOperationException("El usuario ya tiene un formulario de Foodie registrado");
            }

            // Validar unicidad de datos
            await ValidateUniqueDataAsync(requestDto);

            // Crear la entidad
            var formulario = new FormularioFoodie
            {
                UsuarioId = usuarioId,
                NombreCompleto = requestDto.NombreCompleto.Trim(),
                Email = requestDto.Email.Trim().ToLower(),
                NumeroPersonal = requestDto.NumeroPersonal.Trim(),
                FechaNacimiento = requestDto.FechaNacimiento,
                Genero = requestDto.Genero.Trim(),
                Pais = requestDto.Pais.Trim(),
                Ciudad = requestDto.Ciudad.Trim(),
                FrecuenciaContenido = requestDto.FrecuenciaContenido.Trim(),
                UsuarioInstagram = requestDto.UsuarioInstagram.Trim(),
                SeguidoresInstagram = requestDto.SeguidoresInstagram,
                CuentaPublica = requestDto.CuentaPublica,
                UsuarioTikTok = requestDto.UsuarioTikTok.Trim(),
                SeguidoresTikTok = requestDto.SeguidoresTikTok,
                SobreTi = requestDto.SobreTi.Trim(),
                AceptaBeneficios = requestDto.AceptaBeneficios.Trim(),
                AceptaTerminos = requestDto.AceptaTerminos,
                FechaAplicacion = DateTime.UtcNow
            };

            // Guardar en base de datos
            var formularioCreado = await _formularioRepository.CreateAsync(formulario);

            // Verificar si cumple los requisitos para ser Foodie y agregar rol
            await ProcessFoodieRoleAsync(usuarioId, formularioCreado);

            return MapToResponseDto(formularioCreado);
        }

        public async Task<FormularioFoodieResponseDto> UpdateAsync(int id, FormularioFoodieUpdateRequestDto requestDto)
        {
            var formulario = await _formularioRepository.GetByIdAsync(id);
            if (formulario == null)
            {
                throw new KeyNotFoundException("Formulario no encontrado");
            }

            // Actualizar solo los campos que no son null
            if (requestDto.NombreCompleto != null)
                formulario.NombreCompleto = requestDto.NombreCompleto.Trim();

            if (requestDto.Email != null)
                formulario.Email = requestDto.Email.Trim().ToLower();

            if (requestDto.NumeroPersonal != null)
                formulario.NumeroPersonal = requestDto.NumeroPersonal.Trim();

            if (requestDto.FechaNacimiento.HasValue)
                formulario.FechaNacimiento = requestDto.FechaNacimiento.Value;

            if (requestDto.Genero != null)
                formulario.Genero = requestDto.Genero.Trim();

            if (requestDto.Pais != null)
                formulario.Pais = requestDto.Pais.Trim();

            if (requestDto.Ciudad != null)
                formulario.Ciudad = requestDto.Ciudad.Trim();

            if (requestDto.FrecuenciaContenido != null)
                formulario.FrecuenciaContenido = requestDto.FrecuenciaContenido.Trim();

            if (requestDto.UsuarioInstagram != null)
                formulario.UsuarioInstagram = requestDto.UsuarioInstagram.Trim();

            if (requestDto.SeguidoresInstagram.HasValue)
                formulario.SeguidoresInstagram = requestDto.SeguidoresInstagram.Value;

            if (requestDto.CuentaPublica.HasValue)
                formulario.CuentaPublica = requestDto.CuentaPublica.Value;

            if (requestDto.UsuarioTikTok != null)
                formulario.UsuarioTikTok = requestDto.UsuarioTikTok.Trim();

            if (requestDto.SeguidoresTikTok.HasValue)
                formulario.SeguidoresTikTok = requestDto.SeguidoresTikTok.Value;

            if (requestDto.SobreTi != null)
                formulario.SobreTi = requestDto.SobreTi.Trim();

            if (requestDto.AceptaBeneficios != null)
                formulario.AceptaBeneficios = requestDto.AceptaBeneficios.Trim();

            if (requestDto.AceptaTerminos.HasValue)
                formulario.AceptaTerminos = requestDto.AceptaTerminos.Value;

            if (requestDto.Estado != null)
                formulario.Estado = requestDto.Estado.Trim();

            if (requestDto.Comentarios != null)
                formulario.Comentarios = requestDto.Comentarios.Trim();

            var formularioActualizado = await _formularioRepository.UpdateAsync(formulario);

            // Si se actualizaron los seguidores, verificar si ahora cumple requisitos para Foodie
            if (requestDto.SeguidoresInstagram.HasValue || requestDto.SeguidoresTikTok.HasValue)
            {
                await ProcessFoodieRoleAsync(formulario.UsuarioId, formularioActualizado);
            }

            return MapToResponseDto(formularioActualizado);
        }

        public async Task<FormularioFoodieResponseDto?> GetMyFormularioAsync(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                throw new UnauthorizedAccessException("No se pudo identificar al usuario");
            }

            return await GetByUsuarioIdAsync(usuarioId);
        }

        public async Task<FormularioFoodieResponseDto> UpdateMyFormularioAsync(ClaimsPrincipal user, FormularioFoodieUpdateRequestDto requestDto)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                throw new UnauthorizedAccessException("No se pudo identificar al usuario");
            }

            return await UpdateMyFormularioAsync(usuarioId, requestDto);
        }

        private async Task<FormularioFoodieResponseDto> UpdateMyFormularioAsync(int usuarioId, FormularioFoodieUpdateRequestDto requestDto)
        {
            var formularioExistente = await _formularioRepository.GetByUsuarioIdAsync(usuarioId);
            if (formularioExistente == null)
            {
                throw new KeyNotFoundException("No tienes un formulario registrado");
            }

            return await UpdateAsync(formularioExistente.Id, requestDto);
        }

        public async Task<FormularioFoodieResponseDto?> GetByIdAsync(int id)
        {
            var formulario = await _formularioRepository.GetByIdAsync(id);
            return formulario != null ? MapToResponseDto(formulario) : null;
        }

        public async Task<FormularioFoodieResponseDto?> GetByUsuarioIdAsync(int usuarioId)
        {
            var formulario = await _formularioRepository.GetByUsuarioIdAsync(usuarioId);
            return formulario != null ? MapToResponseDto(formulario) : null;
        }

        public async Task<List<FormularioFoodieResponseDto>> GetAllAsync()
        {
            var formularios = await _formularioRepository.GetAllAsync();
            return formularios.Select(MapToResponseDto).ToList();
        }

        public async Task<List<FormularioFoodieResponseDto>> GetByEstadoAsync(string estado)
        {
            var formularios = await _formularioRepository.GetByEstadoAsync(estado);
            return formularios.Select(MapToResponseDto).ToList();
        }

        public async Task<object?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                throw new UnauthorizedAccessException("No se pudo identificar al usuario");
            }

            // TODO: Temporalmente comentado hasta arreglar conexión con UsersApi
            /*
            // Obtener información del usuario desde UsersApi
            var userInfo = await _usersApiService.GetCurrentUserAsync();
            if (userInfo != null)
            {
                return userInfo;
            }
            */

            // Si no se puede obtener del UsersApi, crear respuesta básica
            return new
            {
                id = usuarioId,
                nombre = "Usuario",
                apellido = "Actual",
                correo = "usuario@foodiesbnb.com",
                fechaCreacion = DateTime.UtcNow,
                estaActivo = true
            };
        }

        private async Task ValidateUniqueDataAsync(FormularioFoodieCreateRequestDto requestDto)
        {
            if (await _formularioRepository.ExistsByEmailAsync(requestDto.Email))
            {
                throw new InvalidOperationException("Ya existe un formulario con este email");
            }

            if (await _formularioRepository.ExistsByInstagramAsync(requestDto.UsuarioInstagram))
            {
                throw new InvalidOperationException("Ya existe un formulario con este usuario de Instagram");
            }

            if (await _formularioRepository.ExistsByTikTokAsync(requestDto.UsuarioTikTok))
            {
                throw new InvalidOperationException("Ya existe un formulario con este usuario de TikTok");
            }
        }

        private async Task ProcessFoodieRoleAsync(int usuarioId, FormularioFoodie formulario)
        {
            try
            {
                // Verificar si cumple los requisitos para ser Foodie
                bool cumpleRequisitos = formulario.SeguidoresInstagram >= 1000 || formulario.SeguidoresTikTok >= 1000;

                if (cumpleRequisitos)
                {
                    try
                    {
                        bool rolAgregado = await _usersApiService.AddRoleToUserAsync(usuarioId, "foodie");
                        if (rolAgregado)
                        {
                            _logger.LogInformation($"Usuario {usuarioId} cumple requisitos para ser Foodie - Rol asignado exitosamente");
                        }
                        else
                        {
                            _logger.LogWarning($"Usuario {usuarioId} cumple requisitos para ser Foodie pero no se pudo asignar el rol");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error al asignar rol foodie al usuario {usuarioId}: {ex.Message}");
                        // Continuar sin lanzar excepción para no afectar la creación del formulario
                    }
                }
                else
                {
                    _logger.LogInformation($"Usuario {usuarioId} NO cumple requisitos para ser Foodie (Instagram: {formulario.SeguidoresInstagram}, TikTok: {formulario.SeguidoresTikTok})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al procesar rol de Foodie para usuario {usuarioId}");
                // No lanzamos la excepción para no afectar la creación del formulario
            }
        }

        private static FormularioFoodieResponseDto MapToResponseDto(FormularioFoodie formulario)
        {
            return new FormularioFoodieResponseDto
            {
                Id = formulario.Id,
                UsuarioId = formulario.UsuarioId,
                NombreCompleto = formulario.NombreCompleto,
                Email = formulario.Email,
                NumeroPersonal = formulario.NumeroPersonal,
                FechaNacimiento = formulario.FechaNacimiento,
                Genero = formulario.Genero,
                Pais = formulario.Pais,
                Ciudad = formulario.Ciudad,
                FrecuenciaContenido = formulario.FrecuenciaContenido,
                UsuarioInstagram = formulario.UsuarioInstagram,
                SeguidoresInstagram = formulario.SeguidoresInstagram,
                CuentaPublica = formulario.CuentaPublica,
                UsuarioTikTok = formulario.UsuarioTikTok,
                SeguidoresTikTok = formulario.SeguidoresTikTok,
                SobreTi = formulario.SobreTi,
                AceptaBeneficios = formulario.AceptaBeneficios,
                AceptaTerminos = formulario.AceptaTerminos,
                FechaAplicacion = formulario.FechaAplicacion,
                FechaActualizacion = formulario.FechaActualizacion,
                Estado = formulario.Estado,
                Comentarios = formulario.Comentarios,
                Activo = formulario.Activo
            };
        }
    }
}