using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

// Define your models
public abstract class Asset
{
    public int Id { get; set; }
    public string ModelName { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal Price { get; set; }
    public int OfficeId { get; set; }
    public Office Office { get; set; }
}

public class Laptop : Asset { }
public class MobilePhone : Asset { }

public class MacBook : Laptop { }
public class Asus : Laptop { }
public class Lenovo : Laptop { }

public class Iphone : MobilePhone { }
public class Samsung : MobilePhone { }
public class Nokia : MobilePhone { }

public class Office
{
    public int Id { get; set; }
    public string Location { get; set; }
    public string CurrencyCode { get; set; }
    public List<Asset> Assets { get; set; } = new List<Asset>();
}

// Define your DbContext
public class AssetContext : DbContext
{
    public DbSet<Laptop> Laptops { get; set; }
    public DbSet<MobilePhone> MobilePhones { get; set; }
    public DbSet<Office> Offices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=localhost,1433;Initial Catalog=AssetTrackingDb;User ID=SA;Password=Yourpassword1;Encrypt=True;TrustServerCertificate=True");
    }
}

// AssetService handles CRUD operations
public class AssetService
{
    private readonly AssetContext _context;

    public AssetService(AssetContext context)
    {
        _context = context;
    }

    public void AddAsset(Asset asset)
    {
        _context.Add(asset);
        _context.SaveChanges();
    }

    public void AddOffice(Office office)
    {
        _context.Add(office);
        _context.SaveChanges();
    }

    public List<Asset> GetAllAssets()
    {
        // This will combine laptops and mobile phones into a single list of assets.
        var laptops = _context.Laptops.Include(l => l.Office).Cast<Asset>();
        var mobilePhones = _context.MobilePhones.Include(m => m.Office).Cast<Asset>();

        return laptops.Concat(mobilePhones).ToList();
    }

    public List<Office> GetAllOffices()
    {
        return _context.Offices.ToList();
    }

    // Implement other CRUD methods as needed
}

// Currency Converter (Static Rates)
public class CurrencyConverter
{
    private readonly Dictionary<string, decimal> _conversionRates = new Dictionary<string, decimal> {
        {"USD", 1m},
        {"EUR", 0.85m},
        {"GBP", 0.75m}
    };

    public decimal Convert(decimal amount, string fromCurrency, string toCurrency)
    {
        var amountInUsd = amount / _conversionRates[fromCurrency];
        return amountInUsd * _conversionRates[toCurrency];
    }
}

// Main application
class Program
{
    static void Main(string[] args)
    {
        using (var context = new AssetContext())
        {
            context.Database.EnsureCreated();

            var assetService = new AssetService(context);
            var converter = new CurrencyConverter();

            // Seed some data if needed
            SeedData(assetService);

            // Fetch all assets and display them
            var assets = assetService.GetAllAssets();
            DisplayAssets(assets, converter);
        }
    }

    static void DisplayAssets(List<Asset> assets, CurrencyConverter converter)
    {
        var sortedAssets = assets
            .OrderBy(a => a.OfficeId)
            .ThenBy(a => a.PurchaseDate)
            .ToList();

        foreach (var asset in sortedAssets)
        {
            var timeToLifeEnd = DateTime.Now - asset.PurchaseDate;
            var remainingLife = (3 * 365) - timeToLifeEnd.TotalDays;

            if (remainingLife < 90)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (remainingLife < 180)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }

            var localCurrencyPrice = converter.Convert(asset.Price, "USD", asset.Office.CurrencyCode);

            Console.WriteLine($"{asset.Office.Location} - {asset.GetType().Name} - {asset.ModelName} - " +
                              $"{asset.PurchaseDate.ToShortDateString()} - {localCurrencyPrice} {asset.Office.CurrencyCode}");
            Console.ResetColor();
        }
    }

    private static void SeedData(AssetService assetService)
    {
        // Check if there are any offices already (to prevent re-seeding data)
        if (!assetService.GetAllOffices().Any())
        {
            // Create offices
            var offices = new List<Office>
            {
                new Office { Location = "New York", CurrencyCode = "USD" },
                new Office { Location = "London", CurrencyCode = "GBP" },
                new Office { Location = "Berlin", CurrencyCode = "EUR" }
            };

            // Add offices to the database
            offices.ForEach(office => assetService.AddOffice(office));
        }

        // Check if there are any assets already (to prevent re-seeding data)
        if (!assetService.GetAllAssets().Any())
        {
            // Retrieve offices from the database to attach to new assets
            var offices = assetService.GetAllOffices();

            // Create assets and assign to offices
            var assets = new List<Asset>
            {
                new MacBook { ModelName = "MacBook Pro", PurchaseDate = DateTime.UtcNow.AddYears(-1), Price = 1500, OfficeId = offices.First(o => o.Location == "New York").Id },
                new Lenovo { ModelName = "ThinkPad X1", PurchaseDate = DateTime.UtcNow.AddYears(-2), Price = 1000, OfficeId = offices.First(o => o.Location == "London").Id },
                new Iphone { ModelName = "iPhone 12", PurchaseDate = DateTime.UtcNow.AddYears(-3), Price = 800, OfficeId = offices.First(o => o.Location == "Berlin").Id },
                // Add more assets as needed
            };

            // Add assets to the database
            assets.ForEach(asset => assetService.AddAsset(asset));
        }
    }
}
