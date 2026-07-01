namespace TwoDPro3.DTOs
{
    public class AgentContactResponse
    {
        public int AgentId { get; set; }
        public string AgentName { get; set; } = "";

        public string TelegramUsername { get; set; } = "";

        public string TelegramUrl { get; set; } = "";
    }
}
