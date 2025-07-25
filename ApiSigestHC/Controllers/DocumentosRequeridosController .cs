﻿using ApiSigestHC.Modelos.Dtos;
using ApiSigestHC.Modelos;
using ApiSigestHC.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using XAct.Library.Settings;
using System.Net;
using ApiSigestHC.Servicios.IServicios;
using Microsoft.AspNetCore.Authorization;

namespace ApiSigestHC.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentosRequeridosController : ControllerBase
    {
        private readonly IDocumentoRequeridoRepositorio _documentoRequeridoRepo;
        private readonly ICrearDocumentoRequeridoService _crearDocumentoRequeridoService;
        private readonly IMapper _mapper;

        public DocumentosRequeridosController(IDocumentoRequeridoRepositorio documentoRequeridoRepo,
                                              ICrearDocumentoRequeridoService crearDocumentoRequeridoService,
                                                IMapper mapper)
        {
            _documentoRequeridoRepo = documentoRequeridoRepo;
            _crearDocumentoRequeridoService = crearDocumentoRequeridoService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var docs = await _documentoRequeridoRepo.ObtenerTodosAsync();
                var docsDto = _mapper.Map<IEnumerable<DocumentoRequeridoDto>>(docs);

                return Ok(new RespuestaAPI
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = docsDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener los documentos requeridos.", ex.Message }
                });
            }
        }

        [HttpGet("{estadoAtencionId}")]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerPorEstado(int estadoAtencionId)
        {
            try
            {
                var docs = await _documentoRequeridoRepo.ObtenerPorEstadoAsync(estadoAtencionId);
                var docsDto = _mapper.Map<IEnumerable<DocumentoRequeridoDto>>(docs);

                return Ok(new RespuestaAPI
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = docsDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaAPI
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Error al obtener documentos por estado.", ex.Message }
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RespuestaAPI), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Crear([FromBody] DocumentoRequeridoDto dto)
        {
            var respuesta = await _crearDocumentoRequeridoService.CrearAsync(dto);
            return StatusCode((int)respuesta.StatusCode, respuesta);
        }

    }

}
