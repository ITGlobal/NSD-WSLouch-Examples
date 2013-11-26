using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace NSD.WSLouch
{
    /// <summary>
    ///     Базовый класс примера работы со службой WSL
    /// </summary>
    public abstract class ExampleBase
    {
        #region Публичные методы

        /// <summary>
        ///     Запустить пример
        /// </summary>
        /// <param name="args">
        ///     Аргументы командной строки
        /// </param>
        public void Execute(string[] args)
        {
            Console.WriteLine("Пример работы со службой WSL");
            Console.WriteLine("----");
            Console.WriteLine();

            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            try
            {
                // Разбор параметров командной строки
                ProgramOptions options;
                if (!ProgramOptions.TryParse(args, out options))
                {
                    throw new ApplicationException("Неверные параметры комадной строки");
                }

                // Инициализация объектов
                Console.WriteLine("Инициализация криптопровайдера");
                var vcert = InitializeCryptoProvider();

                Console.WriteLine("Поиск клиентского сертификата");
                var certificate = FindCertificate(options.SerialNumber);
                
                var adapter = CreateAdapter(options.Endpoint, vcert, certificate);

                // Запуск примера
                Run(adapter, options.PersonCode, options.Operation, options.Arguments);

            }
            catch (CommandLineException e)
            {
                PrintError(e.Message);
                PrintUsage();
            }
            catch (Exception e)
            {
                PrintError(e.Message);
            }
        }

        #endregion

        #region Виртуальные методы

        /// <summary>
        ///     Инициализировать криптопровайдер
        /// </summary>
        protected abstract IVcertAPI InitializeCryptoProvider();

        /// <summary>
        ///     Определить адрес конечной точки службы
        /// </summary>
        /// <param name="endpoint">
        ///     Контур службы (PL или PROD)
        /// </param>
        protected abstract Uri ResolveWslUri(WslEndpoint endpoint);

        #endregion

        #region Приватные методы

        /// <summary>
        ///     Вывести в консоль описание параметров командной строки
        /// </summary>
        private static void PrintUsage()
        {
            var executableName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            Console.WriteLine("Неверные параметры командной строки");
            Console.WriteLine("Допустимые параметры:");
            Console.WriteLine("{0} ENDPOINT PERSONCODE SERIALNUMBER GetPackageList DATE", executableName);
            Console.WriteLine("{0} ENDPOINT PERSONCODE SERIALNUMBER GetPackage ID [FILENAME]", executableName);
            Console.WriteLine("{0} ENDPOINT PERSONCODE SERIALNUMBER PutPackage PACKAGENAME FILENAME", executableName);
            Console.WriteLine("Здесь\tENDPOINT - контур службы (PL, PROD);");
            Console.WriteLine("\tPERSONCODE - депозитарный код участника;");
            Console.WriteLine("\tSERIALNUMBER - серийный номер сертификата участника;");
            Console.WriteLine("\tDATE - дата в формате YYYYMMDD;");
            Console.WriteLine("\tID - номер пакета;");
            Console.WriteLine("\tPACKAGENAME - название пакета;");
            Console.WriteLine("\tFILENAME - путь к файлу пакета.");
        }

        /// <summary>
        ///     Вывести в консоль сообщение об ошибке
        /// </summary>
        private static void PrintError(string message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }

        /// <summary>
        ///     Поиск клиентского сертификата в хранилище сертификатов
        /// </summary>
        /// <param name="serialNumber">
        ///     Серийный номер сертификата
        /// </param>
        /// <returns>
        ///     Сертификат, если он найден
        /// </returns>
        /// <exception cref="ApplicationException">
        ///     Указанный сертификат не найден
        /// </exception>
        private static X509Certificate2 FindCertificate(string serialNumber)
        {
            // Приводим серийный номер сертификата к стандартному формату
            serialNumber = serialNumber.Replace(":", "").Replace(" ", "").ToUpperInvariant();

            var store = new X509Store("My");
            try
            {
                store.Open(OpenFlags.ReadOnly);

                var certificates = store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                var certificate = certificates
                    .Cast<X509Certificate2>()
                    .FirstOrDefault(cert => cert.SerialNumber == serialNumber);
                if (certificate == null)
                {
                    throw new Exception(string.Format("Не удалось найти сертификат \"SN={0}\"", serialNumber));
                }

                return certificate;
            }
            finally
            {
                store.Close();
            }
        }

        /// <summary>
        ///     Инициализировать адаптер службы WSL
        /// </summary>
        /// <param name="endpoint">
        ///     Контур службы (PL или PROD)
        /// </param>
        /// <param name="vcert">
        ///     Криптопровайдер
        /// </param>
        /// <param name="certificate">
        ///     Клиентский сертификат
        /// </param>
        /// <returns>
        ///     Адаптер службы WSL
        /// </returns>
        private WslAdapter CreateAdapter(WslEndpoint endpoint, IVcertAPI vcert, X509Certificate2 certificate)
        {
            Console.WriteLine("Инициализация адаптера службы");
            var uri = ResolveWslUri(endpoint);
            var adapter = new WslAdapter(uri, vcert, certificate);
            return adapter;
        }

        /// <summary>
        ///     Запустить пример
        /// </summary>
        /// <param name="adapter">
        ///     Адаптер службы WSL
        /// </param>
        /// <param name="personCode">
        ///      Депозитарный код
        /// </param>
        /// <param name="operation">
        ///      Код операции
        /// </param>
        /// <param name="args">
        ///      Аргументы операции
        /// </param>
        private static void Run(WslAdapter adapter, string personCode, WslOperation operation, string[] args)
        {
            switch (operation)
            {
                case WslOperation.PutPackage:
                    RunPutPackage(adapter, personCode, args);
                    break;
                case WslOperation.GetPackageList:
                    RunGetPackageList(adapter, personCode, args);
                    break;
                case WslOperation.GetPackage:
                    RunGetPackage(adapter, personCode, args);
                    break;
                default:
                    throw new CommandLineException(string.Format("Неизвестная операция - \"{0}\"", operation));
            }
        }

        /// <summary>
        ///     Запустить пример работы с методов PutPackage()
        /// </summary>
        /// <param name="adapter">
        ///     Адаптер службы WSL
        /// </param>
        /// <param name="personCode">
        ///      Депозитарный код
        /// </param>
        /// <param name="args">
        ///      Аргументы операции
        /// </param>
        private static void RunPutPackage(WslAdapter adapter, string personCode, string[] args)
        {
            // Ожидаемые агрументы операции:
            // 1. Имя пакета
            // 2. Путь к файлу пакета
            // Ожидается, что имя пакета сформировано в соответствии с правилами ЭДО НРД.

            if (args.Length < 2)
            {
                throw new CommandLineException("Недостаточно параметров для операции PutPackage");
            }

            // Имя пакета
            var packageName = args[0];
            // Путь к файлу пакета
            var packageFileName = args[1];

            if (!File.Exists(packageFileName))
            {
                throw new Exception(string.Format("Файл не найден - \"{0}\"", packageFileName));
            }

            var packageContent = File.ReadAllBytes(packageFileName);

            // Шаг 1. Инициализация процесса передачи пакета. Пакету присваивается уникальный номер.
            Console.WriteLine("Инициализация передачи пакета \"{0}\"", packageName);
            var packageId = adapter.InitTransferIn(personCode, packageName);
            Console.WriteLine("Получен код пакета: \"{0}\"", packageId);

            // Шаг 2. Передача содержимого пакета.
            // Таких операций может быть несколько, если пакет состоит из нескольких блоков.
            adapter.PutPackage(personCode, packageId, 1, 1, packageContent);
            Console.WriteLine("Передано: {0} байт", packageContent.Length);

            // Шаг 3. Завершение передачи пакета.
            var result = adapter.GetTransferResult(personCode, packageId);
            Console.WriteLine("Результат передачи: \"{0}\"", result);
        }

        /// <summary>
        ///     Запустить пример работы с методов GetPackageList()
        /// </summary>
        /// <param name="adapter">
        ///     Адаптер службы WSL
        /// </param>
        /// <param name="personCode">
        ///      Депозитарный код
        /// </param>
        /// <param name="args">
        ///      Аргументы операции
        /// </param>
        private static void RunGetPackageList(WslAdapter adapter, string personCode, string[] args)
        {
            // Ожидаемые агрументы операции:
            // 1. Дата для выгрузки списка входящих пакетов (в формате YYYYMMDD)

            if (args.Length < 1)
            {
                throw new CommandLineException("Недостаточно параметров для операции GetPackageList");
            }

            // Дата для выгрузки списка входящих пакетов
            DateTime date;
            if (!DateTime.TryParseExact(args[0],
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date))
            {
                throw new CommandLineException(string.Format("Неверный формат даты: \"{0}\". Ожидалась дата в формате YYYYMMDD", args[0]));
            }

            // Загрузка XML со списком пакетов
            Console.WriteLine("Запрос списка пакетов...");
            var xml = adapter.GetPackageList(personCode, date);

            var xmlDocument = XDocument.Parse(xml);

            // XML имеет следующую структуру:
            // <?xml version="1.0" encoding="windows-1251"?>
            // <package_list>
            //  <package>
            //      <id>НОМЕР ПАКЕТА</id>
            //      <name>ИМЯ ФАЙЛА ПАКЕТА</name>
            //      <size>РАЗМЕР ПАКЕТА В БАЙТАХ</size>
            //      <hash>ХЕШ ПАКЕТА</hash>
            //  </package>
            //
            //  <package> ... </package>
            //  ...
            // </package_list>

            // ReSharper disable PossibleNullReferenceException
            var packages =
                (

                    from n in xmlDocument.Element("package_list").Elements("package")
                    select new
                    {
                        Id = n.Element("id").Value,
                        Name = n.Element("name").Value,
                        Size = n.Element("size").Value,
                        Hash = n.Element("hash").Value
                    }
                    ).ToArray();
            // ReSharper restore PossibleNullReferenceException

            Console.WriteLine();
            Console.WriteLine("Входящие пакеты за {0:d MMMM yyyy}", date);
            const string format = " {0,-16}| {1,-16}| {2,-16}| {3,-16}";
            Console.WriteLine(
                    format,
                    "Номер",
                    "Название",
                    "Размер",
                    "Хеш");
            foreach (var package in packages)
            {
                Console.WriteLine(
                    format,
                    package.Id,
                    package.Name,
                    package.Size,
                    package.Hash);
            }

            Console.WriteLine("----");
            Console.WriteLine("Всего {0} пакетов", packages.Length);
        }

        /// <summary>
        ///     Запустить пример работы с методов GetPackage()
        /// </summary>
        /// <param name="adapter">
        ///     Адаптер службы WSL
        /// </param>
        /// <param name="personCode">
        ///      Депозитарный код
        /// </param>
        /// <param name="args">
        ///      Аргументы операции
        /// </param>
        private static void RunGetPackage(WslAdapter adapter, string personCode, string[] args)
        {
            // Ожидаемые агрументы операции:
            // 1. Номер пакета
            // 2. Путь к файлу пакета (опционально)
            // Если путь к файлу пакета не указан, то пакет сохраняется в текущую директорию в файл с именем вида
            // "НОМЕР_ПАКЕТА.package"

            if (args.Length < 1)
            {
                throw new CommandLineException("Недостаточно параметров для операции GetPackage");
            }

            // Номер пакета
            int id;
            if (!int.TryParse(args[0], out id))
            {
                throw new CommandLineException(string.Format("Неверный номер пакета - \"{0}\"", args[0]));
            }

            // Путь к файлу пакета
            var packageFileName = args.Length > 1
                ? args[1] 
                : Path.Combine(Directory.GetCurrentDirectory(), string.Format("{0}.package", id));

            // Загрузка содержимого пакета
            Console.WriteLine("Загрузка пакета \"{0}\"", id);
            var content = adapter.GetPackage(personCode, id, 1, 1);
            File.WriteAllBytes(packageFileName, content);

            Console.WriteLine("Записано {0} байт в файл \"{1}\"", content.Length, packageFileName);
        }

        #endregion
    }
}
