using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;


namespace Microsoft.BotBuilderSamples
{
    public class LuisBotAccessors
    {
        public LuisBotAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }

        public IStatePropertyAccessor<UserProfile> UserProfile { get; set; }

        public ConversationState ConversationState { get; }

        public UserState UserState { get; }
    }
}
