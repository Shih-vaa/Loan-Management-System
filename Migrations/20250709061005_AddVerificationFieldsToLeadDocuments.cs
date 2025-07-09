using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationFieldsToLeadDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "remarks",
                table: "lead_documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "verified_at",
                table: "lead_documents",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "verified_by",
                table: "lead_documents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_lead_documents_verified_by",
                table: "lead_documents",
                column: "verified_by");

            migrationBuilder.AddForeignKey(
                name: "FK_lead_documents_Users_verified_by",
                table: "lead_documents",
                column: "verified_by",
                principalTable: "Users",
                principalColumn: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lead_documents_Users_verified_by",
                table: "lead_documents");

            migrationBuilder.DropIndex(
                name: "IX_lead_documents_verified_by",
                table: "lead_documents");

            migrationBuilder.DropColumn(
                name: "remarks",
                table: "lead_documents");

            migrationBuilder.DropColumn(
                name: "verified_at",
                table: "lead_documents");

            migrationBuilder.DropColumn(
                name: "verified_by",
                table: "lead_documents");
        }
    }
}
