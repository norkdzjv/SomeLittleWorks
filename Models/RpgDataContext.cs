﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TEXT_RPG.Models
{
    public partial class RpgDataContext : DbContext
    {
        public RpgDataContext()
        {
        }

        public RpgDataContext(DbContextOptions<RpgDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CharacterData> CharacterData { get; set; }
        public virtual DbSet<ExpForm> ExpForm { get; set; }
        public virtual DbSet<JobData> JobData { get; set; }
        public virtual DbSet<MonsterData> MonsterData { get; set; }
        public virtual DbSet<SkillData> SkillData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CharacterData>(entity =>
            {
                entity.Property(e => e.Job).HasMaxLength(200);

                entity.Property(e => e.Name).HasMaxLength(200);
            });

            modelBuilder.Entity<JobData>(entity =>
            {
                entity.Property(e => e.JobName).HasMaxLength(200);
            });

            modelBuilder.Entity<MonsterData>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200);
            });

            modelBuilder.Entity<SkillData>(entity =>
            {
                entity.Property(e => e.SkillClass).HasMaxLength(200);

                entity.Property(e => e.SkillName).HasMaxLength(200);

                entity.Property(e => e.SkillType).HasMaxLength(200);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}