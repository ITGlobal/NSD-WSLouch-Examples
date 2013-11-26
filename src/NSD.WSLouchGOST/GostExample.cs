using System;

namespace NSD.WSLouch
{
    /// <summary>
    ///     Пример работы со службой WSL с использованием криптографии ГОСТ
    /// </summary>
    public sealed class GostExample : ExampleBase
    {
        /// <summary>
        ///     Точка входа
        /// </summary>
        /// <param name="args">
        ///     Параметры командной строки
        /// </param>
        public static void Main(string[] args)
        {
            Console.WriteLine("WSL Util (ГОСТ)");
            var example = new GostExample();
            example.Execute(args);
        }

        /// <summary>
        ///     Инициализировать криптопровайдер
        /// </summary>
        protected override IVcertAPI InitializeCryptoProvider()
        {
            return new GostVcertAPI();
        }

        /// <summary>
        ///     Определить адрес конечной точки службы
        /// </summary>
        /// <param name="endpoint">
        ///     Контур службы (PL или PROD)
        /// </param>
        protected override Uri ResolveWslUri(WslEndpoint endpoint)
        {
            string uri;
            switch (endpoint)
            {
                case WslEndpoint.PL:
                    uri = "https://rsa.nsd.ru/onyxpl/WslService";
                    break;
                case WslEndpoint.PROD:
                    uri = "https://edor.nsd.ru/onyxpr/WslService";
                    break;
                default:
                    throw new ApplicationException(string.Format("Неверная конечная точка - \"{0}\"", endpoint));
            }

            return new Uri(uri);
        }
    }
}