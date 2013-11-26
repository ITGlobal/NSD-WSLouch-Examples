using System;
using System.Linq;

namespace NSD.WSLouch
{
    /// <summary>
    ///     Параметры комадной строки
    /// </summary>
    public sealed class ProgramOptions
    {
        #region Поля
        
        private WslEndpoint endpoint;
        private string serialNumber;
        private string personCode;
        private WslOperation operation; 

        #endregion

        #region Свойства

        /// <summary>
        ///     Контур службы WSL
        /// </summary>
        public WslEndpoint Endpoint { get { return endpoint; } }

        /// <summary>
        ///     Серийный номер клиентского сертификата
        /// </summary>
        public string SerialNumber { get { return serialNumber; } }

        /// <summary>
        ///     Депозитарный код
        /// </summary>
        public string PersonCode { get { return personCode; } }

        /// <summary>
        ///     Операция
        /// </summary>
        public WslOperation Operation { get { return operation; } }

        /// <summary>
        ///     Параметры операции
        /// </summary>
        public string[] Arguments { get; private set; } 

        #endregion

        #region Публичные методы

        /// <summary>
        ///     Разбор параметров командной строки
        /// </summary>
        /// <param name="args">
        ///     Список параметров комадной строки
        /// </param>
        /// <param name="options">
        ///     Результаты разбора параметров командной строки
        /// </param>
        /// <returns>
        ///     true, если параметры удалось разобрать. false - в противном случае.
        /// </returns>
        public static bool TryParse(string[] args, out ProgramOptions options)
        {
            // 1 аргумент - контур службы (PL или PROD)
            // 2 аргумент - депозитарный код 
            // 3 аргумент - серийный номер сертификата
            // 4 аргумент - код операции (PutPackage|GetPackageList|GetPackage)
            // 5 аргумент и далее - параметры операции
            if (args.Length < 5)
            {
                options = null;
                return false;
            }

            options = new ProgramOptions();
            if (TryParse(args[0], out options.endpoint) &&
                TryParse(args[1], out options.personCode) &&
                TryParse(args[2], out options.serialNumber) &&
                TryParse(args[3], out options.operation))
            {
                options.Arguments = args.Skip(4).ToArray();
                return true;
            }

            options = null;
            return false;
        } 

        #endregion

        #region Приватные методы

        private static bool TryParse(string value, out WslEndpoint result)
        {
            return Enum.TryParse(value, true, out result);
        }

        private static bool TryParse(string value, out string result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = null;
                return false;
            }

            result = value;
            return true;
        }

        private static bool TryParse(string value, out WslOperation result)
        {
            return Enum.TryParse(value, true, out result);
        } 

        #endregion
    }
}