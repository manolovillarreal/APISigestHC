using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos.Dtos
{
    public class UsuarioCrearDto
    {
        [Required(ErrorMessage ="El usuario es obligatorio")]
        public string NombreUsuario { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio")]
        public string Correo { get; set; }
        [Required(ErrorMessage = "El password es obligatorio")]
        public string Password { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "Los apellidos es obligatorio")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "El Rol es obligatorio")]
        [Column("rol_id")]
        public int RoleId { get; set; }

    }
}
