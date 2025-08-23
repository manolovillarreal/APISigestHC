using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ApiSigestHC.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opciones) : base(opciones) { }

        public DbSet<Administradora> Administradoras { get; set; }
        public DbSet<AnulacionAtencion> AnulacionAtenciones { get; set; }
        public DbSet<Atencion> Atenciones { get; set; }
        public DbSet<CambioEstado> CambiosEstado { get; set; }
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<DocumentoRequerido> DocumentosRequeridos { get; set; }
        public DbSet<EstadoAtencion> EstadosAtencion { get; set; }
        public DbSet<MotivoAnulacionAtencion> MotivosAnulacionAtencion { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<RelacionPacienteAdministradora> RelacionPacienteAdministradoras { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<SolicitudCorreccion> SolicitudCorrecciones { get; set; }
        public DbSet<TipoDocumento> TiposDocumento { get; set; }
        public DbSet<TipoDocumentoRol> TipoDocumentoRoles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<UbicacionPacienteDto> UltimasUbicaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RelacionPacienteAdministradora>()
                .HasNoKey();

            modelBuilder.Entity<Atencion>()
                  .HasOne(a => a.Paciente)
                  .WithMany()
                  .HasForeignKey(a => a.PacienteId);

            modelBuilder.Entity<Atencion>()
                 .HasOne(a => a.EstadoAtencion)
                 .WithMany()
                 .HasForeignKey(a => a.EstadoAtencionId);

            modelBuilder.Entity<Atencion>()
                .HasOne(a => a.Administradora)
                .WithMany()
                .HasForeignKey(a => a.TerceroId);

            modelBuilder.Entity<Atencion>()
                .Property(a => a.PacienteId)
                .HasColumnType("varchar(20)");

            modelBuilder.Entity<Paciente>()
                .Property(p => p.Id)
                .HasColumnType("varchar(20)");

            modelBuilder.Entity<Documento>()
                 .HasOne(d => d.Usuario)
                 .WithMany()
                 .HasForeignKey(d => d.UsuarioId);


            modelBuilder.Entity<Paciente>().HasKey(p => p.Id);

            modelBuilder.Entity<TipoDocumentoRol>()
       .HasKey(tdr => new { tdr.TipoDocumentoId, tdr.RolId });

            modelBuilder.Entity<TipoDocumentoRol>()
                .HasOne(tdr => tdr.TipoDocumento)
                .WithMany(td => td.TipoDocumentoRoles)
                .HasForeignKey(tdr => tdr.TipoDocumentoId);

            modelBuilder.Entity<TipoDocumentoRol>()
                .HasOne(tdr => tdr.Rol)
                .WithMany(r => r.TipoDocumentoRoles)
                .HasForeignKey(tdr => tdr.RolId);

            modelBuilder.Entity<DocumentoRequerido>()
            .HasKey(dr => dr.TipoDocumentoId);


                    modelBuilder.Entity<DocumentoRequerido>()
             .HasOne(dr => dr.TipoDocumento)
             .WithOne(td => td.DocumentoRequerido)
             .HasForeignKey<DocumentoRequerido>(dr => dr.TipoDocumentoId);

            modelBuilder.Entity<DocumentoRequerido>()
                .HasOne(dr => dr.EstadoAtencion)
                .WithMany(ea => ea.DocumentosRequeridos)
                .HasForeignKey(dr => dr.EstadoAtencionId);

            // Configuración para UbicacionPacienteDto
            modelBuilder.Entity<UbicacionPacienteDto>()
                .HasNoKey() // No tiene clave primaria
                .ToView(null); // No está mapeado a una tabla o vista física

            modelBuilder.Entity<SolicitudCorreccion>()
             .HasOne(sc => sc.Documento)                // una solicitud pertenece a un documento
             .WithMany(d => d.SolicitudesCorreccion)    // un documento tiene muchas solicitudes
             .HasForeignKey(sc => sc.DocumentoId);      // la FK está en SolicitudCorreccion

            modelBuilder.Entity<SolicitudCorreccion>()
               .Navigation(s => s.EstadoCorreccion)
               .AutoInclude();

            base.OnModelCreating(modelBuilder);



        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
                .EnableSensitiveDataLogging(); // Incluye datos sensibles en los logs (como valores de parámetros).
        }

    }
}
