using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ContactsBot.Data;

namespace ContactsBot.Migrations
{
    [DbContext(typeof(ContactsBotDbContext))]
    partial class ContactsBotDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("ContactsBot.Data.Karma", b =>
                {
                    b.Property<long>("UserID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("KarmaCount");

                    b.HasKey("UserID");

                    b.ToTable("Karmas","ContactsBotSchema");
                });

            modelBuilder.Entity("ContactsBot.Data.Log", b =>
                {
                    b.Property<long>("LogID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Exception");

                    b.Property<string>("Level")
                        .IsRequired()
                        .HasMaxLength(5);

                    b.Property<string>("Message");

                    b.Property<DateTime>("Timestamp");

                    b.HasKey("LogID");

                    b.ToTable("Logs","ContactsBotSchema");
                });

            modelBuilder.Entity("ContactsBot.Data.Memo", b =>
                {
                    b.Property<string>("MemoName")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50);

                    b.Property<string>("Message")
                        .HasMaxLength(500);

                    b.Property<long>("UserID");

                    b.HasKey("MemoName");

                    b.ToTable("Memos","ContactsBotSchema");
                });
        }
    }
}
