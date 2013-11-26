using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NSD.WSLouch.Internals;
using NSD.WSLouch.Internals.Http;

namespace NSD.WSLouch
{
    /// <summary>
    ///     Адаптер службы WSL
    /// </summary>
    public sealed class WslAdapter
    {
        #region Поля

        private readonly Uri uri;
        private readonly IHttpWorker worker;

        #endregion

        #region Конструктор

        /// <summary>
        ///     Статический конструктор
        /// </summary>
        static WslAdapter()
        {
            // Необходимо, т.к. сертификат НРД не является доверенным
            ServicePointManager.ServerCertificateValidationCallback += (se, cert, chain, sslerror) => true;
        }

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
        public WslAdapter(Uri uri, IVcertAPI vcert, X509Certificate2 clientCertificate = null)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (vcert == null)
                throw new ArgumentNullException("vcert");

            // При подключении через HTTP клиентский сертификат не используется
            if (uri.Scheme == Uri.UriSchemeHttps && clientCertificate == null)
                throw new ArgumentNullException("clientCertificate");

            this.uri = uri;

            worker = (uri.Scheme == Uri.UriSchemeHttps)
                ? (IHttpWorker)new NativeHttpsWorker(uri, vcert, clientCertificate)
                : new ManagedHttpWorker(uri, vcert, clientCertificate);
        }

        #endregion

        #region Методы службы WSL

        /// <summary>
        ///     Инициализация передачи пакета
        /// </summary>
        /// <param name="personCode">
        ///     Депозитарный код участника
        /// </param>
        /// <param name="packageFileName">
        ///     Имя файла пакета (формируется по правилам ЭДО)
        /// </param>
        /// <returns>
        ///     Код пакета
        /// </returns>
        public string InitTransferIn(string personCode, string packageFileName)
        {
            if (string.IsNullOrEmpty(personCode))
                throw new ArgumentNullException("personCode");
            if (string.IsNullOrEmpty(packageFileName))
                throw new ArgumentNullException("packageFileName");

            var message = new RequestMessage("InitTransferIn")
            {
                {"PersonCode", personCode},
                {"PackageFileName", packageFileName}
            };
            var callResult = worker.CallRequest(message);
            var reply = ParseReply(callResult);
            Verify(reply, message);
            return reply.Xml;
        }

        /// <summary>
        ///     Передача блока пакета
        /// </summary>
        /// <param name="personCode">
        ///     Депозитарный код участника
        /// </param>
        /// <param name="packageId">
        ///     Код пакета
        /// </param>
        /// <param name="partNumber">
        ///     Номер блока
        /// </param>
        /// <param name="partsQuantity">
        ///     Общее число блоков
        /// </param>
        /// <param name="packageBody">
        ///     Содержимое блока
        /// </param>
        public string PutPackage(string personCode, string packageId, int partNumber, int partsQuantity, byte[] packageBody)
        {
            if (string.IsNullOrEmpty(personCode))
                throw new ArgumentNullException("personCode");
            if (string.IsNullOrEmpty(packageId))
                throw new ArgumentNullException("packageId");
            if (partNumber <= 0)
                throw new ArgumentNullException("partNumber");
            if (partsQuantity <= 0)
                throw new ArgumentNullException("partsQuantity");
            if (packageBody == null)
                throw new ArgumentNullException("packageBody");
            if (packageBody.Length == 0)
                throw new ArgumentNullException("packageBody", "Блок передаваемого пакета не может быть пустым");

            var message = new RequestMessage("PutPackage")
            {
                {"PersonCode", personCode},
                {"PackageId", packageId},
                {"PartNumber", partNumber.ToString(CultureInfo.InvariantCulture)},
                {"PartsQuantity", partsQuantity.ToString(CultureInfo.InvariantCulture)}
            };

            var callResult = worker.CallRequest(message, packageBody);
            var reply = ParseReply(callResult);
            Verify(reply, message);
            return reply.Xml;
        }

        /// <summary>
        ///     Завершение передачи пакета
        /// </summary>
        /// <param name="personCode">
        ///     Депозитарный код участника
        /// </param>
        /// <param name="packageId">
        ///     Код пакета
        /// </param>
        public string GetTransferResult(string personCode, string packageId)
        {
            if (string.IsNullOrEmpty(personCode))
                throw new ArgumentNullException("personCode");
            if (string.IsNullOrEmpty(packageId))
                throw new ArgumentNullException("packageId");

            var message = new RequestMessage("GetTransferResult")
            {
                {"PersonCode", personCode},
                {"PackageId", packageId}
            };

            var callResult = worker.CallRequest(message);
            var reply = ParseReply(callResult);
            Verify(reply, message);
            return reply.Xml;
        }

        /// <summary>
        ///     Запрос списка входящих пакетов
        /// </summary>
        /// <param name="personCode">
        ///     Депозитарный код участника
        /// </param>
        /// <param name="date">
        ///     Дата для запроса списка пакетов
        /// </param>
        /// <returns>
        ///     XML списка пакетов
        /// </returns>
        public string GetPackageList(string personCode, DateTime date)
        {
            if (string.IsNullOrEmpty(personCode))
                throw new ArgumentNullException("personCode");

            var message = new RequestMessage("GetPackageList")
            {
                {"PersonCode", personCode},
                {"Date", date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)}
            };

