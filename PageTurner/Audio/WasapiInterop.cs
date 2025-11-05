using System.Runtime.InteropServices;

namespace PageTurner.Audio.Interop;

// NOTE: These are raw COM interface definitions for the Windows Core Audio API (WASAPI).
// They are marked as public to ensure they are accessible for COM interop.

[ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
public class MMDeviceEnumeratorCom;

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
public interface IMMDeviceEnumerator
{
    [PreserveSig]
    int NotImpl1(); // int EnumAudioEndpoints(...)
    [PreserveSig]
    int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppDevice);
}

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("D666063F-1587-4E43-81F1-B948E807363F")]
public interface IMMDevice
{
    [PreserveSig]
    int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams,
        [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
}

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("BFA971F1-4D5E-40BB-935B-C97BBFDEDCBA")]
public interface IAudioSessionManager
{
    [PreserveSig]
    int GetAudioSessionControl(
        [MarshalAs(UnmanagedType.LPStruct)] Guid AudioSessionGuid,
        uint StreamFlags,
        [MarshalAs(UnmanagedType.Interface)] out IAudioSessionControl SessionControl
    );

    [PreserveSig]
    int GetSimpleAudioVolume(
        [MarshalAs(UnmanagedType.LPStruct)] Guid AudioSessionGuid,
        uint StreamFlags,
        [MarshalAs(UnmanagedType.Interface)] out ISimpleAudioVolume AudioVolume
    );
}

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("77AA99A2-1BD6-484F-8452-50A717AE5021")]
public interface IAudioSessionManager2 : IAudioSessionManager
{
    [PreserveSig]
    int GetSessionEnumerator(
        [MarshalAs(UnmanagedType.Interface)] out IAudioSessionEnumerator SessionEnum
    );

    [PreserveSig]
    int RegisterSessionNotification(
        [MarshalAs(UnmanagedType.Interface)] IAudioSessionNotification SessionNotification
    );

    [PreserveSig]
    int UnregisterSessionNotification(
        [MarshalAs(UnmanagedType.Interface)] IAudioSessionNotification SessionNotification
    );

    [PreserveSig]
    int RegisterDuckNotification(
        [MarshalAs(UnmanagedType.Interface)] IAudioVolumeDuckNotification DuckNotification
    );

    [PreserveSig]
    int UnregisterDuckNotification(
        [MarshalAs(UnmanagedType.Interface)] IAudioVolumeDuckNotification DuckNotification
    );
}

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("E2F5BB11-0570-40CA-ACDD-3AA01275FA88")]
public interface IAudioSessionEnumerator
{
    [PreserveSig]
    int GetCount(out int SessionCount);
    [PreserveSig]
    int GetSession(int SessionIndex, [MarshalAs(UnmanagedType.Interface)] out IAudioSessionControl session);
}

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("F4B1A599-7266-4876-A218-1FD6F79F8CDC")]
public interface IAudioSessionControl
{
    [PreserveSig]
    int GetState(out int state);
    [PreserveSig]
    int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string name);
    [PreserveSig]
    int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string value, ref Guid eventContext);
    [PreserveSig]
    int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string path);
    [PreserveSig]
    int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string path, ref Guid eventContext);
    [PreserveSig]
    int GetGroupingParam(out Guid param);
    [PreserveSig]
    int SetGroupingParam(ref Guid param, ref Guid eventContext);
    [PreserveSig]
    int RegisterAudioSessionNotification(IntPtr newNotifications);
    [PreserveSig]
    int UnregisterAudioSessionNotification(IntPtr newNotifications);
}

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("24918ACC-64B3-37C1-8CA9-74A66E9957A8")]
public interface IAudioSessionControl2 : IAudioSessionControl
{
    [PreserveSig]
    int GetProcessId(out uint processId);
}

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("87CE5498-68D6-4444-9176-79FD6583F807")]
public interface ISimpleAudioVolume
{
    // Placeholder for methods of ISimpleAudioVolume
}

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("657804FA-D6AB-4AE5-BC0B-02977EE42403")]
public interface IAudioSessionNotification
{
    // Placeholder for methods of IAudioSessionNotification
}

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("7F54626B-9614-48D1-BA07-3B02348E13B7")]
public interface IAudioVolumeDuckNotification
{
    // Placeholder for methods of IAudioVolumeDuckNotification
}

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064")]
public interface IAudioMeterInformation
{
    [PreserveSig]
    int GetPeakValue(out float pfPeak);
}
