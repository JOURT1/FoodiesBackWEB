using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommonApi.Models;

namespace UsersApi.Models
{
    [Table("usuario_roles")]

    public class UsuarioRol : Auditoria
    {
        [Key]
        [Column("id_usuario_rol")]
        public int IdUsuarioRol { get; set; }

        [Column("id_usuario")]
        [ForeignKey("Usuario")]
        [Required]
        public required int IdUsuario { get; set; }

        [Column("id_rol")]
        [ForeignKey("Rol")]
        [Required]
        public required int IdRol { get; set; }

        // Navegación
        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Rol Rol { get; set; } = null!;
    }
}
