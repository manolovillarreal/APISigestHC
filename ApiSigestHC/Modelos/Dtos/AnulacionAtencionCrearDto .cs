namespace ApiSigestHC.Modelos.Dtos
{
    public class AnulacionAtencionCrearDto
    {
        public int AtencionId { get; set; }
        public int MotivoAnulacionAtencionId { get; set; }
        public string? Observacion { get; set; }
    }
}
