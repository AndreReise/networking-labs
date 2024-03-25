using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using TelegramBot.Api.Models;

namespace TelegramBot.Api
{
    public class UserCreationState
    {
        public bool PromptedFullName { get; set; }

        public bool PromptedEmail { get; set; }

        public bool PromptedGroup { get; set; }

        public async Task InitializeAsync(ITurnContext context)
        {
            await context.SendActivityAsync("Please, enter your full name");
        }

        public async Task<CreationState> TransitAsync(ITurnContext<IMessageActivity> context, UserInfo userInfo)
        {
            var prompt = context.Activity.Text?.Trim();

            if (string.IsNullOrEmpty(prompt))
            {
                await context.SendActivityAsync("Invalid input.Try again");

                return CreationState.Pending;
            }

            if (!this.PromptedFullName)
            {
                userInfo.FullName = prompt;
                this.PromptedFullName = true;

                await context.SendActivityAsync("Enter email");

                return CreationState.Pending;
            }

            if (!this.PromptedEmail)
            {
                userInfo.Email = prompt;
                this.PromptedEmail = true;

                await context.SendActivityAsync("Enter group code");

                return CreationState.Pending;
            }

            if (!this.PromptedGroup)
            {
                userInfo.GroupCode = prompt;
                this.PromptedGroup = true;

                await context.SendActivityAsync("Registration finished");

                return CreationState.Finished;
            }

            return CreationState.Finished;
        }
    }

    public enum CreationState
    {
        Pending,
        Finished
    }
}
