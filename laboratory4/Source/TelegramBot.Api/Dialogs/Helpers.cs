using System.Text;
using TelegramBot.Api.Models;

namespace TelegramBot.Api.Dialogs
{
    public class Helpers
    {
        public static string BuildProjectDetailsString(Project project)
        {
            var sb = new StringBuilder();

            sb.AppendLine(project.Title)
                .AppendLine()
                .AppendLine(project.Description)
                .AppendLine();

            for (int i = 0; i < project.Milestones.Length; i++)
            {
                var milestone = project.Milestones[i];

                sb
                    .AppendLine($"{i}:")
                    .AppendLine(milestone.Name)
                    .AppendLine(milestone.Notes)
                    .AppendLine(milestone.DateTimeUtc);
            }

            return sb.ToString();
        }

        public static string BuildMilestoneDetailsString(Milestone milestone)
        {
            var sb = new StringBuilder();

            sb.AppendLine(milestone.Name)
                .AppendLine()
                .AppendLine(milestone.Notes)
                .AppendLine()
                .AppendLine(milestone.DateTimeUtc);

            return sb.ToString();
        }
    }
}
