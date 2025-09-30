namespace WebAPI.Responses
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public ApiResponse(T data, string? message = null)
        {
            IsSuccess = true;
            Data = data;
            Message = message;
        }
        public ApiResponse(string message)
        {
            IsSuccess = true;
            Message = message;
        }
    }

    public class ApiResponse(string message = "Operation success") : ApiResponse<object>(message)
    {
    }
}
