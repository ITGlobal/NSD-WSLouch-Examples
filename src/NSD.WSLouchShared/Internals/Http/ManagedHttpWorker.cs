using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace NSD.WSLouch.Internals.Http
{
    /// <summary>
    ///     Адаптер HTTP протокола. Также может использоваться для HTTPS протокола с шифрованием RSA.
    ///     Не работает для HTTPS протокола с шифрованием ГОСТ.
    /// </summary>
    internal sealed class ManagedHttpWorker : IHttpWorker
    {
        #region Поля
        
        private readonly Uri uri;
        private readonly X509Certificate2 clientCertificate;
        private readonly IVcertAPI vcert; 

        #endregion

        #region Конструктор

        /// <summary>
        ///     Конструктор
        /// </summary>
        /// <param name="uri">
        ///     URL конечной точки службы WSL
        /// </param>
        /// <param name="vcert">
        ///     Криптопровайдер
        /// </param>
        /// <param name="clientCertificate">
        ///     Клиентский сертификат. Используется только в случае, если используется протокол HTTPS
        /// </param>
        public ManagedHttpWorker(Uri uri, IVcertAPI vcert, X509Certificate2 clientCertificate)
        {
            this.uri = uri;
            this.clientCertificate = clientCertificate;
            this.vcert = vcert;
        } 

        #endregion

        #region Публичные методы

        /// <summary>
        ///     Вызвать SOAP-метод
        /// </summary>
        /// <param name="message">
        ///     SOAP-запрос
        /// </param>
        /// <returns>
        ///     Результат HTTP-запроса
        /// </returns>
        public HttpResult CallRequest(RequestMessage message)
        {
            var request = CreateRequest(message);
            return CallRequest(request);
        }

        /// <summary>
        ///     Вызвать SOAP-метод с вложением
        /// </summary>
        /// <param name="message">
        ///     SOAP-запрос
        /// </param>
        /// <param name="attachment">
        ///     Вложение
        /// </param>
        /// <returns>
        ///     Результат HTTP-запроса
        /// </returns>
        public HttpResult CallRequest(RequestMessage message, byte[] attachment)
        {
            var request = CreateRequest(message, attachment);
            return CallRequest(request);
        }

        public void Dispose() { } 

        #endregion

        #region Приватные методы

        /// <summary>
        ///     Создать HTTP запрос
        /// </summary>
        /// <param name="methodName">
        ///     SOAP-метод
        /// </param>
        /// <returns>
        ///     HTTP запрос
        /// </returns>
        private HttpWebRequest CreateRequest(string methodName)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);

            if (uri.Scheme == Uri.UriSchemeHttps)
            {
                request.ClientCertificates.Add(clientCertificate);
            }

            request.Headers.Add("SOAPAction", string.Join("/", uri, methodName));
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Method = "POST";
            request.KeepAlive = true;

            return request;
        }

        /// <summary>
        ///     Создать HTTP запрос
        /// </summary>
        /// <param name="message">
        ///     SOAP-запрос
        /// </param>
        /// <returns>
        ///     HTTP запрос
        /// </returns>
        private HttpWebRequest CreateRequest(RequestMessage message)
        {
            message.GenerateHeader(vcert);

            var request = CreateRequest(message.MethodName);
            request.ContentType = "text/xml; charset=\"utf-8\"";

            using (var stream = request.GetRequestStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(message.ToString());
                }
                stream.Close();
            }

            return request;
        }

        /// <summary>
        ///     Создать HTTP запрос
        /// </summary>
        /// <param name="message">
        ///     SOAP-запрос
        /// </param>
        /// <param name="attachment">
        ///     Вложение
        /// </param>
        /// <returns>
        ///     HTTP запрос
        /// </returns>
        private HttpWebRequest CreateRequest(RequestMessage message, byte[] attachment)
        {
            var mime = new MimeModel
            {
                ContentLength2 = attachment.Length
            };

            message.AddHrefParameter("PackageBody", mime.DataId);
            message.GenerateHeader(vcert);

            mime.Message = message.ToString();

            var request = CreateRequest(message.MethodName);
            request.ContentType = string.Format("multipart/related; type=\"application/xop+xml\";boundary=\"{0}\"", mime.Boundary);

            var mimeStr = RequestBuilder.Build(mime);
            var msgBytes = Encoding.UTF8.GetBytes(mimeStr);

            using (var stream = request.GetRequestStream())
            {
                stream.Write(msgBytes, 0, msgBytes.Length);
                stream.Write(attachment, 0, attachment.Length);

                var ending = Encoding.UTF8.GetBytes(string.Format("\r\n--{0}--", mime.Boundary));
                stream.Write(ending, 0, ending.Length);

                stream.Close();
            }

            return request;
        }

        /// <summary>
        ///     Исполнить HTTP запрос
        /// </summary>
        /// <param name="request">
        ///     HTTP запрос
        /// </param>
        /// <returns>
        ///     Результат HTTP-запроса
        /// </returns>
        private static HttpResult CallRequest(HttpWebRequest request)
        {
            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    throw;
                resp = (HttpWebResponse)ex.Response;
            }

            var result = new HttpResult
            {
                Code = resp.StatusCode,
                ContentType = resp.ContentType
            };

            using (var stream = resp.GetResponseStream())
            {
                result.Content = ReadToEnd(stream);
            }

            return result;
        }

        /// <summary>
        ///     Прочитать содержимое потока до конца
        /// </summary>
        /// <param name="stream">
        ///     Поток
        /// </param>
        /// <returns>
        ///     Содержимое потока
        /// </returns>
        private static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                var readBuffer = new byte[4096];
                var totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        var nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            var temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                var buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        } 

        #endregion
    }
}