            var callResult = worker.CallRequest(message);
            var reply = ParseReply(callResult);
            Verify(reply, message);
            return reply.Xml;
        }

        /// <summary>
        ///     Загрузка пакета
        /// </summary>
        /// <param name="personCode">
        ///     Депозитарный код участника
        /// </param>
        /// <param name="packageId">
        ///     Номер пакета
        /// </param>
        /// <param name="partNumber">
        ///     Номер блока
        /// </param>
        /// <param name="partsQuantity">
        ///     Число блоков
        /// </param>
        /// <returns>
        ///     Содержимое блока
        /// </returns>
        public byte[] GetPackage(string personCode, int packageId, int partNumber, int partsQuantity)
        {
            if (string.IsNullOrEmpty(personCode))
                throw new ArgumentNullException("personCode");
            if (partNumber <= 0)
                throw new ArgumentNullException("partNumber");
            if (partsQuantity <= 0)
                throw new ArgumentNullException("partsQuantity");

            var message = new RequestMessage("GetPackage")
            {
                {"PersonCode", personCode},
                {"PackageId", packageId.ToString(CultureInfo.InvariantCulture)},
                {"PartNumber", partNumber.ToString(CultureInfo.InvariantCulture)},
                {"PartsQuantity", partsQuantity.ToString(CultureInfo.InvariantCulture)}
            };

            var callResult = worker.CallRequest(message);
            var reply = ParseReply(callResult);
            Verify(reply, message);
            return reply.Data;
        }

        #endregion

        #region Приватные методы

        /// <summary>
        ///     Разбор HTTP-ответа службы
        /// </summary>
        private static Response ProcessResponse(HttpResult reply)
        {
            var response = new Response { StatusCode = reply.Code };

            var contentType = reply.ContentType;
            var ctFields = contentType.Split(';');
            var binary = ctFields[0] == "multipart/related";

            if (binary)
            {
                string boundary = null;
                for (var i = 1; i < ctFields.Length; i++)
                {
                    var field = ctFields[i].Trim(' ').Split('=');
                    if (field[0] == "boundary")
                    {
                        boundary = field[1].Trim('\"');
                        break;
                    }
                }

                if (boundary == null)
                {
                    throw new ApplicationException("Multipart boundary не найден в заголовке Content-Type");
                }

                var respData = reply.Content;
                var pos = 0;

                using (var stream = new MemoryStream(respData))
                {
                    using (var reader = new StreamReader(stream, Encoding.ASCII))
                    {
                        Func<string> readLine = () =>
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            var line = reader.ReadLine();
                            if (line != null)
                            {
                                pos += line.Length + 2;
                            }
                            return line;
                        };

                        Action goToEmpty = () =>
                        {
                            string line;
                            do
                            {
                                line = readLine();
                            } while (!string.IsNullOrWhiteSpace(line));
                        };

                        goToEmpty();

                        var xmlString = readLine();
                        response.Xml.LoadXml(xmlString);

                        goToEmpty();
                    }
                }

                var length = respData.Length - (pos + boundary.Length + 6);
                var rData = new byte[length];
                Buffer.BlockCopy(respData, pos, rData, 0, length);
                response.Data = rData;
            }
            else
            {
                response.Xml.LoadXml(Encoding.UTF8.GetString(reply.Content));
            }

            return response;
        }

        /// <summary>
        ///     Анализ ошбок
        /// </summary>
        private static Reply ParseReply(HttpResult result)
        {
            var processedResponse = ProcessResponse(result);
            var reply = new Reply
            {
                Data = processedResponse.Data,
                ErrorCode = 0
            };

            if (processedResponse.StatusCode != HttpStatusCode.OK)
            {
                reply.ErrorCode = -1;

                var faultNode = processedResponse.Xml.GetNodeByLocalName("Fault");
                if (faultNode != null)
                {
                    var faultstringNode = faultNode.SelectSingleNode("faultstring");
                    var reason = faultstringNode != null ? faultstringNode.InnerText : "?";
                    var faultcodeNode = faultNode.SelectSingleNode("faultcode");
                    var code = faultcodeNode != null ? faultcodeNode.InnerText : "?";

                    reply.ErrorDescription = string.Format("FaultException. Code = {0}; Reason = {1};", code, reason);

                    var detailNode = faultNode.SelectSingleNode("detail");
                    if (detailNode != null)
                    {
                        reply.ErrorDescription += " Detail = " + detailNode.InnerText;
                    }
                }
                else
                {
                    reply.ErrorDescription = "Не удалось получить подробности ошибки";
                }
            }
            else
            {
                var responseNode = processedResponse.Xml.GetNodeByLocalName("return");
                reply.Xml = responseNode == null ? null : responseNode.InnerText;
            }

            return reply;
        }

        /// <summary>
        ///     Анализ ошибок в ответе службы WSL
        /// </summary>
        private void Verify(Reply reply, RequestMessage message)
        {
            if (reply.ErrorCode != 0)
            {
                var errorMessage = string.Format(
                    "{0}: Служба WSL вернула ошибку #{1} \"{2}\". Адрес службы: \"{3}\"",
                    message.GetFormattedMethodName(),
                    reply.ErrorCode,
                    reply.ErrorDescription,
                    uri
                    );
                throw new WslException(errorMessage);
            }
        }

        #endregion
    }
}
