using APIBased.ServiceB.Entities;
using APIBased.ServiceB.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<MongoDBService>();

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


app.MapGet("update/{personId}/{newName}", async (
    [FromRoute] string personId,
    [FromRoute] string newName,
    MongoDBService mongoDBService) =>
{
    var employees = mongoDBService.GetCollection<Employee>();
    Employee employee = await (await employees.FindAsync(e => e.PersonId == personId)).FirstOrDefaultAsync();
    employee.Name = newName;
    await employees.FindOneAndReplaceAsync(p => p.Id == employee.Id, employee);
    return true;
});

app.Run();