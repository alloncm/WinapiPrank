using System.Drawing;
using Windows.Win32.Foundation;

namespace WinapiPrank;

public class FallingWindowsPistun
{
    private readonly FallingWindowsOptions _fallingWindowsOptions;
    
    private DateTime _lastMoveWindowsAt = DateTime.Now;
    private readonly int _triggerStationaryMouseIntervalCount;

    public FallingWindowsPistun(FallingWindowsOptions fallingWindowsOptions)
    {
        _fallingWindowsOptions = fallingWindowsOptions;
        _triggerStationaryMouseIntervalCount = (int) Math.Ceiling(fallingWindowsOptions.StationaryMouseDurationForTrigger / fallingWindowsOptions.IntervalTime);
    }


    /// <summary>
    /// blocking
    /// </summary>
    public void Run(CancellationToken cancellationToken = default)
    {
        Point lastMousePos = default;
        int stationaryMouseIntervalCount = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (!Mouse.GetPos(out var pos)) break;

            int diffX = Math.Abs(lastMousePos.X - pos.X);
            int diffY = Math.Abs(lastMousePos.Y - pos.Y);

            bool hasMouseMoved = diffX <= _fallingWindowsOptions.MouseXDifferenceThreshold 
                                 && diffY <= _fallingWindowsOptions.MouseYDifferenceThreshold;

            if (hasMouseMoved)
            {
                stationaryMouseIntervalCount++;

                if (stationaryMouseIntervalCount >= _triggerStationaryMouseIntervalCount)
                {
                    if (!MoveAllWindowsDownByPixels(_fallingWindowsOptions.MoveWindowDownByPixels))
                    {
                        break;
                    }
                }
            }
            else
            {
                stationaryMouseIntervalCount = 0;
            }

            lastMousePos = pos;

            Thread.Sleep(_fallingWindowsOptions.IntervalTime);
        }
    }
    
    private bool MoveAllWindowsDownByPixels(int pixelCount)
    {
        if (DateTime.Now - _lastMoveWindowsAt <= _fallingWindowsOptions.MoveWindowsOncePer)
            return true;

        if (!Window.GetAll(Window.Filter.Visible, out var windows)) return false;

        foreach (Window window in windows)
        {
            if (!window.GetRect(out RECT rect)) return false;

            window.Move(new RECT(rect.left, rect.top + pixelCount, rect.right, rect.bottom + pixelCount));
        }

        _lastMoveWindowsAt = DateTime.Now;
        return true;
    }

    public class FallingWindowsOptions
    {
        public TimeSpan IntervalTime { get; set; } = TimeSpan.FromMilliseconds(10);
        public TimeSpan StationaryMouseDurationForTrigger {get;set;}= TimeSpan.FromSeconds(2);
        public TimeSpan MoveWindowsOncePer {get;set;}= TimeSpan.FromSeconds(1);

        public int MouseXDifferenceThreshold { get; set; } = 10;
        public int MouseYDifferenceThreshold { get; set; } = 10;
        public int MoveWindowDownByPixels { get; set; } = 1;
    }
}