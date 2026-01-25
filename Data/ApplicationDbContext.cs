using Microsoft.EntityFrameworkCore;
using MyPokemoApi.Models.Entities;
using MyPokemoApi.Models.Enums;
using System.Text.Json;

namespace MyPokemoApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Pokemon> Pokemons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Language).HasConversion<string>();
            
            // Configure CaughtPokemonIds as JSON column
            entity.Property(e => e.CaughtPokemonIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<ICollection<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>())
                .HasColumnType("jsonb")
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<ICollection<int>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure Pokemon entity
        modelBuilder.Entity<Pokemon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Order).HasMaxLength(50);
            entity.Property(e => e.Height).HasMaxLength(50);
            entity.Property(e => e.Weight).HasMaxLength(50);
            
            // Configure Sprites as owned entity (stored as JSON)
            entity.OwnsOne(e => e.Sprites, sprites =>
            {
                sprites.Property(s => s.FrontDefault).HasMaxLength(500);
                sprites.Property(s => s.FrontShiny).HasMaxLength(500);
            });
        });
    }
}