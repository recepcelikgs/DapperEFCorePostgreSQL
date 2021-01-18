using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DapperEFCorePostgreSQL.Entities
{
    public class Category
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
