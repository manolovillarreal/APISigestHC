using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using ApiSigestHC.Servicios;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using Microsoft.AspNetCore.Http;
using System.Net;


namespace ApiSigestHC.Tests.Servicios
{
    public class ValidacionCargaDocumentoServiceTests
    {
        private readonly Mock<IAtencionRepositorio> _atencionRepoMock = new();
        private readonly Mock<ITipoDocumentoRepositorio> _tipoDocumentoRepoMock = new();
        private readonly Mock<IDocumentoRepositorio> _documentoRepoMock = new();
        private readonly Mock<IUsuarioContextService> _usuarioContextServiceMock = new();

        private readonly IValidacionCargaArchivoService _service;

        public ValidacionCargaDocumentoServiceTests()
        {
            _service = new ValidacionCargaDocumentoService(
                _atencionRepoMock.Object,
                _tipoDocumentoRepoMock.Object,
                _documentoRepoMock.Object,
                _usuarioContextServiceMock.Object
            );
        }

        [Fact]
        //Prueba exitosa (ya enviada)
        public async Task ValidarCargaDocumentoAsync_DeberiaRetornarSuccess_CuandoTodoEsValido()
        {
            // Arrange
            var dto = new DocumentoCargarDto
            {
                AtencionId = 1,
                TipoDocumentoId = 2,
                Archivo = CrearArchivoMock("documento.pdf", 500_000),
                NumeroRelacion = "REL-123"
            };

            var atencion = new Atencion { Id = 1, EstadoAtencionId = 2 };
            var tipoDoc = new TipoDocumento
            {
                Id = 2,
                ExtensionPermitida = "pdf",
                PermiteMultiples = true,
                RequiereNumeroRelacion = true
            };

            var usuarioContextMock = new Mock<IUsuarioContextService>();
            usuarioContextMock.Setup(u => u.ObtenerRolId()).Returns(1);
            usuarioContextMock.Setup(u => u.ObtenerRolNombre()).Returns("Medico");

            var atencionRepo = new Mock<IAtencionRepositorio>();
            atencionRepo.Setup(r => r.ObtenerAtencionPorIdAsync(1)).ReturnsAsync(atencion);

            var tipoDocRepo = new Mock<ITipoDocumentoRepositorio>();
            tipoDocRepo.Setup(r => r.GetTipoDocumentoPorIdAsync(2)).ReturnsAsync(tipoDoc);

            var documentoRepo = new Mock<IDocumentoRepositorio>();
            documentoRepo.Setup(r => r.PuedeCargarDocumento(1, 2)).ReturnsAsync(true);

            var service = new ValidacionCargaDocumentoService(
                atencionRepo.Object, tipoDocRepo.Object, documentoRepo.Object, usuarioContextMock.Object
            );

            // Act
            var resultado = await service.ValidarCargaDocumentoAsync(dto);

            // Assert
            Assert.True(resultado.Ok);
        }

        [Fact]
        //1. Atención no encontrada
        public async Task ValidarCargaDocumentoAsync_DeberiaRetornarError_SiAtencionNoExiste()
        {
            var dto = new DocumentoCargarDto { AtencionId = 1, TipoDocumentoId = 2 };

            var atencionRepo = new Mock<IAtencionRepositorio>();
            atencionRepo.Setup(r => r.ObtenerAtencionPorIdAsync(1)).ReturnsAsync((Atencion)null);

            var tipoDocRepo = new Mock<ITipoDocumentoRepositorio>();
            tipoDocRepo.Setup(r => r.GetTipoDocumentoPorIdAsync(2)).ReturnsAsync(new TipoDocumento());

            var documentoRepo = new Mock<IDocumentoRepositorio>();
            var usuarioContext = new Mock<IUsuarioContextService>();

            var service = new ValidacionCargaDocumentoService(atencionRepo.Object, tipoDocRepo.Object, documentoRepo.Object, usuarioContext.Object);

            var resultado = await service.ValidarCargaDocumentoAsync(dto);

            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.NotFound, resultado.StatusCode);
        }

