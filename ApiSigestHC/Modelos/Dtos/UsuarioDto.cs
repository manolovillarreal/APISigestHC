namespace ApiSigestHC.Modelos.Dtos
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Correo { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Dni { get; set; }
        public int RolId { get; set; }
        public RolDto Rol { get; set; }
        public bool EstaActivo { get; set; }
    }
}
