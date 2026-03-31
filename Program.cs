using System.Reflection;
using BusinessFlowApp;
using BusinessFlowApp.Core;
using BusinessFlowApp.Core.Strategies;
using BusinessFlowApp.Controllers;
using BusinessFlowApp.Extensions;
using BusinessFlowApp.Flows;
using BusinessFlowApp.Middlewares;
using BusinessFlowApp.Models;
using BusinessFlowApp.Services;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DecisionTableOptions>(
    builder.Configuration.GetSection(DecisionTableOptions.SectionName));

builder.Services.Configure<XlsxGroupResolverOptions>(
    builder.Configuration.GetSection(XlsxGroupResolverOptions.SectionName));

// Автоматическая регистрация всех реализаций IBusinessFlow<T>
RegisterBusinessFlows(builder.Services);

// Регистрация сервисов
builder.Services.AddSingleton<IDecisionTableService, DecisionTableService>();
builder.Services.AddSingleton<IBusinessFlowHandler, BusinessFlowHandler>();

// Регистрация резолверов групп
builder.Services.AddSingleton<DefaultXlsxGroupResolver>();
builder.Services.AddSingleton<ExpressDeliveryGroupResolver>();
builder.Services.AddSingleton<PriorityGroupResolver>();
builder.Services.AddSingleton<ExactMatchGroupResolver>();
builder.Services.AddSingleton<IXlsxGroupResolverFactory, XlsxGroupResolverFactory>();
// XlsxHeadersGroupResolver регистрируется отдельно через ActivatorUtilities для избежания циклической зависимости
builder.Services.AddTransient<XlsxHeadersGroupResolver>();

// Регистрация сервиса разрешения групп
builder.Services.AddSingleton<IGroupResolutionService, GroupResolutionService>();

// Регистрация стратегий через универсальный класс
builder.Services.AddSingleton<IFlowTypeStrategy>(sp =>
    new GenericFlowTypeStrategy<PaymentResult>(
        sp.GetRequiredService<IBusinessFlowFactory>(),
        FlowType.Payments,
        result => new PaymentFlowResult(result),
        sp.GetRequiredService<ILogger<GenericFlowTypeStrategy<PaymentResult>>>()));

builder.Services.AddSingleton<IFlowTypeStrategy>(sp =>
    new GenericFlowTypeStrategy<DeliveryResult>(
        sp.GetRequiredService<IBusinessFlowFactory>(),
        FlowType.Delivery,
        result => new DeliveryFlowResult(result),
        sp.GetRequiredService<ILogger<GenericFlowTypeStrategy<DeliveryResult>>>()));

builder.Services.AddSingleton<IBusinessFlowFactory, BusinessFlowFactory>();
builder.Services.AddSingleton<IBusinessFlowExecutor, BusinessFlowExecutor>();

var app = builder.Build();

// Middleware для обработки исключений
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

static void RegisterBusinessFlows(IServiceCollection services)
{
    var flowTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IBusinessFlow<>)) &&
            !t.IsInterface && !t.IsAbstract);

    foreach (var flowType in flowTypes)
    {
        var interfaceType = flowType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBusinessFlow<>));

        services.AddTransient(interfaceType, flowType);
        services.AddTransient(flowType);
    }
}
