using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace EmojiHelper;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window, IDisposable
{
    private readonly App app;
    private readonly IDisposable _settingsSubscription;

    //private readonly Visual _myVisual;

    public MainWindow(App app)
    {
        InitializeComponent();
        ShowInTaskbar = false;
        Closing += MainWindow_Closing;
        KeyDown += MainWindow_KeyDown;
        this.app = app;

        _settingsSubscription = app.AppSettings.OnChange((settings, _) => OnSettingsChanged(settings))!;

        //_myVisual = new DrawingVisual();
        //_myVisual.EnsureVisualConnected(this);

        DataContext = new MainViewModel(app.AppSettings.CurrentValue.Emojis);
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Cancel the close operation
        e.Cancel = true;
        Hide();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var sourceButtonText = ((Emoji.Wpf.TextBlock)((System.Windows.Controls.Button)sender).Content).Text.ToString();
        Close();

        // Send the character to the focused text area of any other application
        if (!string.IsNullOrEmpty(sourceButtonText))
            InputHelpers.SendText(sourceButtonText);
    }

    private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        Close();
        if (e.Key >= Key.D1 && e.Key <= Key.D9)
        {
            int index = e.Key - Key.D1;
            SendEmojiByIndex(index);
        }
        else if (e.Key == Key.D0)
        {
            SendEmojiByIndex(9);
        }
    }

    private void SendEmojiByIndex(int index)
    {
        var emojis = app.AppSettings.CurrentValue.Emojis;
        if (index >= 0 && index < emojis.Length)
        {
            InputHelpers.SendText(emojis[index]);
        }
    }

    private void OnSettingsChanged(AppSettings newSettings)
    {
        Dispatcher.Invoke(() =>
        {
            DataContext = new MainViewModel(newSettings.Emojis);
        });
    }

    public void Dispose()
    {
        _settingsSubscription.Dispose();
    }
}

public class MainViewModel
{
    public List<KeyValuePair<string, string>> Emojis { get; set; }

    public MainViewModel(string[] emojis)
    {
        Emojis = emojis
            .Select((emoji, index) => new
            {
                Emoji = emoji,
                Number = index == 9
                    ? 0
                    : index + 1,
            })
            .Select(x => new KeyValuePair<string, string>(x.Number.ToString(), x.Emoji))
            .ToList();
    }
}