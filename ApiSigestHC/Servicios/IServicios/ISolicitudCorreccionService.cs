using ApiSigestHC.Modelos;
using ApiSigestHC.Modelos.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ApiSigestHC.Servicios.IServicios
{
    public interface ISolicitudCorreccionService
    {
        Task<RespuestaAPI> AprobarSolicitudAsync(int solicitudId,SolicitudCorreccionAprobarDto dto);
        Task<RespuestaAPI> CrearAsync(SolicitudCorreccionCrearDto solicitud);
        Task<RespuestaAPI> ObtenerPorDocumentoAsync(int documentoId);
        Task<RespuestaAPI> ObtenerPorRolUsuarioAsync();
        Task<RespuestaAPI> ResponderSolicitudAsync(int id, SolicitudCorreccionRespuestaDto dto);
        Task<IActionResult> VerDocumentoCorreccion(int solicitudId);
    }
}