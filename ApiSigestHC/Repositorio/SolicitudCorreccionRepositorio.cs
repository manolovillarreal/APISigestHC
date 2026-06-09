using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
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
                .Include(sc=>sc.UsuarioSolicita)
                .Include(sc => sc.UsuarioCorrige)
                .Include(sc => sc.Documento)
                    .ThenInclude(d => d.TipoDocumento)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<SolicitudCorreccion?> ObtenerPendientePorDocumentoIdAsync(int documentoId)
        {
            return await _db.SolicitudCorrecciones
                .Include(sc => sc.UsuarioSolicita)
                .Include(sc => sc.UsuarioCorrige)
                .FirstOrDefaultAsync(c => c.DocumentoId == documentoId && c.EstadoCorreccionId != 3);
        }

        public async Task ActualizarAsync(SolicitudCorreccion solicitud)
        {
            _db.SolicitudCorrecciones.Update(solicitud);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<SolicitudCorreccion>> ObtenerSolicitudesPorRolAsync(
            int rolId, FiltroCorreccionesDto filtro)
        {
            // Filtro base: excluir aprobadas + solo tipos de documento que el rol puede cargar
            IQueryable<SolicitudCorreccion> query = _db.SolicitudCorrecciones
                .Where(sc => sc.EstadoCorreccionId != 3)
                .Where(sc => _db.Documentos
                    .Any(d => d.Id == sc.DocumentoId &&
                        _db.TipoDocumentoRoles
                            .Any(tdr => tdr.TipoDocumentoId == d.TipoDocumentoId
                                     && tdr.RolId == rolId)));

            // Filtros opcionales
            if (!string.IsNullOrEmpty(filtro.PacienteId))
                query = query.Where(sc =>
                    sc.Documento.Atencion.PacienteId.Contains(filtro.PacienteId));

            if (filtro.EstadoCorreccionId.HasValue)
                query = query.Where(sc =>
                    sc.EstadoCorreccionId == filtro.EstadoCorreccionId.Value);

            if (filtro.FechaInicial.HasValue)
                query = query.Where(sc =>
                    sc.FechaSolicitud >= filtro.FechaInicial.Value);

            if (filtro.FechaFinal.HasValue)
                query = query.Where(sc =>
                    sc.FechaSolicitud <= filtro.FechaFinal.Value.AddDays(1));

            if (filtro.TipoDocumentoId.HasValue)
                query = query.Where(sc =>
                    sc.Documento.TipoDocumentoId == filtro.TipoDocumentoId.Value);

            if (filtro.UsuarioSolicitaId.HasValue)
                query = query.Where(sc =>
                    sc.UsuarioSolicitaId == filtro.UsuarioSolicitaId.Value);

            if (!string.IsNullOrEmpty(filtro.NumeroRelacion))
                query = query.Where(sc =>
                    sc.Documento.NumeroRelacion != null &&
                    sc.Documento.NumeroRelacion.Contains(filtro.NumeroRelacion));

            return await query
                .Include(sc => sc.EstadoCorreccion)
                .Include(sc => sc.UsuarioSolicita)
                .Include(sc => sc.UsuarioCorrige)
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
        public async Task<IEnumerable<SolicitudCorreccion>> ObtenerSolicitudesEnviadasPorRolAsync(int rolId)
        {
            var rol = await _db.Roles.FindAsync(rolId);
            var nombreRol = rol?.Nombre ?? "";

            IQueryable<SolicitudCorreccion> query = _db.SolicitudCorrecciones
                .Where(sc => sc.EstadoCorreccionId != 3);

            if (nombreRol == "Auditoria")
            {
                // Auditoría ve todas las enviadas por Auditoría y Admisiones
                var rolesPermitidos = await _db.Roles
                    .Where(r => r.Nombre == "Auditoria" || r.Nombre == "Admisiones")
                    .Select(r => r.Id)
                    .ToListAsync();

                query = query.Where(sc =>
                    _db.Usuarios.Any(u =>
                        u.Id == sc.UsuarioSolicitaId &&
                        rolesPermitidos.Contains(u.RolId)));
            }
            else if (nombreRol == "Admisiones")
            {
                // Admisiones solo ve las suyas propias
                query = query.Where(sc =>
                    _db.Usuarios.Any(u =>
                        u.Id == sc.UsuarioSolicitaId &&
                        u.RolId == rolId));
            }
            else
            {
                // Otros roles no ven enviadas
                return Enumerable.Empty<SolicitudCorreccion>();
            }

            return await query
                .Include(sc => sc.EstadoCorreccion)
                .Include(sc => sc.UsuarioSolicita)
                .Include(sc => sc.UsuarioCorrige)
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
