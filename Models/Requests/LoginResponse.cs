namespace TwoDPro3.Models.Responses
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}
