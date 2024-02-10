﻿using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using WebUIVanilla.Client;
using WebUIVanilla.Client.Extensions;
using WebUIVanilla.Client.Services;
using WebUIVanilla.Client.Validator;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddSingleton<IDataService, DataService>();
builder.Services.AddSingleton<INameService, NameService>();
builder.Services.AddSingleton<INameValidator, NameValidator>();
builder.Services.AddSingleton<ISelectorService, SelectorService>();

builder.Services.AddLocalization();
builder.Services.AddTransient<MudLocalizer, ResXMudLocalizer>();

var host = builder.Build();

var service = host.Services.GetRequiredService<IDataService>();
await service.InitializeAsync();

await host.SetDefaultCulture();

await host.RunAsync();