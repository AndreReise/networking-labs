using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Text;
using TelegramBot.Api.Models;

namespace TelegramBot.Api.Dialogs
{
    public class ProjectCreationDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<Project> projectAccessor;
        private readonly List<Milestone> milestoneContainer = new();
        public ProjectCreationDialog(UserState userState):
            base(nameof(ProjectCreationDialog))
        {

            this.projectAccessor = userState.CreateProperty<Project>(nameof(Project));

            var waterfallSteps = new WaterfallStep[]
            {
                this.TitleAsync,
                this.DescriptionAsync,
                this.MilestonesAsync,
                this.ConfirmAsync,
                this.FinishAsync
            };

            this.AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            this.AddDialog(new TextPrompt(nameof(TextPrompt)));
            this.AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            this.AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            this.AddDialog(new MilestoneCreationDialog(userState, milestoneContainer));
        }

        private async Task<DialogTurnResult> TitleAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Enter the project title"),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> DescriptionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["title"] = (string) stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Enter the project description"),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> MilestonesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["description"] = (string)stepContext.Result;

            stepContext.Values["milestones"] = new List<Milestone>();

            return await stepContext.BeginDialogAsync(nameof(MilestoneCreationDialog), cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var project = await this.projectAccessor.GetAsync(
                stepContext.Context,
                () => new Project(),
                cancellationToken);

            project.Title = (string) stepContext.Values["title"];
            project.Description = (string)stepContext.Values["description"];
            project.Milestones = this.milestoneContainer.ToArray();

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text(Helpers.BuildProjectDetailsString(project))
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinishAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                await stepContext.Context.SendActivityAsync("Project will be set", cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Project will not be set", cancellationToken: cancellationToken);

                await this.projectAccessor.DeleteAsync(stepContext.Context, cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(
            DialogContext outerDc,
            DialogReason reason,
            object result = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (result is bool and true)
            {
                return await outerDc.BeginDialogAsync(nameof(MilestoneCreationDialog), cancellationToken: cancellationToken);
            }

            return await base.ResumeDialogAsync(outerDc, reason, result, cancellationToken);
        }
    }
}
