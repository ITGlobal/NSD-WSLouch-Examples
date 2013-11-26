using System;

namespace NSD.WSLouch.Internals
{
    /// <summary>
    ///     MIME-модель
    /// </summary>
    internal sealed class MimeModel
    {
        private string message;
        
        /// <summary>
        ///     Конструктор
        /// </summary>
        public MimeModel()
        {
            Boundary = "uuid:" + Guid.NewGuid();
            DataId = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     MIME-разделитель
        /// </summary>
        public string Boundary { get; private set; }

        /// <summary>
        ///     Наименование вложения
        /// </summary>
        public string DataId { get; private set; }

        /// <summary>
        ///     Размер SOAP-запроса
        /// </summary>
        public int ContentLength1 { get; private set; }

        /// <summary>
        ///     Размер вложения
        /// </summary>
        public int ContentLength2 { get; set; }
        
        /// <summary>
        ///     SOAP-запрос
        /// </summary>
        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                ContentLength1 = message.Length;
            }
        }
    }
}
