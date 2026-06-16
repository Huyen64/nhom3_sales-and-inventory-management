using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nhom3.Infrastructure.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Nhom3.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260616190000_AddCustomerMembershipAndAttendance")]
public class AddCustomerMembershipAndAttendance : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "PaidOrderCount",
            table: "Users",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "CustomerTier",
            table: "Users",
            type: "text",
            nullable: false,
            defaultValue: "Regular");

        migrationBuilder.AddColumn<string>(
            name: "WorkStatus",
            table: "Users",
            type: "text",
            nullable: false,
            defaultValue: "Active");

        migrationBuilder.CreateTable(
            name: "AttendanceRecords",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<int>(type: "integer", nullable: false),
                WorkDate = table.Column<DateTime>(type: "date", nullable: false),
                CheckIn = table.Column<TimeSpan>(type: "interval", nullable: true),
                CheckOut = table.Column<TimeSpan>(type: "interval", nullable: true),
                Status = table.Column<string>(type: "text", nullable: false),
                HoursWorked = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                Note = table.Column<string>(type: "text", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                table.ForeignKey(
                    name: "FK_AttendanceRecords_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AttendanceRecords_UserId_WorkDate",
            table: "AttendanceRecords",
            columns: new[] { "UserId", "WorkDate" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AttendanceRecords");
        migrationBuilder.DropColumn(name: "CustomerTier", table: "Users");
        migrationBuilder.DropColumn(name: "PaidOrderCount", table: "Users");
        migrationBuilder.DropColumn(name: "WorkStatus", table: "Users");
    }
}
