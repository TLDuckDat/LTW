using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class SA03_ThanhToanHocPhiNhieuLan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MienGiam_SinhVienId",
                table: "MienGiam");

            migrationBuilder.DropIndex(
                name: "IX_HocBong_SinhVienId",
                table: "HocBong");

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienDaNop",
                table: "HocPhi",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ThanhToanHocPhi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HocPhiId = table.Column<int>(type: "int", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HinhThuc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NguoiThuId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToanHocPhi", x => x.Id);
                    table.CheckConstraint("CK_ThanhToanHocPhi_SoTien", "[SoTien] > 0");
                    table.ForeignKey(
                        name: "FK_ThanhToanHocPhi_HocPhi_HocPhiId",
                        column: x => x.HocPhiId,
                        principalTable: "HocPhi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhToanHocPhi_TaiKhoan_NguoiThuId",
                        column: x => x.NguoiThuId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MienGiam_SinhVienId_HocKyId",
                table: "MienGiam",
                columns: new[] { "SinhVienId", "HocKyId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MienGiam_TyLe",
                table: "MienGiam",
                sql: "[TyLe] BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HocPhi_SoTien",
                table: "HocPhi",
                sql: "[SoTien] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HocPhi_SoTienDaNop",
                table: "HocPhi",
                sql: "[SoTienDaNop] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_HocBong_SinhVienId_HocKyId_Ten",
                table: "HocBong",
                columns: new[] { "SinhVienId", "HocKyId", "Ten" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_HocBong_SoTien",
                table: "HocBong",
                sql: "[SoTien] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DiemRenLuyen_Diem",
                table: "DiemRenLuyen",
                sql: "[Diem] BETWEEN 0 AND 100");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToanHocPhi_HocPhiId",
                table: "ThanhToanHocPhi",
                column: "HocPhiId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToanHocPhi_NguoiThuId",
                table: "ThanhToanHocPhi",
                column: "NguoiThuId");

            // Chuyển dữ liệu cũ: khoản đã đánh dấu thanh toán coi như đã nộp đủ, kèm một dòng lịch sử
            migrationBuilder.Sql(
                "UPDATE [HocPhi] SET [SoTienDaNop] = [SoTien] WHERE [DaThanhToan] = 1;");
            migrationBuilder.Sql(
                "INSERT INTO [ThanhToanHocPhi] ([HocPhiId], [SoTien], [NgayThanhToan], [GhiChu], [CreatedAt]) " +
                "SELECT [Id], [SoTien], COALESCE([UpdatedAt], [CreatedAt]), N'Chuyển từ dữ liệu trước khi hỗ trợ nộp nhiều lần', SYSUTCDATETIME() " +
                "FROM [HocPhi] WHERE [DaThanhToan] = 1 AND [SoTien] > 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThanhToanHocPhi");

            migrationBuilder.DropIndex(
                name: "IX_MienGiam_SinhVienId_HocKyId",
                table: "MienGiam");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MienGiam_TyLe",
                table: "MienGiam");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HocPhi_SoTien",
                table: "HocPhi");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HocPhi_SoTienDaNop",
                table: "HocPhi");

            migrationBuilder.DropIndex(
                name: "IX_HocBong_SinhVienId_HocKyId_Ten",
                table: "HocBong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_HocBong_SoTien",
                table: "HocBong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DiemRenLuyen_Diem",
                table: "DiemRenLuyen");

            migrationBuilder.DropColumn(
                name: "SoTienDaNop",
                table: "HocPhi");

            migrationBuilder.CreateIndex(
                name: "IX_MienGiam_SinhVienId",
                table: "MienGiam",
                column: "SinhVienId");

            migrationBuilder.CreateIndex(
                name: "IX_HocBong_SinhVienId",
                table: "HocBong",
                column: "SinhVienId");
        }
    }
}
