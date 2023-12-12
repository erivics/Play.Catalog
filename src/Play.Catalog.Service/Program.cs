using Play.Catalog.Service.Dtos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
 
app.MapGet("/items", () =>
{
   return Results.Ok(items);
})
.WithOpenApi();

app.MapGet("/items/{id}", (Guid id) =>
{
   var item = items.Where(item => item.Id == id).SingleOrDefault();
   return Results.Ok(item);
})
.WithName("GetById")
.WithOpenApi();

app.MapPost("/items", (CreateItemDto createItemDto) =>
{
   var item = new ItemDto(Guid.NewGuid(), createItemDto.Name, createItemDto.Description, createItemDto.Price, DateTimeOffset.UtcNow);
   items.Add(item);
   return Results.CreatedAtRoute("GetById",  new {id = item.Id}, item);
})
.WithOpenApi();

app.Run();
