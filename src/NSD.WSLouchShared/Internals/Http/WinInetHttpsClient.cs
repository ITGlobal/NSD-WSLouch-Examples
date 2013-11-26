using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace NSD.WSLouch.Internals.Http
{
    /// <summary>
    ///     Реализация HTTPS через библиотеку WinInet.
    ///     Реализация через WinInet необходима для поддержки HTTPS протокола с шифрованием ГОСТ.
    /// </summary>
    internal sealed class WinInetHttpsClient : IDisposable
    {
        #region Поля
        
        private readonly Uri uri;
        private readonly IntPtr pCertContext;
        private readonly bool hasClientCertificate;
        private readonly IntPtr hInternet;
        private readonly IntPtr hConnect; 

        #endregion

        #region Конструктор

        /// <summary>
        ///     Конструктор
        /// </summary>
        /// <param name="uri">
        ///     Адрес конечной точки
        /// </param>
        /// <param name="certificate">
        ///     Клиентский сертифика
        /// </param>
        public WinInetHttpsClient(Uri uri, X509Certificate2 certificate)
        {
            this.uri = uri;

            #region Инициализация Crypt32

            if (certificate != null)
            {
                // Поиск сертификата
                var hStore = Crypt32.CertOpenSystemStoreW(0, "My");
                var hash = certificate.GetCertHash();

                Crypt32.CRYPTOAPI_BLOB cryptBlob;
                cryptBlob.cbData = hash.Length;
                using (var hashHandle = InteropUtil.Pin(hash))
                {
                    cryptBlob.pbData = hashHandle.Address;
                    using (var blobHandle = InteropUtil.Pin(cryptBlob))
                    {
                        pCertContext = Crypt32.CertFindCertificateInStore(
                            hStore,
                            Crypt32.X509_ASN_ENCODING | Crypt32.PKCS_7_ASN_ENCODING,
                            0,
                            Crypt32.CERT_FIND_SHA1_HASH,
                            blobHandle.Address,
                            IntPtr.Zero);
                        if (pCertContext == IntPtr.Zero)
                        {
                            ThrowWinInetLastException("CertFindCertificateInStore");
                        }
                    }
                }

                Crypt32.CertCloseStore(hStore, 0);
                hasClientCertificate = true;
            }
            else
            {
                hasClientCertificate = false;
            }

            #endregion

            #region Инициализация WinInet

            // Инициализация WinInet
            hInternet = WinInet.InternetOpenW(
                "WSL_Proxy",
                WinInet.INTERNET_OPEN_TYPE_DIRECT,
                null,
                null,
                0);
            if (hInternet == IntPtr.Zero)
            {
                ThrowWinInetLastException("InternetOpen");
            }

            hConnect = WinInet.InternetConnectW(hInternet, uri.Host, (short)uri.Port, null, null,
                WinInet.INTERNET_SERVICE_HTTP, 0, IntPtr.Zero);
            if (hConnect == IntPtr.Zero)
            {
                ThrowWinInetLastException("InternetConnect");
            }

            #endregion
        } 

        #endregion

        #region Публичные методы

        /// <summary>
        ///     Выполнить HTTPS запрос
        /// </summary>
        /// <param name="head">
        ///     Заголовки запроса
        /// </param>
        /// <param name="body">
        ///     Тело запроса
        /// </param>
        /// <returns>
        ///     Результат HTTPS-запроса
        /// </returns>
        public HttpResult ExecuteRequest(string head, byte[] body)
        {
            var result = new HttpResult();

            var hRequest = CreateRequest();
            if (hasClientCertificate)
            {
                SetClientCertificate(hRequest);
                SetCertificateCheckOptions(hRequest);
            }

            ExecuteRequest(hRequest, head, body);

            GetStatusCode(hRequest, result);
            GetContentType(hRequest, result);
            GetContent(hRequest, result);

            WinInet.InternetCloseHandle(hRequest);

            return result;
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        public void Dispose()
        {
            WinInet.InternetCloseHandle(hConnect);
            WinInet.InternetCloseHandle(hInternet);
        } 

        #endregion

        #region Приватные методы

        /// <summary>
        ///     Создать HTTPS-запрос
        /// </summary>
        /// <returns>
        ///     Указатель на HTTPS-запрос
        /// </returns>
        private IntPtr CreateRequest()
        {
            var hRequest = WinInet.HttpOpenRequest(
                hConnect,
                "POST",
                uri.AbsolutePath,
                null,
                null,
                IntPtr.Zero,
                WinInet.INTERNET_FLAG_SECURE
                | WinInet.INTERNET_FLAG_IGNORE_CERT_CN_INVALID
                | WinInet.INTERNET_FLAG_IGNORE_CERT_DATE_INVALID
                | WinInet.INTERNET_FLAG_NO_AUTO_REDIRECT
                | WinInet.INTERNET_FLAG_PRAGMA_NOCACHE
                | WinInet.INTERNET_FLAG_NO_CACHE_WRITE
                | WinInet.INTERNET_FLAG_KEEP_CONNECTION,
                (IntPtr)1);
            if (hRequest == IntPtr.Zero)
            {
                ThrowWinInetLastException("HttpOpenRequest");
            }

            return hRequest;
        }

        /// <summary>
        ///     Проставить клиентский сертификат
        /// </summary>
        /// <param name="hRequest">
        ///     Указатель на HTTPS-запрос
        /// </param>
        private void SetClientCertificate(IntPtr hRequest)
        {
            var certSet = WinInet.InternetSetOptionW(
                hRequest,
                WinInet.INTERNET_OPTION_CLIENT_CERT_CONTEXT,
                pCertContext,
                (uint)Marshal.SizeOf(typeof(Crypt32.CERT_CONTEXT)));
            if (!certSet)
            {
                ThrowWinInetLastException("InternetSetOption + INTERNET_OPTION_CLIENT_CERT_CONTEXT");
            }
        }

        /// <summary>
        ///     Проставить опции проверки сертификатов сервера
        /// </summary>
        /// <param name="hRequest">
        ///     Указатель на HTTPS-запрос
        /// </param>
        private static void SetCertificateCheckOptions(IntPtr hRequest)
        {
            uint flagsLen = sizeof(uint);
            var flags = new WinInet.InternetOptionSecurityFlags();
            var pFlags = Marshal.AllocHGlobal(Marshal.SizeOf(flags));
            Marshal.StructureToPtr(flags, pFlags, true);
            var queryOption = WinInet.InternetQueryOptionW(hRequest, WinInet.INTERNET_OPTION_SECURITY_FLAGS, pFlags, ref flagsLen);
            flags = (WinInet.InternetOptionSecurityFlags)
                Marshal.PtrToStructure(pFlags, typeof(WinInet.InternetOptionSecurityFlags));
            Marshal.FreeHGlobal(pFlags);
            if (!queryOption)
            {
                ThrowWinInetLastException("InternetQueryOption + INTERNET_OPTION_SECURITY_FLAGS");
            }

            flags.value |= WinInet.SECURITY_FLAG_IGNORE_REVOCATION;
            flags.value |= WinInet.SECURITY_FLAG_IGNORE_UNKNOWN_CA;

            pFlags = Marshal.AllocHGlobal(Marshal.SizeOf(flags));
            Marshal.StructureToPtr(flags, pFlags, true);
            var certRevSet = WinInet.InternetSetOptionW(hRequest, WinInet.INTERNET_OPTION_SECURITY_FLAGS, pFlags, flagsLen);
            Marshal.FreeHGlobal(pFlags);
            if (!certRevSet)
            {
                ThrowWinInetLastException("InternetSetOption + INTERNET_OPTION_SECURITY_FLAGS");
            }
        }

        /// <summary>
        ///     Отправить HTTPS-запрос
        /// </summary>
        /// <param name="hRequest">
        ///     Указатель на HTTPS-запрос
        /// </param>
        /// <param name="head">
        ///     Заголовки запроса
        /// </param>
        /// <param name="body">
        ///     Тело запроса
        /// </param>
        private static void ExecuteRequest(IntPtr hRequest, string head, byte[] body)
        {
            var bSend = WinInet.HttpSendRequestW(hRequest, head, head.Length, body, body.Length);
            if (!bSend)
            {
                ThrowWinInetLastException("HttpSendRequest");
            }
        }

        /// <summary>
        ///     Получить код статуса HTTP
        /// </summary>
        /// <param name="hRequest">
        ///     Указатель на HTTPS-запрос
        /// </param>
        /// <param name="result">
        ///     Результат HTTPS-запроса
        /// </param>
        private static void GetStatusCode(IntPtr hRequest, HttpResult result)
        {
            uint statusSize = sizeof(uint);
            using (var buffer = InteropUtil.Alloc(statusSize))
            {
                if (!WinInet.HttpQueryInfoW(
                    hRequest,
                    WinInet.HTTP_QUERY_FLAG_NUMBER | WinInet.HTTP_QUERY_STATUS_CODE,
                    buffer.Address,
                    ref statusSize,
                    IntPtr.Zero))
                {
                    ThrowWinInetLastException("HttpQueryInfoW(HTTP_QUERY_FLAG_NUMBER|HTTP_QUERY_STATUS_CODE)");
                }

                result.Code = (HttpStatusCode)Marshal.ReadInt32(buffer.Address);
            }
        }

        /// <summary>
        ///     Получить тип содержимого
        /// </summary>
        /// <param name="hRequest">
        ///     Указатель на HTTPS-запрос
        /// </param>
        /// <param name="result">
        ///     Результат HTTPS-запроса
        /// </param>
        private static void GetContentType(IntPtr hRequest, HttpResult result)
        {
            var statusSize = 256U;
            using (var buffer = InteropUtil.Alloc(statusSize))
            {
                if (!WinInet.HttpQueryInfoW(
                    hRequest,
                    WinInet.HTTP_QUERY_CONTENT_TYPE,
                    buffer.Address,
                    ref statusSize,
                    IntPtr.Zero))
                {
                    ThrowWinInetLastException("HttpQueryInfoW(HTTP_QUERY_CONTENT_TYPE)");
                }

                result.ContentType = Marshal.PtrToStringUni(buffer.Address);
            }
        }

        /// <summary>
        ///     Получить содержимое
        /// </summary>
        /// <param name="hRequest">
        ///     Указатель на HTTPS-запрос
        /// </param>
        /// <param name="result">
        ///     Результат HTTPS-запроса
        /// </param>
        private static void GetContent(IntPtr hRequest, HttpResult result)
        {
            using (var stream = new MemoryStream())
            {
                const uint chunkSize = 1024U;
                using (var buffer = InteropUtil.Alloc(chunkSize))
                {
                    while (true)
                    {
                        var bytesRead = 0U;
                        if (!WinInet.InternetReadFile(hRequest, buffer.Address, chunkSize, ref bytesRead))
                        {
                            ThrowWinInetLastException("InternetReadFile");
                        }

                        for (var i = 0; i < bytesRead; i++)
                        {
                            stream.WriteByte(Marshal.ReadByte(buffer.Address + i));
                        }

                        if (bytesRead <= 0)
                        {
                            break;
                        }
                    }
                }

                result.Content = stream.ToArray();
            }
        }

        /// <summary>
        ///     Бросить исключение, вызванное ошибкой в WinInet
        /// </summary>
        /// <param name="function">
        ///     Функция, в которой возникла ошибка
        /// </param>
        private static void ThrowWinInetLastException(string function)
        {
            var errorCode = Kernel32.GetLastError();

            var lpBuffer = new StringBuilder(1024);
            if (Kernel32.FormatMessage(
                Kernel32.FORMAT_MESSAGE_FROM_SYSTEM | Kernel32.FORMAT_MESSAGE_IGNORE_INSERTS | Kernel32.FORMAT_MESSAGE_FROM_HMODULE,
                Kernel32.GetModuleHandleW("wininet.dll"),
                errorCode,
                Kernel32.MAKELANGID(Kernel32.LANG_NEUTRAL, Kernel32.SUBLANG_NEUTRAL),
                lpBuffer,
                lpBuffer.Capacity + 1,
                IntPtr.Zero) == 0)
            {
                lpBuffer.Clear();
                lpBuffer.Append("Unknown error (0x" + Convert.ToString(errorCode, 16) + ")");
            }

            throw new Win32Exception(
                errorCode,
                string.Format(
                    "Error calling \"{0}\", code = {1}, message = \"{2}\"",
                    function,
                    errorCode,
                    lpBuffer)
                );
        } 

        #endregion
    }
}
