# UnitOfWork для MongoDb

`Unit Of Work` очень полезный паттерн, особенно если говорить в контексте Объектно-Реляционной логики (PoEAA). В приложениях часто используется шаблон `Repository` для инкапсуляции логики работы с БД. Часто приходится оперировать набором сущностей и моделей, для управления которыми создается также большое количество репозиториев. Паттерн `Unit of Work` помогает упростить работу с различными репозиториями и дает уверенность, что все репозитории будут использовать один и тот же `DbContext`. Но это всё про `EntityFramework` и значит про реляционные базы данных. А что же MongoDb?...

## По-другому

Представленный вашему вниманию проект - всего лишь попытка упростить жизнь тем, кто использует MongoDb в повседневной работе, ну, или просто использует достаточно часто. `Unit Of Work прекрасно работает с реляциями, но MongoDb - документо-ориентированная база данных, а это значит, что она работает по-другому. Возникла идея о том, как можно этот паттерн "натянуть" на подобную базу данных.

## MongoDb.Driver

Обязательно надо сказать, что сборка использует другой nuget-пакет, который называется `MongoDB.Driver`. Просто его возможности немного расширены.

## Возможности

Надо сказать честно, что не всё до конца получилось так, как задумывалось. И вс это потому, что с учетом специфики работы самой MongoDb. Но есть полезные штуки, которые могут быть действительные полезны. Попробую перечислить то, что уже реализовано:

* Настройка подключения через appSettings.json
  * ConnectionString
  * набор параметров MongoClientSettings
* MongoDbVerboseLogging
* Тестирование подключение Transactions (ReplicaSet)
* Полный доступ к `Collection` (любые CRUD операции из MongoDB.Driver)
* Получение `IClientSessionHandle` по требованию
* Постраничная разбивка на страницы
* Получение в Repository сущности другие репозитории для других сущностей

## Три простых шага

Для начала скажу, что `IUnitOfWork` создавался в первую очередь для того, чтобы его можно было легко получить через вливание зависимостей, то есть для использования Dependency Injection. Всё что нужно сделать - это добавить секцию настроек в appSettings.json, зарегистрировать в контейнере (`ServiceCollection`) и начать использовать.

### Шаг 1: appsettings.json

Пример настроек для подключения к localhost, без использования ssl и т.д. и т.п.

``` JSON
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

В данном примере указан ConnectionString, которая переопределит все настройки указанные ниже. Просто если есть ConnectionString - остальные настройки игнорируются. В остальных случаях всё как обычно, и, надо сказать, это далеко неполный набор параметров, который используется для создания подключения. По мере необходимости набор может быть расширен.

Использование `appSettings.json` не обязательно. Подключить можно и с фиксированными настройками (hardcoded), задав конфигурацию прямо в коде.

### Шаг 2: регистрация в DI-контейнере

Подключить можно двумя способами. Первый - это прочитать настройки из секции `appSettings.json`:

``` csharp
// read configuration section DatabaseSettings
services.AddUnitOfWork(configuration.GetSection(nameof(DatabaseSettings)));
```
Второй способ не требует наличия `appSettings.json`, просто задайте параметры в коде:

