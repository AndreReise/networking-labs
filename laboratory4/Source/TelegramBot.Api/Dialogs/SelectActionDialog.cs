using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Text;
using TelegramBot.Api.Models;

namespace TelegramBot.Api.Dialogs
{
    public class SelectActionDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<Project> projectAccessor;

        private IList<Choice> choices;

        public SelectActionDialog(UserState userState)
        {
            this.projectAccessor = userState.CreateProperty<Project>(nameof(Project));

            var waterfallSteps = new WaterfallStep[]
            {
                this.SelectOptionAsync,
                this.ConfirmOptionsAsync,
            };

            this.AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            this.AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            this.AddDialog(new ProjectCreationDialog(userState));
        }

        private async Task<DialogTurnResult> SelectOptionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            this.choices = new List<Choice>();

            var existingProject = await this.projectAccessor.GetAsync(
                stepContext.Context,
                () => null,
                cancellationToken);

            if (existingProject is null)
            {
                this.choices.Add(new Choice("Create"));
            }
            else
            {
                this.choices.Add(new Choice("Review"));
                this.choices.Add(new Choice("Edit"));
            }


            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Select next action"),
                    Choices = this.choices,
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmOptionsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var selectionResult = (stepContext.Result as FoundChoice)?.Value;

            if (selectionResult is null)
            {
                return await stepContext.ReplaceDialogAsync(nameof(SelectActionDialog), cancellationToken: cancellationToken);
            }

            switch (selectionResult)
            {
                case "Create":
                    return await stepContext.ReplaceDialogAsync(nameof(ProjectCreationDialog), cancellationToken: cancellationToken);
                case "Review":
                    return await HandleProjectReviewAsync(stepContext, cancellationToken);
                case "Edit":
                    break;
            }

            return await stepContext.ReplaceDialogAsync(nameof(SelectActionDialog), cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> HandleProjectReviewAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var existingProject = await this.projectAccessor.GetAsync(
                stepContext.Context,
                () => null,
                cancellationToken);


            var projectDetails = Helpers.BuildProjectDetailsString(existingProject);

            await stepContext.Context.SendActivityAsync(projectDetails, cancellationToken: cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
