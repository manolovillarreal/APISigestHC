using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos.Dtos
{
    public class UsuarioEditarDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string NombreUsuario { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio")]
        public string Correo { get; set; }

        public string? Contraseña { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "Los apellidos es obligatorio")]
        public string Apellidos { get; set; }
        [Required(ErrorMessage = "El Dni es obligatorio")]
        public string Dni { get; set; }

        [Required(ErrorMessage = "El Rol es obligatorio")]
        [Column("rol_id")]
        public int RolId { get; set; }

        public bool EstaActivo { get; set; }
    }
}
