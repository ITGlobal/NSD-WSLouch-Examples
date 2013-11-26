using System;
using System.Text;
using Validata.PKI;

namespace NSD.WSLouch
{
    /// <summary>
    ///     Обертка над VCERT API для ГОСТ
    /// </summary>
    public sealed class GostVcertAPI : IVcertAPI
    {
        #region Статические поля

        private static readonly SignParameters _SignParameters = new SignParameters
        {
            SendCertificate = false,
            Detached = true,
            Pkcs7 = false,
            SendChain = false
        };

        private static readonly Lazy<VcertObject> _Vcert = new Lazy<VcertObject>(
            () =>
            {
                var vcert = new VcertObject();

                if (!vcert.Initialize("My", VcertObject.InitializeFlags.UseRegistry))
                {
                    throw new ApplicationException("Не удалось инициализировать VCERT API");
                }

                return vcert;
            });

        #endregion

        #region Публичные методы

        /// <summary>
        ///     Вычислить хеш блока текста
        /// </summary>
        /// <param name="input">
        ///     Блок текста
        /// </param>
        /// <returns>
        ///     Хеш
        /// </returns>
        public string Hash(string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            var inputBytes = Encoding.UTF8.GetBytes(input);
            var outputBytes = _Vcert.Value.HashMemory(inputBytes);
            var output = Convert.ToBase64String(outputBytes);
            return output;
        }

        /// <summary>
        ///     Зашифровать блок текста
        /// </summary>
        /// <param name="input">
        ///     Блок текста
        /// </param>
        /// <returns>
        ///     Зашифрованный блок
        /// </returns>
        public string Sign(string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            var inputBytes = Encoding.UTF8.GetBytes(input);
            var outputBytes = _Vcert.Value.SignMemory(_SignParameters, inputBytes, null);
            var output = Convert.ToBase64String(outputBytes);
            return output;
        }

        /// <summary>
        ///     Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        public void Dispose()
        {
            // NOTE VCERT API для .NET не поддерживает деинициализацию
        }

        #endregion
    }
}