``` csharp
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

Теперь в методе можно использовать `_unitOfWork`, например, для получения коллекции объектов с разбиением на страницы (paged data):

``` csharp
public async Task<IActionResult> OnGetAsync(int pageIndex = 0, int pageSize = 10)
{
    var repository = _unitOfWork.GetRepository<Order, int>();

    Data = await repository.GetPagedAsync(
        pageIndex,
        pageSize,
        FilterDefinition<Order>.Empty,
        HttpContext.RequestAborted);

    return Page();
}
```

В данном примере я использую объекты `Order`:

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

и `OrderItem`:

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

### Транзакции в MongoDb

Для примера хочу показать как использовать транзакции в MongoDb при помощи моей библиотеки. Для начала нужно определить, при каких условиях возможен вызов методов CRUD в MongoDb через транзакции.

База данных MongoDb должна быть настроена соответствующим образом. То есть поддерживать транзакции. Как настроить транзакции в MongoDb можно посмотреть в статье [Configuring ReplicaSet](https://github.com/UpSync-Dev/docker-compose-mongo-replica-set). Если MongoDb запущен в режиме *Statenalone* - транзакции работать не будут. Для проверки работоспособности транзакций можно воспользоваться методом [EnsureReplicationSetReady](https://github.com/Calabonga/Calabonga.UnitOfWork.MongoDb/blob/e5b9435ac99578351cad7a104da46b297548d46b/src/Calabonga.UnitOfWork.MongoDb/IUnitOfWork.cs#L10).
Вызов методов, которые долны быть выполнены в транзакции следует через специальные методы [UseTransactionAsync](https://github.com/Calabonga/Calabonga.UnitOfWork.MongoDb/blob/e5b9435ac99578351cad7a104da46b297548d46b/src/Calabonga.UnitOfWork.MongoDb/IUnitOfWork.cs#L38) или другие его перегрузки.

Пример, как вызывать методы с транзакциями:
``` csharp
await unitOfWork.UseTransactionAsync<OrderBase, int>(ProcessDataInTransactionAsync1, HttpContext.RequestAborted, session);

await unitOfWork.UseTransactionAsync(ProcessDataInTransactionAsync2, repository, HttpContext.RequestAborted, session);

await unitOfWork.UseTransactionAsync(ProcessDataInTransactionAsync3, repository, new TransactionContext(new TransactionOptions(), session, HttpContext.RequestAborted));

await unitOfWork.UseTransactionAsync<OrderBase, int>(ProcessDataInTransactionAsync4, new TransactionContext(new TransactionOptions(), session, HttpContext.RequestAborted));

await unitOfWork.UseTransactionAsync<OrderBase, int>(ProcessDataInTransactionAsync5, TransactionContext.Default);
```
А вот сами методы, которые использованы в предыдущем примере кода:

``` csharp
async Task ProcessDataInTransactionAsync1(IRepository<OrderBase, int> repositoryInTransaction, IClientSessionHandle session, CancellationToken cancellationToken)
{
    await repository.Collection.DeleteManyAsync(session, FilterDefinition<OrderBase>.Empty, null, cancellationToken);

    var internalOrder1 = DocumentHelper.GetInternal(99);
    await repositoryInTransaction.Collection.InsertOneAsync(session, internalOrder1, null, cancellationToken);
    logger!.LogInformation("InsertOne: {item1}", internalOrder1);

    var internalOrder2 = DocumentHelper.GetInternal(100);
    await repositoryInTransaction.Collection.InsertOneAsync(session, internalOrder2, null, cancellationToken);
    logger!.LogInformation("InsertOne: {item2}", internalOrder2);

    var filter = Builders<OrderBase>.Filter.Eq(x => x.Id, 99);
    var updateDefinition = Builders<OrderBase>.Update.Set(x => x.Description, "Updated description");
    var result = await repositoryInTransaction.Collection
        .UpdateOneAsync(session, filter, updateDefinition, new UpdateOptions { IsUpsert = false }, cancellationToken);

    if (result.IsModifiedCountAvailable)
    {
        logger!.LogInformation("Update {}", result.ModifiedCount);
    }

    throw new ApplicationException("EXCEPTION! BANG!");
}

async Task ProcessDataInTransactionAsync2(IRepository<OrderBase, int> repositoryInTransaction, IClientSessionHandle session, CancellationToken cancellationToken)
{
    await repositoryInTransaction.Collection.DeleteManyAsync(session, FilterDefinition<OrderBase>.Empty, null, cancellationToken);

    var internalOrder1 = DocumentHelper.GetInternal(99);
    await repositoryInTransaction.Collection.InsertOneAsync(session, internalOrder1, null, cancellationToken);
    logger!.LogInformation("InsertOne: {item1}", internalOrder1);

    var internalOrder2 = DocumentHelper.GetInternal(100);
    await repositoryInTransaction.Collection.InsertOneAsync(session, internalOrder2, null, cancellationToken);
    logger!.LogInformation("InsertOne: {item2}", internalOrder2);

    var filter = Builders<OrderBase>.Filter.Eq(x => x.Id, 99);
    var updateDefinition = Builders<OrderBase>.Update.Set(x => x.Description, "Updated description");
    var result = await repositoryInTransaction.Collection.UpdateOneAsync(session, filter, updateDefinition, new UpdateOptions { IsUpsert = false }, cancellationToken);

    if (result.IsModifiedCountAvailable)
    {
        logger!.LogInformation("Update {}", result.ModifiedCount);
    }

    throw new ApplicationException("EXCEPTION! BANG!");
}

