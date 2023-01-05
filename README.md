# UnitOfWork для MongoDb

Unit Of Work очень полезный паттерн, особенно если говорить в контексте Объектно-Реляционной логики (PoEAA). В приложениях часто используется шаблон Repository для инкапсуляции логики работы с БД. Часто приходится оперировать набором сущностей и моделей, для управления которыми создается также большое количество репозиториев. Паттерн Unit of Work помогает упростить работу с различными репозиториями и дает уверенность, что все репозитории будут использовать один и тот же DbContext. Но это всё про EntityFramework и значит про реляционные базы данных. А что же MongoDb?...

## По-другому

Представленный вашему вниманию проект - всего лишь попытка упростить жизнь тем, кто использует MongoDb в повседневной работе, ну, или просто использует достаточно часто. Unit Of Work прекрасно работает с реляциями, но MongoDb - документо-ориентированная база данных, а это значит, что она работает по-другому. Возникла идея о том, как можно этот паттерн "натянуть" на подобную базу данных.

## MongoDb.Driver

Обязательно надо сказать, что сборка использует другой nuget-пакет, который называется MongoDB.Driver. Просто его возможности немного расширены.

## Возможности

Надо сказать честно, что не всё до конца получилось так, как задумывалось. И вс это потому, что с учем специфики работы самой MongoDb. Но есть полезные штуки, которые могут быть действительные полезны. Попробую перечислить то, что уже реализовано:

* Настройка подключения через appSettings.json
  * ConnectionString
  * набор параметров MongoClientSettings
* MongoDbVerboseLogging
* Тестирование подключение Transactions (ReplicaSet)
* Полный доступ к Collection (любые CRUD операции из MongoDB.Driver)
* Получение IClientSessionHandle по требованию
* Постраничная разбивка на страницы
* Получение в Repository сущности другие репозитории для других сущностей
 
## Три простых шага

Для начала скажу, что IUnitOfWork создавался в первую очередь для того, чтобы его можно было легко получить через вливание зависимостей, то есть для использования Dependency Injection. Всё что нужно сделать - это добавить секцию настроек в appSettings.json, зарегистрировать в контейнере (ServiceCollection) и начать использовать.

### Шаг 1: appsettings.json
Пример настроек для подключения к localhost, без использования ssl и т.д. и т.п.
``` json
{
    "DatabaseSettings": {
        "ConnectionString": "mongodb://localhost:27017/?readPreference=primary&ssl=false&directConnection=true",
        "Credential": {
            "Login": "sa",
            "Password": "P@55w0rd"
        },
        "ApplicationName": "CalabongaDemo",
        "ReplicaSetName": "rs0",
        "DatabaseName": "MyDatabase",
        "Hosts": [ "localhost" ],
        "MongoDbPort": 27017,
        "VerboseLogging": false,
        "DirectConnection": true
    }
}
```

В данном пример указано ConnectionString, которая переопределит все настройки указанные ниже. Просто есть есть ConnectionString - остальные настройки игнорируются. В остальных случаях всё как обычно, и, надо сказать, это далеко неполный набор параметров, который используется для создания подключения. По мере необходимости набор может быть расширен.

Использование appSettings.json не обязательно. Подключить можно и с фиксированными настройками (hardcoded), задав конфигурацию прямо в коде.

### Шаг 2: регистрация в DI-контейнере

Подключить можно двумя способами. Первый - это прочитав настройки из секции appSettings.json:
``` csharp
// read configuration section DatabaseSettings
services.AddUnitOfWork(configuration.GetSection(nameof(DatabaseSettings)));
Bторой способ не требует наличия appSettings.json, просто задайте параметры в коде:

services.AddUnitOfWork(config =>
{
    config.Credential = new CredentialSettings { Login = "sa", Password = "password" };
    config.DatabaseName = "MyDatabase";
    config.Hosts = new[] { "Localhost" };
    config.MongoDbPort = 27017;
    config.VerboseLogging = false;
});
```

### Шаг 3: использование

Можно внедрить зависимость в PageModel, например, так:

``` csharp
namespace WebApplicationWithMongo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IUnitOfWork unitOfWork, ILogger<IndexModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IPagedList<Order>? Data { get; set; }
    }
}
```

Теперь в методе можно использовать `_unitOfWork`, например, для получения коллекции объектов с разбиение на страницы (paged data):

``` csharp
public async Task<IActionResult> OnGetAsync(int pageIndex = 0, int pageSize = 10)
{
    var repository = _unitOfWork.GetRepository<Order, int>();
    Data = await repository.GetPagedAsync(pageIndex, pageSize, FilterDefinition<Order>.Empty, HttpContext.RequestAborted);
    return Page();
}
```

В данном примере я использую объекты Order:

``` csharp
[BsonIgnoreExtraElements]
public class Order : DocumentBase<int>
{
    [BsonElement("number")]
    [BsonRepresentation(BsonType.Int32)]
    public int Number { get; set; }

    [BsonElement("title")]
    [BsonRepresentation(BsonType.String)]
    public string Title { get; set; } = default!;

    [BsonElement("description")]
    [BsonRepresentation(BsonType.String)]
    public string? Description { get; set; }

    [BsonElement("items")]
    public ICollection<OrderItem>? Items { get; set; }
}
```

и OrderItem:

``` csharp
public class OrderItem : DocumentBase<int>
{
    [BsonElement("name")]
    public string Name { get; set; } = default!;

    [BsonElement("quantity")]
    [BsonRepresentation(BsonType.Int32)]
    public int Quantity { get; set; }

    [BsonElement("price")]
    [BsonRepresentation(BsonType.Double)]
    public double Price { get; set; }
}

```

## Комментарии, пожелания, замечания

Пишите комментарии к видео на сайте [www.calabonga.net](https://www.calabonga.net)

# Ссылки

* [Nuget пакет](https://www.nuget.org/packages/Calabonga.UnitOfWork.MongoDb/)
* [Статья в блоге](https://www.calabonga.net/blog/post/unit-of-work-for-mongodb)

# Автор

Сергей Калабонга (Calabonga)

![Author](https://www.calabonga.net/images/Calabonga.gif)

[Блог по программированию](https://www.calabonga.net)
