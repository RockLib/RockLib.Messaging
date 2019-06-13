# Using RockLib.Messaging with a DI/IOC container

## ASP.NET Core Web Application

Senders and Receivers can be rigistered to the service container in `Startup.ConfigureServices`.

``` c#
public void ConfigureServices(IServiceCollection services)
{
    // Other services omitted ..

    // An HttpClientSender is registered using its constructor.
    services.AddSingleton<ISender>(new HttpClientSender("Sender1", "http://localhost:5000/example"));    
}
```

They may also be added using the `MessagingScenarioFactory` where the configuration will be pulled from `appsettings.json`.  Set `appsettings.json` 'Copy to Output Directory' setting to 'Copy if newer' and add the following configuration.

``` json
{
  // Other configuration values omitted ..

  "RockLib.Messaging": {
    "senders": {
      "type": "RockLib.Messaging.Http.HttpClientSender, RockLib.Messaging.Http",
      "value": {
        "name": "Sender1",
        "url": "http://localhost:5000/example"
      }
    }
  }
}
```

``` c#
public void ConfigureServices(IServiceCollection services)
{
    // Other services omitted ..

    // An ISender instance is registered using the 'Sender1' configuration
    services.AddSingleton(MessagingScenarioFactory.CreateSender("Sender1"));
}
```

Now that there is an `ISender` instance registered, it can be injected into the constructor of a class.

``` c#
private readonly ISender _sender;

public ValuesController(ISender sender)
{
    _sender = sender;
}
```