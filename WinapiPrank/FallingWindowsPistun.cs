using System.Drawing;
using Windows.Win32.Foundation;

namespace WinapiPrank;

/// <summary>
/// Move the all user windows down by specified amount of pixels.
///
/// Trigger:
/// wait while the mouse moves pass the thresholds - (<see cref="Options.MouseXDifferenceThreshold"/>, <see cref="Options.MouseYDifferenceThreshold"/>)
/// for duration - <see cref="Options.StationaryMouseDurationForTrigger"/> 
/// then 
/// for then start to move all visible window (and only user windows, not explorer or the toolbar)
/// for specified pixels <see cref="Options.MoveWindowDownByPixels"/> limited by times a sec -<see cref="Options.MoveWindowsOncePer"/>
/// </summary>
public class FallingWindowsPistun
{
    private readonly Options _options;
    
    private DateTime _lastMoveWindowsAt = DateTime.Now;
    private readonly int _triggerStationaryMouseIntervalCount;

    public FallingWindowsPistun(Options options)
    {
        _options = options;
        _triggerStationaryMouseIntervalCount = (int) Math.Ceiling(options.StationaryMouseDurationForTrigger / options.IntervalTime);
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

            bool hasMouseMoved = diffX <= _options.MouseXDifferenceThreshold 
                                 && diffY <= _options.MouseYDifferenceThreshold;

            if (hasMouseMoved)
            {
                stationaryMouseIntervalCount++;

                if (stationaryMouseIntervalCount >= _triggerStationaryMouseIntervalCount)
                {
                    if (!MoveAllWindowsDownByPixels(_options.MoveWindowDownByPixels))
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

            Thread.Sleep(_options.IntervalTime);
        }
    }
    
    private bool MoveAllWindowsDownByPixels(int pixelCount)
    {
        if (DateTime.Now - _lastMoveWindowsAt <= _options.MoveWindowsOncePer)
            return true;

        if (!Window.GetAll(Window.Filter.Visible, out var windows)) return false;

        foreach (Window window in windows)
        {
            if (!window.GetRect(out RECT rect)) return false;
            
            _ = window.Move(rect.X, rect.Y + pixelCount);
        }

        _lastMoveWindowsAt = DateTime.Now;
        return true;
    }

    public class Options
    {
        public TimeSpan IntervalTime { get; set; } = TimeSpan.FromMilliseconds(10);
        public TimeSpan StationaryMouseDurationForTrigger {get;set;}= TimeSpan.FromSeconds(2);
        public TimeSpan MoveWindowsOncePer {get;set;}= TimeSpan.FromSeconds(1);

        public int MouseXDifferenceThreshold { get; set; } = 10;
        public int MouseYDifferenceThreshold { get; set; } = 10;
        public int MoveWindowDownByPixels { get; set; } = 1;
    }
}