namespace TelegramBot.Api.Models
{
    public class Project
    {
        public string Title { get; set; }

        public string? Description { get; set; }

        public Milestone[] Milestones { get; set; }
    }
}
