using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace NSD.WSLouch.Internals.Http
{
    /// <summary>
    ///     Адаптер HTTPS протокола. Может использоваться для HTTPS протокола как с шифрованием RSA, так и с шифрованием ГОСТ.
    ///     Не работает для HTTP протокола.
    /// </summary>
    internal sealed class NativeHttpsWorker : IHttpWorker
    {
        #region Вложенный класс

        /// <summary>
        ///     HTTPS запрос
        /// </summary>
        private sealed class HttpsRequest
        {
            private readonly StringBuilder head = new StringBuilder();

            /// <summary>
            ///     Заголовки запроса
            /// </summary>
            public StringBuilder Head { get { return head; } }

            /// <summary>
            ///     Содержимое запроса
            /// </summary>
            public byte[] Body { get; set; }
        } 

        #endregion

        #region Поля

        private readonly Uri uri;
        private readonly IVcertAPI vcert;
        private readonly WinInetHttpsClient httpsClient; 

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
        public NativeHttpsWorker(Uri uri, IVcertAPI vcert, X509Certificate2 clientCertificate)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (vcert == null)
                throw new ArgumentNullException("vcert");
            if (clientCertificate == null)
                throw new ArgumentNullException("clientCertificate");

            this.uri = uri;
            this.vcert = vcert;

            httpsClient = new WinInetHttpsClient(uri, clientCertificate);
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
            return httpsClient.ExecuteRequest(request.Head.ToString(), request.Body);
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
            return httpsClient.ExecuteRequest(request.Head.ToString(), request.Body);
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        public void Dispose()
        {
            httpsClient.Dispose();
        } 

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
        private HttpsRequest CreateRequest(string methodName)
        {
            var request = new HttpsRequest();

            request.Head.AppendFormat("SOAPAction: {0}\r\n", string.Join("/", uri, methodName));
            request.Head.Append("Accept-Encoding: gzip, deflate\r\n");
            request.Head.AppendFormat("Host: {0}\r\n", uri.Host);

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
        private HttpsRequest CreateRequest(RequestMessage message)
        {
            message.GenerateHeader(vcert);
            var request = CreateRequest(message.MethodName);

            request.Head.Append("Content-Type: text/xml; charset=\"utf-8\"\r\n");
            request.Body = Encoding.UTF8.GetBytes(message.ToString());

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
        private HttpsRequest CreateRequest(RequestMessage message, byte[] attachment)
        {
            var mime = new MimeModel
            {
                ContentLength2 = attachment.Length
            };

            message.AddHrefParameter("PackageBody", mime.DataId);
            message.GenerateHeader(vcert);

            mime.Message = message.ToString();

            var request = CreateRequest(message.MethodName);
            request.Head.AppendFormat("Content-Type: multipart/related; type=\"application/xop+xml\";boundary=\"{0}\"\r\n", mime.Boundary);

            var mimeStr = RequestBuilder.Build(mime);

            var msgBytes = Encoding.UTF8.GetBytes(mimeStr);
            var ending = Encoding.UTF8.GetBytes(string.Format("\r\n--{0}--", mime.Boundary));

            request.Body = new byte[msgBytes.Length + attachment.Length + ending.Length];
            using (var stream = new MemoryStream(request.Body))
            {
                stream.Write(msgBytes, 0, msgBytes.Length);
                stream.Write(attachment, 0, attachment.Length);
                stream.Write(ending, 0, ending.Length);

                stream.Close();
            }

            return request;
        } 

        #endregion
    }
}