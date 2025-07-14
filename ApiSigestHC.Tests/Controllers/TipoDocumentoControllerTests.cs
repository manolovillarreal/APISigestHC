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
    public class TipoDocumentoControllerTests
    {
        private readonly Mock<ITipoDocumentoService> _serviceMock;
        private readonly TipoDocumentoController _controller;

        public TipoDocumentoControllerTests()
        {
            _serviceMock = new Mock<ITipoDocumentoService>();
            _controller = new TipoDocumentoController(_serviceMock.Object);
        }

        //1. ObtenerTodos - Éxito
        [Fact]
        public async Task ObtenerTodos_RetornaOkConRespuesta()
        {
            // Arrange
            var respuesta = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = new List<TipoDocumentoDto> { new TipoDocumentoDto { Id = 1, Codigo = "COD", Nombre = "Nombre" } }
            };

            _serviceMock.Setup(s => s.ObtenerTodosAsync()).ReturnsAsync(respuesta);

            // Act
            var result = await _controller.ObtenerTodos();

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(respuesta, okResult.Value);
        }

        //2. ObtenerPorId - Éxito
        [Fact]
        public async Task ObtenerPorId_RetornaOkConRespuesta()
        {
            // Arrange
            var id = 1;
            var respuesta = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = new TipoDocumentoDto { Id = id, Codigo = "X", Nombre = "Test" }
            };

            _serviceMock.Setup(s => s.ObtenerPorIdAsync(id)).ReturnsAsync(respuesta);

            // Act
            var result = await _controller.ObtenerPorId(id);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(respuesta, okResult.Value);
        }

        //3. ObtenerTiposPermitidosPorRol - Éxito
        [Fact]
        public async Task ObtenerTiposPermitidosPorRol_RetornaOkConRespuesta()
        {
            // Arrange
            var respuesta = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = new List<TipoDocumentoDto>()
            };

            _serviceMock.Setup(s => s.ObtenerTiposPermitidosPorRolAsync()).ReturnsAsync(respuesta);

            // Act
            var result = await _controller.ObtenerTiposPermitidosPorRol();

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(respuesta, okResult.Value);
        }

        // 4. Crear - Éxito
        [Fact]
        public async Task Crear_RetornaCreatedConRespuesta()
        {
            // Arrange
            var dto = new TipoDocumentoCrearDto { Codigo = "NUEVO", Nombre = "Nombre" };

            var respuesta = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.Created,
                Result = new TipoDocumentoDto { Id = 1, Codigo = "NUEVO", Nombre = "Nombre" }
            };

            _serviceMock.Setup(s => s.CrearAsync(dto)).ReturnsAsync(respuesta);

            // Act
            var result = await _controller.Crear(dto);

            // Assert
            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(respuesta, createdResult.Value);
        }

        //5. Editar - Éxito
        [Fact]
        public async Task Editar_RetornaNoContentConRespuesta()
        {
            // Arrange
            var id = 1;
            var dto = new TipoDocumentoCrearDto { Codigo = "ACT", Nombre = "Actualizado" };

            var respuesta = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.NoContent
            };

            _serviceMock.Setup(s => s.EditarAsync(id, dto)).ReturnsAsync(respuesta);

            // Act
            var result = await _controller.Editar(id, dto);

            // Assert
            var noContentResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
            Assert.Equal(respuesta, noContentResult.Value);
        }

    }
}
