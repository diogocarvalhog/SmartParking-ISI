// -----------------------------------------------------------------------------
// Projeto: SmartParking
// Unidade Curricular: ISI (IPCA)
// Autor: Diogo Graça
// Ficheiro: ParkingContext.cs
// Descrição: DbContext EF Core. Define DbSets e configurações de mapeamento.
// Notas:
//  - A connection string/configuração é feita externamente (Program.cs).
//  - Relações configuradas com DeleteBehavior.Cascade para suportar remoções.
// -----------------------------------------------------------------------------

﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SmartParking.API.Models;

#region DbContext: ParkingContext

/// <summary>
/// Contexto EF Core para acesso ao repositório de dados do SmartParking.
/// </summary>
public partial class ParkingContext : DbContext
{
    #region Construtores

    /// <summary>
    /// Construtor vazio (compatibilidade com tooling e cenários de scaffold).
    /// </summary>
    public ParkingContext()
    {
    }

    /// <summary>
    /// Construtor com opções, utilizado pela injeção de dependência.
    /// </summary>
    /// <param name="options">Opções do DbContext.</param>
    public ParkingContext(DbContextOptions<ParkingContext> options)
        : base(options)
    {
    }

    #endregion

    #region DbSets

    /// <summary>
    /// DbSet de Lugares.
    /// </summary>
    public virtual DbSet<Lugar> Lugares { get; set; }

    /// <summary>
    /// DbSet de Parques.
    /// </summary>
    public virtual DbSet<Parque> Parques { get; set; }

    /// <summary>
    /// DbSet de Sensores.
    /// </summary>
    public virtual DbSet<Sensor> Sensores { get; set; }

    /// <summary>
    /// DbSet de Utilizadores (autenticação).
    /// </summary>
    public virtual DbSet<User> Users { get; set; }

    #endregion

    #region Configuração EF Core

    /// <summary>
    /// Configuração do DbContext.
    /// Mantida sem implementação porque a configuração principal é feita no Program.cs.
    /// </summary>
    /// <param name="optionsBuilder">Builder de opções.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configuração feita no Program.cs para evitar erros de ConnectionString
    }

    /// <summary>
    /// Mapeamentos e relacionamentos das entidades.
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder do EF Core.</param>
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

    #endregion

    #region Extensibilidade

    /// <summary>
    /// Extensão parcial para cenários de scaffold/customização.
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder.</param>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    #endregion
}

#endregion