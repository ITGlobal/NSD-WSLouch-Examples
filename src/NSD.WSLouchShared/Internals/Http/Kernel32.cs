using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NSD.WSLouch.Internals.Http
{
    /// <summary>
    ///     P/Invoke для функций из kernel32.dll
    /// </summary>
    // ReSharper disable InconsistentNaming
    internal static class Kernel32
    {
        private const string Kernel32Dll = "kernel32.dll";

        public const int LANG_NEUTRAL = 0x00;
        public const int SUBLANG_NEUTRAL = 0x00;

        public static int MAKELANGID(int p, int s)
        {
            return (short)s << 10 | (short)p;
        }

        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const int FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;

        [DllImport(Kernel32Dll)]
        public static extern int GetLastError();

        [DllImport(Kernel32Dll, CharSet = CharSet.Auto)]
        public static extern int FormatMessage(
            int dwFlags,
            IntPtr lpSource,
            int dwMessageId,
            int dwLanguageId,
            StringBuilder lpBuffer,
            int nSize,
            IntPtr arguments
            );

        [DllImport(Kernel32Dll)]
        public static extern IntPtr GetModuleHandleW([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);
    }
}