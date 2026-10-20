using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class FIX01_RowVersionVaGioUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PhucKhao",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "HocPhi",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            // Thời gian đợt đăng ký trước đây lưu theo giờ máy chủ (Việt Nam, UTC+7); nay thống nhất lưu UTC
            migrationBuilder.Sql(
                "UPDATE DotDangKy SET NgayBatDau = DATEADD(HOUR, -7, NgayBatDau), NgayKetThuc = DATEADD(HOUR, -7, NgayKetThuc);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE DotDangKy SET NgayBatDau = DATEADD(HOUR, 7, NgayBatDau), NgayKetThuc = DATEADD(HOUR, 7, NgayKetThuc);");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PhucKhao");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "HocPhi");
        }
    }
}
