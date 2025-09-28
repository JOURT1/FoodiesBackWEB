using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using CommonApi.Models;

namespace UsersApi.Models
{
    [Table("roles")]
    [Index(nameof(Codigo), IsUnique = true)]
    public class Rol : Auditoria
    {
        [Key]
        [Column("id_rol")]
        public int IdRol { get; set; }

        [Required]
        [StringLength(20)]
        [Column("codigo")]
        public required string Codigo { get; set; }

        [Required]
        [StringLength(50)]
        [Column("nombre")]
        public required string Nombre { get; set; }

        [StringLength(255)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Required]
        [Column("esta_activo")]
        public required bool EstaActivo { get; set; } = true;

        // Relación muchos a muchos con Usuario
        public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    }
}