async Task ProcessDataInTransactionAsync3(IRepository<OrderBase, int> repositoryInTransaction, TransactionContext transactionContext)
{
    await repositoryInTransaction.Collection.DeleteManyAsync(transactionContext.Session, FilterDefinition<OrderBase>.Empty, null, transactionContext.CancellationToken);

    var internalOrder1 = DocumentHelper.GetInternal(99);
    await repositoryInTransaction.Collection.InsertOneAsync(transactionContext.Session, internalOrder1, null, transactionContext.CancellationToken);
    transactionContext.Logger.LogInformation("InsertOne: {item1}", internalOrder1);

    var internalOrder2 = DocumentHelper.GetInternal(100);
    await repositoryInTransaction.Collection.InsertOneAsync(transactionContext.Session, internalOrder2, null, transactionContext.CancellationToken);
    transactionContext.Logger.LogInformation("InsertOne: {item2}", internalOrder2);

    var filter = Builders<OrderBase>.Filter.Eq(x => x.Id, 99);
    var updateDefinition = Builders<OrderBase>.Update.Set(x => x.Description, "Updated description");
    var result = await repositoryInTransaction.Collection.UpdateOneAsync(transactionContext.Session, filter, updateDefinition, new UpdateOptions { IsUpsert = false }, transactionContext.CancellationToken);

    if (result.IsModifiedCountAvailable)
    {
        transactionContext.Logger.LogInformation("Update {}", result.ModifiedCount);
    }

    throw new ApplicationException("EXCEPTION! BANG!");
}

async Task ProcessDataInTransactionAsync4(IRepository<OrderBase, int> repositoryInTransaction, TransactionContext transactionContext)
{
    await repositoryInTransaction.Collection.DeleteManyAsync(transactionContext.Session, FilterDefinition<OrderBase>.Empty, null, transactionContext.CancellationToken);

    var internalOrder1 = DocumentHelper.GetInternal(99);
    await repositoryInTransaction.Collection.InsertOneAsync(transactionContext.Session, internalOrder1, null, transactionContext.CancellationToken);
    transactionContext.Logger.LogInformation("InsertOne: {item1}", internalOrder1);

    var internalOrder2 = DocumentHelper.GetInternal(100);
    await repositoryInTransaction.Collection.InsertOneAsync(transactionContext.Session, internalOrder2, null, transactionContext.CancellationToken);
    transactionContext.Logger.LogInformation("InsertOne: {item2}", internalOrder2);

    var filter = Builders<OrderBase>.Filter.Eq(x => x.Id, 99);
    var updateDefinition = Builders<OrderBase>.Update.Set(x => x.Description, "Updated description");
    var updateResult = await repositoryInTransaction.Collection.UpdateOneAsync(transactionContext.Session, filter, updateDefinition, new UpdateOptions { IsUpsert = false }, transactionContext.CancellationToken);

    if (updateResult.IsModifiedCountAvailable)
    {
        transactionContext.Logger.LogInformation("Update {}", updateResult.ModifiedCount);
    }

    throw new ApplicationException("EXCEPTION! BANG!");
}

