using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SAS.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreviousStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    StudentName = table.Column<string>(nullable: true),
                    PhotoUrl = table.Column<string>(nullable: true),
                    FatherName = table.Column<string>(nullable: true),
                    MotherName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    AadharNo = table.Column<long>(nullable: false),
                    RollNo = table.Column<int>(nullable: false),
                    Div = table.Column<string>(nullable: true),
                    Std = table.Column<int>(nullable: false),
                    PhoneNo = table.Column<long>(nullable: false),
                    PassingYear = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreviousStudents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    StudentName = table.Column<string>(nullable: true),
                    PhotoUrl = table.Column<string>(nullable: true),
                    FatherName = table.Column<string>(nullable: true),
                    MotherName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    AadharNo = table.Column<long>(nullable: false),
                    RollNo = table.Column<int>(nullable: false),
                    Div = table.Column<string>(nullable: true),
                    Std = table.Column<int>(nullable: false),
                    PhoneNo = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    Role = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notices",
                columns: table => new
                {
                    NoticeId = table.Column<Guid>(nullable: false),
                    Subject = table.Column<string>(maxLength: 200, nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Documents = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notices", x => x.NoticeId);
                    table.ForeignKey(
                        name: "FK_Notices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Subjects = table.Column<string>(nullable: true),
                    Stds = table.Column<string>(nullable: true),
                    Qualifications = table.Column<string>(nullable: true),
                    Documents = table.Column<string>(nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Dob = table.Column<DateTime>(nullable: false),
                    Photo = table.Column<string>(nullable: true),
                    Experience = table.Column<int>(nullable: false),
                    JoiningDate = table.Column<DateTime>(nullable: false),
                    Address = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDetails_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notices_UserId",
                table: "Notices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDetails_UserId",
                table: "UserDetails",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notices");

            migrationBuilder.DropTable(
                name: "PreviousStudents");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "UserDetails");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
