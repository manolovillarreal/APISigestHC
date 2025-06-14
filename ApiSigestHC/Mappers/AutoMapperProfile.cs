using AutoMapper;
using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;

namespace ApiSigestHC.Mappers
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Atencion,AtencionCrearDto>().ReverseMap();

            CreateMap<Usuario, UsuarioDto>().ReverseMap();
            CreateMap<Usuario, UsuarioCrearDto>().ReverseMap();
            CreateMap<Usuario, UsuarioLoginDto>().ReverseMap();
            CreateMap<Usuario, UsuarioLoginRespuestaDto>().ReverseMap();

            CreateMap<Rol, RolDto>().ReverseMap();

            CreateMap<Documento, DocumentoDto>().ReverseMap();

            CreateMap<TipoDocumento, TipoDocumentoDto>().ReverseMap();
            CreateMap<TipoDocumento, TipoDocumentoCrearDto>().ReverseMap();

            CreateMap<TipoDocumentoRol, TipoDocumentoRolDto>().ReverseMap();

            CreateMap<DocumentoRequerido, DocumentoRequeridoDto>().ReverseMap();

        }
    }
}
