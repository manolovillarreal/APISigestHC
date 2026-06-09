namespace ApiSigestHC.Modelos.Dtos.Dashboard
{
    public class DashboardMedicoDto
    {
        public int PacientesEnAdmision { get; set; }
        public double TiempoPromedioEsperaHoy { get; set; }   // minutos Admision→Consulta
        public double TiempoPromedioConsultaHoy { get; set; } // minutos Consulta→Ingreso
    }
}
