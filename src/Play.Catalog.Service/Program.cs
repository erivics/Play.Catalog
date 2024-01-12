using System.ComponentModel;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Play.Catalog.Service;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Repositories;
using Play.Catalog.Service.Validations;
using Play.Catalog.Service.BsonFormat;
using Play.Catalog.Service.Settings;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var serviceSetting = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
builder.Services.AddSingleton(serviceProvider =>
{
   var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
   var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
   return mongoClient.GetDatabase(serviceSetting.ServiceName);
});

BsonSerializerRegisterer.BsonSerializerRegisters();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(options =>
{
   options.SuppressAsyncSuffixInActionNames = true;
});
builder.Services.AddSingleton<IItemsRepository,ItemsRepository>();
builder.Services.AddScoped<IValidator<CreateItemDto>,CreateItemValidation>();
builder.Services.AddScoped<IValidator<UpdateItemDto>, UpdateItemValidation>();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp 
    => exceptionHandlerApp.Run(async context 
        => await Results.Problem()
                     .ExecuteAsync(context)));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

//Using Minimal api 
 
 var Items = app.MapGroup("/items");
 
 
 //Get/items
Items.MapGet("/", async (IItemsRepository itemsRepository) =>
{ 
   var items = (await itemsRepository.GetAllAsync())
                  .Select(item => item.AsDto());
   return Results.Ok(items);
})
.WithOpenApi();



//Get/items/{id}
Items.MapGet("/{id}", async (IItemsRepository itemsRepository, Guid id) =>
{
 
      var item = await itemsRepository.GetByIdAsync(id).ConfigureAwait(false);
       return Results.Ok(item.AsDto());
  
})
.WithName("GetByIdAsync")
.WithOpenApi();



//Post/items
Items.MapPost("/", async (IItemsRepository itemsRepository, IValidator<CreateItemDto> validator, CreateItemDto createItemDto) =>
{
   ValidationResult validationResult = validator.Validate(createItemDto);
   
   if (!validationResult.IsValid)
   {
     return Results.ValidationProblem(validationResult.ToDictionary());
   }

   var item = new Item 
   {
      Name = createItemDto.Name, Description = createItemDto.Description, Price = createItemDto.Price, CreatedDate = DateTimeOffset.UtcNow
   };

   await itemsRepository.CreateAsync(item);
   return Results.CreatedAtRoute("GetByIdAsync",  new {id = item.Id}, item);
})
.WithOpenApi();


//Update/items
Items.MapPut("/{id}", async (IItemsRepository itemsRepository, IValidator<UpdateItemDto> validator, UpdateItemDto updateItemDto, Guid id) =>
{
   ValidationResult validationResult = validator.Validate(updateItemDto);
   
   if (!validationResult.IsValid)
   {
     return Results.ValidationProblem(validationResult.ToDictionary());
   }
   
   var existingItem = await itemsRepository.GetByIdAsync(id);
   if(existingItem is null)
   {
      return Results.NotFound();
   }

   existingItem.Name = updateItemDto.Name;
   existingItem.Description = updateItemDto.Description;
   existingItem.Price = updateItemDto.Price;
   await itemsRepository.UpdateAsync(existingItem);

   return Results.NoContent();
})
.WithOpenApi();

//Delete/items/{id}
Items.MapDelete("/{id}", async (IItemsRepository itemsRepository,Guid id) =>
{
   var item = await itemsRepository.GetByIdAsync(id);
   if(item is null)
   {
      return Results.NotFound();
   }
   await itemsRepository.RemoveAsync(item.Id);
   return Results.NoContent();
})
.WithOpenApi();

app.Run();
