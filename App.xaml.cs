using System.Windows;
using System.Windows.Interop;
using Application = System.Windows.Application;

namespace EmojiHelper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private NotifyIcon _notifyIcon = null!;
    private MainWindow _mainWindow = null!;
    private Window _hiddenWindow = null!;
    private HotkeyHelper _hotkeyHelper = null!;

    public string[] Emojis { get; } = [ "😀", "🤔", "🤣", "😂", "🤦‍♀️", "✌", "😎", "😜", "👏", "😜" ];

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _mainWindow = new MainWindow(this);
        _mainWindow.Loaded += MainWindow_Loaded;

        _hiddenWindow = new Window { Width = 0, Height = 0, ShowInTaskbar = false, WindowStyle = WindowStyle.None };
        _hiddenWindow.Loaded += HiddenWindow_Loaded;
        _hiddenWindow.Show();

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "InputHelper"
        };
        _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Exit", null, (s, args) => Shutdown());
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        //_mainWindow.Hide();
    }

    private void HiddenWindow_Loaded(object sender, RoutedEventArgs e)
    {
        IntPtr hWnd = new WindowInteropHelper(_hiddenWindow).Handle;
        _hotkeyHelper = new HotkeyHelper(hWnd, Emojis, this);
        _hotkeyHelper.RegisterHotkeys();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyHelper.UnregisterHotkeys();
        _notifyIcon.Dispose();

        base.OnExit(e);
    }

    public void ShowMainWindow()
    {
        _mainWindow.ShowInTaskbar = true;
        _mainWindow.Visibility = Visibility.Visible;
        _mainWindow.Show();
        _mainWindow.Activate();
    }
}
