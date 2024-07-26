using Cocona;
using WinapiPrank;

public class Program
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

        pistun.Run();
    }
}