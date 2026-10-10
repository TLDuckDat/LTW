using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class Auth06ActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NhatKyHoatDong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaiKhoanId = table.Column<int>(type: "int", nullable: true),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HanhDong = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ThanhCong = table.Column<bool>(type: "bit", nullable: false),
                    DiaChiIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyHoatDong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NhatKyHoatDong_TaiKhoan_TaiKhoanId",
                        column: x => x.TaiKhoanId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHoatDong_CreatedAt",
                table: "NhatKyHoatDong",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHoatDong_TaiKhoanId",
                table: "NhatKyHoatDong",
                column: "TaiKhoanId");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHoatDong_TenDangNhap",
                table: "NhatKyHoatDong",
                column: "TenDangNhap");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NhatKyHoatDong");
        }
    }
}
