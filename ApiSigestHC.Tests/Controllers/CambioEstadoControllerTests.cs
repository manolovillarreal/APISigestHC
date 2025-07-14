using ApiSigestHC.Controllers;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using FluentAssertions;
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
    public class CambioEstadoControllerTests
    {
        private readonly Mock<ICambioEstadoRepositorio> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CambioEstadoController _controller;

        public CambioEstadoControllerTests()
        {
            _repoMock = new Mock<ICambioEstadoRepositorio>();
            _mapperMock = new Mock<IMapper>();

            _controller = new CambioEstadoController(_repoMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task ObtenerHistorial_DeberiaRetornarOk_SiExistenCambios()
        {
            // Arrange
            int atencionId = 1;
            var cambios = new List<CambioEstado>
        {
            new CambioEstado { Id = 1, AtencionId = atencionId, EstadoInicial = 1, EstadoNuevo = 2 },
            new CambioEstado { Id = 2, AtencionId = atencionId, EstadoInicial = 2, EstadoNuevo = 3 }
        };

            _repoMock.Setup(r => r.ObtenerCambiosPorAtencionAsync(atencionId)).ReturnsAsync(cambios);
            _mapperMock.Setup(m => m.Map<List<CambioEstado>>(cambios)).Returns(cambios); // O mapear a DTO si usas uno

            // Act
            var resultado = await _controller.ObtenerHistorial(atencionId);

            // Assert
            var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
            var respuesta = okResult.Value.Should().BeOfType<RespuestaAPI>().Subject;

            respuesta.IsSuccess.Should().BeTrue();
            respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
            (respuesta.Result as List<CambioEstado>).Should().HaveCount(2);
        }

        [Fact]
        public async Task ObtenerHistorial_DeberiaRetornarNotFound_SiNoHayCambios()
        {
            // Arrange
            int atencionId = 99;
            _repoMock.Setup(r => r.ObtenerCambiosPorAtencionAsync(atencionId)).ReturnsAsync(new List<CambioEstado>());

            // Act
            var resultado = await _controller.ObtenerHistorial(atencionId);

            // Assert
            var notFound = resultado.Should().BeOfType<NotFoundObjectResult>().Subject;
            var respuesta = notFound.Value.Should().BeOfType<RespuestaAPI>().Subject;

            respuesta.IsSuccess.Should().BeFalse();
            respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
            respuesta.ErrorMessages.Should().Contain("No se encontró historial para la atención solicitada.");
        }
    }
}
