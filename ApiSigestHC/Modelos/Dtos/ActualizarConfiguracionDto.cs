using System.ComponentModel.DataAnnotations;

namespace ApiSigestHC.Modelos.Dtos
{
    public class ActualizarConfiguracionDto
    {
        [Required]
        [MaxLength(500)]
        public string Valor { get; set; }
    }
}
