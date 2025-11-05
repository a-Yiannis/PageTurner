using System.Runtime.InteropServices;
using PageTurner.Audio.Interop;

namespace PageTurner.Audio;

// --- Enums for WASAPI ---
// Moved here to resolve build errors.

public enum EDataFlow
{
    eRender,
    eCapture,
    eAll
}

public enum ERole
{
    eConsole,
    eMultimedia,
    eCommunications
}

[Flags]
public enum CLSCTX : uint
{
    CLSCTX_INPROC_SERVER = 0x1,
    CLSCTX_INPROC_HANDLER = 0x2,
    CLSCTX_LOCAL_SERVER = 0x4,
    CLSCTX_REMOTE_SERVER = 0x10,
    CLSCTX_ALL = CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER
}

public static class WASAPI
{
    /// <summary>
    /// Gets the current peak audio value (0.0 to 1.0) for the entire system output.
    /// </summary>
    public static float GetSystemPeakValue()
    {
        IMMDeviceEnumerator? enumerator = null;
        IMMDevice? device = null;
        IAudioMeterInformation? meter = null;
        try
        {
            enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorCom();
            
            // Get the default audio render device (speakers)
            if (enumerator.GetDefaultAudioEndpoint((int)EDataFlow.eRender, (int)ERole.eConsole, out device) != 0 || device is null)
            {
                return 0f;
            }

            // Activate the IAudioMeterInformation interface on the device
            var meterGuid = typeof(IAudioMeterInformation).GUID;
            if (device.Activate(ref meterGuid, (int)CLSCTX.CLSCTX_ALL, IntPtr.Zero, out object meterObj) != 0 || meterObj is null)
            {
                return 0f;
            }
            meter = (IAudioMeterInformation)meterObj;

            // Get the peak value from the meter
            meter.GetPeakValue(out float peak);
            return peak;
        }
        catch
        {
            // In case of any COM errors, return 0
            return 0f;
        }
        finally
        {
            // Ensure all COM objects are released
            if (meter is not null) Marshal.ReleaseComObject(meter);
            if (device is not null) Marshal.ReleaseComObject(device);
            if (enumerator is not null) Marshal.ReleaseComObject(enumerator);
        }
    }
}
