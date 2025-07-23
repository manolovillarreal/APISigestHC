using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSigestHC.Modelos.Dtos
{
    public class AtencionDto
    {
        public int Id { get; set; }

        public string PacienteId { get; set; }

        public string TerceroId { get; set; }

        public DateTime Fecha { get; set; }

        public DateTime FechaFinaliza { get; set; }



        public int EstadoAtencionId { get; set; }

        public int UsuarioId { get; set; }


        public Paciente? Paciente { get; set; }
        public Administradora? Administradora { get; set; }

        public EstadoAtencion? EstadoAtencion { get; set; }

    }
}
