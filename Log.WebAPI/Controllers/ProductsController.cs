using Log.WebAPI.Context;
using Log.WebAPI.DTOs;
using Log.WebAPI.Filters;
using Log.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace Log.WebAPI.Controllers;
[Route("api/[controller]/[action]")]
[ApiController]
public class ProductsController(ApplicationDbContext context): ControllerBase
{
    [HttpPost]
    [LogFilter]
    public IActionResult Create(ProductDto request)
    {
        Product product = new()
        {
            Name = request.Name,
            Price = request.Price
        };
        
        context.Add(product);
        context.SaveChanges();

    
        
        return Ok(); 
        
        //Serilog.Log.Information("Starting Creating...");
        //Serilog.Log.Warning("Creating finished");

        //var log = new LoggerConfiguration()
        //    .WriteTo.Console()//Serilog.sinks.console'dan geliyor.
        //    .WriteTo.File("./log.txt",rollingInterval:RollingInterval.Minute)
        //    .CreateLogger();

        //log.Information("Create işlemi başlıyor");

        //logger.LogError("Create işlemi başlıyor");
        //logger.LogWarning("Create işlemi tamamlandı");
        //Logger.LogCritical("Critical Deneme");

    }
    [HttpGet]
    public IActionResult Update(int id,string name,decimal price)
    {
        Product? product = context.Products.Find(id);
        if (product is not null)
        {
            product.Name = name;
            product.Price = price;
            context.SaveChanges();
        }

        //logger.LogInformation("Update işlemi başlıyor");
        //logger.LogInformation("Update işlemi tamamlandı");
        return NoContent();
    }
    [HttpGet]
    public IActionResult DeleteById(int id)
    {
        Product? product = context.Products.Find(id);
        if(product is not null)
        {
            context.Remove(product);
            context.SaveChanges();
        }
        
        return NoContent();
        
        //logger.LogInformation("Delete işlemi başlıyor");
        //logger.LogInformation("Delete işlemi tamamlandı");
    }
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(context.Products.ToList());
    }

}
