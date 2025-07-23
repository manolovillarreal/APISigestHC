namespace ApiSigestHC.Modelos.Dtos
{
    public class AnulacionAtencionCrearDto
    {
        public int atencion_id { get; set; }
        public int motivoAnulacionAtencion_id { get; set; }
        public string? observacion { get; set; }
    }
}
