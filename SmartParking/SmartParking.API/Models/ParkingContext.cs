using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SmartParking.API.Models;

public partial class ParkingContext : DbContext
{
    public ParkingContext()
    {
    }

    public ParkingContext(DbContextOptions<ParkingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Lugar> Lugares { get; set; }
    public virtual DbSet<Parque> Parques { get; set; }
    public virtual DbSet<Sensor> Sensores { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configuração feita no Program.cs para evitar erros de ConnectionString
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lugar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Lugares__3214EC07CFEAEF07");
            entity.Property(e => e.NumeroLugar).HasMaxLength(10);

            // ALTERADO: Cascade permite apagar Parque e Lugares em conjunto
            entity.HasOne(d => d.Parque).WithMany(p => p.Lugares)
                .HasForeignKey(d => d.ParqueId)
                .OnDelete(DeleteBehavior.Cascade) 
                .HasConstraintName("FK_Lugares_Parques");
        });

        modelBuilder.Entity<Parque>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Parques__3214EC0786113281");
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Localizacao).HasMaxLength(100);
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Nome).HasMaxLength(100);
        });

        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sensores__3214EC07A6A98226");
            entity.HasIndex(e => e.LugarId, "UQ__Sensores__1BDE0DE16490C0D1").IsUnique();
            entity.Property(e => e.Tipo).HasMaxLength(50).HasDefaultValue("Presenca");
            entity.Property(e => e.UltimaAtualizacao).HasDefaultValueSql("(getdate())").HasColumnType("datetime");

            // ALTERADO: Cascade permite apagar Lugar e Sensor em conjunto
            entity.HasOne(d => d.Lugar).WithOne(p => p.Sensor)
                .HasForeignKey<Sensor>(d => d.LugarId)
                .OnDelete(DeleteBehavior.Cascade) 
                .HasConstraintName("FK_Sensores_Lugares");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}