namespace ApiSigestHC.Modelos.Dtos
{
    public class TipoDocumentoRolDto
    {
        public int TipoDocumentoId { get; set; }
        public int RolId { get; set; }
        public bool PuedeVer { get; set; }
        public bool PuedeCargar { get; set; }
        public bool Activo { get; set; }
    }
}
