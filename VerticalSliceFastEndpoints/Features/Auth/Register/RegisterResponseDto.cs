namespace VerticalSliceFastEndpoints.Features.Auth.Register
{
    public class RegisterResponseDto
    {
        public RegisterResponseDto()
        {

            ErrorMessages = [];
        }

        public bool IsSuccess { get; set; } = true;
        public object? Result { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<string> ErrorMessages { get; set; }
    }
}
