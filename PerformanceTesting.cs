using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Dapper;
using DapperEFCorePostgreSQL.Context;
using DapperEFCorePostgreSQL.Dtos;
using DapperEFCorePostgreSQL.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DapperEFCorePostgreSQL
{
    [ShortRunJob]
    [MemoryDiagnoser]
    [KeepBenchmarkFiles(true)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
    public class PerformanceTesting
    {
        private readonly IMapper _mapper;

        public PerformanceTesting()
        {
            _mapper = new MapperConfiguration(config =>
            {
                config.CreateMap<Category, CategoryDto>();
                config.CreateMap<Product, ProductDto>();
                config.Advanced.BeforeSeal(provider => provider.CompileMappings());
            }).CreateMapper();
        }

        [Benchmark]
        public void EFCore()
        {
            using var sampleDbContext = new SampleDbContext();
            var categories = sampleDbContext.Categories.Include(i => i.Products).ToList();
        }

        [Benchmark]
        public void EFCore_AsNoTracking()
        {
            using var sampleDbContext = new SampleDbContext();
            var categories = sampleDbContext.Categories.AsNoTracking().Include(i => i.Products).ToList();
        }

        [Benchmark]
        public void EFCore_AutoMapper()
        {
            using var sampleDbContext = new SampleDbContext();
            var categories = sampleDbContext.Categories.ProjectTo<CategoryDto>(_mapper.ConfigurationProvider).ToList();
        }

        [Benchmark]
        public void EFCore_AutoMapper_AsNoTracking()
        {
            using var sampleDbContext = new SampleDbContext();
            var categories = sampleDbContext.Categories.AsNoTracking().ProjectTo<CategoryDto>(_mapper.ConfigurationProvider).ToList();
        }

        [Benchmark]
        public void Dapper_AutoMapper1()
        {
            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            var categories = connection.Query<CategoryDto>("SELECT * FROM \"Categories\"").ToList();
            var products = connection.Query<ProductDto>("SELECT * FROM \"Products\"").ToList();
            foreach (var product in products)
                product.Category = categories.FirstOrDefault(c => c.CategoryId == product.CategoryId);
            foreach (var category in categories)
                category.Products = products.Where(p => p.CategoryId == category.CategoryId).ToList();
        }
        [Benchmark]
        public void Dapper_AutoMapper1_NEW()
        {
            using (var connection = new NpgsqlConnection(Constants.ConnectionString))
            {
                string sql = "SELECT \"Products\".*,\"Categories\".\"CategoryName\" FROM \"Products\" FULL JOIN \"Categories\" ON \"Categories\".\"CategoryId\" = \"Products\".\"CategoryId\"";
                var productCategoryView = connection.Query<CategoryProductView>(sql);
                var products = productCategoryView.Where(w => w.ProductId.HasValue).GroupBy(g => g.ProductId).Select(s =>
                  {
                      var first = s.First();
                      return new ProductDto() { Content = first.Content, ProductId = first.ProductId.Value, Description = first.Description, Name = first.Name, CategoryId = first.CategoryId.HasValue ? first.CategoryId.Value : 0, Category = !first.CategoryId.HasValue ? null : new CategoryDto() { CategoryId = first.CategoryId.Value, CategoryName = first.CategoryName } };
                  });


                var categories = productCategoryView.Where(w => w.CategoryId.HasValue).GroupBy(g => g.CategoryId).Select(s =>
                {
                    var first = s.First();
                    return new CategoryDto() { CategoryId = first.CategoryId.Value, CategoryName = first.CategoryName, Products = products.Where(e => e.CategoryId == first.CategoryId.Value).ToList() };
                });
            }

        }

        [Benchmark]
        public void Dapper_AutoMapper2()
        {
            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            var categories = connection.Query<CategoryDto>("SELECT * FROM \"Categories\"").ToList();
            foreach (var category in categories)
            {
                category.Products = connection.Query<ProductDto>("SELECT * FROM \"Products\" WHERE \"CategoryId\" = @ID", new { ID = category.CategoryId }).ToList();
                foreach (var product in category.Products) product.Category = category;
            }
        }

        [Benchmark]
        public void Dapper_AutoMapper3()
        {
            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            var categories = connection.Query<CategoryDto>("SELECT * FROM \"Categories\"").ToList();
            var products = connection.Query<ProductDto, CategoryDto, ProductDto>(
                "SELECT P.\"ProductId\", P.\"CategoryId\", P.\"Name\", P.\"Description\", P.\"Content\", P.\"CreateDate\", C.\"CategoryName\", C.\"CategoryId\", C.\"CreateDate\" FROM \"Categories\" C LEFT JOIN \"Products\" P ON C.\"CategoryId\"=P.\"CategoryId\"",
                (p, c) => { p.Category = c; return p; },
                splitOn: "CategoryName").ToList();
            // Categories.Products?
            // Products.Category?
            // foreach (var category in categories)
            //    category.Products = products.Where(p => p.CategoryId == category.CategoryId).ToList();
        }

        [Benchmark]
        public void Dapper1()
        {
            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            var categories = connection.Query("SELECT * FROM \"Categories\"").Select(obj => new CategoryDto
            {
                CategoryId = obj.CategoryId,
                CategoryName = obj.CategoryName
            }).ToList();
            foreach (var category in categories)
            {
                category.Products = connection.Query("SELECT * FROM \"Products\" WHERE \"CategoryId\" = @ID", new { ID = category.CategoryId }).Select(obj => new ProductDto
                {
                    ProductId = obj.ProductId,
                    CategoryId = obj.CategoryId,
                    Name = obj.Name,
                    Description = obj.Description,
                    Content = obj.Content,
                    Category = category
                }).ToList();
            }
        }

        [Benchmark]
        public void Dapper2()
        {
            using var connection = new NpgsqlConnection(Constants.ConnectionString);
            var categories = connection.Query("SELECT * FROM \"Categories\"").Select(obj => new CategoryDto
            {
                CategoryId = obj.CategoryId,
                CategoryName = obj.CategoryName
            }).ToList();
            var products = connection.Query("SELECT * FROM \"Products\"").Select(obj => new ProductDto
            {
                ProductId = obj.ProductId,
                CategoryId = obj.CategoryId,
                Name = obj.Name,
                Description = obj.Description,
                Content = obj.Content,
                Category = categories.FirstOrDefault(c => c.CategoryId == obj.CategoryId)
            }).ToList();
            foreach (var category in categories)
                category.Products = products.Where(p => p.CategoryId == category.CategoryId).ToList();
        }

    }
}
