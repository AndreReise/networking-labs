using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using TelegramBot.Api.Dialogs;
using TelegramBot.Api.Models;

namespace TelegramBot.Api.Bots
{
    public class CourseProjectHelperBot : ActivityHandler
    {
        private readonly ConversationState conversationState;
        private readonly UserState userState;

        private readonly IStatePropertyAccessor<UserInfo> userInfoAccessor;
        private readonly IStatePropertyAccessor<BotUserState> userBotStateAccessor;

        private readonly DialogSet dialogs;

        public CourseProjectHelperBot(ConversationState conversationState, UserState userState)
        {
            this.conversationState = conversationState;
            this.userState = userState;

            this.userInfoAccessor = this.userState.CreateProperty<UserInfo>(nameof(UserInfo));
            this.userBotStateAccessor = this.userState.CreateProperty<BotUserState>(nameof(BotUserState));

            this.dialogs = new DialogSet(this.conversationState.CreateProperty<DialogState>(nameof(DialogState)));

            this.dialogs.Add(new SelectActionDialog(this.userState));
            this.dialogs.Add(new UserCreationDialog(this.userState));
            this.dialogs.Add(new ProjectCreationDialog(this.userState));
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = new CancellationToken())
        {
            var dialogContext = await this.dialogs.CreateContextAsync(turnContext, cancellationToken);

            var dialogResult = await dialogContext.ContinueDialogAsync(cancellationToken);

            if (dialogResult.Status == DialogTurnStatus.Empty || dialogResult.Status == DialogTurnStatus.Complete)
            {
                var currentUser = await this.userInfoAccessor.GetAsync(
                    turnContext,
                    () => new UserInfo(),
                    cancellationToken);

                if (currentUser.Initialized)
                {
                    await dialogContext.BeginDialogAsync(nameof(SelectActionDialog), cancellationToken: cancellationToken);
                }
                else
                {
                    await dialogContext.BeginDialogAsync(nameof(UserCreationDialog), cancellationToken: cancellationToken);
                }
                
            }

            await this.userState.SaveChangesAsync(turnContext, false, cancellationToken);
            await this.conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
