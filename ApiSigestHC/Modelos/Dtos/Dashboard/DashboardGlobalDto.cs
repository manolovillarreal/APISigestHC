namespace ApiSigestHC.Modelos.Dtos.Dashboard
{
    public class DashboardGlobalDto
    {
        public int HojasAhorradasHoy { get; set; }
        public int HojasAhorradasMes { get; set; }
        public int HojasAhorradasTotal { get; set; }
        public double ArbolesAhorradosTotal { get; set; }  // Total / 8333
        public double KilosAhorradosTotal { get; set; }    // Total * 5 / 1000
    }
}
