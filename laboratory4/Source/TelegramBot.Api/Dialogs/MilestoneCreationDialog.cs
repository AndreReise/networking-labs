using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using TelegramBot.Api.Models;

namespace TelegramBot.Api.Dialogs
{
    public class MilestoneCreationDialog : ComponentDialog
    {
        private readonly List<Milestone> milestonesContainer;

        public MilestoneCreationDialog(UserState userState, List<Milestone> milestonesContainer) :
            base()
        {
            this.milestonesContainer = milestonesContainer;

            var waterfallSteps = new WaterfallStep[]
            {
                this.NameAsync,
                this.NotesAsync,
                this.TimeAsync,
                this.ConfirmAsync,
                this.ProceedWithCreationAsync,
                this.FinishAsync,
            };

            this.AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            this.AddDialog(new TextPrompt(nameof(TextPrompt)));
            this.AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            this.AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
        }

        private async Task<DialogTurnResult> NameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please enter the milestone name"),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> NotesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["milestone-name"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please enter the milestone notes"),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> TimeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["milestone-notes"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please enter the milestone end time"),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["milestone-date"] = (string)stepContext.Result;

            var milestone = new Milestone()
            {
                Name = (string)stepContext.Values["milestone-name"],
                Notes = (string)stepContext.Values["milestone-notes"],
                DateTimeUtc = (string)stepContext.Values["milestone-date"],
            };

            var milestoneDetails = Helpers.BuildMilestoneDetailsString(milestone);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text(milestoneDetails)
            }, cancellationToken);

        }

        private async Task<DialogTurnResult> ProceedWithCreationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var milestone = new Milestone()
                {
                    Name = (string)stepContext.Values["milestone-name"],
                    Notes = (string)stepContext.Values["milestone-notes"],
                    DateTimeUtc = (string)stepContext.Values["milestone-date"],
                };

                this.milestonesContainer.Add(milestone);

                await stepContext.Context.SendActivityAsync("Milestone is saved");
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Milestone will not be saved");
            }

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Create another milestone?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinishAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(result: stepContext.Result, cancellationToken: cancellationToken);
        }
    }
}
