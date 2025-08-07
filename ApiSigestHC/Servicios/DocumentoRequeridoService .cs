using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Servicios.IServicios;
using AutoMapper;
using System.Net;

namespace ApiSigestHC.Servicios
{
    public class DocumentoRequeridoService : IDocumentoRequeridoService
    {
        private readonly IDocumentoRequeridoRepositorio _documentoRequeridoRepo;
        private readonly IEstadoAtencionRepositorio _estadoAtencionRepo;
        private readonly ITipoDocumentoRepositorio _tipoDocumentoRepo;
        private readonly IMapper _mapper;

        public DocumentoRequeridoService(
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

        public async Task<RespuestaAPI> CrearAsync(DocumentoRequeridoCrearDto dto)
        {
            try
            {
                var estado = await _estadoAtencionRepo.ObtenerPorIdAsync(dto.EstadoAtencionId);
                if (estado == null)
                {
                    return new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "El estado de atención no existe." }
                    };
                }

               

                var tipoDocumento = await _tipoDocumentoRepo.GetTipoDocumentoPorIdAsync(dto.TipoDocumentoId);
                if (tipoDocumento == null)
                {
                    return new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "El tipo de documento no existe." }
                    };
                }

                var yaExiste = await _documentoRequeridoRepo.ExistePorTipoAsync(dto.TipoDocumentoId);
                if (yaExiste)
                {
                    return new RespuestaAPI
                    {
                        Ok = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = new List<string> { "Este documento ya está registrado como requerido en otro estado." }
                    };
                }

                var nuevo = _mapper.Map<DocumentoRequerido>(dto);
                await _documentoRequeridoRepo.AgregarAsync(nuevo);

                var resultDto = _mapper.Map<DocumentoRequeridoCrearDto>(nuevo);

                return new RespuestaAPI
                {
                    Ok = true,
                    StatusCode = HttpStatusCode.Created,
                    Result = resultDto
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string>
                {
                    "Ocurrió un error inesperado al registrar el documento requerido.",
                    ex.Message
                }
                };
            }
        }

        public async Task<RespuestaAPI> EliminarAsync(int tipoDocumentoId)
        {
            var tipoDocumento = await _tipoDocumentoRepo.GetTipoDocumentoPorIdAsync(tipoDocumentoId);
            if (tipoDocumento == null)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "El tipo de documento no existe." }
                };
            }

            var yaExiste = await _documentoRequeridoRepo.ExistePorTipoAsync(tipoDocumentoId);
            if (!yaExiste)
            {
                return new RespuestaAPI
                {
                    Ok = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Este documento no está registrado como requerido en ningun estado." }
                };
            }

            await _documentoRequeridoRepo.EliminarAsync(tipoDocumentoId);

            return new RespuestaAPI
            {
                Ok = true,
                StatusCode = HttpStatusCode.OK,
                Result = "Documento requerido eliminado"
            };
        }
    }
}
