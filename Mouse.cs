using System.Drawing;
using static Windows.Win32.PInvoke;

namespace WinapiPrank;

public static class Mouse
{
    internal static bool GetPos(out Point point)
    {
        return GetCursorPos(out point);
    }
}