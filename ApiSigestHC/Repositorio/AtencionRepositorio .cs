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
                .OrderBy(a => a.FechaAtencion).ToListAsync();
               
        }
        public async Task<IEnumerable<Atencion>> GetAtencionesByStateAsync(int minState,int maxState)
        {
            return await _db.Atenciones
                         .Include(a => a.Paciente)
                         .Where(a => a.EstadoAtencion >= minState && a.EstadoAtencion <= maxState)
                         .ToListAsync();
        }

        public async Task<Atencion> ObtenerAtencionPorIdAsync(int id)
        {
            return await _db.Atenciones.FindAsync(id);
        }

        public async Task CrearAtencionAsync(Atencion atencion)
        {
            atencion.FechaAtencion = DateTime.Now;
            atencion.EstadoAtencion = 1;
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
