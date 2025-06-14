namespace ApiSigestHC.Modelos
{
    public class ResultadoValidacionDto
    {
        public bool EsValido { get; set; }
        public List<string> DocumentosFaltantes { get; set; } = new();
    }
}
