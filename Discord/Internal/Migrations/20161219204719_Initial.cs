using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ContactsBot.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ContactsBotSchema");

            migrationBuilder.CreateTable(
                name: "Karmas",
                schema: "ContactsBotSchema",
                columns: table => new
                {
                    UserID = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    KarmaCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Karmas", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                schema: "ContactsBotSchema",
                columns: table => new
                {
                    LogID = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Exception = table.Column<string>(nullable: true),
                    Level = table.Column<string>(maxLength: 5, nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.LogID);
                });

            migrationBuilder.CreateTable(
                name: "Memos",
                schema: "ContactsBotSchema",
                columns: table => new
                {
                    MemoName = table.Column<string>(maxLength: 50, nullable: false),
                    Message = table.Column<string>(maxLength: 500, nullable: true),
                    UserID = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memos", x => x.MemoName);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Karmas",
                schema: "ContactsBotSchema");

            migrationBuilder.DropTable(
                name: "Logs",
                schema: "ContactsBotSchema");

            migrationBuilder.DropTable(
                name: "Memos",
                schema: "ContactsBotSchema");
        }
    }
}
