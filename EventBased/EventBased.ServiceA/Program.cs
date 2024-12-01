using EventBased.ServiceA.Entities;
using EventBased.ServiceA.Services;
using EventBased.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration["RabbitMQ"]);
    });
});


#region MongoDB'ye Seed Data Ekleme
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
MongoDBService mongoDBService = scope.ServiceProvider.GetService<MongoDBService>();
var collection = mongoDBService.GetCollection<Person>();
if (!collection.FindSync(s => true).Any())
{
    await collection.InsertOneAsync(new() { Name = "Ahmet" });
    await collection.InsertOneAsync(new() { Name = "Mehmet" });
    await collection.InsertOneAsync(new() { Name = "Hasan" });
    await collection.InsertOneAsync(new() { Name = "Hüseyin" });
    await collection.InsertOneAsync(new() { Name = "Hilmi" });
    await collection.InsertOneAsync(new() { Name = "İbrahim" });
}
#endregion


var app = builder.Build();


app.MapGet("updateName/{id}/{newName}", async (
    [FromRoute] string id,
    [FromRoute] string newName,
    MongoDBService mongoDBService,
   IPublishEndpoint publishEndpoint) =>
{
    var persons = mongoDBService.GetCollection<Person>();

    Person person = await (await persons.FindAsync(p => p.Id == ObjectId.Parse(id))).FirstOrDefaultAsync();
    person.Name = newName;
    await persons.FindOneAndReplaceAsync(p => p.Id == ObjectId.Parse(id), person);

    UpdatedPersonNameEvent updatedPersonNameEvent = new()
    {
        PersonId = id,
        NewName = newName
    };

    await publishEndpoint.Publish(updatedPersonNameEvent);
});

app.Run();