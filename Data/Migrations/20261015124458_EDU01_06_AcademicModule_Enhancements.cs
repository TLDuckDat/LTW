using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class EDU01_06_AcademicModule_Enhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChuongTrinhDaoTaoMonHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChuongTrinhDaoTaoId = table.Column<int>(type: "int", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    HocKyGoiY = table.Column<int>(type: "int", nullable: false),
                    BatBuoc = table.Column<bool>(type: "bit", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuongTrinhDaoTaoMonHoc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChuongTrinhDaoTaoMonHoc_ChuongTrinhDaoTao_ChuongTrinhDaoTaoId",
                        column: x => x.ChuongTrinhDaoTaoId,
                        principalTable: "ChuongTrinhDaoTao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChuongTrinhDaoTaoMonHoc_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuyCheHocVu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GiaTri = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DonVi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApDungTuKhoa = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyCheHocVu", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTaoMonHoc_ChuongTrinhDaoTaoId_HocKyGoiY",
                table: "ChuongTrinhDaoTaoMonHoc",
                columns: new[] { "ChuongTrinhDaoTaoId", "HocKyGoiY" });

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTaoMonHoc_ChuongTrinhDaoTaoId_MonHocId",
                table: "ChuongTrinhDaoTaoMonHoc",
                columns: new[] { "ChuongTrinhDaoTaoId", "MonHocId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTaoMonHoc_MonHocId",
                table: "ChuongTrinhDaoTaoMonHoc",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_QuyCheHocVu_ApDungTuKhoa",
                table: "QuyCheHocVu",
                column: "ApDungTuKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_QuyCheHocVu_Ma",
                table: "QuyCheHocVu",
                column: "Ma",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChuongTrinhDaoTaoMonHoc");

            migrationBuilder.DropTable(
                name: "QuyCheHocVu");
        }
    }
}
