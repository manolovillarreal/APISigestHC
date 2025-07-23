using ApiSigestHC.Controllers;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
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
    public class AtencionesControllerTests
    {
        private readonly Mock<IAtencionRepositorio> _atencionRepoMock;
        private readonly Mock<IValidacionDocumentosObligatoriosService> _validacionDocServiceMock;
        private readonly Mock<IVisualizacionEstadoService> _visualizacionEstadoServiceMock;
        private readonly Mock<ICambioEstadoService> _cambioEstadoServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AtencionesController _controller;

        public AtencionesControllerTests()
        {
            _atencionRepoMock = new Mock<IAtencionRepositorio>();
            _validacionDocServiceMock = new Mock<IValidacionDocumentosObligatoriosService>();
            _visualizacionEstadoServiceMock = new Mock<IVisualizacionEstadoService>();
            _cambioEstadoServiceMock = new Mock<ICambioEstadoService>();
            _mapperMock = new Mock<IMapper>();

            _controller = new AtencionesController(
                _atencionRepoMock.Object,
                _validacionDocServiceMock.Object,
                _cambioEstadoServiceMock.Object,
                _visualizacionEstadoServiceMock.Object,
                _mapperMock.Object
            );
        }


        [Fact]
        public async Task GetAtencionesVisibles_ReturnsOkWithAtenciones()
        {
            // Arrange
            var estadosVisibles = new List<int> { 1, 2 };
            _visualizacionEstadoServiceMock.Setup(v => v.ObtenerEstadosVisiblesPorRol()).Returns(estadosVisibles);

            var atenciones = new List<Atencion>
    {
        new Atencion { Id = 1 },
        new Atencion { Id = 2 }
    };
            _atencionRepoMock.Setup(r => r.GetAtencionesPorEstadoAsync(estadosVisibles))
                .ReturnsAsync(atenciones);

            var atencionesDto = new List<AtencionDto>
    {
        new AtencionDto { Id = 1 },
        new AtencionDto { Id = 2 }
    };
            _mapperMock.Setup(m => m.Map<IEnumerable<AtencionDto>>(atenciones)).Returns(atencionesDto);

            // Act
            var result = await _controller.GetAtencionesVisibles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<RespuestaAPI>(okResult.Value);
            Assert.True(response.Ok);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(atencionesDto, response.Result);
        }

        [Fact]
        public async Task GetAtencionesPorRangoFechas_DeberiaRetornarAtenciones()
        {
            // Arrange
            var fechaInicio = new DateTime(2024, 1, 1);
            var fechaFin = new DateTime(2024, 12, 31);
            var page = 1;
            var pageSize = 10;

            var atenciones = new List<Atencion>
    {
        new Atencion { Id = 1, Fecha = new DateTime(2024, 6, 1) },
        new Atencion { Id = 2, Fecha = new DateTime(2024, 6, 15) }
    };

            _atencionRepoMock.Setup(x => x.ObtenerPorFechasAsync(fechaInicio, fechaFin, page, pageSize))
                .ReturnsAsync(atenciones);

            // Act
            var resultado = await _controller.GetAtencionesPorRangoFechas(fechaInicio, fechaFin, page, pageSize);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(okResult.Value);
            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.Equal(atenciones, respuesta.Result);
        }

        [Fact]
        public async Task CrearAtencion_DeberiaRetornarCreatedConRespuestaApiExitosa()
        {
            // Arrange
            var crearDto = new AtencionCrearDto
            {
                // Completa con los campos requeridos para tu DTO
                PacienteId = "123",
                TerceroId = "321",
            };

            var atencionMapeada = new Atencion
            {
                PacienteId = crearDto.PacienteId,
                TerceroId = crearDto.TerceroId,
            };

            _mapperMock.Setup(m => m.Map<Atencion>(crearDto))
                       .Returns(atencionMapeada);

            _atencionRepoMock.Setup(r => r.CrearAtencionAsync(atencionMapeada))
                             .Returns(Task.CompletedTask);


            // Act
            var resultado = await _controller.CrearAtencion(crearDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(resultado);
            Assert.Equal(nameof(_controller.CrearAtencion), createdResult.ActionName);

            var respuesta = Assert.IsType<RespuestaAPI>(createdResult.Value);
            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
            Assert.Equal(atencionMapeada, respuesta.Result);

            _mapperMock.Verify(m => m.Map<Atencion>(crearDto), Times.Once);
            _atencionRepoMock.Verify(r => r.CrearAtencionAsync(atencionMapeada), Times.Once);
        }

        [Fact]
        public async Task EditarAtencion_DeberiaRetornarOkConMensajeExito()
        {
            // Arrange
            var editarDto = new AtencionEditarDto
            {
                AtencionId = 1,
                TerceroId = "123"
            };

            var atencionModificada = new Atencion
            {
                Id = editarDto.AtencionId,
                TerceroId = "123456789" // valor anterior, será reemplazado
            };

            _atencionRepoMock.Setup(r => r.ObtenerAtencionPorIdAsync(editarDto.AtencionId))
                             .ReturnsAsync(atencionModificada);

            _atencionRepoMock.Setup(r => r.EditarAtencionAsync(It.IsAny<Atencion>()))
                             .Returns(Task.CompletedTask);

            // Act
            var resultado = await _controller.EditarAtencion(editarDto);


            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            var respuesta = Assert.IsType<RespuestaAPI>(okResult.Value);
            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.Equal(atencionModificada, respuesta.Result);


            _atencionRepoMock.Verify(r => r.ObtenerAtencionPorIdAsync(editarDto.AtencionId), Times.Once);
            _atencionRepoMock.Verify(r => r.EditarAtencionAsync(It.Is<Atencion>(a => a.TerceroId == editarDto.TerceroId)), Times.Once);
        }

        [Fact]
        public async Task CambiarEstado_DeberiaRetornarOkConRespuestaExitosa()
        {
            // Arrange
            var dto = new AtencionCambioEstadoDto
            {
                AtencionId = 1,
            };

            var respuestaEsperada = new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Result = "Cambio realizado correctamente"
            };

            _cambioEstadoServiceMock.Setup(s => s.CambiarEstadoAsync(dto))
                             .ReturnsAsync(respuestaEsperada);


            // Act
            var resultado = await _controller.CambiarEstado(dto);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(okResult.Value);

            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.Equal(respuestaEsperada.Result, respuesta.Result);

            _cambioEstadoServiceMock.Verify(s => s.CambiarEstadoAsync(dto), Times.Once);
        }



    }

}
