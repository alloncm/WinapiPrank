using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace WinapiPrank;

public class Program
{
    public static int Main(string[] args)
    {
        // In C# the program name is not supplied in the command line args
        if (args.Length < 1)
        {
            Console.WriteLine($"Not enough arguemnts supplied, found {args.Length}");
            return 1;
        }
        CancellationTokenSource tokenSource = new();
        Console.CancelKeyPress += (_, e) => 
        {
            e.Cancel = true;    // Wait for the program to terminate gracefuly 
            tokenSource.Cancel();
        };

        return args[0] switch
        {
            "key-hook"=>KeyHook(tokenSource.Token),
            "falling-windows"=>FallingWindows(tokenSource.Token, args.ElementAtOrDefault(1)),
            _=>1
        };
    }

    public static int FallingWindows(CancellationToken token, string? inputInterval)
    {
        var options = new FallingWindowsPistun.Options();
        if (inputInterval is not null)
        {
            if (TimeSpan.TryParse(inputInterval, out var interval ))
            {
                options.IntervalTime = interval;
            }
            else
            {
                return 1;
            }
        }

        FallingWindowsPistun pistun = new FallingWindowsPistun(options);

        pistun.Run(token);
        return 0;
    }

    public static int KeyHook(CancellationToken token)
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
            keyHook.Run(token);
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