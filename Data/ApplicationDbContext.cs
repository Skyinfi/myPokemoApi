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
    public DbSet<UserPokemon> UserPokemons { get; set; }

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

        // Configure UserPokemon entity
        modelBuilder.Entity<UserPokemon>(entity =>
        {
            // 复合主键
            entity.HasKey(up => new { up.UserId, up.PokemonId });
            
            // 外键关系
            entity.HasOne(up => up.User)
                  .WithMany(u => u.UserPokemons)
                  .HasForeignKey(up => up.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(up => up.Pokemon)
                  .WithMany(p => p.UserPokemons)
                  .HasForeignKey(up => up.PokemonId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // 基本属性配置
            entity.Property(up => up.CaughtAt).IsRequired();
            entity.Property(up => up.Nickname).HasMaxLength(50);
            entity.Property(up => up.IsFavorite).HasDefaultValue(false);
            
            // 游戏机制属性配置
            entity.Property(up => up.Level).HasDefaultValue(1);
            entity.Property(up => up.Experience).HasDefaultValue(0);
            entity.Property(up => up.ExperienceToNextLevel).HasDefaultValue(100);
            entity.Property(up => up.Health).HasDefaultValue(100);
            entity.Property(up => up.MaxHealth).HasDefaultValue(100);
            entity.Property(up => up.BattlesWon).HasDefaultValue(0);
            entity.Property(up => up.BattlesLost).HasDefaultValue(0);
            
            // 索引
            entity.HasIndex(up => up.UserId);
            entity.HasIndex(up => up.PokemonId);
            entity.HasIndex(up => up.CaughtAt);
            entity.HasIndex(up => up.Level);
            entity.HasIndex(up => up.LastBattleAt);
        });
    }
}