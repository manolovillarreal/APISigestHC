using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;
using System;

namespace ApiSigestHC.Repositorio
{
    public class SolicitudCorreccionRepositorio : ISolicitudCorreccionRepositorio
    {
        private readonly ApplicationDbContext _db;

        public SolicitudCorreccionRepositorio(ApplicationDbContext context)
        {
            _db = context;
        }

        public async Task AgregarAsync(SolicitudCorreccion entidad)
        {
            _db.SolicitudCorrecciones.Add(entidad);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<SolicitudCorreccion>> ObtenerPorDocumentoAsync(int documentoId)
        {
            return await _db.SolicitudCorrecciones
                .Where(c => c.DocumentoId == documentoId)
                .ToListAsync();
        }

        public async Task<SolicitudCorreccion?> ObtenerPorIdAsync(int id)
        {
            return await _db.SolicitudCorrecciones
                .Include(sc => sc.EstadoCorreccion)
                .Include(sc => sc.Documento)
                    .ThenInclude(d => d.TipoDocumento)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<SolicitudCorreccion?> ObtenerPendientePorDocumentoIdAsync(int documentoId)
        {
            return await _db.SolicitudCorrecciones
                .FirstOrDefaultAsync(c => c.DocumentoId == documentoId && c.EstadoCorreccionId != 3);
        }

        public async Task ActualizarAsync(SolicitudCorreccion solicitud)
        {
            _db.SolicitudCorrecciones.Update(solicitud);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<SolicitudCorreccion>> ObtenerSolicitudesPorRolAsync(int rolId)
        {
            return await _db.SolicitudCorrecciones
                .Where(sc=>sc.EstadoCorreccionId!=3)
                .Where(sc => _db.Documentos                
                    .Any(d => d.Id == sc.DocumentoId && _db.Usuarios
                        .Any(u => u.Id == d.UsuarioId && u.RolId == rolId)))
                .Include(sc=>sc.EstadoCorreccion)
                .Include(sc => sc.Documento)
                    .ThenInclude(d => d.Atencion)
                        .ThenInclude(a => a.Paciente)
                .Include(sc => sc.Documento)
                    .ThenInclude(d => d.Atencion)
                        .ThenInclude(a => a.Administradora)
                .Include(sc => sc.Documento)
                    .ThenInclude(d => d.TipoDocumento)
                .Include(sc => sc.Documento)
                    .ThenInclude(d => d.Usuario)
                .ToListAsync();
        }
    }

}
