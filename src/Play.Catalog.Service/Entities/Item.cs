namespace Play.Catalog.Service.Entities
{
    public class Item : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty ;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;   
        public DateTimeOffset CreatedDate { get; set; } 
    }
}