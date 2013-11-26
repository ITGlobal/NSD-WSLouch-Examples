using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace NSD.WSLouch.Internals
{
    /// <summary>
    ///     Объект SOAP-запроса
    /// </summary>
    internal sealed class RequestMessage : IEnumerable<RequestParameter>
    {
        #region Поля

        private const string BodyId = "NRDRequest";

        private readonly string methodName;
        private readonly string methodNs;
        private readonly XmlDocument xml;

        private readonly XmlNode head;
        private readonly XmlNode body;
        private readonly XmlNode method;

        private readonly List<RequestParameter> parameters = new List<RequestParameter>();

        #endregion

        #region Конструктор

        /// <summary>
        ///     Конструктор
        /// </summary>
        /// <param name="methodName">
        ///     SOAP-метод
        /// </param>
        public RequestMessage(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentNullException("methodName");

            this.methodName = methodName;
            xml = new XmlDocument { PreserveWhitespace = true };
            var envelopeXml = RequestBuilder.Build(new EnvelopeModel
                {
                    MethodName = methodName,
                    RequestId = BodyId
                });

            xml.LoadXml(envelopeXml);

            method = xml.GetNodeByLocalName(methodName);
            body = xml.GetNodeByLocalName("Body");
            head = xml.GetNodeByLocalName("Header");

            methodNs = method.NamespaceURI;
        } 

        #endregion

        #region Свойства
        
        /// <summary>
        ///     SOAP-метод
        /// </summary>
        public string MethodName { get { return methodName; } } 

        #endregion

        #region Публичные методы

        /// <summary>
        ///     Добавить параметр в SOAP-запрос
        /// </summary>
        /// <param name="name">
        ///     Название параметра
        /// </param>
        /// <param name="value">
        ///     Значение параметра
        /// </param>
        public void Add(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            var parameter = xml.CreateElement(name, methodNs);
            parameter.InnerText = value;
            method.AppendChild(parameter);
            parameters.Add(new RequestParameter(name, value));
        }

        /// <summary>
        ///     Добавить параметр-ссылку в SOAP-запрос
        /// </summary>
        /// <param name="name">
        ///     Название параметра
        /// </param>
        /// <param name="id">
        ///     Значение параметра
        /// </param>
        public void AddHrefParameter(string name, string id)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            var parameter = xml.CreateElement(name, methodNs);
            var value = "cid:" + id;
            parameter.SetAttribute("href", value);
            method.AppendChild(parameter);
            parameters.Add(new RequestParameter(name, value));
        }

        /// <summary>
        ///     Сгенерировать заголовок SOAP-запроса
        /// </summary>
        /// <param name="vcert">
        ///     Криптопровайдер
        /// </param>
        public void GenerateHeader(IVcertAPI vcert)
        {
            if (vcert == null)
                throw new ArgumentNullException("vcert");

            var bodyCanon = body.Canonicalize();
            var digest = vcert.Hash(bodyCanon);

            var securityXml = RequestBuilder.Build(new HeaderModel
                {
                    DigestValue = digest,
                    Uri = BodyId
                });
            head.InnerXml = securityXml;

            var signedInfoNode = xml.GetNodeByLocalName("SignedInfo");
            var signedInfoCanon = signedInfoNode.Canonicalize();
            var signatureValue = vcert.Sign(signedInfoCanon);
            var signatureValueNode = xml.GetNodeByLocalName("SignatureValue");
            signatureValueNode.InnerText = signatureValue;
        }

        /// <summary>
        ///     Возвращает отформатированное название метода с параметрами и их значениями
        /// </summary>
        public string GetFormattedMethodName()
        {
            return string.Format("{0}({1})", methodName, string.Join(", ", parameters));
        }

        /// <summary>
        ///     Возвращает объект <see cref="T:System.String"/>, который представляет текущий объект <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        ///     Объект <see cref="T:System.String"/>, представляющий текущий объект <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return xml.Canonicalize();
        }

        #endregion

        #region Реализация IEnumerable<>. Требуется для поддержки синтаксиса инициализации коллекций.

        public IEnumerator<RequestParameter> GetEnumerator()
        {
            return parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
