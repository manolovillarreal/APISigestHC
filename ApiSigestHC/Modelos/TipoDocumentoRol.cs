using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_TipoDocumento_Rol")]
    public class TipoDocumentoRol
    {
        [Column("tipoDocumento_id")]
        public int TipoDocumentoId { get; set; }

        [Column("rol_id")]
        public int RolId { get; set; }

        public bool PuedeVer { get; set; }
        public bool PuedeCargar { get; set; }

        // Relaciones de navegación
        public TipoDocumento TipoDocumento { get; set; }
        public Rol Rol { get; set; }
    }
}
