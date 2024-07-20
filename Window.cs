using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace WinapiPrank;

internal partial class Window
{
    private readonly Lazy<string> _name;
    private readonly Lazy<Process?> _process;

    public HWND Handle { get; }
    public bool IsVisible { get; }

    public string Name => _name.Value;
    public Process? Process => _process.Value;

    private Window(HWND handle, bool isVisible)
    {
        Handle = handle;
        IsVisible = isVisible;
        _name = new Lazy<string>(() => GetWindowTitle(Handle));
        _process = new Lazy<Process?>(() => GetProcess(Handle));
    }

    public bool Minimize()
    {
        WINDOWPLACEMENT windowplacement = default;
        bool result = GetWindowPlacement(Handle, ref windowplacement);
        if (result == false) return false;
        windowplacement.showCmd = SHOW_WINDOW_CMD.SW_MINIMIZE;
        return SetWindowPlacement(Handle, in windowplacement);
    }

    public bool IsInForeground() => GetForegroundWindow() == Handle;

    public bool GetRect(out RECT rect)
    {
        return GetWindowRect(Handle, out rect);
    }

    public bool Move(in RECT rect)
    {
        return SetWindowPos(Handle, HWND.Null, rect.X, rect.Y, rect.Width, rect.Width,
            SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);
    }

    private static unsafe Process? GetProcess(HWND handle)
    {
        uint processId;
        uint result = GetWindowThreadProcessId(handle, &processId);
        return result == 0 ? null : Process.GetProcessById((int) processId);
    }

    private static unsafe string GetWindowTitle(HWND handle)
    {
        int length = GetWindowTextLength(handle);

        if (length == 0) return string.Empty;

        return string.Create(length + 1, (handle, length: length + 1), static (span, pair) =>
        {
            fixed (char* c = span) GetWindowText(pair.handle, c, pair.length);
        });
    }
}