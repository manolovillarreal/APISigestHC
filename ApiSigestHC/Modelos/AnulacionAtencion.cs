using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{

    [Table("SIG_AnulacionAtencion")]
    public class AnulacionAtencion
    {
        public int Id { get; set; }

        [Column("atencion_id")]
        public int AtencionId { get; set; }
        [Column("motivoAnulacionAtencion_id")]
        public int MotivoAnulacionAtencionId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        public string? Observacion { get; set; }

        // Relaciones (si las quieres usar con EF Core)
        public Atencion? Atencion { get; set; }
        public MotivoAnulacionAtencion? MotivoAnulacionAtencion { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
