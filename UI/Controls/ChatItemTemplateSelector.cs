namespace BabyBuddyHelper.UI.Controls
{
    public enum ChatItemKind
    {
        Parent,
        Cub,
        Pending, //"Cub is thinking…" while a reply is on its way
        Failure //The neutral line shown when Cub couldn't answer (ADR-013: not in Cub's voice)
    }

    //One line of the chat as the page shows it. Only Parent and Cub items are ever sent back to Cub
    public sealed record ChatItem(ChatItemKind Kind, string Text);

    //Picks the chat's bubble or line for each item. The templates are set in ChatPage.xaml
    public sealed class ChatItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ParentTemplate { get; set; }
        public DataTemplate? CubTemplate { get; set; }
        public DataTemplate? PendingTemplate { get; set; }
        public DataTemplate? FailureTemplate { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container) => (item as ChatItem)?.Kind switch
        {
            ChatItemKind.Cub => CubTemplate,
            ChatItemKind.Pending => PendingTemplate,
            ChatItemKind.Failure => FailureTemplate,
            _ => ParentTemplate
        };
    }
}
