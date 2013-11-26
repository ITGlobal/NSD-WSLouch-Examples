using System;
using System.Runtime.InteropServices;

namespace NSD.WSLouch.Internals.Http
{
    /// <summary>
    ///     P/Invoke для функций из wininet.dll
    /// </summary>
    // ReSharper disable InconsistentNaming
    internal static class WinInet
    {
        private const string WinInetDll = "wininet.dll";

        public const uint INTERNET_OPEN_TYPE_DIRECT = 1;
        public const uint INTERNET_SERVICE_HTTP = 3;

        public const uint INTERNET_FLAG_SECURE = 0x00800000;
        public const uint INTERNET_FLAG_IGNORE_CERT_CN_INVALID = 0x00001000;
        public const uint INTERNET_FLAG_IGNORE_CERT_DATE_INVALID = 0x00002000;
        public const uint INTERNET_FLAG_NO_AUTO_REDIRECT = 0x00200000;
        public const uint INTERNET_FLAG_PRAGMA_NOCACHE = 0x00000100;
        public const uint INTERNET_FLAG_NO_CACHE_WRITE = 0x04000000;
        public const uint INTERNET_FLAG_KEEP_CONNECTION = 0x00400000;

        public const uint INTERNET_OPTION_CLIENT_CERT_CONTEXT = 84;
        public const uint INTERNET_OPTION_SECURITY_FLAGS = 31;


        public const uint SECURITY_FLAG_IGNORE_REVOCATION = 0x00000080;
        public const uint SECURITY_FLAG_IGNORE_UNKNOWN_CA = 0x00000100;

        public const uint HTTP_QUERY_FLAG_NUMBER = 0x20000000;
        public const uint HTTP_QUERY_STATUS_CODE = 19;
        public const uint HTTP_QUERY_CONTENT_TYPE = 1;

        [DllImport(WinInetDll, EntryPoint = "InternetOpenW")]
        public static extern IntPtr InternetOpenW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpszAgent,
            uint dwAccessType,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszProxy,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszProxyBypass,
            uint dwFlags
            );

        [DllImport(WinInetDll, EntryPoint = "InternetConnectW")]
        public static extern IntPtr InternetConnectW(
            IntPtr hInternet,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszServerName,
            short nServerPort,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszUserName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszPassword,
            uint dwService,
            uint dwFlags,
            IntPtr dwContext
            );

        [DllImport(WinInetDll, EntryPoint = "InternetCloseHandle")]
        public static extern bool InternetCloseHandle(IntPtr hInternet);

        [DllImport(WinInetDll, EntryPoint = "HttpOpenRequestW")]
        public static extern IntPtr HttpOpenRequest(
            IntPtr hConnect,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszVerb,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszObjectName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszVersion,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszReferrer,
            IntPtr lplpszAcceptTypes,
            uint dwFlags,
            IntPtr dwContext
            );

        [DllImport(WinInetDll, EntryPoint = "InternetSetOptionW")]
        public static extern bool InternetSetOptionW(
            IntPtr hInternet,
            uint dwOption,
            IntPtr lpBuffer,
            uint dwBufferLength
            );

        [DllImport(WinInetDll, EntryPoint = "InternetQueryOptionW")]
        public static extern bool InternetQueryOptionW(
            IntPtr hInternet,
            uint dwOption,
            IntPtr lpBuffer,
            ref uint lpdwBufferLength
            );

        [StructLayout(LayoutKind.Sequential)]
        public struct InternetOptionSecurityFlags
        {
            public uint value;
        }

        [DllImport(WinInetDll, EntryPoint = "HttpSendRequestW")]
        public static extern bool HttpSendRequestW(
            IntPtr hRequest,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszHeaders,
            int dwHeadersLength,
            byte[] lpOptional,
            int dwOptionalLength
            );

        [StructLayout(LayoutKind.Sequential)]
        public struct HttpRequestBody
        {
            public byte[] data;
        }

        [DllImport(WinInetDll, EntryPoint = "HttpQueryInfoW")]
        public static extern bool HttpQueryInfoW(
            IntPtr hRequest,
            uint dwInfoLevel,
            IntPtr lpBuffer,
            ref uint lpdwBufferLength,
            IntPtr lpdwIndex
            );

        [DllImport(WinInetDll, EntryPoint = "InternetReadFile")]
        public static extern bool InternetReadFile(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint dwNumberOfBytesToRead,
            ref uint lpdwNumberOfBytesRead
            );
    }
}