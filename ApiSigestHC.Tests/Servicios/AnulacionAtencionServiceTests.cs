using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios;
using ApiSigestHC.Servicios.IServicios;
using Moq;
using System.Net;
using Xunit;

namespace ApiSigestHC.Tests.Servicios
{
    public class AnulacionAtencionServiceTests
    {
        private readonly Mock<IAtencionRepositorio> _atencionRepoMock;
        private readonly Mock<IMotivoAnulacionAtencionRepositorio> _motivoAnulacionRepoMock;
        private readonly Mock<IUsuarioContextService> _usuarioContextServiceMock;
        private readonly AnulacionAtencionService _service;

        public AnulacionAtencionServiceTests()
        {
            _atencionRepoMock = new Mock<IAtencionRepositorio>();
            _motivoAnulacionRepoMock = new Mock<IMotivoAnulacionAtencionRepositorio>();
            _usuarioContextServiceMock = new Mock<IUsuarioContextService>();

            _service = new AnulacionAtencionService(
                _motivoAnulacionRepoMock.Object,
                _atencionRepoMock.Object,
                _usuarioContextServiceMock.Object
            );
        }

        [Fact]
        public async Task AnularAtencionAsync_AtencionNoExiste_RetornaNotFound()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 1,
                MotivoAnulacionAtencionId = 1,
                Observacion = "Test"
            };

            _atencionRepoMock
                .Setup(r => r.ObtenerAtencionPorIdAsync(dto.AtencionId))
                .ReturnsAsync((Atencion)null!);

