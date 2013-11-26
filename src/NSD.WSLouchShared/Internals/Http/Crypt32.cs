using System;
using System.Runtime.InteropServices;

namespace NSD.WSLouch.Internals.Http
{
    /// <summary>
    ///     P/Invoke для функций из crypt32.dll
    /// </summary>
    // ReSharper disable InconsistentNaming
    internal static class Crypt32
    {
        private const string Crypt32Dll = "crypt32.dll";

        public const int CERT_COMPARE_SHA1_HASH = 1;
        public const int CERT_COMPARE_KEY_IDENTIFIER = 15;
        public const int CERT_FIND_SHA1_HASH = (CERT_COMPARE_SHA1_HASH << CERT_COMPARE_SHIFT);
        public const int CERT_FIND_KEY_IDENTIFIER = (CERT_COMPARE_KEY_IDENTIFIER << CERT_COMPARE_SHIFT);
        public const int CERT_FIND_SUBJECT_STR_W = (CERT_COMPARE_NAME_STR_W) << ((CERT_COMPARE_SHIFT | CERT_INFO_SUBJECT_FLAG));
        public const int CERT_COMPARE_NAME_STR_W = 8;
        public const int CERT_COMPARE_SHIFT = 16;
        public const int CERT_INFO_SUBJECT_FLAG = 7;
        public const int PKCS_7_ASN_ENCODING = 65536;
        public const int X509_ASN_ENCODING = 1;

        [StructLayout(LayoutKind.Sequential)]
        public struct CERT_CONTEXT
        {
            public uint dwCertEncodingType;
            public IntPtr pbCertEncoded;
            public uint cbCertEncoded;
            public IntPtr pCertInfo;
            public IntPtr hCertStore;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPTOAPI_BLOB
        {
            public Int32 cbData;
            public IntPtr pbData;
        }

        [DllImport(Crypt32Dll, EntryPoint = "CertCloseStore")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CertCloseStore(IntPtr hCertStore, uint dwFlags);

        [DllImport(Crypt32Dll, EntryPoint = "CertOpenSystemStoreW")]
        public static extern IntPtr CertOpenSystemStoreW(uint hProv, [In, MarshalAs(UnmanagedType.LPWStr)] string szSubsystemProtocol);

        [DllImport(Crypt32Dll, EntryPoint = "CertOpenSystemStoreW")]
        public static extern IntPtr CertOpenSystemStoreW(uint hProv, IntPtr szSubsystemProtocol);

        [DllImport(Crypt32Dll, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CertFindCertificateInStore(
            IntPtr hCertStore,
            Int32 dwCertEncodingType,
            Int32 dwFindFlags,
            Int32 dwFindType,
            IntPtr pvFindPara,
            IntPtr pPrevCertContext);
    }
}