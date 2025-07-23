using ApiSigestHC.Controllers;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApiSigestHC.Tests.Controllers
{
    public class PacientesControllerTests
    {
        private readonly Mock<IPacienteRepositorio> _repositorioMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly PacientesController _controller;

        public PacientesControllerTests()
        {
            _repositorioMock = new Mock<IPacienteRepositorio>();
            _mapperMock = new Mock<IMapper>();
            _controller = new PacientesController(_repositorioMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetPaciente_RetornaPaciente_ConRespuestaExitosa()
        {
            // Arrange
            string pacienteId = "123456";
            var paciente = new Paciente
            {
                Id = pacienteId,
                PrimerNombre = "Juan",
                PrimerApellido ="Perez"
            };

            _repositorioMock
                .Setup(r => r.ObtenerPacientePorIdAsync(pacienteId))
                .ReturnsAsync(paciente);

            // Act
            var resultado = await _controller.GetPaciente(pacienteId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(okResult.Value);
            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.Equal(paciente, respuesta.Result);
        }
        [Fact]
        public async Task GetPaciente_RetornaNotFound_SiPacienteNoExiste()
        {
            // Arrange
            string pacienteId = "999";
            _repositorioMock
                .Setup(r => r.ObtenerPacientePorIdAsync(pacienteId))
                .ReturnsAsync((Paciente)null);

            // Act
            var resultado = await _controller.GetPaciente(pacienteId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(notFoundResult.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        }

        [Fact]
        public async Task GetPaciente_CuandoOcurreExcepcion_RetornaErrorInterno()
        {
            // Arrange
            string pacienteId = "error";
            _repositorioMock
                .Setup(r => r.ObtenerPacientePorIdAsync(pacienteId))
                .ThrowsAsync(new System.Exception("Fallo de prueba"));

            // Act
            var resultado = await _controller.GetPaciente(pacienteId);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(errorResult.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, respuesta.StatusCode);
            Assert.Contains("Fallo de prueba", respuesta.ErrorMessages[1]);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetPaciente_IdInvalido_RetornaBadRequest(string pacienteId)
        {
            // Act
            var resultado = await _controller.GetPaciente(pacienteId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(badRequest.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
            Assert.Contains("El ID del paciente es obligatorio", respuesta.ErrorMessages[0]);
        }

    }
}
