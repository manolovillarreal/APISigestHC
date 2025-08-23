using AutoMapper;
using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;

namespace ApiSigestHC.Mappers
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Atencion, AtencionDto>()
             .ForMember(atencionDto => atencionDto.TieneCorreccionesPendientes,
                 opt => opt.MapFrom(atencion => 
                     atencion.Documentos.Any(d => d.SolicitudesCorreccion.Any(c => c.EstadoCorreccionId !=3))));

            CreateMap<Atencion,AtencionCrearDto>().ReverseMap();

            CreateMap<Documento, DocumentoDto>().ReverseMap();
            CreateMap<Documento, DocumentoConAtencionDto>().ReverseMap();

            CreateMap<DocumentoRequerido, DocumentoRequeridoDto>().ReverseMap();
            CreateMap<DocumentoRequerido, DocumentoRequeridoCrearDto>().ReverseMap();


            CreateMap<EstadoAtencion, EstadoAtencionDto>().ReverseMap();        

            CreateMap<Rol, RolDto>().ReverseMap();

            CreateMap<SolicitudCorreccion, SolicitudCorreccionDocDto>();
            CreateMap<SolicitudCorreccion, SolicitudCorreccionDto>();

            CreateMap<TipoDocumento, TipoDocumentoDto>().ReverseMap();
            CreateMap<TipoDocumento, TipoDocumentoCrearDto>().ReverseMap();

            CreateMap<TipoDocumentoRol, TipoDocumentoRolDto>().ReverseMap();
            CreateMap<TipoDocumentoRol, TipoDocumentoRolCrearDto>().ReverseMap();

            CreateMap<Usuario, UsuarioDto>().ReverseMap();
            CreateMap<Usuario, UsuarioCrearDto>().ReverseMap();
            CreateMap<Usuario, UsuarioLoginDto>().ReverseMap();
            CreateMap<Usuario, UsuarioLoginRespuestaDto>().ReverseMap();



        }
    }
}
