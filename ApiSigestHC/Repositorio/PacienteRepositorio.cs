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

        public async Task<Paciente> ObtenerUltimoPacienteIngresadoIdAsync()
        {
            // Paso 1: Obtener pacientes ingresados recientemente
            string query = $@"
                SELECT P.*
                FROM (
                    SELECT TOP 20 P.*
                    FROM INGRESO_URGENCIAS IU
                    JOIN PACIENTES P ON P.NU_HIST_PAC = IU.NU_HIST_PAC_INUR
                    ORDER BY IU.FE_FECH_INUR DESC 
                ) P
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM SIG_ATENCION A
                    WHERE A.PACIENTE_ID = P.NU_HIST_PAC
                      AND A.ESTADOATENCION_ID <= 4
                )";

            Paciente paciente = await _db.Pacientes.FromSqlRaw(query).FirstOrDefaultAsync();

            if (paciente != null)
            {
                paciente.Administradoras = (from rpe in _db.RelacionPacienteAdministradoras
                                            join eps in _db.Administradoras
                                            on rpe.NitEps equals eps.Nit
                                            where rpe.PacienteId == paciente.Id && rpe.Activo == "S"
                                            select new Administradora
                                            {
                                                Nit = eps.Nit,
                                                Nombre = eps.Nombre
                                            }).ToList();
            }
            return paciente;
        }
        
        public async Task<List<Paciente>> ObtenerUltimosPacientesIngresadosAsync(int limit)
        {

            // Paso 1: Obtener pacientes ingresados recientemente
            string query = $@"
                SELECT P.*
                FROM (
                    SELECT TOP {limit} P.*
                    FROM INGRESO_URGENCIAS IU
                    JOIN PACIENTES P ON P.NU_HIST_PAC = IU.NU_HIST_PAC_INUR
                    ORDER BY IU.FE_FECH_INUR DESC
                ) P
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM SIG_ATENCION A
                    WHERE A.PACIENTE_ID = P.NU_HIST_PAC
                      AND A.ESTADOATENCION_ID <= 4
                )";

            var pacientes = await _db.Pacientes
                .FromSqlRaw(query)
                .AsNoTracking()
                .ToListAsync();

         
            foreach (var paciente in pacientes)
            {
                paciente.Administradoras = await (
                    from rpe in _db.RelacionPacienteAdministradoras
                    join eps in _db.Administradoras on rpe.NitEps equals eps.Nit
                    where rpe.PacienteId == paciente.Id && rpe.Activo == "S"
                    select new Administradora
                    {
                        Nit = eps.Nit,
                        Nombre = eps.Nombre
                    }).ToListAsync();
            }

            return pacientes;
        }

    }
}
