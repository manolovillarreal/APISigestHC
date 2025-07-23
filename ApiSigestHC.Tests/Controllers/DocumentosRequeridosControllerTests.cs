using ApiSigestHC.Controllers;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ApiSigestHC.Tests.Controllers
{
    public class DocumentosRequeridosControllerTests
    {
        private readonly Mock<IDocumentoRequeridoRepositorio> _documentoRequeridoRepoMock;
        private readonly Mock<ICrearDocumentoRequeridoService> _crearDocumentoRequeridoServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly DocumentosRequeridosController _controller;

        public DocumentosRequeridosControllerTests()
        {
            _documentoRequeridoRepoMock = new Mock<IDocumentoRequeridoRepositorio>();
            _crearDocumentoRequeridoServiceMock = new Mock<ICrearDocumentoRequeridoService>();
            _mapperMock = new Mock<IMapper>();

            _controller = new DocumentosRequeridosController(
                _documentoRequeridoRepoMock.Object,
                _crearDocumentoRequeridoServiceMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task ObtenerTodos_DeberiaRetornarOkConDocumentos()
        {
            // Arrange
            var listaDocs = new List<DocumentoRequerido> {
                new DocumentoRequerido { EstadoAtencionId = 2, TipoDocumentoId = 1 },
                new DocumentoRequerido { EstadoAtencionId = 3, TipoDocumentoId = 2 }
            };

                    var listaDto = new List<DocumentoRequeridoDto> {
                new DocumentoRequeridoDto { EstadoAtencionId = 2, TipoDocumentoId = 1 },
                new DocumentoRequeridoDto { EstadoAtencionId = 3, TipoDocumentoId = 2 }
            };

            _documentoRequeridoRepoMock.Setup(r => r.ObtenerTodosAsync())
                .ReturnsAsync(listaDocs);

            _mapperMock.Setup(m => m.Map<IEnumerable<DocumentoRequeridoDto>>(listaDocs))
                .Returns(listaDto);

            // Act
            var resultado = await _controller.ObtenerTodos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(okResult.Value);
            Assert.True(respuesta.Ok);
            Assert.NotNull(respuesta.Result);
        }

        [Fact]
        public async Task ObtenerPorEstado_DeberiaRetornarOkConDocumentos()
        {
            // Arrange
            int estadoId = 3;

            var documentos = new List<DocumentoRequerido>
            {
                new DocumentoRequerido { EstadoAtencionId = estadoId, TipoDocumentoId = 1 },
                new DocumentoRequerido { EstadoAtencionId = estadoId, TipoDocumentoId = 2 }
            };

                    var documentosDto = new List<DocumentoRequeridoDto>
            {
                new DocumentoRequeridoDto { EstadoAtencionId = estadoId, TipoDocumentoId = 1 },
                new DocumentoRequeridoDto { EstadoAtencionId = estadoId, TipoDocumentoId = 2 }
            };

            _documentoRequeridoRepoMock
                .Setup(repo => repo.ObtenerPorEstadoAsync(estadoId))
                .ReturnsAsync(documentos);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<DocumentoRequeridoDto>>(documentos))
                .Returns(documentosDto);

            // Act
            var resultado = await _controller.ObtenerPorEstado(estadoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(okResult.Value);

            Assert.True(respuesta.Ok);
            Assert.NotNull(respuesta.Result);
        }

        [Fact]
        public async Task Crear_DebeRetornar201_CuandoLaOperacionEsExitosa()
        {
            // Arrange
            var dto = new DocumentoRequeridoDto
            {
                EstadoAtencionId = 2,
                TipoDocumentoId = 5
            };

            var respuestaExitosa = new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.Created,
                Result = dto
            };

            _crearDocumentoRequeridoServiceMock
                .Setup(s => s.CrearAsync(dto))
                .ReturnsAsync(respuestaExitosa);

            // Act
            var resultado = await _controller.Crear(dto) as ObjectResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal((int)HttpStatusCode.Created, resultado.StatusCode);

            var respuesta = Assert.IsType<RespuestaAPI>(resultado.Value);
            Assert.True(respuesta.Ok);
            Assert.Equal(dto, respuesta.Result);
        }


    }
}
