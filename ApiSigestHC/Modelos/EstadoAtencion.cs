namespace ApiSigestHC.Modelos
{
    public class EstadoAtencion
    {
        public int Id { get; set; }
        public int Nombre { get; set; }
        public int Orden { get; set; }

        public ICollection<DocumentoRequerido>DocumentosRequeridos {get; set;}
    }
}
