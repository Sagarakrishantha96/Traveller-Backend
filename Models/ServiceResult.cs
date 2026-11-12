namespace Traveller_backend.Models
{
    // Result returned from the service layer to the controller
    public class ServiceResult<T>
    {
        public bool Success { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorMessage { get; private set; }
        public bool IsNotFound { get; private set; }          // lets the controller return 404 instead of 400

        public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };

        public static ServiceResult<T> Fail(string errorMessage) => new() { Success = false, ErrorMessage = errorMessage };

        public static ServiceResult<T> NotFound(string errorMessage) => new() { Success = false, ErrorMessage = errorMessage, IsNotFound = true };
    }
}
