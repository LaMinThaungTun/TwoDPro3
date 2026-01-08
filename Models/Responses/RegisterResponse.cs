namespace TwoDPro3.Models.Responses
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? UserId { get; set; }
    }
}
