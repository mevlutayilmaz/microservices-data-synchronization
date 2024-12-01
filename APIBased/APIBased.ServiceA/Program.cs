using APIBased.ServiceA.Entities;
using APIBased.ServiceA.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddHttpClient("ServiceB", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://localhost:7063/");
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


app.MapGet("/{id}/{newName}", async (
    [FromRoute] string id,
    [FromRoute] string newName,
    MongoDBService mongoDBService,
    IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient("ServiceB");

    var persons = mongoDBService.GetCollection<Person>();

    Person person = await (await persons.FindAsync(p => p.Id == ObjectId.Parse(id))).FirstOrDefaultAsync();
    person.Name = newName;
    await persons.FindOneAndReplaceAsync(p => p.Id == ObjectId.Parse(id), person);

    var httpResponseMessage = await httpClient.GetAsync($"update/{person.Id}/{person.Name}");
    if (httpResponseMessage.IsSuccessStatusCode)
    {
        var content = await httpResponseMessage.Content.ReadAsStringAsync();
        await Console.Out.WriteLineAsync(content);
    }
});

app.Run();