using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using ApiSigestHC.Servicios;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using System.Net;

namespace ApiSigestHC.Tests.Servicios
{
    public class TipoDocumentoRolServiceTests
    {
        private readonly Mock<ITipoDocumentoRolRepositorio> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly TipoDocumentoRolService _service;

        public TipoDocumentoRolServiceTests()
        {
            _repoMock = new Mock<ITipoDocumentoRolRepositorio>();
            _mapperMock = new Mock<IMapper>();
            _service = new TipoDocumentoRolService(_repoMock.Object, _mapperMock.Object);
        }

        //1. Prueba de éxito: ObtenerPorTipoDocumentoAsync
        [Fact]
        public async Task ObtenerPorTipoDocumentoAsync_RetornaTiposRelacionados_Correctamente()
        {
            // Arrange
            int tipoDocumentoId = 1;
            var entidades = new List<TipoDocumentoRol> { new TipoDocumentoRol { RolId = 2, TipoDocumentoId = 1 } };
            var dtos = new List<TipoDocumentoRolDto> { new TipoDocumentoRolDto { RolId = 2, TipoDocumentoId = 1 } };

            _repoMock.Setup(r => r.GetPorTipoDocumentoAsync(tipoDocumentoId)).ReturnsAsync(entidades);
            _mapperMock.Setup(m => m.Map<IEnumerable<TipoDocumentoRolDto>>(entidades)).Returns(dtos);

            // Act
            var resultado = await _service.ObtenerPorTipoDocumentoAsync(tipoDocumentoId);

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.Equal(dtos, resultado.Result);
        }

        //2. Prueba de éxito: CrearAsync
        [Fact]
        public async Task CrearAsync_CreaRelacion_Correctamente()
        {
            // Arrange
            var dto = new TipoDocumentoRolDto { RolId = 1, TipoDocumentoId = 2 };
            var entidad = new TipoDocumentoRol { RolId = 1, TipoDocumentoId = 2 };

            _mapperMock.Setup(m => m.Map<TipoDocumentoRol>(dto)).Returns(entidad);
            _repoMock.Setup(r => r.CrearAsync(entidad)).Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.CrearAsync(dto);

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
        }

        //3. Prueba de éxito: ActualizarAsync
        [Fact]
        public async Task ActualizarAsync_ActualizaRelacion_Correctamente()
        {
            // Arrange
            var dto = new TipoDocumentoRolDto { RolId = 1, TipoDocumentoId = 2 };
            var existente = new TipoDocumentoRol { RolId = 1, TipoDocumentoId = 2 };

            _repoMock.Setup(r => r.GetPorIdsAsync(dto.TipoDocumentoId, dto.RolId)).ReturnsAsync(existente);
            _mapperMock.Setup(m => m.Map(dto, existente));
            _repoMock.Setup(r => r.ActualizarAsync(existente)).Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.ActualizarAsync(dto);

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.NoContent, resultado.StatusCode);
        }

        //4. Prueba de éxito: EliminarAsync

        [Fact]
        public async Task EliminarAsync_EliminaRelacion_Correctamente()
        {
            // Arrange
            int tipoDocumentoId = 1, rolId = 2;
            var entidad = new TipoDocumentoRol { TipoDocumentoId = tipoDocumentoId, RolId = rolId };

            _repoMock.Setup(r => r.GetPorIdsAsync(tipoDocumentoId, rolId)).ReturnsAsync(entidad);
            _repoMock.Setup(r => r.EliminarAsync(entidad)).Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.EliminarAsync(tipoDocumentoId, rolId);

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.NoContent, resultado.StatusCode);
        }

        #region Casos de Error

        //1. ActualizarAsync: Rol no encontrado
        [Fact]
        public async Task ActualizarAsync_CuandoRelacionNoExiste_RetornaNotFound()
        {
            // Arrange
            var dto = new TipoDocumentoRolDto { TipoDocumentoId = 1, RolId = 2 };

            _repoMock.Setup(r => r.GetPorIdsAsync(dto.TipoDocumentoId, dto.RolId)).ReturnsAsync((TipoDocumentoRol)null);

            // Act
            var resultado = await _service.ActualizarAsync(dto);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, resultado.StatusCode);
            Assert.Contains("no encontrada", resultado.ErrorMessages.First().ToLower());
        }

        //2. EliminarAsync: Relación no existe
        [Fact]
        public async Task EliminarAsync_CuandoRelacionNoExiste_RetornaNotFound()
        {
            // Arrange
            int tipoDocumentoId = 1, rolId = 2;

            _repoMock.Setup(r => r.GetPorIdsAsync(tipoDocumentoId, rolId)).ReturnsAsync((TipoDocumentoRol)null);

            // Act
            var resultado = await _service.EliminarAsync(tipoDocumentoId, rolId);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, resultado.StatusCode);
            Assert.Contains("no encontrada", resultado.ErrorMessages.First().ToLower());
        }


        //CrearAsync: Lanza excepción
        [Fact]
        public async Task CrearAsync_CuandoFallaInternamente_RetornaErrorInterno()
        {
            // Arrange
            var dto = new TipoDocumentoRolDto { TipoDocumentoId = 1, RolId = 2 };
            _mapperMock.Setup(m => m.Map<TipoDocumentoRol>(dto)).Throws(new Exception("Error simulado"));

            // Act
            var resultado = await _service.CrearAsync(dto);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.InternalServerError, resultado.StatusCode);
            Assert.Contains("error", resultado.ErrorMessages.First().ToLower());
        }

        //4. ActualizarAsync: Falla al actualizar
        [Fact]
        public async Task ActualizarAsync_CuandoFallaInternamente_RetornaErrorInterno()
        {
            // Arrange
            var dto = new TipoDocumentoRolDto { TipoDocumentoId = 1, RolId = 2 };
            var entidad = new TipoDocumentoRol();

            _repoMock.Setup(r => r.GetPorIdsAsync(dto.TipoDocumentoId, dto.RolId)).ReturnsAsync(entidad);
            _mapperMock.Setup(m => m.Map(dto, entidad));
            _repoMock.Setup(r => r.ActualizarAsync(entidad)).Throws(new Exception("Falla simulada"));

            // Act
            var resultado = await _service.ActualizarAsync(dto);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.InternalServerError, resultado.StatusCode);
            Assert.Contains("Falla simulada", string.Join(",", resultado.ErrorMessages));
        }

        //5. EliminarAsync: Falla al eliminar
        [Fact]
        public async Task EliminarAsync_CuandoFallaInternamente_RetornaErrorInterno()
        {
            // Arrange
            int tipoDocumentoId = 1, rolId = 2;
            var entidad = new TipoDocumentoRol();

            _repoMock.Setup(r => r.GetPorIdsAsync(tipoDocumentoId, rolId)).ReturnsAsync(entidad);
            _repoMock.Setup(r => r.EliminarAsync(entidad)).Throws(new Exception("Fallo en la base de datos"));

            // Act
            var resultado = await _service.EliminarAsync(tipoDocumentoId, rolId);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(HttpStatusCode.InternalServerError, resultado.StatusCode);
            Assert.Contains("eliminar", string.Join(",", resultado.ErrorMessages).ToLower());
        }



        #endregion

    }

}
