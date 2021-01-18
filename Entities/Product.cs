using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DapperEFCorePostgreSQL.Entities
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Content { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual Category Category { get; set; }
    }
}
