using System.Windows;

namespace EmojiHelper;

public partial class SettingsWindow : Window
{
    private readonly App _app;
    private readonly List<EmojiConfig> _emojis;

    public SettingsWindow(App app)
    {
        InitializeComponent();
        _app = app;
        _emojis = app.AppSettings.CurrentValue.Emojis
            .Select((emoji, index) => new EmojiConfig { Position = (index + 1).ToString(), Emoji = emoji })
            .ToList();
        DataContext = new SettingsViewModel(_emojis);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (SettingsViewModel)DataContext;
        for (int i = 0; i < viewModel.Emojis.Count; i++)
        {
            _app.UpdateConfiguration($"Emojis:{i}", viewModel.Emojis[i].Emoji);
        }
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

public sealed record SettingsViewModel(List<EmojiConfig> Emojis);

public sealed record EmojiConfig
{
    public required string Position { get; set; }
    public required string Emoji { get; set; }
}
