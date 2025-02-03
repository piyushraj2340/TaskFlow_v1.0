namespace TaskMonitoringApp.Models
{
    public class ApiResponseModel<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }

        public ApiResponseModel(bool status, string message, T data)
        {
            Status = status;
            Message = message;
            Data = data;
        }

        public ApiResponseModel(string message)
        {
            Status = false;
            Message = message;
            Data = default;  // This will set Data to null (reference types) or 0 (value types)
        }
    }

}
