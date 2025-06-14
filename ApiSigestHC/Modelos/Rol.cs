using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos
{
    [Table("SIG_Rol")]
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public ICollection<TipoDocumentoRol> TipoDocumentoRoles { get; set; }

    }
}
