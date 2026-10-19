using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class STU01_07_StudentModule_Enhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CCCD",
                table: "SinhVien",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChuyenNganhId",
                table: "SinhVien",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiaChi",
                table: "SinhVien",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GioiTinh",
                table: "SinhVien",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LichSuHocTap",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    TrangThaiCu = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TrangThaiMoi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SoQuyetDinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayHieuLuc = table.Column<DateOnly>(type: "date", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuHocTap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuHocTap_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_CCCD",
                table: "SinhVien",
                column: "CCCD");

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_ChuyenNganhId",
                table: "SinhVien",
                column: "ChuyenNganhId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuHocTap_SinhVienId",
                table: "LichSuHocTap",
                column: "SinhVienId");

            migrationBuilder.AddForeignKey(
                name: "FK_SinhVien_ChuyenNganh_ChuyenNganhId",
                table: "SinhVien",
                column: "ChuyenNganhId",
                principalTable: "ChuyenNganh",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SinhVien_ChuyenNganh_ChuyenNganhId",
                table: "SinhVien");

            migrationBuilder.DropTable(
                name: "LichSuHocTap");

            migrationBuilder.DropIndex(
                name: "IX_SinhVien_CCCD",
                table: "SinhVien");

            migrationBuilder.DropIndex(
                name: "IX_SinhVien_ChuyenNganhId",
                table: "SinhVien");

            migrationBuilder.DropColumn(
                name: "CCCD",
                table: "SinhVien");

            migrationBuilder.DropColumn(
                name: "ChuyenNganhId",
                table: "SinhVien");

            migrationBuilder.DropColumn(
                name: "DiaChi",
                table: "SinhVien");

            migrationBuilder.DropColumn(
                name: "GioiTinh",
                table: "SinhVien");
        }
    }
}
