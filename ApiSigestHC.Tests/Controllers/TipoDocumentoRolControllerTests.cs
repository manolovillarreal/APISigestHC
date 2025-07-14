using ApiSigestHC.Controllers;
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

namespace ApiSigestHC.Tests.Controllers
{
    public class TipoDocumentoRolControllerTests
    {
        private readonly Mock<ITipoDocumentoRolService> _serviceMock;
        private readonly TipoDocumentoRolController _controller;

        public TipoDocumentoRolControllerTests()
        {
            _serviceMock = new Mock<ITipoDocumentoRolService>();
            _controller = new TipoDocumentoRolController(_serviceMock.Object);
        }
        [Fact]
        public async Task Actualizar_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var dto = new TipoDocumentoRolDto { TipoDocumentoId = 1, RolId = 2 };
            var respuesta = new RespuestaAPI { IsSuccess = true, StatusCode = HttpStatusCode.NoContent };

            _serviceMock.Setup(s => s.ActualizarAsync(dto)).ReturnsAsync(respuesta);

            // Act
            var result = await _controller.Actualizar(dto);

            // Assert
            var noContent = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NoContent, noContent.StatusCode);
        }


        [Fact]
        public async Task GetPorTipoDocumento_ReturnsOk_WithLista()
        {
            // Arrange
            var tipoId = 1;
            var lista = new List<TipoDocumentoRolDto> { new TipoDocumentoRolDto { TipoDocumentoId = tipoId, RolId = 2 } };
            var respuesta = new RespuestaAPI { IsSuccess = true, StatusCode = HttpStatusCode.OK, Result = lista };

            _serviceMock.Setup(s => s.ObtenerPorTipoDocumentoAsync(tipoId)).ReturnsAsync(respuesta);

            // Act
            var result = await _controller.GetPorTipoDocumento(tipoId);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.True(((RespuestaAPI)okResult.Value!).IsSuccess);
        }

        [Fact]
        public async Task Crear_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var dto = new TipoDocumentoRolDto { TipoDocumentoId = 1, RolId = 2 };
            var respuesta = new RespuestaAPI { IsSuccess = true, StatusCode = HttpStatusCode.OK };

            _serviceMock.Setup(s => s.CrearAsync(dto)).ReturnsAsync(respuesta);

            // Act
            var result = await _controller.Crear(dto);

            // Assert
            var ok = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, ok.StatusCode);
        }

      
        [Fact]
        public async Task Eliminar_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            int tipoId = 1, rolId = 2;
            var respuesta = new RespuestaAPI { IsSuccess = true, StatusCode = HttpStatusCode.NoContent };

            _serviceMock.Setup(s => s.EliminarAsync(tipoId, rolId)).ReturnsAsync(respuesta);

            // Act
            var result = await _controller.Eliminar(tipoId, rolId);

            // Assert
            var noContent = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NoContent, noContent.StatusCode);
        }
    }

}
