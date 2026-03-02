using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using Moq;
using System.Net;
using Xunit;

namespace ApiSigestHC.Tests.Servicios
{
    public class AtencionesServiceTests
    {
        private readonly Mock<IAtencionRepositorio> _atencionRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IMotivoAnulacionAtencionRepositorio> _motivoAnulacionRepoMock;
        private readonly Mock<IDocumentoRepositorio> _documentoRepoMock;
        private readonly Mock<IUsuarioContextService> _usuarioContextServiceMock;
        private readonly Mock<IVisualizacionEstadoService> _visualizacionEstadoServiceMock;
        private readonly AtencionesService _service;

        public AtencionesServiceTests()
        {
            _atencionRepoMock = new Mock<IAtencionRepositorio>();
            _mapperMock = new Mock<IMapper>();
            _motivoAnulacionRepoMock = new Mock<IMotivoAnulacionAtencionRepositorio>();
            _documentoRepoMock = new Mock<IDocumentoRepositorio>();
            _usuarioContextServiceMock = new Mock<IUsuarioContextService>();
            _visualizacionEstadoServiceMock = new Mock<IVisualizacionEstadoService>();

            _service = new AtencionesService(
                _atencionRepoMock.Object,
                _mapperMock.Object,
                _motivoAnulacionRepoMock.Object,
                _documentoRepoMock.Object,
                _usuarioContextServiceMock.Object,
                _visualizacionEstadoServiceMock.Object
            );
        }

        #region ObteneAtencionesPorFiltroAsync Tests

        [Fact]
        public async Task ObteneAtencionesPorFiltroAsync_FechaInicioMayorQueFechaFin_RetornaBadRequest()
        {
            // Arrange
            var filtro = new AtencionFiltroDto
            {
                FechaInicio = new DateTime(2025, 12, 31),
                FechaFin = new DateTime(2025, 1, 1)
            };

            // Act
            var resultado = await _service.ObteneAtencionesPorFiltroAsync(filtro);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("fecha inicial no puede ser mayor que la final", resultado.ErrorMessages[0]);
        }

        [Fact]
        public async Task ObteneAtencionesPorFiltroAsync_FiltroValido_RetornaAtenciones()
        {
            // Arrange
            var filtro = new AtencionFiltroDto
            {
                Page = 1,
                PageSize = 10,
                EstadoAtencionId = 1
            };

            var atenciones = new List<Atencion>
            {
                new Atencion { Id = 1, EstadoAtencionId = 1, FechaAnulacion = null },
                new Atencion { Id = 2, EstadoAtencionId = 1, FechaAnulacion = null }
            };

            _visualizacionEstadoServiceMock
                .Setup(v => v.ObtenerEstadosPermitidosPorRol())
                .Returns(new List<int> { 1, 2, 3 });

            _atencionRepoMock
                .Setup(r => r.ObtenerAtencionesPorFiltroAsync(It.IsAny<AtencionFiltroDto>()))
                .ReturnsAsync(atenciones);

            // Act
            var resultado = await _service.ObteneAtencionesPorFiltroAsync(filtro);

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.NotNull(resultado.Result);
        }

        #endregion

        #region AnularAtencionAsync Tests

        [Fact]
        public async Task AnularAtencionAsync_AtencionNoExiste_RetornaNotFound()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 999,
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
        public async Task AnularAtencionAsync_EstadoNoEsAdmision_RetornaBadRequest()
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
                EstadoAtencionId = 2, // No es admisión
                FechaAnulacion = null
            };

            _atencionRepoMock
                .Setup(r => r.ObtenerAtencionPorIdAsync(dto.AtencionId))
                .ReturnsAsync(atencion);

            // Act
            var resultado = await _service.AnularAtencionAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("Solo se pueden anular atenciones en estado 'Admisión'", resultado.ErrorMessages[0]);
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
                FechaAnulacion = DateTime.Now.AddDays(-1) // Ya anulada
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
        public async Task AnularAtencionAsync_MotivoInvalido_RetornaBadRequest()
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
            Assert.Contains("Motivo de anulación inválido", resultado.ErrorMessages[0]);
        }

        [Fact]
        public async Task AnularAtencionAsync_DatosValidos_AnulaCorrectamente()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 1,
                MotivoAnulacionAtencionId = 1,
                Observacion = "Anulación por error en datos"
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
                .Setup(u => u.ObtenerUsuarioId())
                .Returns(10);

            _atencionRepoMock
                .Setup(r => r.EditarAtencionAsync(It.IsAny<Atencion>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.AnularAtencionAsync(dto);

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.Equal("Atención anulada correctamente", resultado.Message);

            _atencionRepoMock.Verify(r => r.EditarAtencionAsync(It.Is<Atencion>(a =>
                a.MotivoAnulacionAtencionId == dto.MotivoAnulacionAtencionId &&
                a.FechaAnulacion.HasValue &&
                a.UsuarioAnulaId == 10 &&
                a.ObservacionAnulacion == dto.Observacion
            )), Times.Once);
        }

        [Fact]
        public async Task AnularAtencionAsync_ActualizaCorrectamenteTodosLosCampos()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 5,
                MotivoAnulacionAtencionId = 3,
                Observacion = "Observación detallada de anulación"
            };

            var atencion = new Atencion
            {
                Id = 5,
                EstadoAtencionId = 1,
                FechaAnulacion = null,
                MotivoAnulacionAtencionId = null,
                UsuarioAnulaId = null,
                ObservacionAnulacion = null
            };

            Atencion atencionCapturada = null!;

            _atencionRepoMock
                .Setup(r => r.ObtenerAtencionPorIdAsync(dto.AtencionId))
                .ReturnsAsync(atencion);

            _motivoAnulacionRepoMock
                .Setup(r => r.ExisteAsync(dto.MotivoAnulacionAtencionId))
                .ReturnsAsync(true);

            _usuarioContextServiceMock
                .Setup(u => u.ObtenerUsuarioId())
                .Returns(25);

            _atencionRepoMock
                .Setup(r => r.EditarAtencionAsync(It.IsAny<Atencion>()))
                .Callback<Atencion>(a => atencionCapturada = a)
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.AnularAtencionAsync(dto);

            // Assert
            Assert.NotNull(atencionCapturada);
            Assert.Equal(3, atencionCapturada.MotivoAnulacionAtencionId);
            Assert.NotNull(atencionCapturada.FechaAnulacion);
            Assert.Equal(25, atencionCapturada.UsuarioAnulaId);
            Assert.Equal("Observación detallada de anulación", atencionCapturada.ObservacionAnulacion);
            
            // Verificar que la fecha de anulación es reciente
            var segundosTranscurridos = (DateTime.Now - atencionCapturada.FechaAnulacion.Value).TotalSeconds;
            Assert.True(segundosTranscurridos < 5);
        }

        #endregion
    }
}
