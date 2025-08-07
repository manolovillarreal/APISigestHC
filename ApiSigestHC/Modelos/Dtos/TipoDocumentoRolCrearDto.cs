namespace ApiSigestHC.Modelos.Dtos
{
    public class TipoDocumentoRolCrearDto
    {
        public int TipoDocumentoId { get; set; }
        public int RolId { get; set; }
        public bool PuedeVer { get; set; }
        public bool PuedeCargar { get; set; }
    }
}
