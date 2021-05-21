using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Prestar.Migrations
{
    public partial class ServiceImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Norm");

            migrationBuilder.CreateTable(
                name: "ServiceImage",
                columns: table => new
                {
                    ServiceImageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    Image = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceImage", x => x.ServiceImageID);
                    table.ForeignKey(
                        name: "FK_ServiceImage_Service_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Service",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceImage_ServiceID",
                table: "ServiceImage",
                column: "ServiceID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceImage");

            migrationBuilder.CreateTable(
                name: "Norm",
                columns: table => new
                {
                    NormID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WritterID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Norm", x => x.NormID);
                    table.ForeignKey(
                        name: "FK_Norm_AspNetUsers_WritterID",
                        column: x => x.WritterID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Norm_WritterID",
                table: "Norm",
                column: "WritterID");
        }
    }
}
