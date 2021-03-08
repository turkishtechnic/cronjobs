# TT.Cronjobs

Primitives and APIs for declaring and triggering cronjobs with webhooks.

## Motivation

At Turkish Technic, we have tens of development teams, each tasked with a number of projects. Many applications we
develop need a periodic nudge to perform tasks in the background.

This package provides us with the primitives necessary to define, manage, and trigger them in a central application.

## Usage

Install `TT.Cronjobs.AspNetCore` package

```powershell
dotnet add package TT.Cronjobs.AspNetCore
```

Then implement `ICronjob` interface, and add `[Cron]` attribute. There's only one method you need to
implement: `ExecuteAsync`. This method gets called when the cronjob gets triggered.

```c#
[Cron("1 * * * *")] // every hour
public class CreateHourlyReport : ICronjob
{
    private readonly ILogger<CreateHourlyReport> _logger;
    private readonly AppDbContext _db;

    public CreateHourlyReport(ILogger<CreateReport> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("creating report...");
        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        var sum = _db.Sales.Sum(s => s.Total);
        _logger.LogInformation($"created report. sum = {sum}");
    }
}
```

Register necessary services in `Startup.cs`.

```c#
public void ConfigureServices(IServiceCollection services) {
    services.AddCronjobs();
    services.AddTransient<CreateHourlyReport>();
    // ...
}
```

`AddCronjobs()` extension method registers a background service that calls `RegistrationApi` with the list of cronjobs
defined in the assembly. This service then sends a POST request to our application at intervals defined in `[Cron]`
attribute. (This service is not included in this project)

Define an endpoint for webhooks:

```c#
public void Configure(IApplicationBuilder app)
{
    // ...
    
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapCronjobWebhook("/-/cronjobs");
        // ...
    });
}
```

For configuration you can supply a delegate to `services.AddCronjobs()`, or define it in `appsettings.json`

```c#
// services.Configure<CronjobsOptions>(Configuration.GetSection(CronjobsOptions.Key));
services.AddCronjobs(options => {});
```

```json
{
  "Cronjobs": {
    "RegistrationApiUrl": "https://cronjobs.app.turkishtechnic.com",
    "UrlTemplate": "https://my.app/-/cronjobs/{name}"
  }
}
```

Webhook URLs are generated using `UrlTemplate` option. Here we mapped the webhook URL as `/-/cronjobs`, and configured
an absolute URL template. This means you can trigger a webhook with
`https://my.app/-/cronjobs/createhourlyreport` (case-insensitive).

Other options you can define:

```c#
public class CronjobsOptions
{
    public const string Key = "Cronjobs";
    public string RegistrationApiUrl { get; set; }
    public string UrlTemplate { get; set; }
    public int RetryCount { get; set; } = 5;
    public int WaitSeconds { get; set; } = 5;
    public int TimeoutSeconds { get; set; } = 60;
}
```