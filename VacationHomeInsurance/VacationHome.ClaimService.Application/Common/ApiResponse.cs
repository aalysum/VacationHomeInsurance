namespace VacationHome.ClaimService.Application.Common
{
    public sealed class ApiResponse<T>
    {
        public bool Success { get; set; }


        public string Message { get; set; } = string.Empty;


        public T? Data { get; set; }


        public IEnumerable<string> Errors { get; set; }
            = Enumerable.Empty<string>();


        public static ApiResponse<T> SuccessResponse(
            T data,
            string message = "")
        {
            return new()
            {
                Success = true,
                Message = message,
                Data = data
            };
        }


        public static ApiResponse<T> FailureResponse(
            IEnumerable<string> errors,
            string message = "")
        {
            return new()
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}
