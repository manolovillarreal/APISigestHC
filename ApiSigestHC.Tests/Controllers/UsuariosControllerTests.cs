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
    public class UsuariosControllerTests
    {
        private readonly Mock<IUsuarioService> _usuarioServiceMock;
        private readonly UsuariosController _controller;

        public UsuariosControllerTests()
        {
            _usuarioServiceMock = new Mock<IUsuarioService>();
            _controller = new UsuariosController(_usuarioServiceMock.Object);
        }

        // 1. GetUsuarios
        [Fact]
        public async Task GetUsuarios_DeberiaRetornarOkConLista()
        {
            // Arrange
            var respuesta = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = new List<UsuarioDto> { new UsuarioDto { Id = 1 } }
            };

            _usuarioServiceMock.Setup(s => s.ObtenerUsuariosAsync())
                               .ReturnsAsync(respuesta);

            // Act
            var resultado = await _controller.GetUsuarios();

            // Assert
            var okResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.Equal(respuesta, okResult.Value);
        }

        //2. GetUsuario
        [Fact]
        public async Task GetUsuario_Existe_DeberiaRetornarOk()
        {
            var respuesta = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = new UsuarioDto { Id = 1 }
            };

            _usuarioServiceMock.Setup(s => s.ObtenerUsuarioPorIdAsync(1))
                               .ReturnsAsync(respuesta);

            var resultado = await _controller.GetUsuario(1);

            var okResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.Equal(respuesta, okResult.Value);
        }

        //3. CrearUsuario
        [Fact]
        public async Task CrearUsuario_Valido_DeberiaRetornarCreated()
        {
            var dto = new UsuarioCrearDto { NombreUsuario = "nuevo" };

            var respuesta = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.Created
            };

            _usuarioServiceMock.Setup(s => s.CrearUsuarioAsync(dto))
                               .ReturnsAsync(respuesta);

            var resultado = await _controller.CrearUsuario(dto);

            var result = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        }

        //4. Login
        [Fact]
        public async Task Login_Exitoso_DeberiaRetornarOk()
        {
            var dto = new UsuarioLoginDto { NombreUsuario = "admin", Password = "123" };

            var respuesta = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = new { Token = "abc123", Usuario = new UsuarioDto { Id = 1 } }
            };

            _usuarioServiceMock.Setup(s => s.LoginAsync(dto))
                               .ReturnsAsync(respuesta);

            var resultado = await _controller.Login(dto);

            var okResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.Equal(respuesta, okResult.Value);
        }

        //5. ObtenerPerfil
        [Fact]
        public async Task ObtenerPerfil_ReturnsOkConRespuestaAPI()
        {
            // Arrange
            var usuarioDto = new UsuarioDto
            {
                Id = 1,
                NombreUsuario = "admin"
            };

            var respuestaEsperada = new RespuestaAPI
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = usuarioDto
            };

            _usuarioServiceMock
                .Setup(s => s.ObtenerPerfilAsync())
                .ReturnsAsync(respuestaEsperada);

            // Act
            var resultado = await _controller.ObtenerPerfil();

            // Assert
            var okResult = Assert.IsType<ObjectResult>(resultado);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            var respuesta = Assert.IsType<RespuestaAPI>(okResult.Value);
            Assert.True(respuesta.IsSuccess);
            Assert.Equal(usuarioDto, respuesta.Result);
        }

    }
}
