using System;
using System.Collections.Generic;
using BenchmarkDotNet.Running;
using DapperEFCorePostgreSQL.Context;
using DapperEFCorePostgreSQL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DapperEFCorePostgreSQL
{
    internal class Program
    {
        private static void Main()
        {
            Console.Clear();
            Console.WriteLine("Operations Menu");
            Console.WriteLine("---------------");
            Console.WriteLine("1 > Generate test data (Categories: 100)");
            Console.WriteLine("2 > Generate test data (Categories: 1.000)");
            Console.WriteLine("3 > Generate test data (Categories: 10.000)");
            Console.WriteLine("0 > Run tests");
            Console.Write("Choice: ");
            var input = Console.ReadLine();
            switch (input)
            {
                case "1": GenerateTestData(100); break;
                case "2": GenerateTestData(1000); break;
                case "3": GenerateTestData(10000); break;
                case "0": RunTests(); break;
                default: Console.WriteLine("Aborting..."); break;
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static void GenerateTestData(int categoriesCount)
        {
            Console.WriteLine("Database is migrating...");
            using var sampleDbContext = new SampleDbContext();
            sampleDbContext.Database.Migrate();
            sampleDbContext.SaveChanges();

            Console.WriteLine("Sample data is generating...");
            sampleDbContext.Products.RemoveRange(sampleDbContext.Products);
            var random = new Random();
            var productsCount = 0;
            for (var i = 1; i <= categoriesCount; i++)
            {
                var category = new Category { CategoryName = $"Category {i:000000}", Products = new List<Product>() };
                for (var j = 1; j <= random.Next(100); j++)
                {
                    productsCount++;
                    category.Products.Add(new Product
                    {
                        Name = $"Product Name {productsCount:000000}",
                        Description = $"Product Description {productsCount:000000}",
                        Content = $"Product Content {productsCount:000000}",
                    });
                }
                sampleDbContext.Categories.Add(category);
            }
            sampleDbContext.SaveChanges();
            Console.WriteLine($"{categoriesCount} categories generated.");
            Console.WriteLine($"{productsCount} products generated.");
            Console.WriteLine("Sample data generation completed.");
        }

        static void RunTests()
        {
            //if(System.Diagnostics.Debugger.IsAttached)
            //{
            //    var testing = new PerformanceTesting();
            //    testing.Dapper();
            //    return;
            //}
            BenchmarkRunner.Run<PerformanceTesting>();
        }
    }
}
