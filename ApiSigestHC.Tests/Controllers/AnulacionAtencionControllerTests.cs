using ApiSigestHC.Controllers;
using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using Xunit;

namespace ApiSigestHC.Tests.Controllers
{
    public class AnulacionAtencionControllerTests
    {
        private readonly Mock<IAnulacionAtencionService> _anulacionServiceMock;
        private readonly Mock<IMotivoAnulacionAtencionRepositorio> _motivoRepoMock;
        private readonly AnulacionAtencionController _controller;

        public AnulacionAtencionControllerTests()
        {
            _anulacionServiceMock = new Mock<IAnulacionAtencionService>();
            _motivoRepoMock = new Mock<IMotivoAnulacionAtencionRepositorio>();

            _controller = new AnulacionAtencionController(
                _anulacionServiceMock.Object,
                _motivoRepoMock.Object
            );
        }

        [Fact]
        public async Task Crear_AnulacionExitosa_RetornaOk()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 1,
                MotivoAnulacionAtencionId = 1,
                Observacion = "Anulación de prueba"
            };

            var respuestaEsperada = new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Atención anulada correctamente"
            };

            _anulacionServiceMock
                .Setup(s => s.AnularAtencionAsync(dto))
                .ReturnsAsync(respuestaEsperada);

            // Act
            var resultado = await _controller.Crear(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            
            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.True(respuesta.Ok);
            Assert.Equal("Atención anulada correctamente", respuesta.Message);
        }

        [Fact]
        public async Task Crear_AtencionNoExiste_RetornaNotFound()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 999,
                MotivoAnulacionAtencionId = 1,
                Observacion = "Test"
            };

            var respuestaError = new RespuestaAPI
            {
                Ok = false,
                StatusCode = HttpStatusCode.NotFound,
                ErrorMessages = new List<string> { "La atención no existe." }
            };

            _anulacionServiceMock
                .Setup(s => s.AnularAtencionAsync(dto))
                .ReturnsAsync(respuestaError);

            // Act
            var resultado = await _controller.Crear(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
            
            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.False(respuesta.Ok);
        }

        [Fact]
        public async Task Crear_RolNoAutorizado_RetornaForbidden()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 1,
                MotivoAnulacionAtencionId = 1,
                Observacion = "Test"
            };

            var respuestaError = new RespuestaAPI
            {
                Ok = false,
                StatusCode = HttpStatusCode.Forbidden,
                ErrorMessages = new List<string> { "Solo se pueden anular atenciones en estado 'admisión' y con el rol 'admisiones'." }
            };

            _anulacionServiceMock
                .Setup(s => s.AnularAtencionAsync(dto))
                .ReturnsAsync(respuestaError);

            // Act
            var resultado = await _controller.Crear(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.Forbidden, objectResult.StatusCode);
            
            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.False(respuesta.Ok);
        }

        [Fact]
        public async Task ObtenerMotivos_RetornaListaDeMotivos()
        {
            // Arrange
            var motivos = new List<MotivoAnulacionAtencion>
            {
                new MotivoAnulacionAtencion { Id = 1, Descripcion = "Error en digitación", Activo = true },
                new MotivoAnulacionAtencion { Id = 2, Descripcion = "Paciente no asistió", Activo = true }
            };

            _motivoRepoMock
                .Setup(r => r.ObtenerTodosAsync())
                .ReturnsAsync(motivos);

            // Act
            var resultado = await _controller.ObtenerMotivos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(okResult.Value);
            
            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            
            var listaMotivos = Assert.IsAssignableFrom<List<MotivoAnulacionAtencion>>(respuesta.Result);
            Assert.Equal(2, listaMotivos.Count);
        }
    }
}
