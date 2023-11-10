﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SafePath.EntityFrameworkCore.FastStorage;
using Volo.Abp.EntityFrameworkCore;

#nullable disable

namespace SafePath.Migrations.FastStorage
{
    [DbContext(typeof(SqliteDbContext))]
    [Migration("20231109224740_IndexesAdded")]
    partial class IndexesAdded
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("_Abp_DatabaseProvider", EfCoreDatabaseProvider.SqlServer)
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SafePath.Entities.FastStorage.MapElement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long?>("EdgeId")
                        .HasColumnType("bigint");

                    b.Property<string>("ItineroMappingError")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<double>("Lat")
                        .HasColumnType("float");

                    b.Property<double>("Lng")
                        .HasColumnType("float");

                    b.Property<long?>("OSMNodeId")
                        .HasColumnType("bigint");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<long?>("VertexId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("Lat", "Lng");

                    b.ToTable("MapElement", (string)null);
                });

            modelBuilder.Entity("SafePath.Entities.FastStorage.SafetyScoreElement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long>("EdgeId")
                        .HasColumnType("bigint");

                    b.Property<float>("Score")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.HasIndex("EdgeId")
                        .IsUnique();

                    b.ToTable("SafetyScoreElement", (string)null);
                });

            modelBuilder.Entity("SafePath.Entities.FastStorage.SafetyScoreElementMapElement", b =>
                {
                    b.Property<int>("SafetyScoreElementId")
                        .HasColumnType("int");

                    b.Property<int>("MapElementId")
                        .HasColumnType("int");

                    b.HasKey("SafetyScoreElementId", "MapElementId");

                    b.HasIndex("MapElementId");

                    b.ToTable("SafetyScoreElementMapElement");
                });

            modelBuilder.Entity("SafePath.Entities.FastStorage.SafetyScoreElementMapElement", b =>
                {
                    b.HasOne("SafePath.Entities.FastStorage.MapElement", "MapElement")
                        .WithMany()
                        .HasForeignKey("MapElementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SafePath.Entities.FastStorage.SafetyScoreElement", "SafetyScoreElement")
                        .WithMany()
                        .HasForeignKey("SafetyScoreElementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MapElement");

                    b.Navigation("SafetyScoreElement");
                });
#pragma warning restore 612, 618
        }
    }
}
