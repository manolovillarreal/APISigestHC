namespace ApiSigestHC.Modelos.Dtos.Dashboard
{
    public class DashboardAdmisionesDto
    {
        public int AtencionesSinDocIdentidad { get; set; }
        public int AtencionesSinAutorizacion { get; set; }
        public double TiempoPromedioEsperaHoy { get; set; }   // minutos Admision→Consulta
        public double TiempoPromedioConsultaHoy { get; set; } // minutos Consulta→Ingreso
    }
}
