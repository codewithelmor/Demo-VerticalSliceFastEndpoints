using System.Net;

namespace VerticalSliceFastEndpoints.Features.Auth.Login
{
    public class LoginResponse
    {
        public LoginResponse()
        {

            ErrorMessages = [];
        }

        public bool IsSuccess { get; set; } = true;
        public object? Result { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<string> ErrorMessages { get; set; }
    }
}
