using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ApiSigestHC.Tests.Servicios
{
    public class AlmacenamientoArchivoServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly Mock<IDocumentoRepositorio> _documentoRepoMock;
        private readonly Mock<IAtencionRepositorio> _atencionRepoMock;
        private readonly Mock<ITipoDocumentoRepositorio> _tipoDocumentoRepoMock;
        private readonly AlmacenamientoArchivoService _service;
        private readonly string _basePath;

        public AlmacenamientoArchivoServiceTests()
        {
            _envMock = new Mock<IWebHostEnvironment>();
            _documentoRepoMock = new Mock<IDocumentoRepositorio>();
            _atencionRepoMock = new Mock<IAtencionRepositorio>();
            _tipoDocumentoRepoMock = new Mock<ITipoDocumentoRepositorio>();

            // Usamos un directorio temporal único por test
            _basePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_basePath);

            _envMock.Setup(e => e.ContentRootPath).Returns(_basePath);

            _service = new AlmacenamientoArchivoService(
                _envMock.Object,
                _documentoRepoMock.Object,
                _tipoDocumentoRepoMock.Object,
                _atencionRepoMock.Object

            );
        }

        #region GuardarArchivoAsync
        [Fact]
        public async Task GuardarArchivoAsync_DeberiaGuardarArchivoCorrectamente()
        {
            // Arrange
            var archivoMock = new Mock<IFormFile>();
            var content = "Contenido falso";
            var fileName = "archivo.pdf";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));

            archivoMock.Setup(f => f.FileName).Returns(fileName);
            archivoMock.Setup(f => f.Length).Returns(ms.Length);
            archivoMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                       .Returns<Stream, CancellationToken>((stream, _) => ms.CopyToAsync(stream));

            var atencion = new Atencion
            {
                Id = 1,
                Fecha = new DateTime(2024, 5, 10),
                PacienteId = "123456"
            };

            var tipoDoc = new TipoDocumento
            {
                Id = 1,
                Codigo = "HC",
                PermiteMultiples = true,
                EsAsistencial = true,
            };

            var dto = new DocumentoCargarDto
            {
                AtencionId = 1,
                TipoDocumentoId = 1,
                Archivo = archivoMock.Object,
                Fecha = new DateTime(2024, 5, 10)
            };

            _atencionRepoMock.Setup(r => r.ObtenerAtencionPorIdAsync(1)).ReturnsAsync(atencion);
            _tipoDocumentoRepoMock.Setup(r => r.GetTipoDocumentoPorIdAsync(1)).ReturnsAsync(tipoDoc);
            _documentoRepoMock.Setup(r => r.ContarPorTipoYAtencionAsync(1, 1)).ReturnsAsync(0);

            // Act
            var resultado = await _service.GuardarArchivoAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("HC_1_20240510.pdf", resultado.NombreArchivo);

            // Verificar ruta relativa
            var year = atencion.Fecha.Year.ToString();
            var mes = atencion.Fecha.Month.ToString("D2");
            var carpetaFinal = $"{atencion.Id}_{atencion.Fecha:yyyyMMdd}";
            var expectedRel = Path.Combine("documentos", year, mes, atencion.PacienteId, carpetaFinal)
                                 .Replace("\\", "/");
            Assert.Equal(expectedRel, resultado.RutaRelativa);

            // Cleanup (opcional pero recomendado)
            var fullPath = Path.Combine(resultado.RutaBase, resultado.RutaRelativa, resultado.NombreArchivo);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }

        // 1) Sin múltiple, no asistencial, no relación → "COD.pdf"
        [Fact]
        public async Task GuardarArchivo_Nombre_SoloCodigo()
        {
            // Arrange
            var dto = CrearDto(new DateTime(2025, 6, 17));
            var tipo = new TipoDocumento
            {
                Id = 1,
                Codigo = "COD",
                PermiteMultiples = false,
                RequiereNumeroRelacion = false,
                EsAsistencial = false
            };
            SetupBase(countExisting: 0);
            _tipoDocumentoRepoMock.Setup(r => r.GetTipoDocumentoPorIdAsync(1)).ReturnsAsync(tipo);

            // Act
            var r = await _service.GuardarArchivoAsync(dto);

            // Assert
            Assert.NotNull(r);
            Assert.Equal("COD.pdf", r.NombreArchivo);
        }

        // 2) Con múltiple, no asistencial, no relación → "COD_1.pdf"
        [Fact]
        public async Task GuardarArchivo_Nombre_ConConsecutivo()
        {
            // Arrange
            var dto = CrearDto(new DateTime(2025, 6, 17));
            var tipo = new TipoDocumento
            {
                Id = 1,
                Codigo = "COD",
                PermiteMultiples = true,
                RequiereNumeroRelacion = false,
                EsAsistencial = false
            };
            SetupBase(countExisting: 0);
            _tipoDocumentoRepoMock.Setup(r => r.GetTipoDocumentoPorIdAsync(1)).ReturnsAsync(tipo);

            // Act
            var r = await _service.GuardarArchivoAsync(dto);

            // Assert
            Assert.NotNull(r);
            Assert.Equal("COD_1.pdf", r.NombreArchivo);
        }

        // 3) Sin múltiple, asistencial, no relación → "COD_20250617.pdf"
        [Fact]
        public async Task GuardarArchivo_Nombre_ConFecha()
        {
            // Arrange
            var dto = CrearDto(new DateTime(2025, 6, 17));
            var tipo = new TipoDocumento
            {
                Id = 1,
                Codigo = "COD",
                PermiteMultiples = false,
                RequiereNumeroRelacion = false,
                EsAsistencial = true
            };
            SetupBase(countExisting: 0);
            _tipoDocumentoRepoMock.Setup(r => r.GetTipoDocumentoPorIdAsync(1)).ReturnsAsync(tipo);

            // Act
            var r = await _service.GuardarArchivoAsync(dto);

            // Assert
            Assert.NotNull(r);
            Assert.Equal("COD_20250617.pdf", r.NombreArchivo);
        }

        // 4) Múltiple + asistencial + no relación → "COD_1_20250617.pdf"
        [Fact]
        public async Task GuardarArchivo_Nombre_ConConsecutivoYFecha()
        {
            // Arrange
            var dto = CrearDto(new DateTime(2025, 6, 17));
            var tipo = new TipoDocumento
            {
                Id = 1,
                Codigo = "COD",
                PermiteMultiples = true,
                RequiereNumeroRelacion = false,
                EsAsistencial = true
            };
            SetupBase(countExisting: 0);
            _tipoDocumentoRepoMock.Setup(r => r.GetTipoDocumentoPorIdAsync(1)).ReturnsAsync(tipo);

            // Act
            var r = await _service.GuardarArchivoAsync(dto);

            // Assert
            Assert.NotNull(r);
            Assert.Equal("COD_1_20250617.pdf", r.NombreArchivo);
        }

        // 5) Relación tiene prioridad sobre fecha (sin múltiple) → "COD_REF123.pdf"
        [Fact]
        public async Task GuardarArchivo_Nombre_ConNumeroRelacion_Prioritario()
        {
            // Arrange
            var dto = CrearDto(new DateTime(2025, 6, 17), numeroRelacion: "REF123");
            var tipo = new TipoDocumento
            {
                Id = 1,
                Codigo = "COD",
                PermiteMultiples = false,
                RequiereNumeroRelacion = true,
                EsAsistencial = true
            };
            SetupBase(countExisting: 0);
            _tipoDocumentoRepoMock.Setup(r => r.GetTipoDocumentoPorIdAsync(1)).ReturnsAsync(tipo);

            // Act
            var r = await _service.GuardarArchivoAsync(dto);

            // Assert

            Assert.NotNull(r);
            Assert.Equal("COD_REF123.pdf", r.NombreArchivo);
        }

        // 6) Relación + múltiple → "COD_1_REF123.pdf"
        [Fact]
        public async Task GuardarArchivo_Nombre_ConConsecutivoYRelacion()
        {
            // Arrange
            var dto = CrearDto(new DateTime(2025, 6, 17), numeroRelacion: "REL-456");
            var tipo = new TipoDocumento
            {
                Id = 1,
                Codigo = "COD",
                PermiteMultiples = true,
                RequiereNumeroRelacion = true,
                EsAsistencial = false
            };
            SetupBase(countExisting: 0);
            _tipoDocumentoRepoMock.Setup(r => r.GetTipoDocumentoPorIdAsync(1)).ReturnsAsync(tipo);

            // Act
            var r = await _service.GuardarArchivoAsync(dto);

            // Assert
            Assert.NotNull(r);
            Assert.Equal("COD_1_REL-456.pdf", r.NombreArchivo);
        }

        #endregion

        [Fact]
        public async Task ReemplazarArchivoAsync_DeberiaSobrescribirContenidoYDevolverResultado()
        {
            // Arrange
            var originalContent = "antiguo";
            var newContent = "nuevo contenido";

            // Creamos la estructura de carpetas y el archivo original
            var rutaRelativa = Path.Combine("docs", "2025", "06", "000001", "1_20250614");
            var nombreArchivo = "TD_1_20250614.pdf";
            var absDir = Path.Combine(_basePath, rutaRelativa);
            Directory.CreateDirectory(absDir);
            var absPath = Path.Combine(absDir, nombreArchivo);
            await File.WriteAllTextAsync(absPath, originalContent, Encoding.UTF8);

            // Simulamos el documento existente
            var documento = new Documento
            {
                RutaBase = _basePath,
                RutaRelativa = rutaRelativa,
                NombreArchivo = nombreArchivo
            };

            // Simulamos el nuevo IFormFile
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(newContent));
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                    .Returns<Stream, CancellationToken>((stream, _) => ms.CopyToAsync(stream));

            // Act
            var resultado = await _service.ReemplazarArchivoCorreccionAsync(documento, fileMock.Object);

            // Assert: resultado
            Assert.NotNull(resultado);
            Assert.Equal(nombreArchivo, resultado.NombreArchivo);
            Assert.Equal(rutaRelativa.Replace("\\", "/"), resultado.RutaRelativa);
            Assert.Equal(_basePath.Replace("\\", "/"), resultado.RutaBase);

            // Assert: contenido del archivo debe ser el nuevo
            var diskContent = await File.ReadAllTextAsync(absPath, Encoding.UTF8);
            Assert.Equal(newContent, diskContent);

            // Cleanup
            Directory.Delete(_basePath, recursive: true);
        }

        #region ActualizarNombreSiEsNecesarioAsync

        [Fact]
        //1. Renombramiento exitoso cuando cambia la fecha (y cambia el nombre)
        public async Task ActualizarNombreSiEsNecesarioAsync_DeberiaRenombrarArchivo_SiNombreCambia()
        {
            // Arrange
            var documento = new Documento
            {
                Id = 1,
                TipoDocumentoId = 10,
                Fecha = new DateTime(2024, 1, 1),
                RutaBase = _basePath,
                RutaRelativa = "docs/paciente",
                NombreArchivo = "HC_1_20240101.pdf",
                NumeroRelacion = null
            };

            var tipoDocumento = new TipoDocumento
            {
                Id = 10,
                Codigo = "HC",
                EsAsistencial = true,
                PermiteMultiples = true,
                RequiereNumeroRelacion = false
            };

            var dto = new DocumentoEditarDto
            {
                Id = 1,
                Fecha = new DateTime(2024, 5, 5)
            };

            var nombreEsperado = "HC_1_20240505.pdf";
            var rutaNueva = Path.Combine(documento.RutaBase, documento.RutaRelativa, nombreEsperado);

            _documentoRepoMock.Setup(x => x.ObtenerPorIdAsync(1)).ReturnsAsync(documento);
            _tipoDocumentoRepoMock.Setup(x => x.GetTipoDocumentoPorIdAsync(10)).ReturnsAsync(tipoDocumento);

            var viejoPath = Path.Combine(documento.RutaBase, documento.RutaRelativa, documento.NombreArchivo);
            var absDir = Path.Combine(documento.RutaBase, documento.RutaRelativa);
            Directory.CreateDirectory(absDir);
            File.WriteAllText(viejoPath, "dummy content");

            if (dto.Fecha != null)
                documento.Fecha = (DateTime)dto.Fecha;

            // Act
            var resultado = await _service.ActualizarNombreSiEsNecesarioAsync(dto);

            // Assert
            Assert.Equal(nombreEsperado, resultado.NombreArchivo);
            Assert.True(File.Exists(rutaNueva));
            Assert.False(File.Exists(viejoPath));
        }

        [Fact]
        //2. No renombra si el nombre no cambia (por ejemplo, misma fecha)
        public async Task ActualizarNombreSiEsNecesarioAsync_NoDebeRenombrar_SiNombreNoCambia()
        {
            // Arrange
            var documento = new Documento
            {
                Id = 2,
                TipoDocumentoId = 20,
                Fecha = new DateTime(2024, 3, 10),
                RutaBase = _basePath,
                RutaRelativa = "docs/test",
                NombreArchivo = "HC_1_20240310.pdf"
            };

            var tipoDocumento = new TipoDocumento
            {
                Id = 20,
                Codigo = "HC",
                EsAsistencial = true,
                PermiteMultiples = true,
                RequiereNumeroRelacion = false
            };

            var dto = new DocumentoEditarDto
            {
                Id = 2,
                Fecha = new DateTime(2024, 3, 10)
            };

            if (dto.Fecha != null)
                documento.Fecha = (DateTime)dto.Fecha;

            _documentoRepoMock.Setup(x => x.ObtenerPorIdAsync(2)).ReturnsAsync(documento);
            _tipoDocumentoRepoMock.Setup(x => x.GetTipoDocumentoPorIdAsync(20)).ReturnsAsync(tipoDocumento);

            // Act
            var resultado = await _service.ActualizarNombreSiEsNecesarioAsync(dto);

            // Assert
            Assert.Equal(documento.NombreArchivo, resultado.NombreArchivo);
        }

        [Fact]
        //3.Debe renombrar el archivo cuando requiere número de relación
        public async Task ActualizarNombreSiEsNecesarioAsync_DebeRenombrarArchivo_CuandoRequiereNumeroRelacion()
        {
            // Arrange
            var documento = new Documento
            {
                Id = 2,
                TipoDocumentoId = 20,
                Fecha = new DateTime(2024, 3, 10),
                RutaBase = _basePath,
                RutaRelativa = "docs/test",
                NombreArchivo = "REF_1_20240601.pdf",
                NumeroRelacion = "XYZ000"
            };

            var tipoDocumento = new TipoDocumento
            {
                Id = documento.TipoDocumentoId,
                Codigo = "REF",
                RequiereNumeroRelacion = true,
                EsAsistencial = false,
                PermiteMultiples = false
            };

            var dto = new DocumentoEditarDto
            {
                Id = documento.Id,
                NumeroRelacion = "ABC123"
            };

            documento.NumeroRelacion = dto.NumeroRelacion;

            _documentoRepoMock.Setup(x => x.ObtenerPorIdAsync(documento.Id)).ReturnsAsync(documento);
            _tipoDocumentoRepoMock.Setup(x => x.GetTipoDocumentoPorIdAsync(documento.TipoDocumentoId)).ReturnsAsync(tipoDocumento);
            _documentoRepoMock.Setup(x => x.ActualizarAsync(It.IsAny<Documento>())).Returns(Task.CompletedTask);


            var viejoPath = Path.Combine(documento.RutaBase, documento.RutaRelativa, documento.NombreArchivo);
            var absDir = Path.Combine(_basePath, documento.RutaRelativa);
            Directory.CreateDirectory(absDir);
            File.WriteAllText(viejoPath, "dummy content");

            var nuevoNombre = "REF_ABC123.pdf";
            var nuevoPath = Path.Combine(documento.RutaBase, documento.RutaRelativa, nuevoNombre);


            // Act
            var resultado = await _service.ActualizarNombreSiEsNecesarioAsync(dto);

            // Assert
            Assert.Equal(nuevoNombre, resultado.NombreArchivo);
            Assert.True(File.Exists(nuevoPath));
            Assert.False(File.Exists(viejoPath));
        }
        [Fact]
        //4. No debe renombrar si el tipo de documento no es asistencial ni requiere número de relación
        public async Task ActualizarNombreSiEsNecesarioAsync_NoDebeRenombrarArchivo_CuandoNoRequiereNada()
        {
            // Arrange
            var documento = new Documento
            {
                Id = 2,
                TipoDocumentoId = 20,
                Fecha = new DateTime(2024, 3, 10),
                RutaBase = _basePath,
                RutaRelativa = "docs/test",
                NombreArchivo = "GEN_1.pdf"
            };

            var tipoDocumento = new TipoDocumento
            {
                Id = documento.TipoDocumentoId,
                Codigo = "GEN",
                RequiereNumeroRelacion = false,
                EsAsistencial = false,
                PermiteMultiples = true
            };

            var dto = new DocumentoEditarDto
            {
                Id = documento.Id,
                NumeroRelacion = "XYZ999",
                Fecha = documento.Fecha
            };

            _documentoRepoMock.Setup(x => x.ObtenerPorIdAsync(documento.Id)).ReturnsAsync(documento);
            _tipoDocumentoRepoMock.Setup(x => x.GetTipoDocumentoPorIdAsync(documento.TipoDocumentoId)).ReturnsAsync(tipoDocumento);

            var viejoPath = Path.Combine(documento.RutaBase, documento.RutaRelativa, documento.NombreArchivo);            
            var absDir = Path.Combine(_basePath, documento.RutaRelativa);
            Directory.CreateDirectory(absDir);
            File.WriteAllText(viejoPath, "dummy content");

            // Act
            var resultado = await _service.ActualizarNombreSiEsNecesarioAsync(dto);

            // Assert
            Assert.Equal("GEN_1.pdf", resultado.NombreArchivo);
            Assert.True(File.Exists(viejoPath)); // archivo no renombrado
        }
        [Fact]
        // 5. Lanza excepción si archivo físico no existe
        public async Task ActualizarNombreSiEsNecesarioAsync_DeberiaLanzarExcepcion_SiArchivoNoExiste()
        {
            var documento = new Documento
            {
                Id = 4,
                TipoDocumentoId = 40,
                Fecha = new DateTime(2024, 1, 1),
                RutaBase = _basePath,
                RutaRelativa = "missing",
                NombreArchivo = "HCL_1_20240101.pdf"
            };

            var tipoDocumento = new TipoDocumento
            {
                Id = 40,
                Codigo = "HCL",
                EsAsistencial = true,
                PermiteMultiples = true,
                RequiereNumeroRelacion = false
            };

            _documentoRepoMock.Setup(x => x.ObtenerPorIdAsync(4)).ReturnsAsync(documento);
            _tipoDocumentoRepoMock.Setup(x => x.GetTipoDocumentoPorIdAsync(40)).ReturnsAsync(tipoDocumento);

            var dto = new DocumentoEditarDto { Id = 4, Fecha = new DateTime(2024, 5, 5) };
            if (dto.Fecha != null)
                documento.Fecha = (DateTime)dto.Fecha;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _service.ActualizarNombreSiEsNecesarioAsync(dto));
            Assert.Contains("No existe el archivo físico", ex.Message);
        }


        #endregion
        [Fact]
        public async Task DescargarDocumentoAsync_DeberiaRetornarNull_SiArchivoNoExiste()
        {
            // Arrange
            var doc = new Documento
            {
                RutaBase = _basePath,
                RutaRelativa = "no/existe",
                NombreArchivo = "inexistente.pdf"
            };

            // Act
            var result = await _service.DescargarDocumentoAsync(doc);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DescargarDocumentoAsync_DeberiaRetornarFileStreamResult_ConNombreYMimeCorrectos()
        {
            // Arrange
            var contenido = "prueba";
            var rutaRel = Path.Combine("docs", "2025", "06");
            var nombre = "archivo.pdf";
            var dir = Path.Combine(_basePath, rutaRel);
            Directory.CreateDirectory(dir);
            var fullPath = Path.Combine(dir, nombre);
            await File.WriteAllTextAsync(fullPath, contenido, Encoding.UTF8);

            var atencion = new Atencion
            {
                Id = 42,
                PacienteId = "ABC123",
                Fecha = new DateTime(2025, 6, 14)
            };
            var doc = new Documento
            {
                RutaBase = _basePath,
                RutaRelativa = rutaRel,
                NombreArchivo = nombre,
                Atencion = atencion
            };

            // Act
            var result = await _service.DescargarDocumentoAsync(doc);

            // Assert
            // 1) No es nulo
            Assert.NotNull(result);
            // 2) Tipo correcto
            Assert.IsType<FileStreamResult>(result);
            var fsr = result as FileStreamResult;
            Assert.Equal("application/pdf", fsr.ContentType);
            // 3) FileDownloadName tiene el formato "{Id}_{PacienteId}_{Fecha:yyyyMMdd}_{NombreArchivo}"
            var expectedDownloadName = $"{atencion.Id}_{atencion.PacienteId}_{atencion.Fecha:yyyyMMdd}_{nombre}";
            Assert.Equal(expectedDownloadName, fsr.FileDownloadName);

            // 4) El stream permite leer el mismo contenido
            using var reader = new StreamReader(fsr.FileStream, Encoding.UTF8);
            var textoLeido = await reader.ReadToEndAsync();
            Assert.Equal(contenido, textoLeido);

            // Cleanup
            fsr.FileStream.Dispose();
            Directory.Delete(_basePath, recursive: true);
        }

        [Fact]
        public async Task ObtenerArchivoParaVisualizacionAsync_DeberiaRetornarNull_SiArchivoNoExiste()
        {
            // Arrange
            var doc = new Documento
            {
                RutaBase = _basePath,
                RutaRelativa = "no/existe",
                NombreArchivo = "inexistente.pdf"
            };

            // Act
            var result = await _service.ObtenerArchivoParaVisualizacionAsync(doc);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ObtenerArchivoParaVisualizacionAsync_DeberiaRetornarFileStreamResult_SinFileDownloadName()
        {
            // Arrange
            var contenido = "ver contenido";
            var rutaRel = Path.Combine("view", "2025", "06");
            var nombre = "vista.pdf";
            var dir = Path.Combine(_basePath, rutaRel);
            Directory.CreateDirectory(dir);
            var fullPath = Path.Combine(dir, nombre);
            await File.WriteAllTextAsync(fullPath, contenido, Encoding.UTF8);

            var atencion = new Atencion
            {
                Id = 99,
                PacienteId = "XYZ789",
                Fecha = new DateTime(2025, 6, 14)
            };
            var doc = new Documento
            {
                RutaBase = _basePath,
                RutaRelativa = rutaRel,
                NombreArchivo = nombre,
                Atencion = atencion
            };

            // Act
            var result = await _service.ObtenerArchivoParaVisualizacionAsync(doc);

            // Assert
            Assert.NotNull(result);
            var fsr = Assert.IsType<FileStreamResult>(result);

            // Mismo content type que descarga
            Assert.Equal("application/pdf", fsr.ContentType);

            // En este caso no debe definirse FileDownloadName
            Assert.Empty(fsr.FileDownloadName);

            // Verificar contenido
            using var reader = new StreamReader(fsr.FileStream, Encoding.UTF8);
            var textoLeido = await reader.ReadToEndAsync();
            Assert.Equal(contenido, textoLeido);

            // Cleanup
            fsr.FileStream.Dispose();
            Directory.Delete(_basePath, recursive: true);
        }

        #region Eliminar Archivo
        [Fact]
        public async Task EliminarArchivoAsync_ArchivoExiste_EliminaCorrectamente()
        {
            // Arrange
            var rutaRelativa = Path.Combine("documentos", "2025", "06", "000001", "123_20250614");
            var nombreArchivo = "TD_1_20250614.pdf";

            var absDir = Path.Combine(_basePath, rutaRelativa);
            Directory.CreateDirectory(absDir);

            var absPath = Path.Combine(absDir, nombreArchivo);
            await File.WriteAllTextAsync(absPath, "contenido de prueba");

            var documento = new Documento
            {
                RutaBase = _basePath,
                RutaRelativa = rutaRelativa,
                NombreArchivo = nombreArchivo
            };

            // Act
            await _service.EliminarArchivoAsync(documento);

            // Assert
            Assert.False(File.Exists(absPath));
        }

        [Fact]
        public async Task EliminarArchivoAsync_ArchivoNoExiste_LanzaFileNotFoundException()
        {
            // Arrange
            var documento = new Documento
            {
                RutaBase = _basePath,
                RutaRelativa = "documentos/fake/path",
                NombreArchivo = "noexiste.pdf"
            };

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _service.EliminarArchivoAsync(documento));
        }


        #endregion


        #region Metodos Privado
        private IFormFile CrearArchivoMock(string nombre, string contenido = "x")
        {
            var bytes = Encoding.UTF8.GetBytes(contenido);
            var ms = new MemoryStream(bytes);
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(nombre);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                    .Returns<Stream, CancellationToken>((s, _) => ms.CopyToAsync(s));
            return fileMock.Object;
        }

        private DocumentoCargarDto CrearDto(DateTime fecha, string numeroRelacion = null)
        => new DocumentoCargarDto
        {
            AtencionId = 1,
            TipoDocumentoId = 1,
            Archivo = CrearArchivoMock("dummy.pdf"),
            Fecha = fecha,
            NumeroRelacion = numeroRelacion
        };

        private Atencion CrearAtencion(DateTime fecha)
            => new Atencion { Id = 1, Fecha = fecha, PacienteId = "000123" };

        private void SetupBase(int countExisting)
        {
            // siempre devolvemos esta atención y código
            _atencionRepoMock.Setup(r => r.ObtenerAtencionPorIdAsync(1))
                             .ReturnsAsync(CrearAtencion(new DateTime(2025, 6, 17)));
            _documentoRepoMock.Setup(r => r.ContarPorTipoYAtencionAsync(1, 1))
                        .ReturnsAsync(countExisting);
        }

        #endregion
    }
}
