using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using courseProject.Models;

namespace courseProject.Data;

public partial class AppDbContext : DbContext
{
    private readonly string? _connectionString;

    public AppDbContext()
    {
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        _connectionString = configuration.GetConnectionString("Default");
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            if (!string.IsNullOrEmpty(_connectionString))
            {
                optionsBuilder.UseMySQL(_connectionString);
            }
        }
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceCategory> ServiceCategories { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<RequestEntity> RequestEntities { get; set; }
    public virtual DbSet<RequestItemEntity> RequestItemEntities { get; set; }
    public virtual DbSet<StatusEntity> RequestStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("PRIMARY");

            entity.ToTable("clients");

            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(100)
                .HasColumnName("contact_person");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.TaxId)
                .HasMaxLength(20)
                .HasColumnName("tax_id");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PRIMARY");

            entity.ToTable("contracts");

            entity.HasIndex(e => e.ClientId, "client_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.ContractId).HasColumnName("contract_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Client).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("contracts_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("contracts_ibfk_2");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServicesId).HasName("PRIMARY");

            entity.ToTable("services");

            entity.HasIndex(e => e.CategoryId, "category_id");

            entity.Property(e => e.ServicesId).HasColumnName("services_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Price)
                .HasPrecision(10)
                .HasColumnName("price");
            entity.Property(e => e.ServicesTitle)
                .HasMaxLength(100)
                .HasColumnName("services_title");

            entity.HasOne(d => d.Category).WithMany(p => p.Services)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("services_ibfk_1");
        });

        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("service_categories");

            entity.HasIndex(e => e.CategoryTitle, "category_title").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryTitle)
                .HasMaxLength(100)
                .HasColumnName("category_title");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.AvatarPath)
                .HasMaxLength(255)
                .HasColumnName("avatar_path");
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'accountant'")
                .HasColumnType("enum('admin','accountant','assistant')")
                .HasColumnName("role");
        });

        modelBuilder.Entity<RequestEntity>(entity =>
        {
            entity.HasKey(e => e.RequestEntityId);
            entity.ToTable("RequestEntities");
            entity.Property(e => e.RequestEntityId).HasColumnName("RequestEntityId");
            entity.Property(e => e.CustomerName).IsRequired();
            entity.Property(e => e.CustomerEmail).IsRequired();
            entity.Property(e => e.CustomerPhone);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.AssignedTo);
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<RequestItemEntity>(entity =>
        {
            entity.HasKey(e => e.RequestItemEntityId);
            entity.ToTable("RequestItemEntities");
            entity.Property(e => e.RequestItemEntityId).HasColumnName("RequestItemEntityId");
            entity.Property(e => e.Title);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.RequestEntityId);
            entity.HasOne(e => e.Request).WithMany(r => r.Items).HasForeignKey(e => e.RequestEntityId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StatusEntity>(entity =>
        {
            entity.HasKey(e => e.StatusEntityId);
            entity.ToTable("request_statuses");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
