using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Pdf.Filters;

namespace ApiSigestHC.Repositorio
{
    public class EstadoAtencionRepositorio : IEstadoAtencionRepositorio
    {
        private readonly ApplicationDbContext _db;
        private readonly IVisualizacionEstadoService _visualizacionEstadoService;

        public EstadoAtencionRepositorio(ApplicationDbContext db, IVisualizacionEstadoService visualizacionEstadoService)
        {
            _db = db;
            _visualizacionEstadoService = visualizacionEstadoService;   
        }

        public async Task<IEnumerable<EstadoAtencion>> ObtenerTodosAsync()
        {
            return await _db.EstadosAtencion.OrderBy(e => e.Orden).ToListAsync();
        }

        public async Task<EstadoAtencion?> ObtenerPorIdAsync(int id)
        {
            return await _db.EstadosAtencion.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<EstadoAtencion>> ObtenerPermitidosAsync()
        {
            var IdsDeEstadosPermitidos = _visualizacionEstadoService.ObtenerEstadosPermitidosPorRol();

            var estadosPermitidos = await  _db.EstadosAtencion                
                .OrderBy(e => e.Orden)
                .ToListAsync();

            return estadosPermitidos.Where(e => IdsDeEstadosPermitidos.Contains(e.Id));
        }
    }
}
