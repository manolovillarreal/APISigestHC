using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios;
using ApiSigestHC.Servicios.IServicios;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApiSigestHC.Tests.Servicios
{
    public class CambiarEsadoServiceTests
    {
        private readonly Mock<IAtencionRepositorio> _atencionRepoMock;
        private readonly Mock<ICambioEstadoRepositorio> _cambioEstadoRepoMock;
        private readonly Mock<IUsuarioContextService> _usuarioContextMock;
        private readonly Mock<IValidacionDocumentosObligatoriosService> _validacionDocServiceMock;
        private readonly ICambioEstadoService _service;

        public CambiarEsadoServiceTests()
        {
            _atencionRepoMock = new Mock<IAtencionRepositorio>();
            _cambioEstadoRepoMock = new Mock<ICambioEstadoRepositorio>();
            _usuarioContextMock = new Mock<IUsuarioContextService>();
            _validacionDocServiceMock = new Mock<IValidacionDocumentosObligatoriosService>();

            _service = new CambioEstadoService(
                _atencionRepoMock.Object,
                _cambioEstadoRepoMock.Object,
                _validacionDocServiceMock.Object,
                _usuarioContextMock.Object
            );
        }

        [Fact]
        //1. Debe retornar NotFound si la atención no existe
        public async Task CambiarEstadoAsync_DebeRetornarNotFound_SiAtencionNoExiste()
        {
            // Arrange
            _atencionRepoMock.Setup(r => r.ObtenerAtencionPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Atencion)null);

            var dto = new AtencionCambioEstadoDto { AtencionId = 1 };

            // Act
            var respuesta = await _service.CambiarEstadoAsync(dto);

            // Assert
            Assert.False(respuesta.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
            Assert.Contains("La atención no existe", respuesta.ErrorMessages);
        }

        [Fact]
        //2. Debe retornar Forbidden si el rol no puede cambiar el estado
        public async Task CambiarEstadoAsync_DebeRetornarForbidden_SiRolNoPuedeCambiarEstado()
        {
            // Arrange
            var atencion = new Atencion { Id = 1, EstadoAtencionId = 3 }; // Estado donde el rol no tiene permiso
            _atencionRepoMock.Setup(r => r.ObtenerAtencionPorIdAsync(1))
                .ReturnsAsync(atencion);
            _usuarioContextMock.Setup(u => u.ObtenerRolNombre())
                .Returns("Facturación"); // Rol que no puede avanzar desde estado 3

            var dto = new AtencionCambioEstadoDto { AtencionId = 1 };

            // Act
            var respuesta = await _service.CambiarEstadoAsync(dto);

            // Assert
            Assert.False(respuesta.IsSuccess);
            Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
            Assert.Contains("no puede cambiar desde el estado", respuesta.ErrorMessages[0]);
        }
        [Fact]
        //3. Debe retornar BadRequest si faltan documentos requeridos
        public async Task CambiarEstadoAsync_DebeRetornarBadRequest_SiFaltanDocumentos()
        {
            // Arrange
            var atencion = new Atencion { Id = 1, EstadoAtencionId = 2 };
            _atencionRepoMock.Setup(r => r.ObtenerAtencionPorIdAsync(1))
                .ReturnsAsync(atencion);
            _usuarioContextMock.Setup(u => u.ObtenerRolNombre()).Returns("Medico");
            _validacionDocServiceMock.Setup(v => v.ValidarDocumentosObligatoriosAsync(atencion))
                .ReturnsAsync(new ResultadoValidacionDto
                {
                    EsValido = false,
                    DocumentosFaltantes = new List<string> { "Historia Clínica", "Consentimiento Informado" }
                });

            var dto = new AtencionCambioEstadoDto { AtencionId = 1 };

            // Act
            var respuesta = await _service.CambiarEstadoAsync(dto);

            // Assert
            Assert.False(respuesta.IsSuccess);
            Assert.Contains("Faltan documentos requeridos", respuesta.ErrorMessages[0]);
            Assert.Contains("Historia Clínica", respuesta.ErrorMessages);
            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        }

        [Fact]
        //4. Debe cambiar el estado y registrar cambio exitosamente
        public async Task CambiarEstadoAsync_DebeActualizarEstadoYRegistrarCambio_SiTodoEsValido()
        {
            // Arrange
            var atencion = new Atencion { Id = 1, EstadoAtencionId = 1 };
            _atencionRepoMock.Setup(r => r.ObtenerAtencionPorIdAsync(1))
                .ReturnsAsync(atencion);
            _usuarioContextMock.Setup(u => u.ObtenerRolNombre()).Returns("Admisiones");
            _usuarioContextMock.Setup(u => u.ObtenerUsuarioId()).Returns(123);
            _validacionDocServiceMock.Setup(v => v.ValidarDocumentosObligatoriosAsync(atencion))
                .ReturnsAsync(new ResultadoValidacionDto { EsValido = true });

            var dto = new AtencionCambioEstadoDto
            {
                AtencionId = 1,
                Obervaciones = "Todo en orden"
            };

            // Act
            var respuesta = await _service.CambiarEstadoAsync(dto);
            atencion.EstadoAtencionId = 2; 
            // Assert
            Assert.True(respuesta.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.Equal(atencion, respuesta.Result);

            _cambioEstadoRepoMock.Verify(c => c.RegistrarCambioAsync(It.Is<CambioEstado>(
                ce => ce.AtencionId == 1 && ce.EstadoNuevo == 2 && ce.UsuarioId == 123
            )), Times.Once);

            _atencionRepoMock.Verify(a => a.EditarAtencionAsync(atencion), Times.Once);
        }




    }
}
