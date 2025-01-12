using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace EmojiHelper;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly App app;
    //private readonly Visual _myVisual;

    public MainWindow(App app)
    {
        InitializeComponent();
        ShowInTaskbar = false;
        Closing += MainWindow_Closing;
        KeyDown += MainWindow_KeyDown;
        this.app = app;
        //_myVisual = new DrawingVisual();
        //_myVisual.EnsureVisualConnected(this);

        DataContext = new MainViewModel(app.Emojis);
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
        if (index >= 0 && index < app.Emojis.Length)
        {
            InputHelpers.SendText(app.Emojis[index]);
        }
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