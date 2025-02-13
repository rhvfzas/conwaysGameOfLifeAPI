﻿// <auto-generated />
using ConwaysGameOfLifeApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ConwaysGameOfLifeApi.Migrations
{
    [DbContext(typeof(GameOfLifeContext))]
    partial class GameOfLifeContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.15")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ConwaysGameOfLifeApi.Models.Board", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("GameBoardData")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("GameId")
                        .HasColumnType("integer");

                    b.Property<int>("StateNumber")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GameId", "StateNumber");

                    b.ToTable("Boards");
                });

            modelBuilder.Entity("ConwaysGameOfLifeApi.Models.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("ConwaysGameOfLifeApi.Models.Board", b =>
                {
                    b.HasOne("ConwaysGameOfLifeApi.Models.Game", "Game")
                        .WithMany("Boards")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");
                });

            modelBuilder.Entity("ConwaysGameOfLifeApi.Models.Game", b =>
                {
                    b.Navigation("Boards");
                });
#pragma warning restore 612, 618
        }
    }
}
