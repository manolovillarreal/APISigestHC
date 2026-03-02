namespace ApiSigestHC.Modelos
{
    public class ResultadoGuardadoArchivo
    {
        public string RutaBase { get; set; }
        public string RutaRelativa { get; set; }
        public string NombreArchivo { get; set; }
        public int Consecutivo { get; set; }
        public long TamanoBytes { get; set; }
        public int NumeroPaginas { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public DateTime? FechaEliminacion { get; set; }
        public int? UsuarioEliminacion { get; set; }
    }
}
