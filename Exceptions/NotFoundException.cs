namespace CoworkingApp.API.Exceptions
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
