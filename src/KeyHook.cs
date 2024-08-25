using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using static Windows.Win32.PInvoke;

namespace WinapiPrank;

internal class KeyHook : IDisposable
{
    private readonly HOOKPROC _hookProcDelegate;     // Keeping a ref to the delegate, otherwise the GC will dealocate the delegate we pass to SetHook

    private readonly uint _key;
    private readonly TimeSpan _maxDelayBetweenKeyStrokes;
    private readonly int _numberOfContiguousKeyStrokesToTrigger;
    private readonly Action _actionToTrigger;
    private readonly int? _randomness;

    private int _strokesCounter = 0;
    private DateTime _lastStroke = DateTime.MinValue;
    private HHOOK _hook;

    internal KeyHook(VIRTUAL_KEY key, TimeSpan maxDelayBetweenStrokes, int numberOfContiguousKeyStrokesToTrigger, Action actionToTrigger, int? randomness = null)
    {
        _hookProcDelegate = new HOOKPROC(HookCallback);

        _key = (uint)key;
        _maxDelayBetweenKeyStrokes = maxDelayBetweenStrokes;
        _numberOfContiguousKeyStrokesToTrigger = numberOfContiguousKeyStrokesToTrigger;
        _actionToTrigger = actionToTrigger;
        _randomness = randomness;

        _hook = SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, _hookProcDelegate, HINSTANCE.Null, 0);
        if (_hook.IsNull)
        {
            throw new Exception("Could not install a low level keybaord hook");
        }
    }

    public void Run(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // In order handle events and keep the WinApi process responsive 
            PeekMessage(out MSG _, HWND.Null, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE);
            Thread.Sleep(1);    // Prevent busy wait
        }
    }

    private unsafe LRESULT HookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        KBDLLHOOKSTRUCT* keyboard = (KBDLLHOOKSTRUCT*)lParam.Value;
        if ((*keyboard).vkCode == _key && wParam == WM_KEYDOWN)
        {
            var now = DateTime.Now;
            if (now - _lastStroke <= _maxDelayBetweenKeyStrokes)
            {
                _strokesCounter++;
            }
            else
            {
                _strokesCounter = 1;
            }
            _lastStroke = now;
            if (_strokesCounter == _numberOfContiguousKeyStrokesToTrigger)
            {
                if (_randomness.HasValue)
                {
                    if (Random.Shared.Next(_randomness.Value) == 0)
                    {
                        _actionToTrigger();
                    }
                }
                else
                {
                    _actionToTrigger();
                }
                _strokesCounter = 0;
            }
        }
        return CallNextHookEx(HHOOK.Null, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hook.IsNull) return;
        UnhookWindowsHookEx(_hook);
        _hook = HHOOK.Null;
    }
}