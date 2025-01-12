using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace EmojiHelper;

public partial class HotkeyHelper
{
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    public struct GUITHREADINFO
    {
        public int cbSize;
        public uint flags;
        public IntPtr hwndActive;
        public IntPtr hwndFocus;
        public IntPtr hwndCapture;
        public IntPtr hwndMenuOwner;
        public IntPtr hwndMoveSize;
        public IntPtr hwndCaret;
        public RECT rcCaret;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    private const int HOTKEY_ID = 9000;
    private const uint MOD_CONTROL = 0x0002; // CTRL
    private const uint MOD_ALT = 0x0001; // ALT
    private const uint VK_E = 0x45; // E
    private const int WM_HOTKEY = 0x0312;

    private readonly IntPtr _hWnd;
    private readonly string[] _emojis;
    private readonly App _app;

    public HotkeyHelper(IntPtr hWnd, string[] emojis, App app)
    {
        _hWnd = hWnd;
        _emojis = emojis;
        _app = app;
    }

    public void RegisterHotkeys()
    {
        HwndSource source = HwndSource.FromHwnd(_hWnd);
        source.AddHook(HwndHook);

        RegisterHotKey(_hWnd, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_E);

        // 0x30 is the virtual key code for '0'
        const int zero = 0x30;

        for (int i = 0; i < 10; i++)
        {
            RegisterHotKey(_hWnd, HOTKEY_ID + i + 1, MOD_CONTROL | MOD_ALT, (uint)(zero + i));
        }
    }

    public void UnregisterHotkeys()
    {
        UnregisterHotKey(_hWnd, HOTKEY_ID);

        for (int i = 0; i < 10; i++)
        {
            UnregisterHotKey(_hWnd, HOTKEY_ID + i + 1);
        }
    }

    public IntPtr HwndHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WM_HOTKEY)
            return IntPtr.Zero;

        int id = wParam.ToInt32();
        if (id == HOTKEY_ID)
        {
            ShowMainWindowNearCaret();
            handled = true;
        }
        else if (id >= HOTKEY_ID + 1 && id <= HOTKEY_ID + 10)
        {
            int keyNumber = id - HOTKEY_ID - 1;
            var index = keyNumber == 0
                ? 9
                : keyNumber - 1;
            InputHelpers.SendText(_emojis[index]);
            handled = true;
        }
        return IntPtr.Zero;
    }

    private void ShowMainWindowNearCaret()
    {
        GUITHREADINFO guiInfo = new GUITHREADINFO();
        guiInfo.cbSize = Marshal.SizeOf(guiInfo);

        if (GetGUIThreadInfo(0, ref guiInfo))
        {
            var caretRect = guiInfo.rcCaret;

            // Convert caret position to screen coordinates
            POINT caretPoint = new() { X = caretRect.Right, Y = caretRect.Top };
            ClientToScreen(guiInfo.hwndCaret, ref caretPoint);

            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double screenWidth = SystemParameters.PrimaryScreenWidth;

            double left = 0;
            double top = 0;

            if (caretPoint.X == 0 && caretPoint.Y == 0)
            {
                left = screenWidth / 2 - _app.MainWindow.Width / 2;
                top = screenHeight - _app.MainWindow.Height - 100;
            }
            else
            {
                // Convert screen coordinates to window coordinates
                left = caretPoint.X - _app.MainWindow.Width / 2;
                top = caretPoint.Y - _app.MainWindow.Height / 2;


                if (top + _app.MainWindow.Height > screenHeight - 100)
                    top = screenHeight - _app.MainWindow.Height - 100;
            }

            _app.MainWindow.Left = left;
            _app.MainWindow.Top = top;

            _app.ShowMainWindow();
        }
        else
        {
            _app.ShowMainWindow();
        }
    }
}
