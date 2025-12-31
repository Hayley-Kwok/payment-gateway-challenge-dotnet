using FluentValidation;
using PaymentGateway.Api.Models.Validators;
using PaymentGateway.Api.Services.Clients;
using PaymentGateway.Api.Services.Processors;
using PaymentGateway.Api.Services.Repositories;
using PaymentGateway.Api.Services.Retrievers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<ProcessPaymentRequestValidator>();

builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddSingleton<HttpClient>();

// there is a better way to feed in the url (e.g. from config/option) but for now this will do
builder.Services.AddSingleton<IAcquiringBankClient, AcquiringBankClient>(sp => new AcquiringBankClient(AcquiringBankClient.LocalTestUrl,
    sp.GetRequiredService<HttpClient>()));

builder.Services.AddSingleton<IPaymentRetriever, PaymentRetriever>();
builder.Services.AddScoped<IPaymentProcessor, PaymentProcessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
