using FluentValidation;
using FluentValidation.Results;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Validations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddScoped<IValidator<CreateItemDto>,CreateItemValidation>();
builder.Services.AddScoped<IValidator<UpdateItemDto>, UpdateItemValidation>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

//Using Minimal api 
var items =  new List<ItemDto>()
{
    new ItemDto(Guid.NewGuid(), "Nike","Pure White", 98, DateTimeOffset.UtcNow),
    new ItemDto(Guid.NewGuid(), "Lascorte","Dark colour with shining stones ", 110, DateTimeOffset.UtcNow),
    new ItemDto(Guid.NewGuid(), "Verrari","Badest colour", 50, DateTimeOffset.UtcNow),
};
 
 var Items = app.MapGroup("/items");
 //Get/items
Items.MapGet("/", () =>
{
   return Results.Ok(items);
})
.WithOpenApi();


//Get/items/{id}
Items.MapGet("/{id}", (Guid id) =>
{
   var item = items.Where(item => item.Id == id).SingleOrDefault();
   if (item is null) return Results.NotFound();
   return Results.Ok(item);
})
.WithName("GetById")
.WithOpenApi();

//Post/items
Items.MapPost("/", (IValidator<CreateItemDto> validator, CreateItemDto createItemDto) =>
{
   ValidationResult validationResult = validator.Validate(createItemDto);
   
   if (!validationResult.IsValid)
   {
     return Results.ValidationProblem(validationResult.ToDictionary());
   }

   var item = new ItemDto(Guid.NewGuid(), createItemDto.Name, createItemDto.Description, createItemDto.Price, DateTimeOffset.UtcNow);
   items.Add(item);
   return Results.CreatedAtRoute("GetById",  new {id = item.Id}, item);
})
.WithOpenApi();

//Update/items
Items.MapPut("/{id}", (IValidator<UpdateItemDto> validator, UpdateItemDto updateItemDto, Guid id) =>
{
   ValidationResult validationResult = validator.Validate(updateItemDto);
   
   if (!validationResult.IsValid)
   {
     return Results.ValidationProblem(validationResult.ToDictionary());
   }
   
   var existingItem = items.Where(item => item.Id == id).SingleOrDefault();
   if (existingItem is null) return Results.NotFound();
   var updatedItem = existingItem with
   {
        Name = updateItemDto.Name,
        Description = updateItemDto.Description,
        Price = updateItemDto.Price

   };

   var index = items.FindIndex(existingItem => existingItem.Id == id);
   items[index] = updatedItem;

   return Results.NoContent();
})
.WithOpenApi();

//Delete/items/{id}
Items.MapDelete("/{id}", (Guid id) =>
{
   var index = items.FindIndex(existingItem => existingItem.Id == id);
   if (index < 0) return Results.NotFound();

   items.RemoveAt(index);
   return Results.NoContent();
})
.WithOpenApi();

app.Run();
