using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class Auth07LienKetSinhVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SinhVienId",
                table: "TaiKhoan",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_SinhVienId",
                table: "TaiKhoan",
                column: "SinhVienId",
                unique: true,
                filter: "[SinhVienId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TaiKhoan_SinhVien_SinhVienId",
                table: "TaiKhoan",
                column: "SinhVienId",
                principalTable: "SinhVien",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaiKhoan_SinhVien_SinhVienId",
                table: "TaiKhoan");

            migrationBuilder.DropIndex(
                name: "IX_TaiKhoan_SinhVienId",
                table: "TaiKhoan");

            migrationBuilder.DropColumn(
                name: "SinhVienId",
                table: "TaiKhoan");
        }
    }
}
