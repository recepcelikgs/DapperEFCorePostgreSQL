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
        public void Dapper()
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

    }
}
