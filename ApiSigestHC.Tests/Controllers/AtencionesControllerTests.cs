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
        private readonly Mock<IAtencionesService> _atencionesServiceMock;
        private readonly Mock<IUsuarioContextService> _usuarioContextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AtencionesController _controller;

        public AtencionesControllerTests()
        {
            _atencionRepoMock = new Mock<IAtencionRepositorio>();
            _validacionDocServiceMock = new Mock<IValidacionDocumentosObligatoriosService>();
            _visualizacionEstadoServiceMock = new Mock<IVisualizacionEstadoService>();
            _cambioEstadoServiceMock = new Mock<ICambioEstadoService>();
            _atencionesServiceMock = new Mock<IAtencionesService>();
            _usuarioContextMock = new Mock<IUsuarioContextService>();
            _mapperMock = new Mock<IMapper>();

            _controller = new AtencionesController(
                _atencionRepoMock.Object,
                _validacionDocServiceMock.Object,
                _cambioEstadoServiceMock.Object,
                _visualizacionEstadoServiceMock.Object,
                _atencionesServiceMock.Object,
                _usuarioContextMock.Object,
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

            // El endpoint ahora devuelve una página: { data, page, pageSize, total, totalPages }
            var pagina = Assert.IsType<ResultadoPaginadoDto<AtencionDto>>(response.Result);
            Assert.Equal(atencionesDto, pagina.Data);
            Assert.Equal(2, pagina.Total);
            Assert.Equal(1, pagina.Page);
        }

        //[Fact]
        //public async Task GetAtencionesPorRangoFechas_DeberiaRetornarAtenciones()
        //{
        //    // Arrange
        //    var fechaInicio = new DateTime(2024, 1, 1);
        //    var fechaFin = new DateTime(2024, 12, 31);
        //    var page = 1;
        //    var pageSize = 10;
        //
        //    var atenciones = new List<Atencion>
        //    {
        //        new Atencion { Id = 1, Fecha = new DateTime(2024, 6, 1) },
        //        new Atencion { Id = 2, Fecha = new DateTime(2024, 6, 15) }
        //    };
        //
        //    _atencionRepoMock.Setup(x => x.ObtenerPorFechasAsync(fechaInicio, fechaFin, page, pageSize))
        //        .ReturnsAsync(atenciones);
        //
        //    // Act
        //    var resultado = await _controller.GetAtencionesPorRangoFechas(fechaInicio, fechaFin, page, pageSize);
        //
        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(resultado);
        //    var respuesta = Assert.IsType<RespuestaAPI>(okResult.Value);
        //    Assert.True(respuesta.Ok);
        //    Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        //    Assert.Equal(atenciones, respuesta.Result);
        //}

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

            _atencionRepoMock.Setup(r => r.ObtenerAtencionPorIdAsync(It.IsAny<int>()))
                             .ReturnsAsync(atencionMapeada);


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

            var atencionDtoResultado = new AtencionDto
            {
                Id = editarDto.AtencionId,
                TerceroId = editarDto.TerceroId
            };

            _mapperMock.Setup(m => m.Map<AtencionDto>(It.IsAny<Atencion>()))
                       .Returns(atencionDtoResultado);

            // Act
            var resultado = await _controller.EditarAtencion(editarDto.AtencionId, editarDto);


            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            var respuesta = Assert.IsType<RespuestaAPI>(okResult.Value);
            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

            var resultadoDto = Assert.IsType<AtencionDto>(respuesta.Result);
            Assert.Equal(editarDto.TerceroId, resultadoDto.TerceroId);


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

        [Fact]
        public async Task AnularAtencion_AnulacionExitosa_RetornaOk()
        {
            // Arrange
            var dto = new AnulacionAtencionCrearDto
            {
                AtencionId = 1,
                MotivoAnulacionAtencionId = 1,
                Observacion = "Anulación por error"
            };

            var respuestaEsperada = new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Atención anulada correctamente"
            };

            _atencionesServiceMock
                .Setup(s => s.AnularAtencionAsync(dto))
                .ReturnsAsync(respuestaEsperada);

            // Act
            var resultado = await _controller.AnularAtencion(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);

            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.True(respuesta.Ok);
            Assert.Equal("Atención anulada correctamente", respuesta.Message);

            _atencionesServiceMock.Verify(s => s.AnularAtencionAsync(dto), Times.Once);
        }

        [Fact]
        public async Task AnularAtencion_AtencionNoExiste_RetornaNotFound()
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

            _atencionesServiceMock
                .Setup(s => s.AnularAtencionAsync(dto))
                .ReturnsAsync(respuestaError);

            // Act
            var resultado = await _controller.AnularAtencion(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);

            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.False(respuesta.Ok);
            Assert.Contains("no existe", respuesta.ErrorMessages[0]);
        }

        [Fact]
        public async Task AnularAtencion_EstadoIncorrecto_RetornaBadRequest()
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
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessages = new List<string> { "Solo se pueden anular atenciones en estado 'Admisión'." }
            };

            _atencionesServiceMock
                .Setup(s => s.AnularAtencionAsync(dto))
                .ReturnsAsync(respuestaError);

            // Act
            var resultado = await _controller.AnularAtencion(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);

            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.False(respuesta.Ok);
        }

        [Fact]
        public async Task AnularAtencion_YaAnulada_RetornaBadRequest()
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
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessages = new List<string> { "La atención ya ha sido anulada." }
            };

            _atencionesServiceMock
                .Setup(s => s.AnularAtencionAsync(dto))
                .ReturnsAsync(respuestaError);

            // Act
            var resultado = await _controller.AnularAtencion(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);

            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.False(respuesta.Ok);
            Assert.Contains("ya ha sido anulada", respuesta.ErrorMessages[0]);
        }


    }

}
