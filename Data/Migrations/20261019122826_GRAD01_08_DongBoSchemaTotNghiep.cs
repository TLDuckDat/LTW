using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Data.Migrations
{
    /// <summary>
    /// Đưa vào CSDL các thay đổi schema của module Graduation (commit 8112664) và điều kiện tốt nghiệp
    /// của ChuongTrinhDaoTao. Commit đó chỉ sửa ModelSnapshot mà không tạo migration nên CSDL thiếu các
    /// bảng/cột này. Các thao tác của STU01_07 và EDU01_06 (đã có trong CSDL) được lược bỏ khỏi Up/Down.
    /// Lưu ý: ThucTap.CoSoThucTapId là cột bắt buộc mới; CSDL nào đã có dòng ThucTap cần gán cơ sở trước.
    /// </summary>
    public partial class GRAD01_08_DongBoSchemaTotNghiep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ThucTap_SinhVienId",
                table: "ThucTap");

            migrationBuilder.DropIndex(
                name: "IX_DeTaiKhoaLuan_SinhVienId",
                table: "DeTaiKhoaLuan");

            migrationBuilder.AlterColumn<int>(
                name: "TongTinChiTichLuy",
                table: "XetTotNghiep",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiemTrungBinhTichLuy",
                table: "XetTotNghiep",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,2)",
                oldPrecision: 4,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "LyDo",
                table: "XetTotNghiep",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DonViThucTap",
                table: "ThucTap",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "CoSoThucTapId",
                table: "ThucTap",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayGhiNhanKetQua",
                table: "ThucTap",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhanXet",
                table: "ThucTap",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "ThucTap",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "DangKy");

            migrationBuilder.AlterColumn<int>(
                name: "SinhVienId",
                table: "DeTaiKhoaLuan",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "GiangVienHuongDanId",
                table: "DeTaiKhoaLuan",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayGhiNhanKetQua",
                table: "DeTaiKhoaLuan",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhanXetBaoVe",
                table: "DeTaiKhoaLuan",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CpaToiThieu",
                table: "ChuongTrinhDaoTao",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiemBaoVeToiThieu",
                table: "ChuongTrinhDaoTao",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiemThucTapToiThieu",
                table: "ChuongTrinhDaoTao",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "YeuCauKhoaLuan",
                table: "ChuongTrinhDaoTao",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "YeuCauThucTap",
                table: "ChuongTrinhDaoTao",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CoSoThucTap",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NguoiLienHe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DangTiepNhan = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoSoThucTap", x => x.Id);
                    table.CheckConstraint("CK_CoSoThucTap_Ma", "LEN(TRIM([Ma])) > 0");
                    table.CheckConstraint("CK_CoSoThucTap_Ten", "LEN(TRIM([Ten])) > 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThucTap_CoSoThucTapId",
                table: "ThucTap",
                column: "CoSoThucTapId");

            migrationBuilder.CreateIndex(
                name: "IX_ThucTap_SinhVienId_HocKyId",
                table: "ThucTap",
                columns: new[] { "SinhVienId", "HocKyId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThucTap_KetQua",
                table: "ThucTap",
                sql: "([Diem] IS NULL AND [TrangThai] <> 'HoanThanh') OR ([Diem] BETWEEN 0 AND 10 AND [TrangThai] = 'HoanThanh')");

            migrationBuilder.CreateIndex(
                name: "IX_DeTaiKhoaLuan_SinhVienId",
                table: "DeTaiKhoaLuan",
                column: "SinhVienId",
                unique: true,
                filter: "[SinhVienId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeTaiKhoaLuan_Diem",
                table: "DeTaiKhoaLuan",
                sql: "[Diem] IS NULL OR ([Diem] >= 0 AND [Diem] <= 10)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeTaiKhoaLuan_TenDeTai",
                table: "DeTaiKhoaLuan",
                sql: "LEN(TRIM([TenDeTai])) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChuongTrinhDaoTao_Cpa",
                table: "ChuongTrinhDaoTao",
                sql: "[CpaToiThieu] IS NULL OR [CpaToiThieu] BETWEEN 0 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChuongTrinhDaoTao_DiemBaoVe",
                table: "ChuongTrinhDaoTao",
                sql: "[DiemBaoVeToiThieu] IS NULL OR [DiemBaoVeToiThieu] BETWEEN 0 AND 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChuongTrinhDaoTao_DiemThucTap",
                table: "ChuongTrinhDaoTao",
                sql: "[DiemThucTapToiThieu] IS NULL OR [DiemThucTapToiThieu] BETWEEN 0 AND 10");

            migrationBuilder.CreateIndex(
                name: "IX_CoSoThucTap_Ma",
                table: "CoSoThucTap",
                column: "Ma",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ThucTap_CoSoThucTap_CoSoThucTapId",
                table: "ThucTap",
                column: "CoSoThucTapId",
                principalTable: "CoSoThucTap",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThucTap_CoSoThucTap_CoSoThucTapId",
                table: "ThucTap");

            migrationBuilder.DropTable(
                name: "CoSoThucTap");

            migrationBuilder.DropIndex(
                name: "IX_ThucTap_CoSoThucTapId",
                table: "ThucTap");

            migrationBuilder.DropIndex(
                name: "IX_ThucTap_SinhVienId_HocKyId",
                table: "ThucTap");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThucTap_KetQua",
                table: "ThucTap");

            migrationBuilder.DropIndex(
                name: "IX_DeTaiKhoaLuan_SinhVienId",
                table: "DeTaiKhoaLuan");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeTaiKhoaLuan_Diem",
                table: "DeTaiKhoaLuan");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeTaiKhoaLuan_TenDeTai",
                table: "DeTaiKhoaLuan");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChuongTrinhDaoTao_Cpa",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChuongTrinhDaoTao_DiemBaoVe",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChuongTrinhDaoTao_DiemThucTap",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "LyDo",
                table: "XetTotNghiep");

            migrationBuilder.DropColumn(
                name: "CoSoThucTapId",
                table: "ThucTap");

            migrationBuilder.DropColumn(
                name: "NgayGhiNhanKetQua",
                table: "ThucTap");

            migrationBuilder.DropColumn(
                name: "NhanXet",
                table: "ThucTap");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "ThucTap");

            migrationBuilder.DropColumn(
                name: "NgayGhiNhanKetQua",
                table: "DeTaiKhoaLuan");

            migrationBuilder.DropColumn(
                name: "NhanXetBaoVe",
                table: "DeTaiKhoaLuan");

            migrationBuilder.DropColumn(
                name: "CpaToiThieu",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "DiemBaoVeToiThieu",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "DiemThucTapToiThieu",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "YeuCauKhoaLuan",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropColumn(
                name: "YeuCauThucTap",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.AlterColumn<int>(
                name: "TongTinChiTichLuy",
                table: "XetTotNghiep",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiemTrungBinhTichLuy",
                table: "XetTotNghiep",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,2)",
                oldPrecision: 4,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DonViThucTap",
                table: "ThucTap",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SinhVienId",
                table: "DeTaiKhoaLuan",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GiangVienHuongDanId",
                table: "DeTaiKhoaLuan",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThucTap_SinhVienId",
                table: "ThucTap",
                column: "SinhVienId");

            migrationBuilder.CreateIndex(
                name: "IX_DeTaiKhoaLuan_SinhVienId",
                table: "DeTaiKhoaLuan",
                column: "SinhVienId");
        }
    }
}
