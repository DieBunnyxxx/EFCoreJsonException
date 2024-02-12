// See https://aka.ms/new-console-template for more information

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class Case
{
    public int Id { get; set; }
    public string Type { get; set; }
    public List<CaseCustomer> CaseCustomers { get; set; }

}

public class CaseCustomer
{
    public int Id { get; set; }

    public int CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public List<ContactInfo> ContactInfo { get; set; }
}

public class ContactInfo
{
    public string ContactInfoType { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class CustomDbContext : DbContext
{
    public DbSet<Case> Cases { get; set; }
    public DbSet<Customer> Customers { get; set; }

    public DbSet<CaseCustomer> CaseCustomers { get; set; }

    public static readonly ILoggerFactory ConsoleLoggerFactory
      = LoggerFactory.Create(builder =>
      {
          builder
       .AddFilter((category, level) =>
           category == DbLoggerCategory.Database.Command.Name
           && level == LogLevel.Information)
       .AddConsole();
      });

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
           .UseLoggerFactory(ConsoleLoggerFactory)
           .EnableSensitiveDataLogging()
           .UseSqlServer("Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = CaseData");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().OwnsMany(c => c.ContactInfo, ownedNavigationBuilder =>
        {
            ownedNavigationBuilder.ToJson();
        });
    }
}

public static class Program
{
    private static CustomDbContext context = new CustomDbContext();

    public static async Task Main()
    {
        context.Database.EnsureCreated();

        var customerAlan = new Customer
        {
            Name = "Alan",
            LastName = "XXX",
            ContactInfo = new List<ContactInfo>
                {
                    new ContactInfo { ContactInfoType = "Home", Email = "alan@someone.eu", Phone = "45325252532"},
                    new ContactInfo { ContactInfoType = "Work", Email = "alan@company.eu", Phone = "+44220000077"}
                }
        };

        var customerDylan = new Customer
        {
            Name = "Dylan",
            LastName = "YYY",
            ContactInfo = new List<ContactInfo>
                {
                    new ContactInfo { ContactInfoType = "Home", Email = "dylan@someone.eu", Phone = "7254367347"},
                    new ContactInfo { ContactInfoType = "Work", Email = "dylan@company.eu", Phone = "547546-2323542"},
                    new ContactInfo { ContactInfoType = "Work2", Email = "dylan@company2.eu", Phone = "55522255"}
                }
        };

        var customerRita = new Customer
        {
            Name = "Rita",
            LastName = "ZZZ",
            ContactInfo = new List<ContactInfo>
                {
                    new ContactInfo { ContactInfoType = "Work", Email = "rita@business.eu", Phone = "+46253253"},
                    new ContactInfo { ContactInfoType = "Home", Email = "rita@home.eu", Phone = "68263000"},
                    new ContactInfo { ContactInfoType = "SummerHome", Email = "rita@summer.eu", Phone = "5555555"}
                }
        };

        context.Customers.Add(customerAlan);
        context.Customers.Add(customerDylan);
        context.Customers.Add(customerRita);
        await context.SaveChangesAsync();

        context.Cases.Add(new Case
        {
            Type = "a",
            CaseCustomers = new List<CaseCustomer>
        {
            new CaseCustomer{ CustomerId = customerAlan.Id},
            new CaseCustomer{ CustomerId = customerDylan.Id},
            new CaseCustomer{ CustomerId = customerAlan.Id},
        }
        });

        context.Cases.Add(new Case
        {
            Type = "B",
            CaseCustomers = new List<CaseCustomer>
        {
            new CaseCustomer{ CustomerId = customerRita.Id},
            new CaseCustomer{ CustomerId = customerDylan.Id}
        }
        });

        await context.SaveChangesAsync();

        // returns results
        var resultsi = await context.Set<CaseCustomer>().AsNoTracking().Include(cc => cc.Customer).Where(e => e.CaseId == 1).ToListAsync();


        // exception :O System.InvalidOperationException: 'Invalid token type: 'StartObject
        var results1i = await context.Set<CaseCustomer>().AsNoTrackingWithIdentityResolution().Include(cc => cc.Customer).Where(e => e.CaseId == 1).ToListAsync();
    }
}
