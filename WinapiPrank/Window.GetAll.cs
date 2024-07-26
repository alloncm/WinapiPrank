using System.Runtime.InteropServices;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace WinapiPrank;

internal partial class Window
{
    private record Parameters(List<Window> Windows, Filter Filter);

    public static bool GetAll(Filter filter, out List<Window> windowsInfo)
    {
        windowsInfo = [];

        Parameters parameters = new Parameters(windowsInfo, filter);
        GCHandle gcHandle = GCHandle.Alloc(parameters, GCHandleType.Normal);

        try
        {
            bool result = EnumWindows(static (handle, lParam) =>
            {
                Parameters parameters = (GCHandle.FromIntPtr(lParam).Target as Parameters)!;

                Window window = new Window(handle);

                bool hasInfo = window.GetInfo(out var info);
                bool visible = window.IsVisible();

                if (parameters.Filter.HasFlag(Filter.Visible) != visible) return true;
                if (hasInfo)
                {
                    if (parameters.Filter.HasFlag(Filter.ToolWindow) != info.dwExStyle.HasFlag(WINDOW_EX_STYLE.WS_EX_TOOLWINDOW))
                        return true;
                    if (parameters.Filter.HasFlag(Filter.Popup) != info.dwStyle.HasFlag(WINDOW_STYLE.WS_POPUP))
                        return true;
                }

                parameters.Windows.Add(window);

                return true;
            }, GCHandle.ToIntPtr(gcHandle));

            return result;
        }
        finally
        {
            if (gcHandle.IsAllocated) gcHandle.Free();
        }
    }

    [Flags]
    public enum Filter
    {
        None = 0,
        Visible = 1,
        ToolWindow = 2,
        Popup = 4
    }
}