using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using ApiSigestHC.Controllers;
using Microsoft.AspNetCore.Http;

namespace ApiSigestHC.Tests.Controllers
{
    public class DocumentosControllerTests
    {
        private readonly Mock<IDocumentoService> _documentoServiceMock;
        private readonly Mock<IFormFile> _formFileMock;

        private readonly DocumentosController _controller;

        public DocumentosControllerTests()
        {
            _documentoServiceMock = new Mock<IDocumentoService>();
            _formFileMock = new Mock<IFormFile>();

            _controller = new DocumentosController(
                _documentoServiceMock.Object
            );
        }
        
        
        [Fact]
        public async Task ObtenerDocumentosPorAtencion_RetornaOkConDocumentos()
        {
            // Arrange
            int atencionId = 1;
            var respuestaEsperada = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = new List<DocumentoDto> { new DocumentoDto { Id = 1 } }
            };

            _documentoServiceMock
                .Setup(s => s.ObtenerDocumentosPorAtencionAsync(atencionId))
                .ReturnsAsync(respuestaEsperada);

            // Act
            var resultado = await _controller.ObtenerDocumentosPorAtencion(atencionId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.True(respuesta.IsSuccess);
            Assert.NotNull(respuesta.Result);
        }

        [Fact]
        public async Task ObtenerDocumentosPorAtencion_RetornaErrorSiFallaServicio()
        {
            // Arrange
            int atencionId = 1;
            var respuestaFallida = new RespuestaAPI
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.InternalServerError,
                ErrorMessages = new List<string> { "Error interno" }
            };

            _documentoServiceMock
                .Setup(s => s.ObtenerDocumentosPorAtencionAsync(atencionId))
                .ReturnsAsync(respuestaFallida);

            // Act
            var resultado = await _controller.ObtenerDocumentosPorAtencion(atencionId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.False(respuesta.IsSuccess);
            Assert.Contains("Error interno", respuesta.ErrorMessages);
        }

        [Fact]
        public async Task CargarDocumento_RetornaOkSiCargaExitosa()
        {
            // Arrange
            var dto = new DocumentoCargarDto
            {
                AtencionId = 1,
                TipoDocumentoId = 2,
                Archivo = _formFileMock.Object // Usa un mock válido
            };

            var respuestaEsperada = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = new DocumentoDto { Id = 10 }
            };

            _documentoServiceMock
                .Setup(s => s.CargarDocumentoAsync(dto))
                .ReturnsAsync(respuestaEsperada);

            // Act
            var resultado = await _controller.CargarDocumento(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.True(respuesta.IsSuccess);
            Assert.IsType<DocumentoDto>(respuesta.Result);
        }

        [Fact]
        public async Task EditarDocumento_RetornaOkSiEdicionExitosa()
        {
            // Arrange
            var dto = new DocumentoEditarDto
            {
                Id = 5,
                NumeroRelacion = "REL-001",
                Observacion = "Edición de prueba",
                Fecha = new DateTime(2024, 1, 1)
            };

            var respuestaEsperada = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = new DocumentoDto { Id = 5 }
            };

            _documentoServiceMock
                .Setup(s => s.EditarDocumentoAsync(dto))
                .ReturnsAsync(respuestaEsperada);

            // Act
            var resultado = await _controller.EditarDocumento(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.True(respuesta.IsSuccess);
            Assert.IsType<DocumentoDto>(respuesta.Result);
        }

        [Fact]
        public async Task ReemplazarDocumento_RetornaOkSiReemplazoExitoso()
        {
            // Arrange
            var dto = new DocumentoReemplazarDto
            {
                Id = 7,
                Archivo = _formFileMock.Object
            };

            var respuestaEsperada = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = "Documento reemplazado exitosamente"
            };

            _documentoServiceMock
                .Setup(s => s.ReemplazarDocumentoAsync(dto))
                .ReturnsAsync(respuestaEsperada);

            // Act
            var resultado = await _controller.ReemplazarDocumento(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.True(respuesta.IsSuccess);
            Assert.Equal("Documento reemplazado exitosamente", respuesta.Result);
        }

        [Fact]
        public async Task CorregirDocumento_RetornaOkSiCorreccionExitosa()
        {
            // Arrange
            var dto = new DocumentoReemplazarDto
            {
                Id = 22,
                Archivo = _formFileMock.Object
            };

            var respuestaEsperada = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = "Corrección aplicada exitosamente."
            };

            _documentoServiceMock
                .Setup(s => s.CorregirDocumentoAsync(dto))
                .ReturnsAsync(respuestaEsperada);

            // Act
            var resultado = await _controller.CorregirDocumento(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var respuesta = Assert.IsType<RespuestaAPI>(objectResult.Value);
            Assert.True(respuesta.IsSuccess);
            Assert.Equal("Corrección aplicada exitosamente.", respuesta.Result);
        }

        [Fact]
        public async Task DescargarDocumento_RetornaFileStreamResult_SiExitoso()
        {
            // Arrange
            int documentoId = 99;

            var fileStream = new MemoryStream();
            var fileStreamResult = new FileStreamResult(fileStream, "application/pdf")
            {
                FileDownloadName = "archivo.pdf"
            };

            _documentoServiceMock
                .Setup(s => s.DescargarDocumentoAsync(documentoId))
                .ReturnsAsync(fileStreamResult);

            // Act
            var resultado = await _controller.DescargarDocumento(documentoId);

            // Assert
            var result = Assert.IsType<FileStreamResult>(resultado);
            Assert.Equal("application/pdf", result.ContentType);
            Assert.Equal("archivo.pdf", result.FileDownloadName);
        }

        [Fact]
        public async Task VerDocumento_RetornaFileStreamResult_SiExitoso()
        {
            // Arrange
            int documentoId = 88;

            var stream = new MemoryStream();
            var fileResult = new FileStreamResult(stream, "application/pdf");

            _documentoServiceMock
                .Setup(s => s.VerDocumentoAsync(documentoId))
                .ReturnsAsync(fileResult);

            // Act
            var resultado = await _controller.VerDocumento(documentoId);

            // Assert
            var result = Assert.IsType<FileStreamResult>(resultado);
            Assert.Equal("application/pdf", result.ContentType);
        }

        [Fact]
        public async Task EliminarDocumento_RetornaOk_SiEliminacionExitosa()
        {
            // Arrange
            int documentoId = 77;

            var respuesta = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = $"Documento con id {documentoId} eliminado correctamente"
            };

            _documentoServiceMock
                .Setup(s => s.EliminarDocumentoAsync(documentoId))
                .ReturnsAsync(respuesta);

            // Act
            var resultado = await _controller.EliminarDocumento(documentoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var respuestaAPI = Assert.IsType<RespuestaAPI>(okResult.Value);
            Assert.True(respuestaAPI.IsSuccess);
            Assert.Equal($"Documento con id {documentoId} eliminado correctamente", respuestaAPI.Result);
        }


    }
}
