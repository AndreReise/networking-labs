using System.Text;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using TelegramBot.Api.Models;

namespace TelegramBot.Api.Dialogs
{
    public class UserCreationDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserInfo> userInfoAccessor;

        public UserCreationDialog(UserState userState):
            base(nameof(UserCreationDialog))
        {
            this.userInfoAccessor = userState.CreateProperty<UserInfo>(nameof(UserInfo));

            var waterfallSteps = new WaterfallStep[]
            {
                this.FullNameStepAsync,
                this.EmailStepAsync,
                this.GroupStepAsync,
                this.ConfirmStepAsync,
                this.FinishStepAsync
            };

            this.AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            this.AddDialog(new TextPrompt(nameof(TextPrompt)));
            this.AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            this.AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
        }

        private async Task<DialogTurnResult> FullNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please enter your full name"),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Store previous step result
            stepContext.Values["full-name"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please enter your email"),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> GroupStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["email"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Select your group"),
                    Choices = ChoiceFactory.ToChoices(new string[] { "PZPI-20-1", "PZPI-20-2", "PZPI-20-3", "PZPI-20-4" }),
                });
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["group"] = (stepContext.Result as FoundChoice)?.Value ?? "NA";

            var sb = new StringBuilder();

            sb.AppendLine("Is this ok?");
            sb.AppendLine(stepContext.Values["full-name"].ToString());
            sb.AppendLine(stepContext.Values["email"].ToString());
            sb.AppendLine(stepContext.Values["group"].ToString());

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text(sb.ToString())
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinishStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool) stepContext.Result)
            {
                var userInfo = await this.userInfoAccessor.GetAsync(
                    stepContext.Context,
                    () => new UserInfo(),
                    cancellationToken);

                userInfo.FullName = (string)stepContext.Values["full-name"];
                userInfo.Email = (string)stepContext.Values["email"];
                userInfo.GroupCode = (string)stepContext.Values["group"];
                userInfo.Initialized = true;

                await stepContext.Context.SendActivityAsync("Your profile will be set");
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Your profile will not be set");
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
