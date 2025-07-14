using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApiSigestHC.Tests.Servicios
{
    public class CrearDocumentoRequeridoServiceTests
    {
        private readonly Mock<IDocumentoRequeridoRepositorio> _documentoRequeridoRepoMock;
        private readonly Mock<IEstadoAtencionRepositorio> _estadoAtencionRepoMock;
        private readonly Mock<ITipoDocumentoRepositorio> _tipoDocumentoRepoMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly CrearDocumentoRequeridoService _service;

        public CrearDocumentoRequeridoServiceTests()
        {
            _documentoRequeridoRepoMock = new Mock<IDocumentoRequeridoRepositorio>();
            _estadoAtencionRepoMock = new Mock<IEstadoAtencionRepositorio>();
            _tipoDocumentoRepoMock = new Mock<ITipoDocumentoRepositorio>();
            _mapperMock = new Mock<IMapper>();

            _service = new CrearDocumentoRequeridoService(
                _documentoRequeridoRepoMock.Object,
                _estadoAtencionRepoMock.Object,
                _tipoDocumentoRepoMock.Object,
                _mapperMock.Object
            );
        }

        //1. Prueba de éxito
        [Fact]
        public async Task CrearAsync_DeberiaCrearDocumentoRequerido_CuandoTodoEsValido()
        {
            // Arrange
            var dto = new DocumentoRequeridoDto { EstadoAtencionId = 2, TipoDocumentoId = 10 };

            var estadoMock = new EstadoAtencion { Id = 2, Orden = 2 };
            var tipoMock = new TipoDocumento { Id = 10 };
            var entidad = new DocumentoRequerido { EstadoAtencionId = 2, TipoDocumentoId = 10 };

            _estadoAtencionRepoMock.Setup(r => r.ObtenerPorIdAsync(dto.EstadoAtencionId)).ReturnsAsync(estadoMock);
            _tipoDocumentoRepoMock.Setup(r => r.GetTipoDocumentoPorIdAsync(dto.TipoDocumentoId)).ReturnsAsync(tipoMock);
            _documentoRequeridoRepoMock.Setup(r => r.ExisteAsync(dto.EstadoAtencionId, dto.TipoDocumentoId)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<DocumentoRequerido>(dto)).Returns(entidad);
            _mapperMock.Setup(m => m.Map<DocumentoRequeridoDto>(entidad)).Returns(dto);

            // Act
            var resultado = await _service.CrearAsync(dto);

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.Created, resultado.StatusCode);
            Assert.Equal(dto, resultado.Result);
        }
        //2. Estado no existe
        [Fact]
        public async Task CrearAsync_DeberiaRetornarBadRequest_SiEstadoNoExiste()
        {
            // Arrange
            var dto = new DocumentoRequeridoDto { EstadoAtencionId = 5 };

            _estadoAtencionRepoMock
                .Setup(r => r.ObtenerPorIdAsync(dto.EstadoAtencionId))
                .ReturnsAsync((EstadoAtencion)null);

            // Act
            var resultado = await _service.CrearAsync(dto);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("El estado de atención no existe.", resultado.ErrorMessages);
        }
        //3. Estado tiene orden <= 1
        [Fact]
        public async Task CrearAsync_DeberiaRetornarBadRequest_SiEstadoTieneOrdenMenorIgualA1()
        {
            // Arrange
            var dto = new DocumentoRequeridoDto { EstadoAtencionId = 1 };

            var estado = new EstadoAtencion { Id = 1, Orden = 1 };

            _estadoAtencionRepoMock
                .Setup(r => r.ObtenerPorIdAsync(dto.EstadoAtencionId))
                .ReturnsAsync(estado);

            // Act
            var resultado = await _service.CrearAsync(dto);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("No se pueden registrar documentos requeridos para el estado inicial.", resultado.ErrorMessages);
        }

        //4. Tipo de documento no existe
        [Fact]
        public async Task CrearAsync_DeberiaRetornarBadRequest_SiTipoDocumentoNoExiste()
        {
            // Arrange
            var dto = new DocumentoRequeridoDto { EstadoAtencionId = 2, TipoDocumentoId = 99 };
            var estado = new EstadoAtencion { Id = 2, Orden = 2 };

            _estadoAtencionRepoMock
                .Setup(r => r.ObtenerPorIdAsync(dto.EstadoAtencionId))
                .ReturnsAsync(estado);

            _tipoDocumentoRepoMock
                .Setup(r => r.GetTipoDocumentoPorIdAsync(dto.TipoDocumentoId))
                .ReturnsAsync((TipoDocumento)null);

            // Act
            var resultado = await _service.CrearAsync(dto);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("El tipo de documento no existe.", resultado.ErrorMessages);
        }

        // 5. Ya existe un registro con ese estado y tipo
        [Fact]
        public async Task CrearAsync_DeberiaRetornarBadRequest_SiDocumentoYaExiste()
        {
            // Arrange
            var dto = new DocumentoRequeridoDto { EstadoAtencionId = 2, TipoDocumentoId = 10 };
            var estado = new EstadoAtencion { Id = 2, Orden = 2 };
            var tipo = new TipoDocumento { Id = 10 };

            _estadoAtencionRepoMock
                .Setup(r => r.ObtenerPorIdAsync(dto.EstadoAtencionId))
                .ReturnsAsync(estado);

            _tipoDocumentoRepoMock
                .Setup(r => r.GetTipoDocumentoPorIdAsync(dto.TipoDocumentoId))
                .ReturnsAsync(tipo);

            _documentoRequeridoRepoMock
                .Setup(r => r.ExisteAsync(dto.EstadoAtencionId, dto.TipoDocumentoId))
                .ReturnsAsync(true);

            // Act
            var resultado = await _service.CrearAsync(dto);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("Este documento ya está registrado como requerido.", resultado.ErrorMessages);
        }

        //6. Error inesperado
        [Fact]
        public async Task CrearAsync_DeberiaRetornarInternalServerError_SiOcurreUnaExcepcion()
        {
            // Arrange
            var dto = new DocumentoRequeridoDto { EstadoAtencionId = 2, TipoDocumentoId = 10 };

            _estadoAtencionRepoMock
                .Setup(r => r.ObtenerPorIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Error de base de datos"));

            // Act
            var resultado = await _service.CrearAsync(dto);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.InternalServerError, resultado.StatusCode);
            Assert.Contains("Ocurrió un error inesperado al registrar el documento requerido.", resultado.ErrorMessages);
            Assert.Contains("Error de base de datos", resultado.ErrorMessages);
        }



    }
}
