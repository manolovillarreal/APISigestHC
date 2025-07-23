using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace ApiSigestHC.Repositorio
{
    public class AtencionRepositorio : IAtencionRepositorio
    {
        private readonly ApplicationDbContext _db;
        private readonly ICambioEstadoRepositorio _cambioEstadoRepo;

        public AtencionRepositorio(ApplicationDbContext contexto, ICambioEstadoRepositorio cambioEstadoRepo)
        {
            _db = contexto;
            _cambioEstadoRepo = cambioEstadoRepo;
        }

        public async Task<ICollection<Atencion>> ObtenerAtencionesAsync()
        {

            return await _db.Atenciones
                .Include(a => a.Paciente)
                .OrderBy(a => a.Fecha).ToListAsync();
               
        }
        public async Task<IEnumerable<Atencion>> GetAtencionesPorEstadoAsync(List<int> estados)
        {
            if (estados == null || !estados.Any())
                return Enumerable.Empty<Atencion>();

            // Obtiene solo los datos necesarios primero
            var atenciones = await _db.Atenciones
                .Include(a => a.Paciente)
                .Include(a => a.EstadoAtencion)
                .Include(a=>a.Administradora)
                .ToListAsync(); // Fuerza ejecución sin OPENJSON

            return atenciones.Where(a => estados.Contains(a.EstadoAtencionId));
        }

        public async Task<IEnumerable<Atencion>> ObtenerPorFechasAsync(DateTime fechaInicio, DateTime fechaFin, int page, int pageSize)
        {
            return await _db.Atenciones
                .Include(a => a.Paciente)
                .Where(a => a.Fecha >= fechaInicio && a.Fecha <= fechaFin)
                .OrderBy(a => a.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        public async Task<Atencion> ObtenerAtencionPorIdAsync(int id)
        {
            return await _db.Atenciones.FindAsync(id);
        }

        public async Task CrearAtencionAsync(Atencion atencion)
        {
            atencion.Fecha = DateTime.Now;
            atencion.EstadoAtencionId = 1;
            atencion.UsuarioId = 1;
            await _db.Atenciones.AddAsync(atencion);
            await _db.SaveChangesAsync();
        }

        public async Task EditarAtencionAsync(Atencion atencion)
        {            
            _db.Atenciones.Update(atencion);
            await _db.SaveChangesAsync();
        }

        public async Task EliminarAtencionAsync(int id)
        {
            var atencion = await ObtenerAtencionPorIdAsync(id);
            if (atencion != null)
            {
                _db.Atenciones.Remove(atencion);
                await _db.SaveChangesAsync();
            }
        }

     
       

    }
}
