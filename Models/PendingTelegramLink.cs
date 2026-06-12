namespace TwoDPro3.Models
{
    public class PendingTelegramLink
    {
        public int Id { get; set; }

        public string Token { get; set; } = "";

        public string PhoneNumber { get; set; } = "";

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
