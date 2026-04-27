namespace SmartWalletAI.WebAPI.Middlewares
{
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public object? Errors { get; set; }
        public string? Detail { get; set; }
    }
}
