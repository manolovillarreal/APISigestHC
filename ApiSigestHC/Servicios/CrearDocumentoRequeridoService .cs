using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using System.Net;

namespace ApiSigestHC.Servicios
{
    public class CrearDocumentoRequeridoService : ICrearDocumentoRequeridoService
    {
        private readonly IDocumentoRequeridoRepositorio _documentoRequeridoRepo;
        private readonly IEstadoAtencionRepositorio _estadoAtencionRepo;
        private readonly ITipoDocumentoRepositorio _tipoDocumentoRepo;
        private readonly IMapper _mapper;

        public CrearDocumentoRequeridoService(
            IDocumentoRequeridoRepositorio documentoRequeridoRepo,
            IEstadoAtencionRepositorio estadoAtencionRepo,
            ITipoDocumentoRepositorio tipoDocumentoRepo,
            IMapper mapper)
        {
            _documentoRequeridoRepo = documentoRequeridoRepo;
            _estadoAtencionRepo = estadoAtencionRepo;
            _tipoDocumentoRepo = tipoDocumentoRepo;
            _mapper = mapper;
        }

        public async Task<RespuestaAPI> CrearAsync(DocumentoRequeridoDto dto)
        {
            try
            {
                var estado = await _estadoAtencionRepo.ObtenerPorIdAsync(dto.EstadoAtencionId);
                if (estado == null)
                {
                    return new RespuestaAPI
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "El estado de atención no existe." }
                    };
                }

                if (estado.Orden <= 1)
                {
                    return new RespuestaAPI
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "No se pueden registrar documentos requeridos para el estado inicial." }
                    };
                }

                var tipoDocumento = await _tipoDocumentoRepo.GetTipoDocumentoPorIdAsync(dto.TipoDocumentoId);
                if (tipoDocumento == null)
                {
                    return new RespuestaAPI
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "El tipo de documento no existe." }
                    };
                }

                var yaExiste = await _documentoRequeridoRepo.ExisteAsync(dto.EstadoAtencionId, dto.TipoDocumentoId);
                if (yaExiste)
                {
                    return new RespuestaAPI
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "Este documento ya está registrado como requerido." }
                    };
                }

                var nuevo = _mapper.Map<DocumentoRequerido>(dto);
                await _documentoRequeridoRepo.AgregarAsync(nuevo);

                var resultDto = _mapper.Map<DocumentoRequeridoDto>(nuevo);

                return new RespuestaAPI
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Created,
                    Result = resultDto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string>
                {
                    "Ocurrió un error inesperado al registrar el documento requerido.",
                    ex.Message
                }
                };
            }
        }
    }
}
