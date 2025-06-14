namespace ApiSigestHC.Servicios.IServicios
{
    public interface IUsuarioContextService
    {
        int ObtenerRolId();
        int ObtenerUsuarioId();
        string ObtenerNombreUsuario();
        string ObtenerRolNombre();
    }
}
