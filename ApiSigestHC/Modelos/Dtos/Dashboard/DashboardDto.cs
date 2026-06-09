namespace ApiSigestHC.Modelos.Dtos.Dashboard
{
    public class DashboardDto
    {
        public DashboardGlobalDto Global { get; set; }
        public object PorRol { get; set; }  // DTO específico según el rol del usuario
    }
}
