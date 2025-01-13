using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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

    private IHost _host = null!;

    /*
        😂
        ❤️
        👍
        😭
        🔥
        🥺
        💀
        ✅
        ❌
        ✨

        😊
        ⭐
        🙏
        👀
        🛒
        🎉
        😍
        😔
        👉👈
     */

    public string[] Emojis { get; } = [ "😂", "❤️", "👍", "😭", "🔥", "🥺", "💀", "✅", "❌", "✨" ];

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder(e.Args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var connectionString = "Data Source=appsettings.db";
                config.AddSqliteConfiguration(connectionString);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<AppSettings>(context.Configuration);
            })
            .Build();

        var appSettings = _host.Services.GetRequiredService<IOptionsMonitor<AppSettings>>();
        appSettings.OnChange(OnAppSettingsChanged);

        _mainWindow = new MainWindow(this);
        _mainWindow.Loaded += MainWindow_Loaded;

        _hiddenWindow = new Window { Width = 0, Height = 0, ShowInTaskbar = false, WindowStyle = WindowStyle.None };
        _hiddenWindow.Loaded += HiddenWindow_Loaded;
        _hiddenWindow.Show();

        using var iconStream = typeof(App).Assembly.GetManifestResourceStream("EmojiHelper.favicon.ico")
            ?? throw new InvalidOperationException("Icon not found");
        _notifyIcon = new NotifyIcon
        {
            Icon = new Icon(iconStream),
            Visible = true,
            Text = "InputHelper"
        };
        _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Exit", null, (s, args) => Shutdown());
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private static void OnAppSettingsChanged(AppSettings newSettings)
    {
        Console.WriteLine($"Settings changed: Setting1={newSettings.Setting1}, Setting2={newSettings.Setting2}");
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
        _host.Dispose();

        base.OnExit(e);
    }

    public void ShowMainWindow()
    {
        _mainWindow.ShowInTaskbar = true;
        _mainWindow.Visibility = Visibility.Visible;
        _mainWindow.Show();
        _mainWindow.Activate();
    }

    public void UpdateConfiguration(string key, string value)
    {
        var configuration = _host.Services.GetRequiredService<IConfiguration>();
        configuration[key] = value;
    }
}
