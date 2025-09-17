using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SAS.Migrations
{
    public partial class RemoveUserInBill : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_Users_VendorId",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Bills_VendorId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "Bills");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Students",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "PreviousStudents",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Bills",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorEmail",
                table: "Bills",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorName",
                table: "Bills",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "Students",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bills_UserId",
                table: "Bills",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_Users_UserId",
                table: "Bills",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_Users_UserId",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Students_Email",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Bills_UserId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "VendorEmail",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "VendorName",
                table: "Bills");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "PreviousStudents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "Bills",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Bills_VendorId",
                table: "Bills",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_Users_VendorId",
                table: "Bills",
                column: "VendorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
