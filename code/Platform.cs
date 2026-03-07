namespace RJCP.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    internal static class Platform
    {
#if NET6_0_OR_GREATER
        [SupportedOSPlatformGuard("windows")]
#endif
        public static bool IsWinNT()
        {
#if NET6_0_OR_GREATER
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
#endif
        }

#if NET6_0_OR_GREATER
        [SupportedOSPlatformGuard("linux")]
#endif
        public static bool IsUnix()
        {
#if NET6_0_OR_GREATER
            return
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#else
            int platform = (int)Environment.OSVersion.Platform;
            return (platform is 4 or 6 or 128);
#endif
        }
    }
}
