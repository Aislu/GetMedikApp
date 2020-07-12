using System;
using System.Data.Entity;
using System.Data.Common;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Configuration;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ParsCatalogForms.Model
{
    public class context : DbContext
    {
        public DbSet<Category> Categorys { get; set; }
        public DbSet<MedicData> MedicDatas { get; set; }
        public context() : base(GetConnection(), false)
        {

        }
        public static DbConnection GetConnection()
        {
            var connection = ConfigurationManager.ConnectionStrings["SQLiteConnection"];
            var factory = DbProviderFactories.GetFactory(connection.ProviderName);
            var dbCon = factory.CreateConnection();
            dbCon.ConnectionString = connection.ConnectionString;
            return dbCon;
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            Database.SetInitializer<context>(null);
            base.OnModelCreating(modelBuilder);

        }
    }

    public class Category
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Index("IX_Numurs_Unique", 0, IsUnique = true)]
        public string numurs { get; set; }
        public string nosaukums { get; set; }

    }
    public class MedicData
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public Category Category { get; set; }

        [Index("IX_Numurs_Unique", 0, IsUnique = true)]
        public string numurs { get; set; }
        public string nosaukums { get; set; }
        public string kods { get; set; }
        public double cenaBPVN { get; set; }
    }
}
