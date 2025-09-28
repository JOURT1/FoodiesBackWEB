using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using CommonApi.Models;

namespace UsersApi.Models
{
    [Table("Usuarios")]
    [Index(nameof(CodigoUsuario), IsUnique = true)]
    public class Usuario : Auditoria
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Required]
        [Column("nombre")]
        public required string Nombre { get; set; }

        [Required]
        [Column("codigo_usuario")]
        public required string CodigoUsuario { get; set; }

        [Required]
        [Column("contrasenia")]
        public required string Contrasenia { get; set; }

        // Email removido temporalmente - la tabla no tiene esta columna
        [NotMapped]
        public string? Correo { get; set; }

        [Required]
        [Column("intentos_fallidos")]
        [DefaultValue(0)]
        public required int IntentosFallidos { get; set; }

        [Column("fecha_vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [Column("fecha_ultimo_acceso")]
        public DateTime? FechaUltimoAcceso { get; set; }

        [Column("fecha_bloqueo")]
        public DateTime? FechaBloqueo { get; set; }

        [Required]
        [Column("esta_activo")]
        [DefaultValue(true)]
        public required bool EstaActivo { get; set; } = true;

        // Campo roles en la base de datos (comma-separated values)
        [Required]
        [Column("roles")]
        public required string Roles { get; set; } = "cliente"; // Valor por defecto

        // Relación muchos a muchos con Rol
        public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();

        // Propiedad calculada para obtener los nombres de roles como lista
        [NotMapped]
        public List<string> RolesLista 
        {
            get => string.IsNullOrEmpty(Roles) ? new List<string>() : Roles.Split(',').ToList();
            set => Roles = string.Join(",", value);
        }

    }
}