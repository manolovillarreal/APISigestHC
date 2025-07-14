namespace ApiSigestHC.Modelos
{
    public class ResultadoGuardadoArchivo
    {
        public string RutaBase { get; set; }
        public string RutaRelativa { get; set; }
        public string NombreArchivo { get; set; }
        public int Consecutivo { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

    }
}
