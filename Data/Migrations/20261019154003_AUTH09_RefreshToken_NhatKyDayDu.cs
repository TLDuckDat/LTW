using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AUTH09_RefreshToken_NhatKyDayDu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NhatKyHoatDong_TaiKhoanId",
                table: "NhatKyHoatDong");

            migrationBuilder.DropIndex(
                name: "IX_NhatKyHoatDong_TenDangNhap",
                table: "NhatKyHoatDong");

            migrationBuilder.AlterColumn<string>(
                name: "HanhDong",
                table: "NhatKyHoatDong",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "ChiTiet",
                table: "NhatKyHoatDong",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuongDan",
                table: "NhatKyHoatDong",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaPhanHoi",
                table: "NhatKyHoatDong",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhuongThuc",
                table: "NhatKyHoatDong",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaiKhoanId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nchar(64)", fixedLength: true, maxLength: 64, nullable: false),
                    HetHan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThuHoiLuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LyDoThuHoi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DiaChiIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_TaiKhoan_TaiKhoanId",
                        column: x => x.TaiKhoanId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHoatDong_TaiKhoanId_CreatedAt",
                table: "NhatKyHoatDong",
                columns: new[] { "TaiKhoanId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHoatDong_TenDangNhap_HanhDong_CreatedAt",
                table: "NhatKyHoatDong",
                columns: new[] { "TenDangNhap", "HanhDong", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_TaiKhoanId_HetHan",
                table: "RefreshToken",
                columns: new[] { "TaiKhoanId", "HetHan" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_TokenHash",
                table: "RefreshToken",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropIndex(
                name: "IX_NhatKyHoatDong_TaiKhoanId_CreatedAt",
                table: "NhatKyHoatDong");

            migrationBuilder.DropIndex(
                name: "IX_NhatKyHoatDong_TenDangNhap_HanhDong_CreatedAt",
                table: "NhatKyHoatDong");

            migrationBuilder.DropColumn(
                name: "ChiTiet",
                table: "NhatKyHoatDong");

            migrationBuilder.DropColumn(
                name: "DuongDan",
                table: "NhatKyHoatDong");

            migrationBuilder.DropColumn(
                name: "MaPhanHoi",
                table: "NhatKyHoatDong");

            migrationBuilder.DropColumn(
                name: "PhuongThuc",
                table: "NhatKyHoatDong");

            migrationBuilder.AlterColumn<string>(
                name: "HanhDong",
                table: "NhatKyHoatDong",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHoatDong_TaiKhoanId",
                table: "NhatKyHoatDong",
                column: "TaiKhoanId");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHoatDong_TenDangNhap",
                table: "NhatKyHoatDong",
                column: "TenDangNhap");
        }
    }
}
