# Примеры работы с веб-службой НРД на C# #

Примеры разбиты на 3 сборки:
* NSD.WSLouchShared - содержит общий для всех примеров код
* NSD.WSLouchRSA - содержит пример работы с веб-службой с использованием криптографии RSA
* NSD.WSLouchGOST - содержит пример работы с веб-службой с использованием криптографии ГОСТ

## Предварительная настройка окружения и внешние зависимости ##

### Установка ПО ###
Для доступа к веб-службе НРД требуется установить и настроить Справочник Сертификатов. Разрядность Справочника Сертификатов определяется тем, в какой конфигурации планируется запускать примеры. Для запуска в конфигурации x86 требуется 32-разрядная версия Справочника Сертификатов.
Версия Справочника Сертификатов определяется желаемой криптографией:
* Для работы с криптографией ГОСТ требуется установить ["АПК Клиент ММВБ: Справочник сертификатов"](http://ca.micex.ru/sed/userfiles/file/po/xcs_v40-268-01_32bit.zip)
* Для работы с криптографией RSA требуется установить ["ПКЗИ СЭД МБ: Справочник сертификатов"](http://ca.micex.ru/sed/userfiles/file/po/rcs_v40-267-12_32bit.zip)

Для работы с криптографией ГОСТ (и соответственно для подключения к веб-службе через TLS-туннель с шифрованием ГОСТ) требуется также установить "Криптопровайдер Валидата CSP" соответствующей разрядности ([x86](http://ca.micex.ru/sed/userfiles/file/po/CSP_v40-267-11_32bit_psw.zip), [x64](http://ca.micex.ru/sed/userfiles/file/po/CSP_v40-267-11_64bit_psw.zip)).

Устанавливать SDK для Справочника Сертификатов не требуется, т.к. соответствующие библиотеки уже включены в состав примеров и не требуют внешних компонентов.

### Настройка ПО ###
Необходимо установить [сертификаты ключей](https://www.nsd.ru/ru/workflow/system/crypto/) для доступа к TLS-туннелю и веб-службе.

## Использование примеров ##
Данные примеры используют шифрование RSA. Для того, чтобы выполнить запросы с шифрованием ГОСТ, замените **wslr** на **wslg**. Параметры командной строки остаются прежними.
Примеры предполагают, что используется тестовый контур (**PL**), запросы выполняются от участника с депозитарным кодом **TESTORGNZ001** и серийным номером клиентского сертификата **40:40:00:11:22:33:44:55:66:77:88:99:AA:BB:CC:DD**

### GetPackageList ###
Для получения списка входящих пакетов за 22-е ноября 2013 года выполните команду:
```
wslr PL TESTORGNZ001 40:40:00:11:22:33:44:55:66:77:88:99:AA:BB:CC:DD GetPackageList 20131122
```
В ответ в консоль будет выведен список входящих пакетов за указанную дату.

### GetPackage ###
Для получения входящего пакета с номером **12346789** в файл **PACKAGE_1.DAT** выполните команду:
```
wslr PL TESTORGNZ001 40:40:00:11:22:33:44:55:66:77:88:99:AA:BB:CC:DD GetPackage 12346789 PACKAGE_1.DAT
```
Указанный входящий пакет будет скачан и записан в указанный файл.
Если не указать имя файла, оно будет сформировано автоматически.

### PutPackage ###
Для отправки исходящего пакета с номером **F22B1120.CRY** из файла **OUTPACKAGE.DAT** выполните команду:
```
wslr PL TESTORGNZ001 40:40:00:11:22:33:44:55:66:77:88:99:AA:BB:CC:DD PutPackage F22B1120.CRY OUTPACKAGE.DAT
```
Указанный пакет будет отправлен в систему ЭДО НРД. Номер исходящего пакета и его содержимое должны формироваться в соответствии с правилами ЭДО НРД.
