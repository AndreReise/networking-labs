namespace TelegramBot.Api
{
    public class BotUserState
    {
        public State State { get; set; }

        public object StateObject { get; set; }
    }

    public enum State
    {
        None,
        Initializing,
        Editing,
        Creating
    }
}