        [Fact]
        // 2. Tipo de documento inválido
        public async Task ValidarCargaDocumentoAsync_DeberiaRetornarError_SiTipoDocumentoNoExiste()
        {
            var dto = new DocumentoCargarDto { AtencionId = 1, TipoDocumentoId = 2 };

            var atencionRepo = new Mock<IAtencionRepositorio>();
            atencionRepo.Setup(r => r.ObtenerAtencionPorIdAsync(1)).ReturnsAsync(new Atencion());

            var tipoDocRepo = new Mock<ITipoDocumentoRepositorio>();
            tipoDocRepo.Setup(r => r.GetTipoDocumentoPorIdAsync(2)).ReturnsAsync((TipoDocumento)null);

            var documentoRepo = new Mock<IDocumentoRepositorio>();
            var usuarioContext = new Mock<IUsuarioContextService>();

            var service = new ValidacionCargaDocumentoService(atencionRepo.Object, tipoDocRepo.Object, documentoRepo.Object, usuarioContext.Object);

            var resultado = await service.ValidarCargaDocumentoAsync(dto);

            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
        }

        [Fact]
        // 3. Archivo nulo o vacío
        public async Task ValidarCargaDocumentoAsync_DeberiaRetornarError_SiArchivoEsNulo()
        {
            var dto = new DocumentoCargarDto
            {
                AtencionId = 1,
                TipoDocumentoId = 2,
                Archivo = null
            };

            var tipoDoc = new TipoDocumento
            {
                Id = 2,
                ExtensionPermitida = "pdf"
            };

            var atencion = new Atencion { EstadoAtencionId = 2 };

            var usuarioContext = new Mock<IUsuarioContextService>();
            usuarioContext.Setup(u => u.ObtenerRolId()).Returns(1);
            usuarioContext.Setup(u => u.ObtenerRolNombre()).Returns("Medico");

            var atencionRepo = new Mock<IAtencionRepositorio>();
            atencionRepo.Setup(r => r.ObtenerAtencionPorIdAsync(1)).ReturnsAsync(atencion);

            var tipoDocRepo = new Mock<ITipoDocumentoRepositorio>();
            tipoDocRepo.Setup(r => r.GetTipoDocumentoPorIdAsync(2)).ReturnsAsync(tipoDoc);

            var documentoRepo = new Mock<IDocumentoRepositorio>();
            documentoRepo.Setup(r => r.PuedeCargarDocumento(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

            var service = new ValidacionCargaDocumentoService(atencionRepo.Object, tipoDocRepo.Object, documentoRepo.Object, usuarioContext.Object);

            var resultado = await service.ValidarCargaDocumentoAsync(dto);

            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
        }

        [Fact]
        //4. Extensión no permitida
        public async Task ValidarCargaDocumentoAsync_DeberiaRetornarError_SiExtensionNoPermitida()
        {
            var dto = new DocumentoCargarDto
            {
                AtencionId = 1,
                TipoDocumentoId = 2,
                Archivo = CrearArchivoMock("archivo.exe", 500_000)
            };

            var tipoDoc = new TipoDocumento
            {
                Id = 2,
                ExtensionPermitida = "pdf"
            };

            var atencion = new Atencion { EstadoAtencionId = 2 };

            var usuarioContext = new Mock<IUsuarioContextService>();
            usuarioContext.Setup(u => u.ObtenerRolId()).Returns(1);
            usuarioContext.Setup(u => u.ObtenerRolNombre()).Returns("Medico");

            var atencionRepo = new Mock<IAtencionRepositorio>();
            atencionRepo.Setup(r => r.ObtenerAtencionPorIdAsync(1)).ReturnsAsync(atencion);

            var tipoDocRepo = new Mock<ITipoDocumentoRepositorio>();
            tipoDocRepo.Setup(r => r.GetTipoDocumentoPorIdAsync(2)).ReturnsAsync(tipoDoc);

            var documentoRepo = new Mock<IDocumentoRepositorio>();
            documentoRepo.Setup(r => r.PuedeCargarDocumento(1, 2)).ReturnsAsync(true);

            var service = new ValidacionCargaDocumentoService(atencionRepo.Object, tipoDocRepo.Object, documentoRepo.Object, usuarioContext.Object);

            var resultado = await service.ValidarCargaDocumentoAsync(dto);

            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, resultado.StatusCode);
        }

        [Fact]
        // 5. Tamaño mayor a 10 MB
        public async Task ValidarCargaDocumentoAsync_DeberiaRetornarError_SiArchivoExcedeTamano()
        {
            var dto = new DocumentoCargarDto
            {
                AtencionId = 1,
                TipoDocumentoId = 2,
                Archivo = CrearArchivoMock("archivo.pdf", 11 * 1024 * 1024)
            };

            var tipoDoc = new TipoDocumento
            {
                Id = 2,
                ExtensionPermitida = "pdf"
            };

            var atencion = new Atencion { EstadoAtencionId = 2 };

            var usuarioContext = new Mock<IUsuarioContextService>();
            usuarioContext.Setup(u => u.ObtenerRolId()).Returns(1);
            usuarioContext.Setup(u => u.ObtenerRolNombre()).Returns("Medico");

            var atencionRepo = new Mock<IAtencionRepositorio>();
            atencionRepo.Setup(r => r.ObtenerAtencionPorIdAsync(1)).ReturnsAsync(atencion);

            var tipoDocRepo = new Mock<ITipoDocumentoRepositorio>();
            tipoDocRepo.Setup(r => r.GetTipoDocumentoPorIdAsync(2)).ReturnsAsync(tipoDoc);

            var documentoRepo = new Mock<IDocumentoRepositorio>();
            documentoRepo.Setup(r => r.PuedeCargarDocumento(1, 2)).ReturnsAsync(true);

            var service = new ValidacionCargaDocumentoService(atencionRepo.Object, tipoDocRepo.Object, documentoRepo.Object, usuarioContext.Object);

            var resultado = await service.ValidarCargaDocumentoAsync(dto);

            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
        }

        [Fact]
        // 6. No tiene permisos para cargar
        public async Task ValidarCargaDocumentoAsync_DeberiaRetornarError_SiNoTienePermisoDeCarga()
        {
            var dto = new DocumentoCargarDto
            {
                AtencionId = 1,
                TipoDocumentoId = 2,
                Archivo = CrearArchivoMock("archivo.pdf", 500_000)
            };

            var tipoDoc = new TipoDocumento { Id = 2, ExtensionPermitida = "pdf" };
            var atencion = new Atencion { EstadoAtencionId = 2 };

            var usuarioContext = new Mock<IUsuarioContextService>();
            usuarioContext.Setup(u => u.ObtenerRolId()).Returns(1);
            usuarioContext.Setup(u => u.ObtenerRolNombre()).Returns("Medico");

            var atencionRepo = new Mock<IAtencionRepositorio>();
            atencionRepo.Setup(r => r.ObtenerAtencionPorIdAsync(1)).ReturnsAsync(atencion);

            var tipoDocRepo = new Mock<ITipoDocumentoRepositorio>();
            tipoDocRepo.Setup(r => r.GetTipoDocumentoPorIdAsync(2)).ReturnsAsync(tipoDoc);

            var documentoRepo = new Mock<IDocumentoRepositorio>();
            documentoRepo.Setup(r => r.PuedeCargarDocumento(1, 2)).ReturnsAsync(false);

            var service = new ValidacionCargaDocumentoService(atencionRepo.Object, tipoDocRepo.Object, documentoRepo.Object, usuarioContext.Object);

            var resultado = await service.ValidarCargaDocumentoAsync(dto);

            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.Forbidden, resultado.StatusCode);
        }

