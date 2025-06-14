using ApiSigestHC.Data;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiSigestHC.Repositorio
{
    public class PacienteRepositorio : IPacienteRepositorio
    {
        private readonly ApplicationDbContext _db;

        public PacienteRepositorio(ApplicationDbContext contexto)
        {
            _db = contexto;
        }


        async Task<Paciente> IPacienteRepositorio.ObtenerPacientePorIdAsync(string pacienteId)
        {
            Paciente paciente =  _db.Pacientes.FirstOrDefault(p => p.Id == pacienteId);

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


            return await _db.Pacientes.FirstOrDefaultAsync(p=>p.Id == pacienteId);
        }
    }
}
