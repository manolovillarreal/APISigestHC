using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_Usuario")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Correo { get; set; }
        public string Contraseña { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Dni { get; set; }
        public bool EstaActivo { get; set; }
        [Column("rol_id")]
        public int RolId { get; set; }

        public Rol Rol { get; set; } // Navegación

        [NotMapped]
        public string RolNombre => Rol?.Nombre; // No se guarda en BD
    }
}