        [Fact]
        //7. Documento ya existe y no permite múltiples
        public async Task ValidarCargaDocumentoAsync_DeberiaRetornarError_SiYaExisteYNoPermiteMultiples()
        {
            var dto = new DocumentoCargarDto
            {
                AtencionId = 1,
                TipoDocumentoId = 2,
                Archivo = CrearArchivoMock("archivo.pdf", 500_000)
            };

            var tipoDoc = new TipoDocumento
            {
                Id = 2,
                ExtensionPermitida = "pdf",
                PermiteMultiples = false
            };

            var atencion = new Atencion { EstadoAtencionId = 2 };

            var usuarioContext = new Mock<IUsuarioContextService>();
            usuarioContext.Setup(u => u.ObtenerRolId()).Returns(1);
            usuarioContext.Setup(u => u.ObtenerRolNombre()).Returns("Medico");

            var atencionRepo = new Mock<IAtencionRepositorio>();
            atencionRepo.Setup(r => r.ObtenerAtencionPorIdAsync(1)).ReturnsAsync(atencion);

            var tipoDocRepo = new Mock<ITipoDocumentoRepositorio>();
            tipoDocRepo.Setup(r => r.GetTipoDocumentoPorIdAsync(2)).ReturnsAsync(tipoDoc);

            var documentoRepo = new Mock<IDocumentoRepositorio>();
            documentoRepo.Setup(r => r.PuedeCargarDocumento(1, 2)).ReturnsAsync(true);
            documentoRepo.Setup(r => r.ExisteDocumentoAsync(1, 2)).ReturnsAsync(true);

            var service = new ValidacionCargaDocumentoService(atencionRepo.Object, tipoDocRepo.Object, documentoRepo.Object, usuarioContext.Object);

            var resultado = await service.ValidarCargaDocumentoAsync(dto);

            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
        }

