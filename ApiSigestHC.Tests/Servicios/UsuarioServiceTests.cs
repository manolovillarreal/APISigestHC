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
using ApiSigestHC.Servicios.IServicios;
using Microsoft.Extensions.Configuration;

namespace ApiSigestHC.Tests.Servicios
{
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioRepositorio> _usuarioRepoMock;
        private readonly Mock<IUsuarioContextService> _usuarioContextServiceMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UsuarioService _service;

        public UsuarioServiceTests()
        {
            _usuarioRepoMock = new Mock<IUsuarioRepositorio>();
            _usuarioContextServiceMock = new Mock<IUsuarioContextService>();
            _mapperMock = new Mock<IMapper>();
            _service = new UsuarioService(_usuarioRepoMock.Object,_usuarioContextServiceMock.Object,_configMock.Object ,_mapperMock.Object);
        }

        //1. Prueba para ObtenerUsuariosAsync (éxito)

        [Fact]
        public async Task ObtenerUsuariosAsync_DeberiaRetornarListaUsuariosEnRespuestaAPI()
        {
            // Arrange
            var usuarios = new List<Usuario> { new Usuario { Id = 1, NombreUsuario = "admin" } };
            var usuariosDto = new List<UsuarioDto> { new UsuarioDto { Id = 1, NombreUsuario = "admin" } };

            _usuarioRepoMock.Setup(r => r.GetUsuariosAsync()).ReturnsAsync(usuarios);
            _mapperMock.Setup(m => m.Map<IEnumerable<UsuarioDto>>(usuarios)).Returns(usuariosDto);

            // Act
            var resultado = await _service.ObtenerUsuariosAsync();

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.Equal(usuariosDto, resultado.Result);
        }

        // 2. Prueba para ObtenerUsuarioPorIdAsync (éxito y no encontrado)
        [Fact]
        public async Task ObtenerUsuarioPorIdAsync_UsuarioExiste_DeberiaRetornarOk()
        {
            var usuario = new Usuario { Id = 1 };
            var usuarioDto = new UsuarioDto { Id = 1 };

            _usuarioRepoMock.Setup(r => r.GetUsuarioAsync(1)).ReturnsAsync(usuario);
            _mapperMock.Setup(m => m.Map<UsuarioDto>(usuario)).Returns(usuarioDto);

            var resultado = await _service.ObtenerUsuarioPorIdAsync(1);

            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.Equal(usuarioDto, resultado.Result);
        }

