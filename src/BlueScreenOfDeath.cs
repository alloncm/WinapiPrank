using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace WinapiPrank;

public static class BlueScreenOfDeath
{
    private const uint STATUS_ASSERTION_FAILURE = 0xC0000420;

    public static void Trigger()
    {
#if EnableBSOD
        RtlAdjustPrivilege(19, true, false, out bool previousValue);

        NTSTATUS ntstatus = NtRaiseHardError(STATUS_ASSERTION_FAILURE,
            0,
            0,
            IntPtr.Zero,
            6,
            out uint oul
        );

        if (ntstatus != 0)
        {
            throw new Exception($"Triggering BSOD failed with status {(int)ntstatus}");
        }
#endif
    }

    [DllImport("ntdll.dll")]
    private static extern uint RtlAdjustPrivilege(
        int privilege,
        bool bEnablePrivilege,
        bool isThreadPrivilege,
        out bool previousValue
    );

    [DllImport("ntdll.dll")]
    private static extern NTSTATUS NtRaiseHardError(
        uint errorStatus,
        uint numberOfParameters,
        uint unicodeStringParameterMask,
        IntPtr parameters,
        uint validResponseOption,
        out uint response
    );
}