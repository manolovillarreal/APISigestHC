﻿using ApiSigestHC.Servicios.IServicios;

namespace ApiSigestHC.Servicios
{
    public class VisualizacionEstadoService : IVisualizacionEstadoService
    {
        private readonly IUsuarioContextService _usuarioContextService;

        public VisualizacionEstadoService(IUsuarioContextService usuarioContextService)
        {
            _usuarioContextService = usuarioContextService;
        }
        private static readonly Dictionary<string, List<int>> EstadosPorRol = new()
            {
                { "Admisiones", new List<int> { 1, 2, 3, 4 } },
                { "Médico", new List<int> { 1, 2, 3 } },
                { "Enfermería", new List<int> { 3 } },
                { "Auditoría", new List<int> { 4, 5, 6 } },
                { "Facturación", new List<int> { 5, 6 } },
                { "Archivo", new List<int> { 7 } },
                { "Admin", new List<int> { 1, 2, 3, 4, 5, 6, 7 } }
            };

        public List<int> ObtenerEstadosVisiblesPorRol()
        {
            var rolNombre = _usuarioContextService.ObtenerRolNombre();

            if (string.IsNullOrWhiteSpace(rolNombre))
                throw new UnauthorizedAccessException("No se pudo determinar el rol del usuario");

            if (EstadosPorRol.TryGetValue(rolNombre, out var estados))
                return estados;

            // Por defecto, si el rol no está reconocido, no se permite acceso
            return new List<int>();
        }
    }

}
