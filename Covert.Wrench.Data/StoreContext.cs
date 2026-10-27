using System.Net.ServerSentEvents;
using Covert.Wrench.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Covert.Wrench.Data
{
    public class StoreContext : DbContext
    {
        public StoreContext(DbContextOptions<StoreContext> options)
            : base(options)
        {    }
        public DbSet<Item> Items { get; set; }
    }
}
