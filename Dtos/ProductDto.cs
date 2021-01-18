namespace DapperEFCorePostgreSQL.Dtos
{
    public class ProductDto
    {
        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Content { get; set; }

        public virtual CategoryDto Category { get; set; }
    }
}
