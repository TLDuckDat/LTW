using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AUTH08_LienKetGiangVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GiangVienId",
                table: "TaiKhoan",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_GiangVienId",
                table: "TaiKhoan",
                column: "GiangVienId",
                unique: true,
                filter: "[GiangVienId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TaiKhoan_GiangVien_GiangVienId",
                table: "TaiKhoan",
                column: "GiangVienId",
                principalTable: "GiangVien",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaiKhoan_GiangVien_GiangVienId",
                table: "TaiKhoan");

            migrationBuilder.DropIndex(
                name: "IX_TaiKhoan_GiangVienId",
                table: "TaiKhoan");

            migrationBuilder.DropColumn(
                name: "GiangVienId",
                table: "TaiKhoan");
        }
    }
}
