using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace WinapiPrank;

internal record WindowInfo(HWND Handle, bool IsVisible)
{
    private readonly Lazy<string> _name = new(() => Window.GetWindowTitle(Handle));
    public string Name => _name.Value;

    private readonly Lazy<Process?> _process = new(() => Window.GetProcess(Handle));
    public Process? Process => _process.Value;

    public bool Minimize()
    {
        WINDOWPLACEMENT windowplacement = default;
        bool result = GetWindowPlacement(Handle, ref windowplacement);
        if (result == false) return false;
        windowplacement.showCmd = SHOW_WINDOW_CMD.SW_MINIMIZE;
        return SetWindowPlacement(Handle, in windowplacement);
    }

    public bool GetRect(out RECT rect)
    {
        return GetWindowRect(Handle, out rect);
    }

    public bool Move(in RECT rect)
    {
        return SetWindowPos(Handle, HWND.Null, rect.X, rect.Y, rect.Width, rect.Width,
            SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);
    }
}

internal static class Window
{
    private record Parameters(List<WindowInfo> Windows, Filter Filter);

    public static bool GetWindowsInfo(Filter filter, out List<WindowInfo> windowsInfo)
    {
        windowsInfo = [];

        Parameters parameters = new Parameters(windowsInfo, filter);
        GCHandle gcHandle = GCHandle.Alloc(parameters);

        bool result = EnumWindows(static (handle, lParam) =>
        {
            Parameters parameters = (GCHandle.FromIntPtr(lParam).Target as Parameters)!;

            bool visible = IsWindowVisible(handle);

            if (parameters.Filter.HasFlag(Filter.Visible) && !visible) return true;

            parameters.Windows.Add(new WindowInfo(handle, visible));

            return true;
        }, GCHandle.ToIntPtr(gcHandle));

        if (gcHandle.IsAllocated) gcHandle.Free();

        return result;
    }

    public static unsafe Process? GetProcess(HWND handle)
    {
        uint processId;
        uint result = GetWindowThreadProcessId(handle, &processId);
        return result == 0 ? null : Process.GetProcessById((int) processId);
    }

    public static unsafe string GetWindowTitle(HWND handle)
    {
        int length = GetWindowTextLength(handle);

        if (length == 0) return string.Empty;

        return string.Create(length + 1, (handle, length: length + 1), static (span, pair) =>
        {
            fixed (char* c = span) GetWindowText(pair.handle, c, pair.length);
        });
    }

    [Flags]
    public enum Filter
    {
        None = 0,
        Visible = 1,
    }
}