async Task ProcessDataInTransactionAsync5(IRepository<OrderBase, int> repositoryInTransaction, TransactionContext transactionContext)
{
    await repositoryInTransaction.Collection.DeleteManyAsync(transactionContext.Session, FilterDefinition<OrderBase>.Empty, null, transactionContext.CancellationToken);

    var internalOrder1 = DocumentHelper.GetInternal(99);
    await repositoryInTransaction.Collection.InsertOneAsync(transactionContext.Session, internalOrder1, null, transactionContext.CancellationToken);
    transactionContext.Logger.LogInformation("InsertOne: {item1}", internalOrder1);

    var internalOrder2 = DocumentHelper.GetInternal(100);
    await repositoryInTransaction.Collection.InsertOneAsync(transactionContext.Session, internalOrder2, null, transactionContext.CancellationToken);
    transactionContext.Logger.LogInformation("InsertOne: {item2}", internalOrder2);

    var filter = Builders<OrderBase>.Filter.Eq(x => x.Id, 99);
    var updateDefinition = Builders<OrderBase>.Update.Set(x => x.Description, "Updated description");
    var updateResult = await repositoryInTransaction.Collection.UpdateOneAsync(transactionContext.Session, filter, updateDefinition, new UpdateOptions { IsUpsert = false }, transactionContext.CancellationToken);

    if (updateResult.IsModifiedCountAvailable)
    {
        transactionContext.Logger.LogInformation("Update {}", updateResult.ModifiedCount);
    }

    throw new ApplicationException("EXCEPTION! BANG!");
}
```

Настоятельно рекомендую ознакомиться с тем, как используются методы вызова CRUD операций внутри каждого из методов. Обратите внимание на то, что обязательно использование *session* для вызова операций на операции *Insert/Update/Delete*. В противном случае, при ошибке или исключении отмены операции не произойдет.

## Профилировщик (Profiler)


Чтобы включить профилировки (profiler) перед запросом или лучше в начале метода где возможно выполнить множество запросов выполните команду:

``` csharp
// Enable MongoDb profiler
using var profiler = new MongoDbProfiler(repository, logger);
```

`MongoDbProfiler` отключит профилировщик при выходе из области действия метода, потому что реализует `IDisposable`.

Для отладки запросов, проверки нагрузки и определения наличия индексов MongoDb можно в режиме отладки включить профилировщик ваших запросов. Профилировщик даст возможность логировать все запросы в MongoDb. Вы можете посмотреть их в своей БД в коллекции `system.profile`.

``` JavaScript

// можно без фильтров, тогда увидите всё, что для вас собрал профилировщик
db.system.profile().find()

```

Отладочную информацию можно также записать в `ILogger`. Для этого надо в опциях запроса в комментарии указать уникальный идентификатор запроса:

``` csharp
var options1 = new InsertManyOptions { Comment = "07be0e36-f1c3-f6a7-4e52-5333eb32e00e" };
await repository.Collection.InsertManyAsync(session, both, options1, cancellationToken);

profiler.LogRequest("07be0e36-f1c3-f6a7-4e52-5333eb32e00e");
```
А после этого запроса выполнить команду `LogRequest`.

>Внимание!!!
>Профилировщик потребляет значительные ресурсы! Не используйте профилирование, если нет потребности в анализе данных и производительности.
>Не забудьте отключить профилировщик для использования на PRODUCTION!!!
>

## Комментарии, пожелания, замечания

Пишите комментарии к видео на сайте [www.calabonga.net](https://www.calabonga.net/blog/post/unit-of-work-for-mongodb)

# Ссылки

* [Nuget пакет](https://www.nuget.org/packages/Calabonga.UnitOfWork.MongoDb/)
* [Статья в блоге](https://www.calabonga.net/blog/post/unit-of-work-for-mongodb)

# YouTube

<a href="http://www.youtube.com/watch?feature=player_embedded&v=xqqR7YVZJww
" target="_blank"><img src="http://img.youtube.com/vi/xqqR7YVZJww/0.jpg"
alt="IMAGE ALT TEXT HERE" width="240" height="180" border="10" /></a>

<a href="http://www.youtube.com/watch?feature=player_embedded&v=otVeeM3pS74
" target="_blank"><img src="http://img.youtube.com/vi/otVeeM3pS74/0.jpg"
alt="IMAGE ALT TEXT HERE" width="240" height="180" border="10" /></a>

# Автор

Сергей Калабонга (Calabonga)

![Author](./whatnot/Calabonga.gif)

[Блог по программированию](https://www.calabonga.net)
