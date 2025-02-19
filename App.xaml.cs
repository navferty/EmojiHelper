using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System.IO;
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

    private readonly string[] _defaultEmojis = [ "😂", "♥", "👍", "😭", "🔥", "🥺", "💀", "✔", "❌", "✨" ];

    public IOptionsMonitor<AppSettings> AppSettings => _host.Services.GetRequiredService<IOptionsMonitor<AppSettings>>();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Serilog.Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.log"))
            .CreateLogger();

        _host = Host.CreateDefaultBuilder(e.Args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var connectionString = "Data Source=appsettings.db";
                config.AddSqliteConfiguration(connectionString);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<AppSettings>(context.Configuration);
                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddSerilog();
                });
            })
            .Build();

        var logger = _host.Services.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Application started");

        var appSettings = _host.Services.GetRequiredService<IOptionsMonitor<AppSettings>>();
        appSettings.OnChange(OnAppSettingsChanged);

        var shouldShowSettings = false;
        var config = _host.Services.GetRequiredService<IConfiguration>();
        if (config.GetValue<bool>("FirstRun", true))
        {
            logger.LogInformation("First run detected, setting default emojis");

            foreach (var (emoji, index) in _defaultEmojis.Select((emoji, index) => (emoji, index)))
            {
                config[$"Emojis:{index}"] = emoji;
            }

            config["FirstRun"] = "false";

            shouldShowSettings = true;
        }

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
        contextMenu.Items.Add("Settings", null, (s, args) => ShowSettingsWindow());
        contextMenu.Items.Add("Exit", null, (s, args) => Shutdown());
        _notifyIcon.ContextMenuStrip = contextMenu;

        if (shouldShowSettings)
        {
            ShowSettingsWindow();
        }
    }

    private static void OnAppSettingsChanged(AppSettings newSettings)
    {
        var emojis = newSettings.Emojis ?? [];
        Console.WriteLine($"Settings changed: current emojis: {string.Join(", ", emojis)}");
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        //_mainWindow.Hide();
    }

    private void HiddenWindow_Loaded(object sender, RoutedEventArgs e)
    {
        IntPtr hWnd = new WindowInteropHelper(_hiddenWindow).Handle;

        // TODO use DI
        _hotkeyHelper = new HotkeyHelper(hWnd, AppSettings, this, _host.Services.GetRequiredService<ILogger<HotkeyHelper>>());
        _hotkeyHelper.RegisterHotkeys();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyHelper.UnregisterHotkeys();
        _notifyIcon.Dispose();
        _host.Dispose();
        _mainWindow.Dispose();

        Serilog.Log.CloseAndFlush();

        base.OnExit(e);
    }

    public void ShowMainWindow()
    {
        _mainWindow.ShowInTaskbar = true;
        _mainWindow.Visibility = Visibility.Visible;
        _mainWindow.Show();
        _mainWindow.Activate();
    }

    public void ShowSettingsWindow()
    {
        var settingsWindow = new SettingsWindow(this);
        settingsWindow.ShowDialog();
    }

    public void UpdateConfiguration(string key, string value)
    {
        var configuration = _host.Services.GetRequiredService<IConfiguration>();
        configuration[key] = value;
    }
}
