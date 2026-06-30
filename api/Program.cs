using System.Net;
using GuguEveryday;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 5000);
});

builder.Host.UseAutofac();

builder.Services.AddApplication<GuguEverydayModule>();

var app = builder.Build();

await app.InitializeApplicationAsync();

app.Run();