            // Act
            var resultado = await _service.AnularAtencionAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.NotFound, resultado.StatusCode);
            Assert.Contains("La atención no existe.", resultado.ErrorMessages);
        }

        [Fact]
        public async Task AnularAtencionAsync_AtencionYaAnulada_RetornaBadRequest()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 1,
                MotivoAnulacionAtencionId = 1,
                Observacion = "Test"
            };

            var atencion = new Atencion
            {
                Id = 1,
                EstadoAtencionId = 1,
                FechaAnulacion = DateTime.Now // Ya está anulada
            };

            _atencionRepoMock
                .Setup(r => r.ObtenerAtencionPorIdAsync(dto.AtencionId))
                .ReturnsAsync(atencion);

            // Act
            var resultado = await _service.AnularAtencionAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("ya ha sido anulada", resultado.ErrorMessages[0]);
        }

        [Fact]
        public async Task AnularAtencionAsync_MotivoNoExiste_RetornaBadRequest()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 1,
                MotivoAnulacionAtencionId = 999,
                Observacion = "Test"
            };

            var atencion = new Atencion
            {
                Id = 1,
                EstadoAtencionId = 1,
                FechaAnulacion = null
            };

            _atencionRepoMock
                .Setup(r => r.ObtenerAtencionPorIdAsync(dto.AtencionId))
                .ReturnsAsync(atencion);

            _motivoAnulacionRepoMock
                .Setup(r => r.ExisteAsync(dto.MotivoAnulacionAtencionId))
                .ReturnsAsync(false);

            // Act
            var resultado = await _service.AnularAtencionAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("Motivo de anulación inválido.", resultado.ErrorMessages);
        }

        [Fact]
        public async Task AnularAtencionAsync_EstadoNoEsAdmision_RetornaForbidden()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 1,
                MotivoAnulacionAtencionId = 1,
                Observacion = "Test"
            };

            var atencion = new Atencion
            {
                Id = 1,
                EstadoAtencionId = 2, // No es estado 1 (Admisión)
                FechaAnulacion = null
            };

            _atencionRepoMock
                .Setup(r => r.ObtenerAtencionPorIdAsync(dto.AtencionId))
                .ReturnsAsync(atencion);

            _motivoAnulacionRepoMock
                .Setup(r => r.ExisteAsync(dto.MotivoAnulacionAtencionId))
                .ReturnsAsync(true);

            _usuarioContextServiceMock
                .Setup(u => u.ObtenerRolNombre())
                .Returns("admisiones");

            // Act
            var resultado = await _service.AnularAtencionAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.Forbidden, resultado.StatusCode);
            Assert.Contains("Solo se pueden anular atenciones en estado 'admisión'", resultado.ErrorMessages[0]);
        }

        [Fact]
        public async Task AnularAtencionAsync_RolNoEsAdmisiones_RetornaForbidden()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 1,
                MotivoAnulacionAtencionId = 1,
                Observacion = "Test"
            };

            var atencion = new Atencion
            {
                Id = 1,
                EstadoAtencionId = 1,
                FechaAnulacion = null
            };

            _atencionRepoMock
                .Setup(r => r.ObtenerAtencionPorIdAsync(dto.AtencionId))
                .ReturnsAsync(atencion);

            _motivoAnulacionRepoMock
                .Setup(r => r.ExisteAsync(dto.MotivoAnulacionAtencionId))
                .ReturnsAsync(true);

            _usuarioContextServiceMock
                .Setup(u => u.ObtenerRolNombre())
                .Returns("Medico"); // Rol incorrecto

            // Act
            var resultado = await _service.AnularAtencionAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.Forbidden, resultado.StatusCode);
            Assert.Contains("con el rol 'admisiones'", resultado.ErrorMessages[0]);
        }

        [Fact]
        public async Task AnularAtencionAsync_DatosValidos_AnulaCorrectamente()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 1,
                MotivoAnulacionAtencionId = 1,
                Observacion = "Anulación por error"
            };

            var atencion = new Atencion
            {
                Id = 1,
                EstadoAtencionId = 1,
                FechaAnulacion = null,
                MotivoAnulacionAtencionId = null,
                UsuarioAnulaId = null,
                ObservacionAnulacion = null
            };

            _atencionRepoMock
                .Setup(r => r.ObtenerAtencionPorIdAsync(dto.AtencionId))
                .ReturnsAsync(atencion);

            _motivoAnulacionRepoMock
                .Setup(r => r.ExisteAsync(dto.MotivoAnulacionAtencionId))
                .ReturnsAsync(true);

            _usuarioContextServiceMock
                .Setup(u => u.ObtenerRolNombre())
                .Returns("admisiones");

            _usuarioContextServiceMock
                .Setup(u => u.ObtenerUsuarioId())
                .Returns(100);

            _atencionRepoMock
                .Setup(r => r.EditarAtencionAsync(It.IsAny<Atencion>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.AnularAtencionAsync(dto);

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.Equal("Atención anulada correctamente", resultado.Message);

            // Verificar que se actualizó la atención con los campos de anulación
            _atencionRepoMock.Verify(r => r.EditarAtencionAsync(It.Is<Atencion>(a =>
                a.MotivoAnulacionAtencionId == dto.MotivoAnulacionAtencionId &&
                a.FechaAnulacion.HasValue &&
                a.UsuarioAnulaId == 100 &&
                a.ObservacionAnulacion == dto.Observacion
            )), Times.Once);
        }

        [Fact]
        public async Task AnularAtencionAsync_VerificaCamposDeAnulacion_SonAsignadosCorrectamente()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 1,
                MotivoAnulacionAtencionId = 5,
                Observacion = "Observación de prueba"
            };

            var atencion = new Atencion
            {
                Id = 1,
                EstadoAtencionId = 1,
                FechaAnulacion = null
            };

            Atencion atencionCapturada = null!;

            _atencionRepoMock
                .Setup(r => r.ObtenerAtencionPorIdAsync(dto.AtencionId))
                .ReturnsAsync(atencion);

            _motivoAnulacionRepoMock
                .Setup(r => r.ExisteAsync(dto.MotivoAnulacionAtencionId))
                .ReturnsAsync(true);

            _usuarioContextServiceMock
                .Setup(u => u.ObtenerRolNombre())
                .Returns("admisiones");

            _usuarioContextServiceMock
                .Setup(u => u.ObtenerUsuarioId())
                .Returns(50);

            _atencionRepoMock
                .Setup(r => r.EditarAtencionAsync(It.IsAny<Atencion>()))
                .Callback<Atencion>(a => atencionCapturada = a)
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.AnularAtencionAsync(dto);

            // Assert
            Assert.NotNull(atencionCapturada);
            Assert.Equal(dto.MotivoAnulacionAtencionId, atencionCapturada.MotivoAnulacionAtencionId);
            Assert.NotNull(atencionCapturada.FechaAnulacion);
            Assert.Equal(50, atencionCapturada.UsuarioAnulaId);
            Assert.Equal(dto.Observacion, atencionCapturada.ObservacionAnulacion);
            Assert.True((DateTime.Now - atencionCapturada.FechaAnulacion.Value).TotalSeconds < 5);
        }
    }
}
