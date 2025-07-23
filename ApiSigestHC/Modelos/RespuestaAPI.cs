using System.Net;

namespace ApiSigestHC.Modelos
{
    public class RespuestaAPI
    {
        public RespuestaAPI()
        {
            ErrorMessages = new List<string>();
        }

        public HttpStatusCode StatusCode { get; set; }
        public int CustomCode { get; set; }
        public bool Ok { get; set; }
        public string Message { get; set; }
        public List<string> ErrorMessages { get;  set; }
        public object Result { get; set; }
    }
}
