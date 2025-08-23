using Xunit;
using Moq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using ApiSigestHC.Servicios;
using AutoMapper;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace ApiSigestHC.Tests.Servicios
{
    public class DocumentoServiceTests
    {
        private readonly Mock<IAlmacenamientoArchivoService> _almacenamientoArchivoServiceMock;
        private readonly Mock<ISolicitudCorreccionRepositorio> _solicitudCorreccionRepoMock;
        private readonly Mock<IValidacionCargaArchivoService> _validacionCargaDocumentoServiceMock;
        private readonly Mock<IDocumentoRepositorio> _documentoRepoMock;
        private readonly Mock<ITipoDocumentoRolRepositorio> _tipoDocumentoRolRepoMock;
        private readonly Mock<IUsuarioContextService> _usuarioContextServiceMock;
        private readonly Mock<IFormFile> _formFileMock;

        private readonly Mock<IMapper> _mapperMock;

        private readonly DocumentoService _documentoService;

        public DocumentoServiceTests()
        {
            _almacenamientoArchivoServiceMock = new Mock<IAlmacenamientoArchivoService>();
            _solicitudCorreccionRepoMock = new Mock<ISolicitudCorreccionRepositorio>();
            _validacionCargaDocumentoServiceMock = new Mock<IValidacionCargaArchivoService>();
            _documentoRepoMock = new Mock<IDocumentoRepositorio>();
            _tipoDocumentoRolRepoMock = new Mock<ITipoDocumentoRolRepositorio>();
            _usuarioContextServiceMock = new Mock<IUsuarioContextService>();
            _formFileMock = new Mock<IFormFile>();
            _mapperMock = new Mock<IMapper>();

            _documentoService = new DocumentoService(
                _almacenamientoArchivoServiceMock.Object,
                _solicitudCorreccionRepoMock.Object,
                _validacionCargaDocumentoServiceMock.Object,
                _documentoRepoMock.Object,
                _tipoDocumentoRolRepoMock.Object,
                _usuarioContextServiceMock.Object,
                _mapperMock.Object
            );

            SetFormFileMock();
        }
        #region Obtener Documentos

        [Fact]
        public async Task ObtenerDocumentosPorAtencionAsync_DebeRetornarDocumentos_CuandoExitoso()
        {
            // Arrange
            int atencionId = 1;
            int rolId = 2;
            var documentos = new List<Documento>
            {
                new Documento { Id = 10, AtencionId = atencionId, TipoDocumentoId = 3 }
            };

                    var documentosDto = new List<DocumentoDto>
            {
                new DocumentoDto { Id = 10, TipoDocumentoId = 3 }
            };

            _usuarioContextServiceMock.Setup(x => x.ObtenerRolId()).Returns(rolId);
            _documentoRepoMock.Setup(x => x.ObtenerPermitidosParaCargar(atencionId, rolId))
                              .ReturnsAsync(documentos);
            _mapperMock.Setup(x => x.Map<IEnumerable<DocumentoDto>>(documentos)).Returns(documentosDto);

            // Act
            var resultado = await _documentoService.ObtenerDocumentosPorAtencionAsync(atencionId);

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.Equal(documentosDto, resultado.Result);
        }

        [Fact]
        public async Task ObtenerDocumentosPorAtencionAsync_DebeRetornarErrorInterno_CuandoExcepcion()
        {
            // Arrange
            int atencionId = 1;
            _usuarioContextServiceMock.Setup(x => x.ObtenerRolId()).Throws(new Exception("Fallo interno"));

            // Act
            var resultado = await _documentoService.ObtenerDocumentosPorAtencionAsync(atencionId);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, resultado.StatusCode);
            Assert.Contains("Error interno al obtener los documentos.", resultado.ErrorMessages);
            Assert.Contains("Fallo interno", resultado.ErrorMessages.Last());
        }


        #endregion

        #region CargarDocumento

        [Fact]
        public async Task CargarDocumentoAsync_DeberiaRetornarRespuestaExitosa()
        {
            // Arrange
            var dto = new DocumentoCargarDto
            {
                AtencionId = 1,
                TipoDocumentoId = 2,
                Fecha = new DateTime(2025, 6, 18),
                NumeroRelacion = "123",
                Observacion = "Observación",
                Archivo = _formFileMock.Object
            };

            var resultadoGuardado = new ResultadoGuardadoArchivo
            {
                RutaBase = "/base",
                RutaRelativa = "relativa/ruta",
                NombreArchivo = "DOC_123.pdf"
            };

            var documentoMapeado = new DocumentoDto
            {
                Id = 99,
                AtencionId = 1,
                TipoDocumentoId = 2,
                Fecha = new DateTime(2025, 6, 18),
                NumeroRelacion = "123",
                Observacion = "Observación",
            };

            _validacionCargaDocumentoServiceMock
                .Setup(v => v.ValidarCargaDocumentoAsync(dto))
                .ReturnsAsync(new RespuestaAPI { Ok = true });

            _almacenamientoArchivoServiceMock
                .Setup(s => s.GuardarArchivoAsync(dto))
                .ReturnsAsync(resultadoGuardado);

            _usuarioContextServiceMock
                .Setup(u => u.ObtenerUsuarioId())
                .Returns(10);

            _mapperMock
                .Setup(m => m.Map<DocumentoDto>(It.IsAny<Documento>()))
                .Returns(documentoMapeado);

            // Act
            var resultado = await _documentoService.CargarDocumentoAsync(dto);

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.Equal(documentoMapeado, resultado.Result);

            _validacionCargaDocumentoServiceMock.Verify(v => v.ValidarCargaDocumentoAsync(dto), Times.Once);
            _almacenamientoArchivoServiceMock.Verify(s => s.GuardarArchivoAsync(dto), Times.Once);
            _documentoRepoMock.Verify(r => r.GuardarAsync(It.Is<Documento>(d =>
                d.AtencionId == dto.AtencionId &&
                d.TipoDocumentoId == dto.TipoDocumentoId &&
                d.NombreArchivo == resultadoGuardado.NombreArchivo &&
                d.RutaBase == resultadoGuardado.RutaBase &&
                d.RutaRelativa == resultadoGuardado.RutaRelativa
            )), Times.Once);
        }

        [Fact]
        public async Task CargarDocumentoAsync_ValidacionFalla_RetornaRespuestaDeError()
        {
            // Arrange
            var dto = new DocumentoCargarDto
            {
                AtencionId = 1,
                TipoDocumentoId = 1,
                Fecha = DateTime.UtcNow,
                NumeroRelacion = "001",
                Observacion = "Observación prueba",
                Archivo = _formFileMock.Object
            };

            var respuestaError = new RespuestaAPI
            {
                Ok = false,
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessages = new List<string> { "Falta el campo obligatorio: TipoDocumentoId" }
            };

            _validacionCargaDocumentoServiceMock
                .Setup(v => v.ValidarCargaDocumentoAsync(dto))
                .ReturnsAsync(respuestaError);

            // Act
            var resultado = await _documentoService.CargarDocumentoAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("Falta el campo obligatorio", resultado.ErrorMessages.First());

            _validacionCargaDocumentoServiceMock.Verify(v => v.ValidarCargaDocumentoAsync(dto), Times.Once);
            _almacenamientoArchivoServiceMock.Verify(g => g.GuardarArchivoAsync(It.IsAny<DocumentoCargarDto>()), Times.Never);
            _documentoRepoMock.Verify(r => r.GuardarAsync(It.IsAny<Documento>()), Times.Never);
        }

        [Fact]
        public async Task CargarDocumentoAsync_GuardarArchivoLanzaExcepcion_RetornaRespuestaError()
        {
            // Arrange
            var dto = new DocumentoCargarDto
            {
                AtencionId = 1,
                TipoDocumentoId = 2,
                Fecha = DateTime.UtcNow,
                NumeroRelacion = "123",
                Observacion = "Observación de prueba",
                Archivo = _formFileMock.Object
            };

            _validacionCargaDocumentoServiceMock
                .Setup(v => v.ValidarCargaDocumentoAsync(dto))
                .ReturnsAsync(new RespuestaAPI { Ok = true });

            _almacenamientoArchivoServiceMock
                .Setup(a => a.GuardarArchivoAsync(dto))
                .ThrowsAsync(new Exception("Fallo en el disco"));

            // Act
            var respuesta = await _documentoService.CargarDocumentoAsync(dto);

            // Assert
            Assert.NotNull(respuesta);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, respuesta.StatusCode);
            Assert.Contains("Error interno al editar el documento.", respuesta.ErrorMessages);
            Assert.Contains("Fallo en el disco", respuesta.ErrorMessages);

            _documentoRepoMock.Verify(r => r.GuardarAsync(It.IsAny<Documento>()), Times.Never);
        }

        #endregion

        #region Editar Documento

        [Fact]
        public async Task EditarDocumentoAsync_DocumentoExiste_DevuelveRespuestaExitosa()
        {
            // Arrange
            var dto = new DocumentoEditarDto
            {
                Id = 1,
                NumeroRelacion = "REL-123",
                Observacion = "Actualización",
                Fecha = new DateTime(2025, 6, 1)
            };

            var documento = new Documento
            {
                Id = 1,
                NombreArchivo = "viejo_nombre.pdf",
                TipoDocumentoId = 2,
                RutaBase = "/base",
                RutaRelativa = "/rel",
                NumeroRelacion = "REL-001",
                Observacion = "obs vieja",
                Fecha = new DateTime(2025, 1, 1)
            };

            var resultadoRenombrado = new ResultadoGuardadoArchivo
            {
                NombreArchivo = "viejo_nombre.pdf",
                RutaBase = "/base",
                RutaRelativa = "/rel",
            };

            var documentoDto = new DocumentoDto
            {
                Id = 1
            };

            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(dto.Id))
                              .ReturnsAsync(documento);

            _almacenamientoArchivoServiceMock.Setup(s => s.ActualizarNombreSiEsNecesarioAsync(dto))
                                             .ReturnsAsync(resultadoRenombrado);

            _mapperMock.Setup(m => m.Map<DocumentoDto>(It.IsAny<Documento>()))
                       .Returns(documentoDto);

            // Act
            var respuesta = await _documentoService.EditarDocumentoAsync(dto);

            // Assert
            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.NotNull(respuesta.Result);
            var resultadoDto = Assert.IsType<DocumentoDto>(respuesta.Result);

            _documentoRepoMock.Verify(r => r.ObtenerPorIdAsync(dto.Id), Times.Once);
            _documentoRepoMock.Verify(r => r.ActualizarAsync(It.Is<Documento>(d =>
                d.Observacion == dto.Observacion &&
                d.NumeroRelacion == dto.NumeroRelacion &&
                d.NombreArchivo == resultadoRenombrado.NombreArchivo
            )), Times.Once);
        }
        [Fact]
        public async Task EditarDocumentoAsync_DocumentoNoExiste_RetornaNotFound()
        {
            // Arrange
            var dto = new DocumentoEditarDto
            {
                Id = 999,
                NumeroRelacion = "REL-X",
                Observacion = "Observación",
                Fecha = DateTime.UtcNow
            };

            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(dto.Id))
                              .ReturnsAsync((Documento)null); // Simula documento inexistente

            // Act
            var respuesta = await _documentoService.EditarDocumentoAsync(dto);

            // Assert
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
            Assert.Contains("Documento con id", respuesta.ErrorMessages.First());

            _documentoRepoMock.Verify(r => r.ObtenerPorIdAsync(dto.Id), Times.Once);
            _documentoRepoMock.Verify(r => r.ActualizarAsync(It.IsAny<Documento>()), Times.Never);
            _almacenamientoArchivoServiceMock.Verify(s => s.ActualizarNombreSiEsNecesarioAsync(It.IsAny<DocumentoEditarDto>()), Times.Never);
        }

        [Fact]
        public async Task EditarDocumentoAsync_ExcepcionInesperada_RetornaError500()
        {
            // Arrange
            var dto = new DocumentoEditarDto
            {
                Id = 1,
                NumeroRelacion = "REL-X",
                Observacion = "Observación",
                Fecha = DateTime.UtcNow
            };
            var documento = new Documento
            {
                Id = 1,
                NombreArchivo = "viejo_nombre.pdf",
                TipoDocumentoId = 2,
                RutaBase = "/base",
                RutaRelativa = "/rel",
                NumeroRelacion = "REL-001",
                Observacion = "obs vieja",
                Fecha = new DateTime(2025, 1, 1)
            };
            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(dto.Id))
                             .ReturnsAsync(documento);
            _almacenamientoArchivoServiceMock.Setup(s => s.ActualizarNombreSiEsNecesarioAsync(dto))
                                             .ThrowsAsync(new Exception("Error inesperado"));

            // Act
            var respuesta = await _documentoService.EditarDocumentoAsync(dto);

            // Assert
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, respuesta.StatusCode);
            Assert.Contains("Error interno al editar el documento.", respuesta.ErrorMessages.First());

            _documentoRepoMock.Verify(r => r.ObtenerPorIdAsync(dto.Id), Times.Once);
            _documentoRepoMock.Verify(r => r.ActualizarAsync(It.IsAny<Documento>()), Times.Never);
            _almacenamientoArchivoServiceMock.Verify(s => s.ActualizarNombreSiEsNecesarioAsync(It.IsAny<DocumentoEditarDto>()), Times.Once);
        }

        #endregion

        #region Reemplazar Documento

        [Fact]
        public async Task ReemplazarDocumentoAsync_Exito_RetornaOk()
        {
            // Arrange
            var dto = new DocumentoReemplazarDto
            {
                Id = 1,
                Archivo = _formFileMock.Object
            };

            var resultadoGuardado = new ResultadoGuardadoArchivo
            {
                RutaBase = "/base",
                RutaRelativa = "relativa/ruta",
                NombreArchivo = "DOC_123.pdf"
            };

            var documento = new Documento { Id = dto.Id };

            _validacionCargaDocumentoServiceMock
                .Setup(v => v.ValidarReemplazoDocumentoAsync(dto))
                .ReturnsAsync(new RespuestaAPI { Ok = true });

            _documentoRepoMock
                .Setup(r => r.ObtenerPorIdAsync(dto.Id))
                .ReturnsAsync(documento);


            _almacenamientoArchivoServiceMock
                .Setup(s => s.ReemplazarArchivoCorreccionAsync(documento, dto.Archivo))
                .ReturnsAsync(resultadoGuardado);

            _usuarioContextServiceMock
                .Setup(u => u.ObtenerUsuarioId())
                .Returns(99);

            // Act
            var respuesta = await _documentoService.ReemplazarDocumentoCorreccionAsync(dto);

            // Assert
            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

            _documentoRepoMock.Verify(r => r.ObtenerPorIdAsync(dto.Id), Times.Once);
            _almacenamientoArchivoServiceMock.Verify(a => a.ReemplazarArchivoCorreccionAsync(documento, dto.Archivo), Times.Once);
            _documentoRepoMock.Verify(r => r.ActualizarAsync(It.Is<Documento>(d => d.UsuarioId == 99)), Times.Once);
        }

        [Fact]
        public async Task ReemplazarDocumentoAsync_DocumentoNoEncontrado_RetornaNotFound()
        {
            // Arrange
            var dto = new DocumentoReemplazarDto
            {
                Id = 999,
                Archivo = _formFileMock.Object
            };

            _validacionCargaDocumentoServiceMock
                .Setup(v => v.ValidarReemplazoDocumentoAsync(dto))
                .ReturnsAsync(new RespuestaAPI { Ok = true });

            _documentoRepoMock
                .Setup(r => r.ObtenerPorIdAsync(dto.Id))
                .ReturnsAsync((Documento)null!);

            // Act
            var respuesta = await _documentoService.ReemplazarDocumentoCorreccionAsync(dto);

            // Assert
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
            Assert.Contains("no encontrado", respuesta.ErrorMessages.First().ToLower());
            _almacenamientoArchivoServiceMock.Verify(a => a.ReemplazarArchivoCorreccionAsync((Documento)null, dto.Archivo), Times.Never);

        }

        [Fact]
        public async Task ReemplazarDocumentoAsync_ValidacionFalla_RetornaError()
        {
            // Arrange
            var dto = new DocumentoReemplazarDto { Id = 1, Archivo = _formFileMock.Object };

            var respuestaError = new RespuestaAPI
            {
                Ok = false,
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessages = new List<string> { "Archivo inválido" }
            };

            _validacionCargaDocumentoServiceMock
                .Setup(v => v.ValidarReemplazoDocumentoAsync(dto))
                .ReturnsAsync(respuestaError);

            // Act
            var respuesta = await _documentoService.ReemplazarDocumentoCorreccionAsync(dto);

            // Assert
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
            Assert.Contains("archivo inválido", respuesta.ErrorMessages.First().ToLower());
        }

        [Fact]
        public async Task ReemplazarDocumentoAsync_LanzaArgumentException_RetornaBadRequest()
        {
            // Arrange
            var dto = new DocumentoReemplazarDto { Id = 1, Archivo = _formFileMock.Object };
            var documento = new Documento { Id = 1 };

            _validacionCargaDocumentoServiceMock
                .Setup(v => v.ValidarReemplazoDocumentoAsync(dto))
                .ReturnsAsync(new RespuestaAPI { Ok = true });

            _documentoRepoMock
                .Setup(r => r.ObtenerPorIdAsync(dto.Id))
                .ReturnsAsync(documento);

            _almacenamientoArchivoServiceMock
                .Setup(a => a.ReemplazarArchivoCorreccionAsync(documento, dto.Archivo))
                .ThrowsAsync(new ArgumentException("Archivo no válido"));

            // Act
            var respuesta = await _documentoService.ReemplazarDocumentoCorreccionAsync(dto);

            // Assert
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
            Assert.Contains("Archivo no válido", respuesta.ErrorMessages.First());
        }

        [Fact]
        public async Task ReemplazarDocumentoAsync_CuandoOcurreExcepcionInterna_DeberiaRetornarRespuestaError500()
        {
            // Arrange
            var dto = new DocumentoReemplazarDto
            {
                Id = 99,
                Archivo = _formFileMock.Object
            };

            _validacionCargaDocumentoServiceMock.Setup(s => s.ValidarReemplazoDocumentoAsync(dto))
                .ReturnsAsync(new RespuestaAPI { Ok = true });

            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(dto.Id))
                .ReturnsAsync(new Documento());

            _almacenamientoArchivoServiceMock.Setup(s => s.ReemplazarArchivoCorreccionAsync(It.IsAny<Documento>(), dto.Archivo))
                .ThrowsAsync(new Exception("Error al reemplazar el documento"));

            // Act
            var resultado = await _documentoService.ReemplazarDocumentoCorreccionAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, resultado.StatusCode);
            Assert.Contains("Error al reemplazar el documento.", resultado.ErrorMessages);
            //Assert.Contains("Fallo inesperado", resultado.ErrorMessages[1]);
        }



        #endregion

        #region Coregir Documento

        [Fact]
        public async Task CorregirDocumentoAsync_CuandoTodoEsCorrecto_DeberiaRetornarOk()
        {
            // Arrange
            var dto = new DocumentoReemplazarDto { Id = 1, Archivo = _formFileMock.Object };
            var documento = new Documento { Id = 1 };
            var correccion = new SolicitudCorreccion { Id = 5, DocumentoId = 1, Pendiente = true };

            var resultadoGuardado = new ResultadoGuardadoArchivo
            {
                RutaBase = "/base",
                RutaRelativa = "relativa/ruta",
                NombreArchivo = "DOC_123.pdf"
            };

            _solicitudCorreccionRepoMock.Setup(r => r.ObtenerPendientePorDocumentoIdAsync(dto.Id))
                .ReturnsAsync(correccion);

            _validacionCargaDocumentoServiceMock.Setup(s => s.ValidarReemplazoDocumentoAsync(dto))
                .ReturnsAsync(new RespuestaAPI { Ok = true });

            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(dto.Id))
                .ReturnsAsync(documento);

            _almacenamientoArchivoServiceMock.Setup(s => s.ReemplazarArchivoCorreccionAsync(documento, dto.Archivo))
                .ReturnsAsync(resultadoGuardado);

            _usuarioContextServiceMock.Setup(s => s.ObtenerUsuarioId()).Returns(10);

            // Act
            var resultado = await _documentoService.CorregirDocumentoAsync(dto);

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.Equal("Corrección aplicada exitosamente.", resultado.Result);
        }


        [Fact]
        public async Task CorregirDocumentoAsync_SinCorreccionPendiente_DeberiaRetornarBadRequest()
        {
            // Arrange
            var dto = new DocumentoReemplazarDto { Id = 1 };
            _solicitudCorreccionRepoMock
                .Setup(r => r.ObtenerPendientePorDocumentoIdAsync(dto.Id))
                .ReturnsAsync((SolicitudCorreccion)null);

            // Act
            var resultado = await _documentoService.CorregirDocumentoAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("solicitud de corrección pendiente", resultado.ErrorMessages.First());
        }

        [Fact]
        public async Task CorregirDocumentoAsync_ValidacionReemplazoInvalida_DeberiaRetornarErrorValidacion()
        {
            // Arrange
            var dto = new DocumentoReemplazarDto { Id = 1 };
            var correccion = new SolicitudCorreccion { Id = 99, DocumentoId = 1, Pendiente = true };

            _solicitudCorreccionRepoMock
                .Setup(r => r.ObtenerPendientePorDocumentoIdAsync(dto.Id))
                .ReturnsAsync(correccion);

            _validacionCargaDocumentoServiceMock
                .Setup(s => s.ValidarReemplazoDocumentoAsync(dto))
                .ReturnsAsync(new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "No puede reemplazar en este estado." }
                });

            // Act
            var resultado = await _documentoService.CorregirDocumentoAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("No puede reemplazar en este estado.", resultado.ErrorMessages);
        }

        [Fact]
        public async Task CorregirDocumentoAsync_DocumentoNoExiste_DeberiaRetornarNotFound()
        {
            // Arrange
            var dto = new DocumentoReemplazarDto { Id = 1 };
            var correccion = new SolicitudCorreccion { Id = 1, DocumentoId = 1, Pendiente = true };

            _solicitudCorreccionRepoMock.Setup(r => r.ObtenerPendientePorDocumentoIdAsync(dto.Id))
                .ReturnsAsync(correccion);

            _validacionCargaDocumentoServiceMock.Setup(s => s.ValidarReemplazoDocumentoAsync(dto))
                .ReturnsAsync(new RespuestaAPI { Ok = true });

            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(dto.Id))
                .ReturnsAsync((Documento)null);

            // Act
            var resultado = await _documentoService.CorregirDocumentoAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.NotFound, resultado.StatusCode);
            Assert.Contains("Documento con id 1 no encontrado.", resultado.ErrorMessages.First());
        }


        #endregion

        #region Eliminar Documento

        [Fact]
        public async Task EliminarDocumentoAsync_DocumentoNoExiste_DeberiaRetornarNotFound()
        {
            // Arrange
            int documentoId = 123;
            _documentoRepoMock
                .Setup(r => r.ObtenerPorIdAsync(documentoId))
                .ReturnsAsync((Documento)null);

            // Act
            var resultado = await _documentoService.EliminarDocumentoAsync(documentoId);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.NotFound, resultado.StatusCode);
            Assert.Contains($"No se encontró el documento con id {documentoId}", resultado.ErrorMessages);
        }

        [Fact]
        public async Task EliminarDocumentoAsync_DocumentoExiste_DeberiaEliminarYRetornarOk()
        {
            // Arrange
            int documentoId = 123;
            var documento = new Documento { Id = documentoId };

            _documentoRepoMock
                .Setup(r => r.ObtenerPorIdAsync(documentoId))
                .ReturnsAsync(documento);

            _almacenamientoArchivoServiceMock
                .Setup(s => s.EliminarArchivoAsync(documento))
                .Returns(Task.CompletedTask);

            _documentoRepoMock
                .Setup(r => r.EliminarAsync(documento))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _documentoService.EliminarDocumentoAsync(documentoId);

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.Equal($"Documento con id {documentoId} eliminado correctamente", resultado.Result);
        }

        [Fact]
        public async Task EliminarDocumentoAsync_CuandoLanzaExcepcion_DeberiaRetornarError500()
        {
            // Arrange
            int documentoId = 123;
            var documento = new Documento { Id = documentoId };

            _documentoRepoMock
                .Setup(r => r.ObtenerPorIdAsync(documentoId))
                .ReturnsAsync(documento);

            _almacenamientoArchivoServiceMock
                .Setup(s => s.EliminarArchivoAsync(documento))
                .ThrowsAsync(new Exception("Error crítico al eliminar archivo"));

            // Act
            var resultado = await _documentoService.EliminarDocumentoAsync(documentoId);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, resultado.StatusCode);
            Assert.Contains("Error al eliminar el documento.", resultado.ErrorMessages);
            Assert.Contains("Error crítico al eliminar archivo", resultado.ErrorMessages[1]);
        }


        #endregion

        #region Descargar Documento

        [Fact]
        public async Task DescargarDocumentoAsync_DocumentoNoExiste_RetornaNotFound()
        {
            // Arrange
            int documentoId = 1;
            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(documentoId))
                              .ReturnsAsync((Documento)null);

            // Act
            var resultado = await _documentoService.DescargarDocumentoAsync(documentoId);

            // Assert
            var objeto = Assert.IsType<NotFoundObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(objeto.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        }

        [Fact]
        public async Task DescargarDocumentoAsync_SinPermisoParaVerDocumento_RetornaForbidden()
        {
            // Arrange
            int documentoId = 1;
            var documento = new Documento { Id = documentoId, TipoDocumentoId = 10 };

            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(documentoId))
                              .ReturnsAsync(documento);

            _usuarioContextServiceMock.Setup(u => u.ObtenerRolId()).Returns(3);
            _documentoRepoMock.Setup(r => r.PuedeVerDocumento(3, documento.TipoDocumentoId))
                              .ReturnsAsync(false);

            // Act
            var resultado = await _documentoService.DescargarDocumentoAsync(documentoId);

            // Assert
            var forbidden = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
            var respuesta = Assert.IsType<RespuestaAPI>(forbidden.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        }

        [Fact]
        public async Task DescargarDocumentoAsync_ArchivoNoExiste_RetornaNotFound()
        {
            // Arrange
            int documentoId = 1;
            var documento = new Documento { Id = documentoId, TipoDocumentoId = 5 };

            _usuarioContextServiceMock.Setup(u => u.ObtenerRolId()).Returns(2);

            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(documentoId))
                              .ReturnsAsync(documento);

            _documentoRepoMock.Setup(r => r.PuedeVerDocumento(2, documento.TipoDocumentoId))
                              .ReturnsAsync(true);

            _almacenamientoArchivoServiceMock.Setup(s => s.DescargarDocumentoAsync(documento))
                                      .ReturnsAsync((FileStreamResult)null);

            // Act
            var resultado = await _documentoService.DescargarDocumentoAsync(documentoId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(notFound.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        }

        [Fact]
        public async Task DescargarDocumentoAsync_ConPermisosYArchivoExiste_RetornaFileStream()
        {
            // Arrange
            int documentoId = 1;
            var documento = new Documento { Id = documentoId, TipoDocumentoId = 5 };
            var stream = new MemoryStream();
            var fileResult = new FileStreamResult(stream, "application/pdf");

            _usuarioContextServiceMock.Setup(u => u.ObtenerRolId()).Returns(1);

            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(documentoId))
                              .ReturnsAsync(documento);

            _documentoRepoMock.Setup(r => r.PuedeVerDocumento(1, documento.TipoDocumentoId))
                              .ReturnsAsync(true);

            _almacenamientoArchivoServiceMock.Setup(s => s.DescargarDocumentoAsync(documento))
                                      .ReturnsAsync(fileResult);

            // Act
            var resultado = await _documentoService.DescargarDocumentoAsync(documentoId);

            // Assert
            var archivo = Assert.IsType<FileStreamResult>(resultado);
            Assert.Equal("application/pdf", archivo.ContentType);
        }

        [Fact]
        public async Task DescargarDocumentoAsync_ExcepcionLanzada_RetornaBadRequest()
        {
            // Arrange
            int documentoId = 1;

            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(documentoId))
                              .ThrowsAsync(new Exception("Excepción de prueba"));

            // Act
            var resultado = await _documentoService.DescargarDocumentoAsync(documentoId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(badRequest.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
            Assert.Contains("Ocurrió un error al intentar descargar el documento.", respuesta.ErrorMessages[0]);
        }


        #endregion

        #region Ver Documento

        [Fact]
        public async Task VerDocumentoAsync_DocumentoNoExiste_RetornaNotFound()
        {
            // Arrange
            int documentoId = 1;
            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(documentoId))
                              .ReturnsAsync((Documento)null);

            // Act
            var resultado = await _documentoService.VerDocumentoAsync(documentoId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(notFound.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
            Assert.Contains("Documento no encontrado", respuesta.ErrorMessages.First());
        }

        [Fact]
        public async Task VerDocumentoAsync_SinPermiso_RetornaForbidden()
        {
            // Arrange
            int documentoId = 1;
            var doc = new Documento { Id = documentoId, TipoDocumentoId = 2 };

            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(documentoId)).ReturnsAsync(doc);
            _usuarioContextServiceMock.Setup(u => u.ObtenerRolId()).Returns(5);
            _documentoRepoMock.Setup(r => r.PuedeVerDocumento(5, doc.TipoDocumentoId)).ReturnsAsync(false);

            // Act
            var resultado = await _documentoService.VerDocumentoAsync(documentoId);

            // Assert
            var forbidden = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
            var respuesta = Assert.IsType<RespuestaAPI>(forbidden.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        }

        [Fact]
        public async Task VerDocumentoAsync_ArchivoNoExiste_RetornaNotFound()
        {
            // Arrange
            int documentoId = 2;
            var doc = new Documento { Id = documentoId, TipoDocumentoId = 10 };

            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(documentoId)).ReturnsAsync(doc);
            _usuarioContextServiceMock.Setup(u => u.ObtenerRolId()).Returns(1);
            _documentoRepoMock.Setup(r => r.PuedeVerDocumento(1, doc.TipoDocumentoId)).ReturnsAsync(true);
            _almacenamientoArchivoServiceMock.Setup(s => s.ObtenerArchivoParaVisualizacionAsync(doc)).ReturnsAsync((FileStreamResult)null);

            // Act
            var resultado = await _documentoService.VerDocumentoAsync(documentoId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(notFound.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
            Assert.Contains("El archivo físico no fue encontrado", respuesta.ErrorMessages.First());
        }

        [Fact]
        public async Task VerDocumentoAsync_ConPermisosYArchivoExiste_RetornaFileStreamResult()
        {
            // Arrange
            int documentoId = 3;
            var doc = new Documento { Id = documentoId, TipoDocumentoId = 4 };
            var stream = new MemoryStream();
            var fileResult = new FileStreamResult(stream, "application/pdf");

            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(documentoId)).ReturnsAsync(doc);
            _usuarioContextServiceMock.Setup(u => u.ObtenerRolId()).Returns(1);
            _documentoRepoMock.Setup(r => r.PuedeVerDocumento(1, doc.TipoDocumentoId)).ReturnsAsync(true);
            _almacenamientoArchivoServiceMock.Setup(s => s.ObtenerArchivoParaVisualizacionAsync(doc)).ReturnsAsync(fileResult);

            // Act
            var resultado = await _documentoService.VerDocumentoAsync(documentoId);

            // Assert
            var archivo = Assert.IsType<FileStreamResult>(resultado);
            Assert.Equal("application/pdf", archivo.ContentType);
        }

        [Fact]
        public async Task VerDocumentoAsync_ExcepcionLanzada_RetornaBadRequest()
        {
            // Arrange
            int documentoId = 4;
            _documentoRepoMock.Setup(r => r.ObtenerPorIdAsync(documentoId))
                              .ThrowsAsync(new Exception("Error inesperado"));

            // Act
            var resultado = await _documentoService.VerDocumentoAsync(documentoId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
            var respuesta = Assert.IsType<RespuestaAPI>(badRequest.Value);
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
            Assert.Contains("Error al intentar ver el documento", respuesta.ErrorMessages.First());
        }

        #endregion

        #region metodos privados

        private void SetFormFileMock()
        {
            // Configurar el comportamiento del archivo simulado
            var content = "Contenido simulado del archivo";
            var fileName = "archivo.pdf";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            _formFileMock.Setup(f => f.FileName).Returns(fileName);
            _formFileMock.Setup(f => f.Length).Returns(ms.Length);
            _formFileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            _formFileMock.Setup(f => f.ContentType).Returns("application/pdf");
            _formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                         .Returns<Stream, CancellationToken>((stream, token) =>
                         {
                             return ms.CopyToAsync(stream, token);
                         });
        }

        #endregion
    }
}
