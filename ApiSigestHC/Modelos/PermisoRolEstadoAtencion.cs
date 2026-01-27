using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_PermisosRolEstadoAtencion")]
    public class PermisoRolEstadoAtencion
    {
        [Column("rol_id")]
        public int RolId { get; set; }

        [Column("estadoAtencion_id")]
        public int EstadoAtencionId { get; set; }

        [Column("esVisible")]
        public bool EsVisible { get; set; }

        [Column("esPermitido")]
        public bool EsPermitido { get; set; }

        // Navegación
        public Rol Rol { get; set; }
        public EstadoAtencion EstadoAtencion { get; set; }
    }
}
