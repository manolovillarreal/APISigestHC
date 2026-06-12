using ApiSigestHC.Data;
using ApiSigestHC.Modelos.Dtos.Dashboard;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiSigestHC.Repositorio
{
    /*
     * IDs de EstadoAtencion (verificados en CambioEstadoService.cs):
     *   1 = Admision      5 = Auditoria
     *   2 = Consulta      6 = Facturacion
     *   3 = Ingreso       7 = Radicacion
     *   4 = Salida        8 = Archivado
     *
     * Nombres de rol (verificados en CambioEstadoService.cs):
     *   "Admisiones", "Medico", "Enfermeria", "Auditoria",
     *   "Facturacion", "Administrador"
     */
    public class DashboardRepositorio : IDashboardRepositorio
    {
        private readonly ApplicationDbContext _db;

        public DashboardRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────────
        // GLOBAL — hojas ahorradas al digitalizar
        // ─────────────────────────────────────────────
        public async Task<DashboardGlobalDto> ObtenerMetricasGlobalesAsync()
        {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var hojasHoy = await _db.Documentos
                .Where(d => d.FechaCarga.Date == hoy && d.FechaEliminacion == null)
                .SumAsync(d => (int?)d.NumeroPaginas) ?? 0;

            var hojasMes = await _db.Documentos
                .Where(d => d.FechaCarga >= inicioMes && d.FechaEliminacion == null)
                .SumAsync(d => (int?)d.NumeroPaginas) ?? 0;

            var hojasTotal = await _db.Documentos
                .Where(d => d.FechaEliminacion == null)
                .SumAsync(d => (int?)d.NumeroPaginas) ?? 0;

            return new DashboardGlobalDto
            {
                HojasAhorradasHoy = hojasHoy,
                HojasAhorradasMes = hojasMes,
                HojasAhorradasTotal = hojasTotal,
                ArbolesAhorradosTotal = Math.Round(hojasTotal / 8333.0, 2),
                KilosAhorradosTotal = Math.Round(hojasTotal * 5.0 / 1000.0, 2)
            };
        }

        // ─────────────────────────────────────────────
        // ADMISIONES
        // ─────────────────────────────────────────────
        public async Task<DashboardAdmisionesDto> ObtenerMetricasAdmisionesAsync()
        {
            // CambioEstado.Fecha se guarda en UTC (ver CambioEstadoService),
            // por eso el día de corte para los tiempos también debe ser UTC.
            var hoyUtc = DateTime.UtcNow.Date;

            var sinIdentidad = await _db.Atenciones
                .Where(a => a.EstadoAtencionId != 8
                    && a.FechaAnulacion == null
                    && !_db.Documentos.Any(d =>
                        d.AtencionId == a.Id
                        && d.TipoDocumento.Codigo == "ID"
                        && d.FechaEliminacion == null))
                .CountAsync();

            var sinAutorizacion = await _db.Atenciones
                .Where(a => a.EstadoAtencionId != 8
                    && a.FechaAnulacion == null
                    && !_db.Documentos.Any(d =>
                        d.AtencionId == a.Id
                        && d.TipoDocumento.Codigo == "AUT"
                        && d.FechaEliminacion == null))
                .CountAsync();

            // Tiempo promedio Admision(1)→Consulta(2) para cambios ocurridos hoy
            var tiempoEspera = await TiempoPromedioMinutosAsync(hoyUtc, estadoInicial: 1, estadoNuevo: 2);

            // Tiempo promedio Consulta(2)→Ingreso(3) para cambios ocurridos hoy
            var tiempoConsulta = await TiempoPromedioMinutosAsync(hoyUtc, estadoInicial: 2, estadoNuevo: 3);

            return new DashboardAdmisionesDto
            {
                AtencionesSinDocIdentidad = sinIdentidad,
                AtencionesSinAutorizacion = sinAutorizacion,
                TiempoPromedioEsperaHoy = tiempoEspera,
                TiempoPromedioConsultaHoy = tiempoConsulta
            };
        }

        // ─────────────────────────────────────────────
        // MÉDICO
        // ─────────────────────────────────────────────
        public async Task<DashboardMedicoDto> ObtenerMetricasMedicoAsync()
        {
            var hoyUtc = DateTime.UtcNow.Date;

            var enAdmision = await _db.Atenciones
                .Where(a => a.EstadoAtencionId == 1 && a.FechaAnulacion == null)
                .CountAsync();

            var tiempoEspera = await TiempoPromedioMinutosAsync(hoyUtc, estadoInicial: 1, estadoNuevo: 2);
            var tiempoConsulta = await TiempoPromedioMinutosAsync(hoyUtc, estadoInicial: 2, estadoNuevo: 3);

            return new DashboardMedicoDto
            {
                PacientesEnAdmision = enAdmision,
                TiempoPromedioEsperaHoy = tiempoEspera,
                TiempoPromedioConsultaHoy = tiempoConsulta
            };
        }

        // ─────────────────────────────────────────────
        // ENFERMERÍA
        // ─────────────────────────────────────────────
        public async Task<DashboardEnfermeriaDto> ObtenerMetricasEnfermeriaAsync()
        {
            var enIngreso = await _db.Atenciones
                .Where(a => a.EstadoAtencionId == 3 && a.FechaAnulacion == null)
                .CountAsync();

            return new DashboardEnfermeriaDto
            {
                PacientesEnIngreso = enIngreso
            };
        }

        // ─────────────────────────────────────────────
        // AUDITORÍA  (estado 5)
        // ─────────────────────────────────────────────
        public async Task<DashboardAuditoriaDto> ObtenerMetricasAuditoriaAsync()
        {
            var hoyUtc = DateTime.UtcNow.Date;

            var pendientesRevision = await _db.Atenciones
                .Where(a => a.EstadoAtencionId == 5 && a.FechaAnulacion == null)
                .CountAsync();

            var correcciones = await _db.SolicitudCorrecciones
                .Where(sc => sc.EstadoCorreccionId == 2) // Respondida, pendiente de aprobación
                .CountAsync();

            // Tiempo promedio Auditoria(5)→Facturacion(6) para cambios ocurridos hoy
            var tiempoRevision = await TiempoPromedioMinutosAsync(hoyUtc, estadoInicial: 5, estadoNuevo: 6);

            return new DashboardAuditoriaDto
            {
                AtencionesPendientesRevision = pendientesRevision,
                CorrecccionesPendientes = correcciones,
                TiempoPromedioRevisionHoy = tiempoRevision
            };
        }

        // ─────────────────────────────────────────────
        // FACTURACIÓN  (estado 6)
        // ─────────────────────────────────────────────
        public async Task<DashboardFacturacionDto> ObtenerMetricasFacturacionAsync()
        {
            var pendientes = await _db.Atenciones
                .Where(a => a.EstadoAtencionId == 6 && a.FechaAnulacion == null)
                .CountAsync();

            // Atenciones en riesgo: pendientes de facturar/radicar (estados 6 y 7,
            // sin archivar ni anular) cuya antigüedad supera los 22 días.
            // Referencia = fecha de radicación (entrada al estado 7); si la
            // atención aún no ha sido radicada, se usa su FechaCreacion (Atencion.Fecha).
            var limite = DateTime.UtcNow.AddDays(-22);
            var enRiesgo = await _db.Atenciones
                .Where(a => (a.EstadoAtencionId == 6 || a.EstadoAtencionId == 7)
                    && a.FechaAnulacion == null)
                .Select(a => new
                {
                    Referencia = _db.CambiosEstado
                        .Where(ce => ce.AtencionId == a.Id && ce.EstadoNuevo == 7)
                        .Max(ce => (DateTime?)ce.Fecha) ?? a.Fecha
                })
                .CountAsync(x => x.Referencia <= limite);

            return new DashboardFacturacionDto
            {
                AtencionesPendientes = pendientes,
                AtencionesenRiesgo = enRiesgo
            };
        }

        // ─────────────────────────────────────────────
        // ADMINISTRADOR
        // ─────────────────────────────────────────────
        public async Task<DashboardAdminDto> ObtenerMetricasAdminAsync()
        {
            var porEstado = await _db.Atenciones
                .Where(a => a.EstadoAtencionId != 8 && a.FechaAnulacion == null)
                .GroupBy(a => a.EstadoAtencion.Nombre)
                .Select(g => new { Estado = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Estado, x => x.Count);

            var usuariosActivos = await _db.Usuarios
                .Where(u => u.EstaActivo)
                .CountAsync();

            return new DashboardAdminDto
            {
                AtencionesPorEstado = porEstado,
                UsuariosActivos = usuariosActivos
            };
        }

        // ─────────────────────────────────────────────
        // Helper: tiempo promedio en minutos entre dos transiciones
        // para cambios registrados en el día indicado
        // ─────────────────────────────────────────────
        private async Task<double> TiempoPromedioMinutosAsync(
            DateTime dia, int estadoInicial, int estadoNuevo)
        {
            // Para cada atencion que llegó a estadoNuevo hoy,
            // buscar cuándo salió de estadoInicial y calcular la diferencia.
            var llegadas = await _db.CambiosEstado
                .Where(ce => ce.EstadoNuevo == estadoNuevo
                          && ce.Fecha.Date == dia)
                .ToListAsync();

            if (!llegadas.Any()) return 0;

            var atencionIds = llegadas.Select(c => c.AtencionId).Distinct().ToList();

            // Obtener el momento en que cada atención entró al estadoInicial
            var salidas = await _db.CambiosEstado
                .Where(ce => ce.EstadoNuevo == estadoInicial
                          && atencionIds.Contains(ce.AtencionId))
                .ToListAsync();

            var tiempos = new List<double>();
            foreach (var llegada in llegadas)
            {
                var salida = salidas
                    .Where(s => s.AtencionId == llegada.AtencionId && s.Fecha <= llegada.Fecha)
                    .OrderByDescending(s => s.Fecha)
                    .FirstOrDefault();

                if (salida != null)
                    tiempos.Add((llegada.Fecha - salida.Fecha).TotalMinutes);
            }

            return tiempos.Any() ? Math.Round(tiempos.Average(), 1) : 0;
        }
    }
}
