using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
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

        public async Task<IEnumerable<Atencion>> GetAtencionesPorEstadoAsync(List<int> estados)
        {
            if (estados == null || !estados.Any())
                return Enumerable.Empty<Atencion>();

            // Obtiene solo los datos necesarios primero
            var atenciones = await _db.Atenciones
                .Where(a => !a.EstaAnulada)
                .Include(a => a.Paciente)
                .Include(a => a.EstadoAtencion)
                .Include(a=>a.Administradora)
                .Include(a => a.Documentos)
                    .ThenInclude(d => d.SolicitudesCorreccion)
                .ToListAsync(); // Fuerza ejecución sin OPENJSON

            return atenciones.Where(a => estados.Contains(a.EstadoAtencionId));
        }

        public async Task<Atencion> ObtenerAtencionPorIdAsync(int id)
        {
            return await _db.Atenciones
                  .Include(a => a.Paciente)
                .Include(a => a.EstadoAtencion)
                .Include(a => a.Administradora)
                .Include(a => a.Documentos)
                    .ThenInclude(d => d.SolicitudesCorreccion)
                .FirstOrDefaultAsync(a=>a.Id == id);
        }

        public async Task CrearAtencionAsync(Atencion atencion)
        {
            
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

        public async Task<IEnumerable<UbicacionPacienteDto>> GetUltimaUbicacionPacientesAsync(string[] pacienteIds)
        {
            if (pacienteIds == null || pacienteIds.Length == 0)
                return Enumerable.Empty<UbicacionPacienteDto>();

            var idsParam = string.Join("','", pacienteIds);

            var sql = $@"
        SELECT
            paciente_id AS PacienteId,
            nombre AS Nombre,
            codigo AS Codigo
        FROM (
            SELECT 
                I.paciente_id,
                U.nombre,
                U.codigo,
                ROW_NUMBER() OVER (
                    PARTITION BY I.paciente_id 
                    ORDER BY I.fecha DESC
                ) AS rn
            FROM HOSP_ingresos I
            JOIN Hosp_Ubicaciones U 
                ON U.id = I.ubicacion_id
            WHERE I.paciente_id IN ('{idsParam}')
        ) t
        WHERE rn = 1;
    ";

            return await _db.Set<UbicacionPacienteDto>()
                .FromSqlRaw(sql)
                .ToListAsync();
        }

        public async Task<ICollection<Atencion>> ObtenerAtencionesPorFiltroAsync(
            AtencionFiltroDto filtro)
        {
            var query = _db.Atenciones.AsQueryable();

            if (filtro.AtencionId.HasValue)
                query = query.Where(a => a.Id == filtro.AtencionId.Value);

            if (filtro.EstadoAtencionId.HasValue)
                query = query.Where(a => a.EstadoAtencionId == filtro.EstadoAtencionId.Value);

            if (!string.IsNullOrEmpty(filtro.TerceroId))
                query = query.Where(a => a.TerceroId == filtro.TerceroId);

            if (!string.IsNullOrEmpty(filtro.PacienteId))
                query = query.Where(a => a.PacienteId == filtro.PacienteId);

            if (filtro.FechaInicio.HasValue)
                query = query.Where(a => a.Fecha >= filtro.FechaInicio.Value);

            if (filtro.FechaFin.HasValue)
                query = query.Where(a => a.Fecha <= filtro.FechaFin.Value);

            if (!filtro.consultarAnuladas)
                query = query.Where(a => !a.EstaAnulada);

            query = query
                .Include(a => a.Paciente)
                .Include(a => a.EstadoAtencion)
                .Include(a => a.Administradora)
                .OrderByDescending(a => a.Fecha)
                .Skip((filtro.Page - 1) * filtro.PageSize)
                .Take(filtro.PageSize);

            return await query.ToListAsync();
        }

    }
}
