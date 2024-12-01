using EventBased.ServiceB.Consumers;
using EventBased.ServiceB.Entities;
using EventBased.ServiceB.Services;
using EventBased.Shared;
using MassTransit;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<UpdatePersonNameEventConsumer>();

    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration["RabbitMQ"]);

        _configurator.ReceiveEndpoint(RabbitMQSettings.ServiceB_UpdatePersonNameEventQueue, e => e.ConfigureConsumer<UpdatePersonNameEventConsumer>(context));
    });
});

#region MongoDB'ye Seed Data Ekleme
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
MongoDBService mongoDBService = scope.ServiceProvider.GetService<MongoDBService>();
var collection = mongoDBService.GetCollection<Employee>();
if (!collection.FindSync(s => true).Any())
{
    await collection.InsertOneAsync(new() { PersonId = "6722499ec658a0ec1344d902", Name = "Ahmet", Department = "Yazılım" });
    await collection.InsertOneAsync(new() { PersonId = "6722499ec658a0ec1344d903", Name = "Mehmet", Department = "Pazarlama" });
    await collection.InsertOneAsync(new() { PersonId = "6722499ec658a0ec1344d904", Name = "Hasan", Department = "Şoför" });
    await collection.InsertOneAsync(new() { PersonId = "6722499ec658a0ec1344d905", Name = "Hüseyin", Department = "İnsan Kaynakları" });
    await collection.InsertOneAsync(new() { PersonId = "6722499ec658a0ec1344d906", Name = "Hilmi", Department = "Hukuk" });
    await collection.InsertOneAsync(new() { PersonId = "6722499ec658a0ec1344d906", Name = "İbrahim", Department = "Muhasebe" });
}
#endregion

var app = builder.Build();

app.Run();