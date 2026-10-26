using Microsoft.EntityFrameworkCore;
using TheFarm.API.Domain.Entities;

namespace TheFarm.API.Data;

public class AppDbContext : DbContext
{
    // se crea el constructor de la clase AppDbContext heredando de la clase DbContext
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /*se crea la propiedad DbSet para cada entidad*/
    public DbSet<User> Users {get; set;}
    public DbSet<PointSale> PointSales {get; set;}
    public DbSet<Category> Categories {get; set;}
    public DbSet<Product> Products {get; set;}
    public DbSet<Inventory> Inventories {get; set;}
    public DbSet<InventoryTransfer> InventoryTransfers {get; set;}
    public DbSet<InventoryTransferDetail> InventoryTransferDetails {get; set;}
    public DbSet<Resupply> Resupplies {get; set;}
    public DbSet<ResupplyDetail> ResupplyDetails {get; set;}
    public DbSet<Client> Clients {get; set;}
    public DbSet<Sale> Sales {get; set;}
    public DbSet<SaleDetail> SaleDetails {get; set;}
    public DbSet<Bill> Bills {get; set;}
    public DbSet<Commission> Commissions {get; set;}
    public DbSet<CommissionDetail> CommissionDetails {get; set;}

    /*se crea el metodo OnModelCreating para configurar las entidades*/
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("farm");
        // User
        modelBuilder.Entity<User>( e =>
        {
            e.ToTable("User");
            e.HasKey(x => x.IdUser);
            e.Property(x => x.IdUser).ValueGeneratedOnAdd();
        });

        // PointSale
        modelBuilder.Entity<PointSale>(e =>
        {
            e.ToTable("Point_Sale");
            e.HasKey(x => x.IdPointSale);
            e.Property(x => x.IdPointSale).ValueGeneratedOnAdd();

            // referencia de la fk usando la propiedad de nevagacion en la entidad User
            e.HasOne(x => x.User).WithOne(u => u.PointSale).HasForeignKey<PointSale>(x => x.IdUser);
        });

        // Category - product
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("Category");
            e.HasKey(x => x.IdCategory);
            e.Property(x => x.IdCategory).ValueGeneratedOnAdd();
        });

        // Product
        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("Product");
            e.HasKey(x => x.IdProduct);
            e.Property(x => x.IdProduct).ValueGeneratedOnAdd();
            // referencia de la fk usando la propiedad de nevagacion en la entidad Category
            e.HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.IdCategory);
        });

        // ProductDetail
        modelBuilder.Entity<ProductDetail>( e => {
            e.ToTable("Product_Detail");
            e.HasKey(x => x.IdProductDetail);
            e.Property(x => x.IdProductDetail).ValueGeneratedOnAdd();

            // referencia de la fk usando la propiedad de nevagacion en la entidad Product
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.IdProduct);
        });

        // Inventory
        modelBuilder.Entity<Inventory>(e => {
            e.ToTable("Inventory");
            e.HasKey(x => x.IdInventory);
            e.Property(x => x.IdInventory).ValueGeneratedOnAdd();

            // referencia de la fk usando la propiedad de nevagacion en la entidad Product
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.IdProduct);
            e.HasOne(x => x.PointSale).WithMany().HasForeignKey(x => x.IdPointSale);
        });

        // InventoryTransfer (2 FK a PointSale y 2 a User)
        modelBuilder.Entity<InventoryTransfer>(e => {
            e.ToTable("Inventory_Transfer");
            e.HasKey(x => x.IdInventoryTransfer);
            e.Property(x => x.IdInventoryTransfer).ValueGeneratedOnAdd();

            // referencia de la fk usando la propiedad de nevagacion en la entidad PointSale
            e.HasOne(x => x.OriginatingPointSale).WithMany().HasForeignKey(x => x.IdOriginatingPointSale).OnDelete(DeleteBehavior.Restrict); // no se puede eliminar el registro principal porque estos hijos depdenden de el 
            e.HasOne(x => x.DestinationPointSale).WithMany().HasForeignKey(x => x.IdDestinationPointSale).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RequestingUser).WithMany().HasForeignKey(x => x.IdRequestingUser).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ApprovingUser).WithMany().HasForeignKey(x => x.IdApprovingUser).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryTransferDetail>(e =>{
            e.ToTable("Inventory_Transfer_Detail");
            e.HasKey(x => x.IdInventoryTransferDetail);
            e.Property(x => x.IdInventoryTransferDetail).ValueGeneratedOnAdd();

            e.HasOne(x => x.InventoryTransfer).WithMany(i => i.InventoryTransferDetails).HasForeignKey(x => x.IdInventoryTransfer);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.IdProduct);
        });

        // Resupply
        modelBuilder.Entity<Resupply>(e => {
            e.ToTable("Resupply");
            e.HasKey(x => x.IdResupply);
            e.Property(x => x.IdResupply).ValueGeneratedOnAdd();

            e.HasOne(x => x.PointSale).WithMany().HasForeignKey(x => x.IdPointSale);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.IdUser);
        });

        modelBuilder.Entity<ResupplyDetail>(e => {
            e.ToTable("Resupply_Detail");
            e.HasKey(x => x.IdResupplyDetail);
            e.Property(x => x.IdResupplyDetail).ValueGeneratedOnAdd();

            e.HasOne(x => x.Resupply).WithMany(r => r.ResupplyDetails).HasForeignKey(x => x.IdResupply);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.IdProduct);
        });

        // Client / Sale / Bill
        modelBuilder.Entity<Client>(e => {
            e.ToTable("Client");
            e.HasKey(x => x.IdClient);
            e.Property(x => x.IdClient).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Sale>( e => {
            e.ToTable("Sale");
            e.HasKey(x => x.IdSale);
            e.Property(x => x.IdSale).ValueGeneratedOnAdd();

            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.IdUser);
            e.HasOne(x => x.PointSale).WithMany().HasForeignKey(x => x.IdPointSale);
        });

        modelBuilder.Entity<SaleDetail>(e => {
            e.ToTable("Sale_Detail");
            e.HasKey(x => x.IdSaleDetail);
            e.Property(x => x.IdSaleDetail).ValueGeneratedOnAdd();

            e.HasOne(x => x.Sale).WithMany(s => s.SaleDetails).HasForeignKey(x => x.IdSale);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.IdProduct);
        });
        
        // Bill
        modelBuilder.Entity<Bill>(e => {
            e.ToTable("Bill");
            e.HasKey(x => x.IdBill);
            e.Property(x => x.IdBill).ValueGeneratedOnAdd();

            e.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.IdClient);
            e.HasOne(x => x.Sale).WithOne(s => s.Bill).HasForeignKey<Bill>(x => x.IdSale);
        });

        // Commission
        modelBuilder.Entity<Commission>(e => {
            e.ToTable("Commission");
            e.HasKey(x => x.IdCommission);
            e.Property(x => x.IdCommission).ValueGeneratedOnAdd();

            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.IdUser);
        });

        modelBuilder.Entity<CommissionDetail>(e => {
            e.ToTable("Commission_Detail");
            e.HasKey(x => x.IdCommissionDetail);
            e.HasOne(x => x.Commission).WithMany(c => c.CommissionDetails).HasForeignKey(x => x.IdCommission);
            e.HasOne(x => x.Sale).WithMany().HasForeignKey(x => x.IdSale);
        });

        base.OnModelCreating(modelBuilder);
    }
}