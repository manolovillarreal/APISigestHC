using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{

    [Table("SIG_EstadoAtencion")]
    public class EstadoAtencion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Orden { get; set; }

        public ICollection<DocumentoRequerido>DocumentosRequeridos {get; set;}
    }
}
