using ApiSigestHC.Controllers;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using Xunit;

namespace ApiSigestHC.Tests.Controllers
{
    public class MotivosAnulacionAtencionControllerTests
    {
        private readonly Mock<IMotivoAnulacionAtencionRepositorio> _motivoRepoMock;
        private readonly MotivosAnulacionAtencionController _controller;

        public MotivosAnulacionAtencionControllerTests()
        {
            _motivoRepoMock = new Mock<IMotivoAnulacionAtencionRepositorio>();
            _controller = new MotivosAnulacionAtencionController(_motivoRepoMock.Object);
        }

        [Fact]
        public async Task ObtenerTodos_RetornaListaDeMotivos()
        {
            // Arrange
            var motivos = new List<MotivoAnulacionAtencion>
            {
                new MotivoAnulacionAtencion { Id = 1, Descripcion = "Error en digitación", Activo = true },
                new MotivoAnulacionAtencion { Id = 2, Descripcion = "Paciente no asistió", Activo = true },
                new MotivoAnulacionAtencion { Id = 3, Descripcion = "Cancelación por solicitud del paciente", Activo = true }
            };

            _motivoRepoMock
                .Setup(r => r.ObtenerTodosAsync())
                .ReturnsAsync(motivos);

            // Act
            var resultado = await _controller.ObtenerTodos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(okResult.Value);
            
            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            
            var listaMotivos = Assert.IsAssignableFrom<List<MotivoAnulacionAtencion>>(respuesta.Result);
            Assert.Equal(3, listaMotivos.Count);
        }

        [Fact]
        public async Task ObtenerTodos_CuandoFalla_RetornaError500()
        {
            // Arrange
            _motivoRepoMock
                .Setup(r => r.ObtenerTodosAsync())
                .ThrowsAsync(new Exception("Error de base de datos"));

            // Act
            var resultado = await _controller.ObtenerTodos();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            
            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, respuesta.StatusCode);
        }

        [Fact]
        public async Task ObtenerPorId_MotivoExiste_RetornaMotivo()
        {
            // Arrange
            int motivoId = 1;
            var motivo = new MotivoAnulacionAtencion 
            { 
                Id = motivoId, 
                Descripcion = "Error en digitación", 
                Activo = true 
            };

            _motivoRepoMock
                .Setup(r => r.ObtenerPorIdAsync(motivoId))
                .ReturnsAsync(motivo);

            // Act
            var resultado = await _controller.ObtenerPorId(motivoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(okResult.Value);
            
            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            
            var motivoResultado = Assert.IsType<MotivoAnulacionAtencion>(respuesta.Result);
            Assert.Equal(motivoId, motivoResultado.Id);
            Assert.Equal("Error en digitación", motivoResultado.Descripcion);
        }

        [Fact]
        public async Task ObtenerPorId_MotivoNoExiste_RetornaNotFound()
        {
            // Arrange
            int motivoId = 999;

            _motivoRepoMock
                .Setup(r => r.ObtenerPorIdAsync(motivoId))
                .ReturnsAsync((MotivoAnulacionAtencion)null!);

            // Act
            var resultado = await _controller.ObtenerPorId(motivoId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(notFoundResult.Value);
            
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
            Assert.Contains($"Motivo de anulación con id {motivoId} no encontrado", respuesta.ErrorMessages[0]);
        }

        [Fact]
        public async Task ObtenerPorId_CuandoFalla_RetornaError500()
        {
            // Arrange
            int motivoId = 1;
            
            _motivoRepoMock
                .Setup(r => r.ObtenerPorIdAsync(motivoId))
                .ThrowsAsync(new Exception("Error de base de datos"));

            // Act
            var resultado = await _controller.ObtenerPorId(motivoId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            
            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, respuesta.StatusCode);
        }
    }
}
