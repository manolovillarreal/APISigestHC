using ApiSigestHC.Servicios.IServicios;

namespace ApiSigestHC.Servicios
{
    public class FileSystemService : IFileSystemService
    {
        public bool DirectoryExists(string path) => Directory.Exists(path);
    }
}
