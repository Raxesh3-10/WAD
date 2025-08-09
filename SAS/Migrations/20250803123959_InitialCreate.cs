using Microsoft.EntityFrameworkCore.Migrations;

namespace SAS.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentName = table.Column<string>(nullable: false),
                    FatherName = table.Column<string>(nullable: false),
                    MotherName = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    AadharNo = table.Column<long>(nullable: false),
                    RollNo = table.Column<int>(nullable: false),
                    Div = table.Column<string>(nullable: false),
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Password = table.Column<string>(nullable: false),
                    Role = table.Column<int>(nullable: false),
                    Subject = table.Column<string>(nullable: true),
                    Std = table.Column<int>(nullable: true),
                    Div = table.Column<string>(nullable: true),
                    Salary = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
