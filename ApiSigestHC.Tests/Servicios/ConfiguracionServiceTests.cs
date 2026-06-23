using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios;
using ApiSigestHC.Servicios.IServicios;
using Moq;
using System.Net;
using Xunit;

namespace ApiSigestHC.Tests.Servicios
{
    public class ConfiguracionServiceTests
    {
        private readonly Mock<IConfiguracionRepositorio> _configuracionRepoMock;
        private readonly Mock<IUsuarioContextService> _usuarioContextServiceMock;
        private readonly Mock<IFileSystemService> _fileSystemMock;
        private readonly ConfiguracionService _service;

        public ConfiguracionServiceTests()
        {
            _configuracionRepoMock = new Mock<IConfiguracionRepositorio>();
            _usuarioContextServiceMock = new Mock<IUsuarioContextService>();
            _fileSystemMock = new Mock<IFileSystemService>();

            _usuarioContextServiceMock.Setup(u => u.ObtenerUsuarioId()).Returns(1);

            _service = new ConfiguracionService(
                _configuracionRepoMock.Object,
                _usuarioContextServiceMock.Object,
                _fileSystemMock.Object
            );
        }

        [Fact]
        public async Task ActualizarRutaBaseDocumentosAsync_RutaExiste_RetornaOk()
        {
            // Arrange
            var ruta = "C:\\documentos";
            var config = new Configuracion { Id = 1, Clave = "ruta_base_documentos", Valor = "C:\\vieja", FechaActualizacion = DateTime.UtcNow };

            _fileSystemMock.Setup(f => f.DirectoryExists(ruta)).Returns(true);
            _configuracionRepoMock.Setup(r => r.ObtenerPorClaveAsync("ruta_base_documentos")).ReturnsAsync(config);
            _configuracionRepoMock.Setup(r => r.ActualizarAsync(It.IsAny<Configuracion>())).Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.ActualizarRutaBaseDocumentosAsync(ruta);

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            _configuracionRepoMock.Verify(r => r.ActualizarAsync(It.IsAny<Configuracion>()), Times.Once);
        }

        [Fact]
        public async Task ActualizarRutaBaseDocumentosAsync_RutaNoExiste_Retorna400()
        {
            // Arrange
            var ruta = "C:\\ruta_inexistente";

            _fileSystemMock.Setup(f => f.DirectoryExists(ruta)).Returns(false);

            // Act
            var resultado = await _service.ActualizarRutaBaseDocumentosAsync(ruta);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            _configuracionRepoMock.Verify(r => r.ActualizarAsync(It.IsAny<Configuracion>()), Times.Never);
        }

        [Fact]
        public async Task ObtenerRutaBaseDocumentosAsync_RetornaValorCorrecto()
        {
            // Arrange
            var config = new Configuracion { Clave = "ruta_base_documentos", Valor = "C:\\documentos" };
            _configuracionRepoMock.Setup(r => r.ObtenerPorClaveAsync("ruta_base_documentos")).ReturnsAsync(config);

            // Act
            var resultado = await _service.ObtenerRutaBaseDocumentosAsync();

            // Assert
            Assert.Equal("C:\\documentos", resultado);
        }

        [Fact]
        public async Task ObtenerRutaBaseDocumentosAsync_ClaveNoExiste_RetornaStringVacio()
        {
            // Arrange
            _configuracionRepoMock.Setup(r => r.ObtenerPorClaveAsync("ruta_base_documentos")).ReturnsAsync((Configuracion?)null);

            // Act
            var resultado = await _service.ObtenerRutaBaseDocumentosAsync();

            // Assert
            Assert.Equal(string.Empty, resultado);
        }
    }
}
