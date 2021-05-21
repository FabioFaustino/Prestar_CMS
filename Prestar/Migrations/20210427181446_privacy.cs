using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Prestar.Migrations
{
    public partial class privacy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrivacyPolicySection",
                columns: table => new
                {
                    PrivacyPolicySectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivacyPolicyID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivacyPolicySection", x => x.PrivacyPolicySectionID);
                });

            migrationBuilder.DropColumn(
                name: "PrivacyPolicyID",
                table: "PrivacyPolicySection");

            migrationBuilder.AddColumn<string>(
                name: "LastUpdateUserID",
                table: "PrivacyPolicySection",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrivacyPolicySectionLastUpdate",
                table: "PrivacyPolicySection",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyPolicySection_LastUpdateUserID",
                table: "PrivacyPolicySection",
                column: "LastUpdateUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_PrivacyPolicySection_AspNetUsers_LastUpdateUserID",
                table: "PrivacyPolicySection",
                column: "LastUpdateUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "LastUpdateUserID",
                table: "PrivacyPolicySection");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicySectionLastUpdate",
                table: "PrivacyPolicySection");

            migrationBuilder.AddColumn<int>(
                name: "PrivacyPolicyID",
                table: "PrivacyPolicySection",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PrivacyPolicy",
                columns: table => new
                {
                    PrivacyPolicyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastUpdateUserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PrivacyPolicyLastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivacyPolicy", x => x.PrivacyPolicyID);
                    table.ForeignKey(
                        name: "FK_PrivacyPolicy_AspNetUsers_LastUpdateUserID",
                        column: x => x.LastUpdateUserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyPolicySection_PrivacyPolicyID",
                table: "PrivacyPolicySection",
                column: "PrivacyPolicyID");

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyPolicy_LastUpdateUserID",
                table: "PrivacyPolicy",
                column: "LastUpdateUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_PrivacyPolicySection_PrivacyPolicy_PrivacyPolicyID",
                table: "PrivacyPolicySection",
                column: "PrivacyPolicyID",
                principalTable: "PrivacyPolicy",
                principalColumn: "PrivacyPolicyID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
