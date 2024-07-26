using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using WinapiPrank;
using static Windows.Win32.PInvoke;

var action = () => { _ = MessageBox(HWND.Null, "Surprise!", "Shtek", MESSAGEBOX_STYLE.MB_OK); };
var keyHook = new KeyHook(VIRTUAL_KEY.VK_LCONTROL, TimeSpan.FromMilliseconds(300), 3, action);
if (keyHook.Setup())
{
    Console.WriteLine("Hooked installed successfully");
}
else
{
    Console.WriteLine("Couldn't install hook");
    return;
}
Console.CancelKeyPress += (_, _) =>
{
    keyHook.Dispose();
    Console.WriteLine("Disposed hook successfully");
};

keyHook.Run();  // blocking