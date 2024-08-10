using Cocona;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace WinapiPrank;

public class Program : CoconaConsoleAppBase
{
    public static void Main(string[] args)
    {
        CoconaApp.Run<Program>(args);
    }

    [Command(nameof(FallingWindows))]
    public void FallingWindows([Option(['i'])] TimeSpan? interval)
    {
        var options = new FallingWindowsPistun.Options();

        if (interval.HasValue) options.IntervalTime = interval.Value;

        FallingWindowsPistun pistun = new FallingWindowsPistun(options);

        pistun.Run(Context.CancellationToken);
    }

    [Command(nameof(KeyHook))]
    public int KeyHook()
    {
#if EnableBSOD
        var action = BlueScreenOfDeath.Trigger;
#else
        var action = () => { _ = MessageBox(HWND.Null, "Triggered hook", "Alert", MESSAGEBOX_STYLE.MB_OK); };
#endif
        try
        {
            using var keyHook = new KeyHook(VIRTUAL_KEY.VK_LCONTROL, TimeSpan.FromMilliseconds(300), 3, action);
            Console.WriteLine("Hook installed successfully");
            keyHook.Run(Context.CancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while runnign the hook: {ex.Message}");
            return 1;
        }

        Console.WriteLine("Hook uninstalled");
        return 0;
    }
}