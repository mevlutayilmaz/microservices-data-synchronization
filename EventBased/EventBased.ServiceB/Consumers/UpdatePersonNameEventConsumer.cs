﻿using EventBased.ServiceB.Entities;
using EventBased.ServiceB.Services;
using EventBased.Shared.Events;
using MassTransit;
using MongoDB.Driver;

namespace EventBased.ServiceB.Consumers
{
    public class UpdatePersonNameEventConsumer : IConsumer<UpdatedPersonNameEvent>
    {
        readonly MongoDBService _mongoDBService;
        public UpdatePersonNameEventConsumer(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }
        public async Task Consume(ConsumeContext<UpdatedPersonNameEvent> context)
        {
            var employees = _mongoDBService.GetCollection<Employee>();
            Employee employee = await (await employees.FindAsync(e => e.PersonId == context.Message.PersonId)).FirstOrDefaultAsync();
            employee.Name = context.Message.NewName;
            await employees.FindOneAndReplaceAsync(p => p.Id == employee.Id, employee);
        }
    }
}