        [Fact]
        public async Task ObtenerUsuarioPorIdAsync_UsuarioNoExiste_DeberiaRetornarNotFound()
        {
            _usuarioRepoMock.Setup(r => r.GetUsuarioAsync(1)).ReturnsAsync((Usuario)null);

            var resultado = await _service.ObtenerUsuarioPorIdAsync(1);

            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.NotFound, resultado.StatusCode);
        }

        // 3. Prueba para CrearUsuarioAsync
        [Fact]
        public async Task CrearUsuarioAsync_UsuarioValido_DeberiaRetornarCreated()
        {
            var dto = new UsuarioCrearDto { NombreUsuario = "nuevo" };
            var usuario = new Usuario { Id = 1 };
            var usuarioDto = new UsuarioDto { Id = 1 };

            _usuarioRepoMock.Setup(r => r.IsUniqueUser(dto.NombreUsuario)).ReturnsAsync(true);
            _usuarioRepoMock.Setup(r => r.CrearUsuario(dto)).ReturnsAsync(usuario);
            _mapperMock.Setup(m => m.Map<UsuarioDto>(usuario)).Returns(usuarioDto);

            var resultado = await _service.CrearUsuarioAsync(dto);

            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.Created, resultado.StatusCode);
            Assert.Equal(usuarioDto, resultado.Result);
        }

        [Fact]
        public async Task CrearUsuarioAsync_UsuarioYaExiste_DeberiaRetornarBadRequest()
        {
            var dto = new UsuarioCrearDto { NombreUsuario = "repetido" };

            _usuarioRepoMock.Setup(r => r.IsUniqueUser(dto.NombreUsuario)).ReturnsAsync(false);

            var resultado = await _service.CrearUsuarioAsync(dto);

            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("ya existe", resultado.ErrorMessages[0]);
        }


        //4. Prueba para LoginAsync
        [Fact]
        public async Task LoginAsync_CredencialesValidas_DeberiaRetornarTokenYUsuario()
        {
            var dto = new UsuarioLoginDto { NombreUsuario = "admin", Password = "123" };
            var usuario = new UsuarioDto { Id = 1 };
            var respuestaLogin = new UsuarioLoginRespuestaDto { Usuario = usuario, Token = "abc123" };
            var usuarioDto = new UsuarioDto { Id = 1 };

            //_usuarioRepoMock.Setup(r => r.ObtenerPorCredencialesAsync(dto.NombreUsuario,dto.Password)).ReturnsAsync(usuario);
            _mapperMock.Setup(m => m.Map<UsuarioDto>(usuario)).Returns(usuarioDto);

            var resultado = await _service.LoginAsync(dto);

            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);

            UsuarioLoginRespuestaDto resultObj = (UsuarioLoginRespuestaDto)resultado.Result!;
            Assert.Equal("abc123", resultObj.Token);
            Assert.Equal(usuarioDto.Id, resultObj.Usuario.Id);
        }

        [Fact]
        public async Task LoginAsync_CredencialesInvalidas_DeberiaRetornarBadRequest()
        {
            var dto = new UsuarioLoginDto { NombreUsuario = "admin", Password = "wrong" };
            var respuestaLogin = new UsuarioLoginRespuestaDto { Usuario = null, Token = "" };

            //_usuarioRepoMock.Setup(r => r.Login(dto)).ReturnsAsync(respuestaLogin);

            var resultado = await _service.LoginAsync(dto);

            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            Assert.Contains("incorrectos", resultado.ErrorMessages[0]);
        }

        //5 Pruebas para ObtenerPerfilAsync
        [Fact]
        public async Task ObtenerPerfilAsync_RetornaPerfil_ConExito()
        {
            // Arrange
            int usuarioId = 10;
            var usuario = new Usuario { Id = usuarioId, NombreUsuario = "admin" };
            var usuarioDto = new UsuarioDto { Id = usuarioId, NombreUsuario = "admin" };

            _usuarioContextServiceMock.Setup(x => x.ObtenerUsuarioId()).Returns(usuarioId);
            _usuarioRepoMock.Setup(x => x.GetUsuarioAsync(usuarioId)).ReturnsAsync(usuario);
            _mapperMock.Setup(x => x.Map<UsuarioDto>(usuario)).Returns(usuarioDto);

            // Act
            var resultado = await _service.ObtenerPerfilAsync();

            // Assert
            Assert.True(resultado.Ok);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.Equal(usuarioDto, resultado.Result);
        }
        [Fact]
        public async Task ObtenerPerfilAsync_UsuarioNoEncontrado_RetornaNotFound()
        {
            // Arrange
            int usuarioId = 999;
            _usuarioContextServiceMock.Setup(x => x.ObtenerUsuarioId()).Returns(usuarioId);
            _usuarioRepoMock.Setup(x => x.GetUsuarioAsync(usuarioId)).ReturnsAsync((Usuario)null!);

            // Act
            var resultado = await _service.ObtenerPerfilAsync();

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.NotFound, resultado.StatusCode);
            Assert.Contains("Usuario no encontrado", resultado.ErrorMessages);
        }
        [Fact]
        public async Task ObtenerPerfilAsync_Excepcion_RetornaErrorInterno()
        {
            // Arrange
            _usuarioContextServiceMock.Setup(x => x.ObtenerUsuarioId()).Throws(new Exception("Error de prueba"));

            // Act
            var resultado = await _service.ObtenerPerfilAsync();

            // Assert
            Assert.False(resultado.Ok);
            Assert.Equal(HttpStatusCode.InternalServerError, resultado.StatusCode);
            Assert.Contains("Error al obtener perfil", resultado.ErrorMessages);
        }



    }
}
