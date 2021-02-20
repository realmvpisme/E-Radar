using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace E_Radar.Data.Migrations
{
    public partial class AddSentMessagesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SentMessages",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    SentTime = table.Column<DateTime>(nullable: false),
                    UniqueId = table.Column<string>(nullable: false),
                    From = table.Column<string>(nullable: false),
                    To = table.Column<string>(nullable: false),
                    Subject = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentMessages", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SentMessages");
        }
    }
}
