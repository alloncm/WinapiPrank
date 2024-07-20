using System.Runtime.InteropServices;
using static Windows.Win32.PInvoke;

namespace WinapiPrank;

internal partial class Window
{
    private record Parameters(List<Window> Windows, Filter Filter);

    public static bool GetAll(Filter filter, out List<Window> windowsInfo)
    {
        windowsInfo = [];

        Parameters parameters = new Parameters(windowsInfo, filter);
        GCHandle gcHandle = GCHandle.Alloc(parameters);

        bool result = EnumWindows(static (handle, lParam) =>
        {
            Parameters parameters = (GCHandle.FromIntPtr(lParam).Target as Parameters)!;

            bool visible = IsWindowVisible(handle);

            if (parameters.Filter.HasFlag(Filter.Visible) && !visible) return true;

            parameters.Windows.Add(new Window(handle, visible));

            return true;
        }, GCHandle.ToIntPtr(gcHandle));

        if (gcHandle.IsAllocated) gcHandle.Free();

        return result;
    }

    [Flags]
    public enum Filter
    {
        None = 0,
        Visible = 1,
    }
}