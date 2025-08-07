using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios;
using ApiSigestHC.Servicios.IServicios;
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
    public class TipoDocumentoServiceTests
    {
        private readonly Mock<ITipoDocumentoRepositorio> _tipoDocumentoRepositoryMock;
        private readonly Mock<ITipoDocumentoRolRepositorio> _rolRepositorioMock;
        private readonly Mock<IUsuarioContextService> _usuarioContextServiceMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly TipoDocumentoService _service;

        public TipoDocumentoServiceTests()
        {
            _tipoDocumentoRepositoryMock = new Mock<ITipoDocumentoRepositorio>();
            _rolRepositorioMock = new Mock<ITipoDocumentoRolRepositorio>();
            _usuarioContextServiceMock = new Mock<IUsuarioContextService>();
            _mapperMock = new Mock<IMapper>();

            _service = new TipoDocumentoService(
                _tipoDocumentoRepositoryMock.Object,
                _rolRepositorioMock.Object,
                _usuarioContextServiceMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task ObtenerTiposPermitidosPorRolAsync_RetornaTiposEsperados()
        {
            // Arrange
            int rolId = 2;
            _usuarioContextServiceMock.Setup(x => x.ObtenerRolId()).Returns(rolId);

            var relaciones = new List<TipoDocumentoRol>
        {
            new TipoDocumentoRol { TipoDocumento = new TipoDocumento { Id = 1, Nombre = "Doc1" } },
            new TipoDocumentoRol { TipoDocumento = new TipoDocumento { Id = 2, Nombre = "Doc2" } },
        };

            var tipos = relaciones.Select(r => r.TipoDocumento).Distinct();

            _rolRepositorioMock.Setup(x => x.GetByRolAsync(rolId)).ReturnsAsync(relaciones);

            var tipoDtos = new List<TipoDocumentoDto>
        {
            new TipoDocumentoDto { Id = 1, Nombre = "Doc1" },
            new TipoDocumentoDto { Id = 2, Nombre = "Doc2" }
        };

            _mapperMock.Setup(m => m.Map<IEnumerable<TipoDocumentoDto>>(It.IsAny<IEnumerable<TipoDocumento>>()))
                       .Returns(tipoDtos);

            // Act
            var respuesta = await _service.ObtenerTiposPermitidosPorRolAsync();

            // Assert
            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.NotNull(respuesta.Result);

            var resultado = Assert.IsAssignableFrom<IEnumerable<TipoDocumentoDto>>(respuesta.Result);
            Assert.Equal(2, resultado.Count());
        }
        [Fact]
        public async Task ObtenerTiposPermitidosPorRolAsync_CuandoFalla_RetornaError500()
        {
            // Arrange
            _usuarioContextServiceMock.Setup(x => x.ObtenerRolId()).Throws(new Exception("Error inesperado"));

            // Act
            var respuesta = await _service.ObtenerTiposPermitidosPorRolAsync();

            // Assert
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, respuesta.StatusCode);
            Assert.Contains("Error al obtener tipos autorizados.", respuesta.ErrorMessages);
            Assert.Contains("Error inesperado", respuesta.ErrorMessages.Last());
        }

        [Fact]
        public async Task ObtenerTodosAsync_CuandoExitoso_RetornaListaTipos()
        {
            // Arrange
            var tipos = new List<TipoDocumento>
            {
                new TipoDocumento { Id = 1, Nombre = "DocA" },
                new TipoDocumento { Id = 2, Nombre = "DocB" }
            };

                    var tiposDto = new List<TipoDocumentoDto>
            {
                new TipoDocumentoDto { Id = 1, Nombre = "DocA" },
                new TipoDocumentoDto { Id = 2, Nombre = "DocB" }
            };

            _tipoDocumentoRepositoryMock.Setup(r => r.GetTiposDocumentoAsync()).ReturnsAsync(tipos);
            _mapperMock.Setup(m => m.Map<IEnumerable<TipoDocumentoDto>>(tipos)).Returns(tiposDto);

            
            // Act
            var respuesta = await _service.ObtenerTodosAsync();

            // Assert
            Assert.True(respuesta.Ok);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

            var resultado = Assert.IsAssignableFrom<IEnumerable<TipoDocumentoDto>>(respuesta.Result);
            Assert.Equal(2, resultado.Count());
        }

        [Fact]
        public async Task ObtenerTodosAsync_CuandoExcepcion_RetornaError500()
        {
            // Arrange
            _tipoDocumentoRepositoryMock.Setup(r => r.GetTiposDocumentoAsync()).ThrowsAsync(new Exception("Error inesperado"));

            // Act
            var respuesta = await _service.ObtenerTodosAsync();

            // Assert
            Assert.False(respuesta.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, respuesta.StatusCode);
            Assert.Contains("Error al obtener todos los tipos.", respuesta.ErrorMessages);
            Assert.Contains("Error inesperado", respuesta.ErrorMessages.Last());
        }

        [Fact]
        public async Task ObtenerPorIdAsync_TipoDocumentoExiste_RetornaRespuestaExitosa()
        {
            // Arrange
            int id = 1;
            var tipo = new TipoDocumento { Id = id, Nombre = "Certificado" };
            var tipoDto = new TipoDocumentoDto { Id = id, Nombre = "Certificado" };

            _tipoDocumentoRepositoryMock.Setup(r => r.GetTipoDocumentoPorIdAsync(id))
                                        .ReturnsAsync(tipo);

            _mapperMock.Setup(m => m.Map<TipoDocumentoDto>(tipo))
                       .Returns(tipoDto);

            // Act
            var resultado = await _service.ObtenerPorIdAsync(id);

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.IsType<TipoDocumentoDto>(resultado.Result);
            Assert.Equal("Certificado", ((TipoDocumentoDto)resultado.Result).Nombre);
        }

        [Fact]
        public async Task ObtenerPorIdAsync_TipoDocumentoNoExiste_RetornaNotFound()
        {
            // Arrange
            int id = 10;

            _tipoDocumentoRepositoryMock.Setup(r => r.GetTipoDocumentoPorIdAsync(id))
                                        .ReturnsAsync((TipoDocumento)null);

            // Act
            var resultado = await _service.ObtenerPorIdAsync(id);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.NotFound, resultado.StatusCode);
            Assert.Contains("No encontrado", resultado.ErrorMessages);
        }

        [Fact]
        public async Task ObtenerPorIdAsync_CuandoLanzaExcepcion_RetornaErrorInterno()
        {
            // Arrange
            int id = 5;

            _tipoDocumentoRepositoryMock.Setup(r => r.GetTipoDocumentoPorIdAsync(id))
                                        .ThrowsAsync(new Exception("Falló la conexión"));

            // Act
            var resultado = await _service.ObtenerPorIdAsync(id);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, resultado.StatusCode);
            Assert.Contains("Error al obtener tipo por id", resultado.ErrorMessages);
            Assert.Contains("Falló la conexión", resultado.ErrorMessages.Last());
        }

        //Pruebas unitarias para CrearAsync(TipoDocumentoCrearDto dto)

        // 1. Prueba de éxito: tipo creado correctamente
        [Fact]
        public async Task CrearAsync_TipoDocumentoValido_RetornaCreado()
        {
            // Arrange
            var dto = new TipoDocumentoCrearDto { Codigo = "ABC", Nombre = "Certificado" };
            var entidad = new TipoDocumento { Id = 1, Codigo = "ABC", Nombre = "Certificado" };
            var resultDto = new TipoDocumentoDto { Id = 1, Codigo = "ABC", Nombre = "Certificado" };

            _tipoDocumentoRepositoryMock.Setup(r => r.ExisteTipoDocumentoPorCodigoAsync(dto.Codigo))
                                        .ReturnsAsync(false);

            _mapperMock.Setup(m => m.Map<TipoDocumento>(dto))
                       .Returns(entidad);

            _mapperMock.Setup(m => m.Map<TipoDocumentoDto>(entidad))
                       .Returns(resultDto);

            // Act
            var resultado = await _service.CrearAsync(dto);

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.Created, resultado.StatusCode);
            Assert.IsType<TipoDocumentoDto>(resultado.Result);
            Assert.Equal("Certificado", ((TipoDocumentoDto)resultado.Result).Nombre);

            _tipoDocumentoRepositoryMock.Verify(r => r.CrearTipoDocumentoAsync(entidad), Times.Once);
        }
        // 2. Prueba de conflicto: código ya existe
        public async Task CrearAsync_CodigoDuplicado_RetornaConflict()
        {
            // Arrange
            var dto = new TipoDocumentoCrearDto { Codigo = "DUP" };

            _tipoDocumentoRepositoryMock.Setup(r => r.ExisteTipoDocumentoPorCodigoAsync(dto.Codigo))
                                        .ReturnsAsync(true);

            // Act
            var resultado = await _service.CrearAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.Conflict, resultado.StatusCode);
            Assert.Contains("Código duplicado", resultado.ErrorMessages);

            _tipoDocumentoRepositoryMock.Verify(r => r.CrearTipoDocumentoAsync(It.IsAny<TipoDocumento>()), Times.Never);
        }
        //3. Prueba de error interno: excepción lanzada
        [Fact]
        public async Task CrearAsync_Excepcion_RetornaErrorInterno()
        {
            // Arrange
            var dto = new TipoDocumentoCrearDto { Codigo = "EXC" };

            _tipoDocumentoRepositoryMock.Setup(r => r.ExisteTipoDocumentoPorCodigoAsync(dto.Codigo))
                                        .ThrowsAsync(new Exception("Error en BD"));

            // Act
            var resultado = await _service.CrearAsync(dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, resultado.StatusCode);
            Assert.Contains("Error al crear tipo", resultado.ErrorMessages);
            Assert.Contains("Error en BD", resultado.ErrorMessages.Last());
        }

        //Pruebas unitarias para EditarAsync

        // 1. Prueba de éxito: tipo editado correctamente
        [Fact]
        public async Task EditarAsync_TipoExiste_RetornaNoContent()
        {
            // Arrange
            var id = 1;
            var dto = new TipoDocumentoCrearDto
            {
                Codigo = "NUEVO",
                Nombre = "Nombre actualizado",
                Descripcion = "Descripción actualizada"
            };

            var existente = new TipoDocumento
            {
                Id = id,
                Codigo = "ANTERIOR",
                Nombre = "Anterior",
                Descripcion = "Vieja"
            };

            _tipoDocumentoRepositoryMock.Setup(r => r.GetTipoDocumentoPorIdAsync(id))
                                        .ReturnsAsync(existente);

            // Act
            var resultado = await _service.EditarAsync(id, dto);

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.NoContent, resultado.StatusCode);
            Assert.Null(resultado.Result);

            _tipoDocumentoRepositoryMock.Verify(r => r.ActualizarAsync(existente), Times.Once);
        }
        // 2. Prueba de error: tipo no encontrado
        [Fact]
        public async Task EditarAsync_TipoNoExiste_RetornaNotFound()
        {
            // Arrange
            var id = 99;
            var dto = new TipoDocumentoCrearDto { Codigo = "X", Nombre = "Y", Descripcion = "Z" };

            _tipoDocumentoRepositoryMock.Setup(r => r.GetTipoDocumentoPorIdAsync(id))
                                        .ReturnsAsync((TipoDocumento)null);

            // Act
            var resultado = await _service.EditarAsync(id, dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.NotFound, resultado.StatusCode);
            Assert.Contains("No encontrado", resultado.ErrorMessages);

            _tipoDocumentoRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<TipoDocumento>()), Times.Never);
        }
        // 3. Prueba de error interno: excepción
        [Fact]
        public async Task EditarAsync_Excepcion_RetornaErrorInterno()
        {
            // Arrange
            var id = 1;
            var dto = new TipoDocumentoCrearDto { Codigo = "X", Nombre = "Y", Descripcion = "Z" };

            _tipoDocumentoRepositoryMock.Setup(r => r.GetTipoDocumentoPorIdAsync(id))
                                        .ThrowsAsync(new Exception("Falló la BD"));

            // Act
            var resultado = await _service.EditarAsync(id, dto);

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, resultado.StatusCode);
            Assert.Contains("Error al editar tipo", resultado.ErrorMessages);
            Assert.Contains("Falló la BD", resultado.ErrorMessages.Last());
        }

    }

}

