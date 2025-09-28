using UsersApi.Models;

namespace UsersApi.Dtos.Response
{
    public class UsuarioResponseDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CodigoUsuario { get; set; } = string.Empty;
        public bool EstaActivo { get; set; }
        public DateTime? FechaUltimoAcceso { get; set; }
        public DateTime? FechaBloqueo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int IntentosFallidos { get; set; }
        
        /// <summary>
        /// Lista de roles del usuario
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// String de roles para compatibilidad (deprecated)
        /// </summary>
        public string Rol => string.Join(",", Roles);
    }
}