        [Fact]
        //8. Falta número de relación cuando es obligatorio
        public async Task ValidarCargaDocumentoAsync_DeberiaRetornarError_SiRequiereRelacionYNoLaTiene()
        {
            var dto = new DocumentoCargarDto
            {
                AtencionId = 1,
                TipoDocumentoId = 2,
                Archivo = CrearArchivoMock("archivo.pdf", 500_000),
                NumeroRelacion = ""
            };

            var tipoDoc = new TipoDocumento
            {
                Id = 2,
                ExtensionPermitida = "pdf",
                PermiteMultiples = true,
                RequiereNumeroRelacion = true
            };

            var atencion = new Atencion { EstadoAtencionId = 2 };

            var usuarioContext = new Mock<IUsuarioContextService>();
            usuarioContext.Setup(u => u.ObtenerRolId()).Returns(1);
            usuarioContext.Setup(u => u.ObtenerRolNombre()).Returns("Medico");

            var atencionRepo = new Mock<IAtencionRepositorio>();
            atencionRepo.Setup(r => r.ObtenerAtencionPorIdAsync(1)).ReturnsAsync(atencion);

            var tipoDocRepo = new Mock<ITipoDocumentoRepositorio>();
            tipoDocRepo.Setup(r => r.GetTipoDocumentoPorIdAsync(2)).ReturnsAsync(tipoDoc);

            var documentoRepo = new Mock<IDocumentoRepositorio>();
            documentoRepo.Setup(r => r.PuedeCargarDocumento(1, 2)).ReturnsAsync(true);

            var service = new ValidacionCargaDocumentoService(atencionRepo.Object, tipoDocRepo.Object, documentoRepo.Object, usuarioContext.Object);

            var resultado = await service.ValidarCargaDocumentoAsync(dto);

            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
        }



        private IFormFile CrearArchivoMock(string nombre, long tamano)
        {
            var stream = new MemoryStream(new byte[tamano]);
            return new FormFile(stream, 0, tamano, "Archivo", nombre)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };
        }



    }
}
