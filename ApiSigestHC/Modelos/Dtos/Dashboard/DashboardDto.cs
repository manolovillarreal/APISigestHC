namespace ApiSigestHC.Modelos.Dtos.Dashboard
{
    public class DashboardDto
    {
        public string Rol { get; set; }     // Rol normalizado que determina el bloque PorRol
        public DashboardGlobalDto Global { get; set; }
        public object PorRol { get; set; }  // DTO específico según el rol del usuario
    }
}
