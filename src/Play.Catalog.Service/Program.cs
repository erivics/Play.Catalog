using FluentValidation;
using FluentValidation.Results;
using Play.Catalog.Service;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Validations;
using Play.Common;
using Play.Common.MongoDB;
using Play.Common.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
 

var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

builder.Services.AddMongo()
                  .AddMongoRepository<Item>("items");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(options =>
{
   options.SuppressAsyncSuffixInActionNames = true;
});
  
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
Items.MapGet("/", async (IRepository<Item> itemsRepository) =>
{ 
   var items = (await itemsRepository.GetAllAsync())
                  .Select(item => item.AsDto());
   return Results.Ok(items);
})
.WithOpenApi();



//Get/items/{id}
Items.MapGet("/{id}", async (IRepository<Item> itemsRepository, Guid id) =>
{
 
      var item = await itemsRepository.GetAsync(id).ConfigureAwait(false);
      if (item == null) return Results.NotFound();
       return Results.Ok(item.AsDto());
  
})
.WithName("GetAsync")
.WithOpenApi();



//Post/items
Items.MapPost("/", async (IRepository<Item> itemsRepository, IValidator<CreateItemDto> validator, CreateItemDto createItemDto) =>
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
   return Results.CreatedAtRoute("GetAsync",  new {id = item.Id}, item);
})
.WithOpenApi();


//Update/items
Items.MapPut("/{id}", async (IRepository<Item> itemsRepository, IValidator<UpdateItemDto> validator, UpdateItemDto updateItemDto, Guid id) =>
{
   ValidationResult validationResult = validator.Validate(updateItemDto);
   
   if (!validationResult.IsValid)
   {
     return Results.ValidationProblem(validationResult.ToDictionary());
   }
   
   var existingItem = await itemsRepository.GetAsync(id);
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
Items.MapDelete("/{id}", async (IRepository<Item> itemsRepository,Guid id) =>
{
   var item = await itemsRepository.GetAsync(id);
   if(item is null)
   {
      return Results.NotFound();
   }
   await itemsRepository.RemoveAsync(item.Id);
   return Results.NoContent();
})
.WithOpenApi();

app.Run();
