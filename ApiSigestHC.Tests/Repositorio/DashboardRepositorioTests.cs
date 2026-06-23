using ApiSigestHC.Data;
using ApiSigestHC.Repositorio;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;

namespace ApiSigestHC.Tests.Repositorio
{
    public class DashboardRepositorioTests
    {
        private ApplicationDbContext ObtenerContextoReal()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=10.10.1.1;Database=CLINICA;User ID=ADMIN;Password=TRIACON;TrustServerCertificate=true;MultipleActiveResultSets=true")
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task ObtenerMetricasAdmisionesAsync_DebeEjecutarseSinErroresSql()
        {
            // Arrange
            using var context = ObtenerContextoReal();
            var repositorio = new DashboardRepositorio(context);

            // Act
            // El objetivo de este test es únicamente ejecutar el método contra la BD real
            // para ver si explota por SqlNullValueException
            var resultado = await repositorio.ObtenerMetricasAdmisionesAsync();

            // Assert
            Assert.NotNull(resultado);
        }
    }
}
