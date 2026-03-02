using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ApiSigestHC.Repositorio
{
    public class PacienteRepositorio : IPacienteRepositorio
    {
        private readonly ApplicationDbContext _db;

        public PacienteRepositorio(ApplicationDbContext contexto)
        {
            _db = contexto;
        }


        public async Task<Paciente> ObtenerPacientePorIdAsync(string pacienteId)
        {
            Paciente paciente =  await _db.Pacientes.FirstOrDefaultAsync(p => p.Id == pacienteId);

            if (paciente != null)
            {
                paciente.Administradoras = (from rpe in _db.RelacionPacienteAdministradoras
                                            join eps in _db.Administradoras
                                            on rpe.NitEps equals eps.Nit
                                            where rpe.PacienteId == pacienteId && rpe.Activo == "S"
                                            select new Administradora
                                            {
                                                Nit = eps.Nit,
                                                Nombre = eps.Nombre
                                            }).ToList();
            }


            return paciente;
        }

        public async Task<IngresoUrgencias> ObtenerUltimoPacienteIngresadoIdAsync()
        {
            var ingreso = await _db.IngresosUrgencias
                .FromSqlRaw(getIngresosQuery(1))
                .Include(i => i.Paciente)
                .FirstOrDefaultAsync();

            if (ingreso?.Paciente != null)
            {
                ingreso.Paciente.Administradoras = (from rpe in _db.RelacionPacienteAdministradoras
                                            join eps in _db.Administradoras
                                            on rpe.NitEps equals eps.Nit
                                            where rpe.PacienteId == ingreso.PacienteId && rpe.Activo == "S"
                                            select new Administradora
                                            {
                                                Nit = eps.Nit,
                                                Nombre = eps.Nombre
                                            }).ToList();
            }
            return ingreso;
        }
        
        public async Task<List<IngresoUrgencias>> ObtenerUltimosPacientesIngresadosAsync(int limit)
        {
            var ingresos = await _db.IngresosUrgencias
                .FromSqlRaw(getIngresosQuery(20))
                .Include(i => i.Paciente)
                .AsNoTracking()
                .OrderByDescending(i => i.FechaIngreso)
                .ToListAsync();

            foreach (var ingreso in ingresos)
            {
                if (ingreso.Paciente != null)
                {
                    ingreso.Paciente.Administradoras = await (
                        from rpe in _db.RelacionPacienteAdministradoras
                        join eps in _db.Administradoras on rpe.NitEps equals eps.Nit
                        where rpe.PacienteId == ingreso.PacienteId && rpe.Activo == "S"
                        select new Administradora
                        {
                            Nit = eps.Nit,
                            Nombre = eps.Nombre
                        }).ToListAsync();
                }
            }

            return ingresos;
        }

        private string getIngresosQuery(int limit)
        {
            string ingresosQuery = @"
            SELECT TOP "+limit+ @" 
                   IU.NU_NUME_INUR,
                   IU.TX_OBSER_INUR,
                   DATEADD(SECOND, 
                           DATEDIFF(SECOND, '00:00:00', CAST(IU.TX_HORA_INUR AS TIME)), 
                           CAST(IU.FE_FECING_INUR AS DATETIME)
                   ) AS FE_FECING_INUR, 
                   DATEADD(SECOND, 
                           DATEDIFF(SECOND, '00:00:00', CAST(IU.TX_HORA_INUR AS TIME)), 
                           CAST(IU.FE_FECH_INUR AS DATETIME)
                   ) AS FE_FECH_INUR,
                   IU.NU_HIST_PAC_INUR
            FROM INGRESO_URGENCIAS IU
            WHERE NOT EXISTS (
                SELECT 1
                FROM SIG_ATENCION A
                WHERE A.PACIENTE_ID = IU.NU_HIST_PAC_INUR
                  AND A.ESTADOATENCION_ID <= 4
            )
            ORDER BY FE_FECING_INUR DESC";

            return ingresosQuery;
        }
    }

        
}
