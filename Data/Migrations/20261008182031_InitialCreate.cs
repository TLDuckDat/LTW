using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Khoa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SoTinChi = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NamHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NamHoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhongHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SucChua = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhongHoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DaKhoa = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoMon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    KhoaId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoMon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoMon_Khoa_KhoaId",
                        column: x => x.KhoaId,
                        principalTable: "Khoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Nganh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    KhoaId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nganh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nganh_Khoa_KhoaId",
                        column: x => x.KhoaId,
                        principalTable: "Khoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonHocTienQuyet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    TienQuyetId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHocTienQuyet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonHocTienQuyet_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonHocTienQuyet_MonHoc_TienQuyetId",
                        column: x => x.TienQuyetId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HocKy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SoThuTu = table.Column<int>(type: "int", nullable: false),
                    NamHocId = table.Column<int>(type: "int", nullable: false),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocKy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HocKy_NamHoc_NamHocId",
                        column: x => x.NamHocId,
                        principalTable: "NamHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    NguoiTaoId = table.Column<int>(type: "int", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThongBao_TaiKhoan_NguoiTaoId",
                        column: x => x.NguoiTaoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GiangVien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HocVi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BoMonId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiangVien", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiangVien_BoMon_BoMonId",
                        column: x => x.BoMonId,
                        principalTable: "BoMon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChuongTrinhDaoTao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NganhId = table.Column<int>(type: "int", nullable: false),
                    KhoaTuyen = table.Column<int>(type: "int", nullable: false),
                    TongSoTinChi = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuongTrinhDaoTao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChuongTrinhDaoTao_Nganh_NganhId",
                        column: x => x.NganhId,
                        principalTable: "Nganh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChuyenNganh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NganhId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenNganh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChuyenNganh_Nganh_NganhId",
                        column: x => x.NganhId,
                        principalTable: "Nganh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DotDangKy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HocKyId = table.Column<int>(type: "int", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DotDangKy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DotDangKy_HocKy_HocKyId",
                        column: x => x.HocKyId,
                        principalTable: "HocKy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoiDongBaoVe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NgayBaoVe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChuTichId = table.Column<int>(type: "int", nullable: false),
                    ThuKyId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoiDongBaoVe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoiDongBaoVe_GiangVien_ChuTichId",
                        column: x => x.ChuTichId,
                        principalTable: "GiangVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoiDongBaoVe_GiangVien_ThuKyId",
                        column: x => x.ThuKyId,
                        principalTable: "GiangVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LopHanhChinh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NganhId = table.Column<int>(type: "int", nullable: false),
                    KhoaTuyen = table.Column<int>(type: "int", nullable: false),
                    CoVanId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHanhChinh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LopHanhChinh_GiangVien_CoVanId",
                        column: x => x.CoVanId,
                        principalTable: "GiangVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LopHanhChinh_Nganh_NganhId",
                        column: x => x.NganhId,
                        principalTable: "Nganh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LopHocPhan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    HocKyId = table.Column<int>(type: "int", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    GiangVienId = table.Column<int>(type: "int", nullable: true),
                    SiSoToiDa = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHocPhan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LopHocPhan_GiangVien_GiangVienId",
                        column: x => x.GiangVienId,
                        principalTable: "GiangVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LopHocPhan_HocKy_HocKyId",
                        column: x => x.HocKyId,
                        principalTable: "HocKy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LopHocPhan_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SinhVien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MSSV = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NganhId = table.Column<int>(type: "int", nullable: false),
                    LopHanhChinhId = table.Column<int>(type: "int", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinhVien", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SinhVien_LopHanhChinh_LopHanhChinhId",
                        column: x => x.LopHanhChinhId,
                        principalTable: "LopHanhChinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SinhVien_Nganh_NganhId",
                        column: x => x.NganhId,
                        principalTable: "Nganh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LopHocPhanId = table.Column<int>(type: "int", nullable: false),
                    Thu = table.Column<int>(type: "int", nullable: false),
                    TietBatDau = table.Column<int>(type: "int", nullable: false),
                    SoTiet = table.Column<int>(type: "int", nullable: false),
                    PhongHocId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichHoc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichHoc_LopHocPhan_LopHocPhanId",
                        column: x => x.LopHocPhanId,
                        principalTable: "LopHocPhan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichHoc_PhongHoc_PhongHocId",
                        column: x => x.PhongHocId,
                        principalTable: "PhongHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DangKyHocPhan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    LopHocPhanId = table.Column<int>(type: "int", nullable: false),
                    LoaiDangKy = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyHocPhan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DangKyHocPhan_LopHocPhan_LopHocPhanId",
                        column: x => x.LopHocPhanId,
                        principalTable: "LopHocPhan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DangKyHocPhan_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeTaiKhoaLuan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDeTai = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    GiangVienHuongDanId = table.Column<int>(type: "int", nullable: false),
                    HoiDongBaoVeId = table.Column<int>(type: "int", nullable: true),
                    Diem = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeTaiKhoaLuan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeTaiKhoaLuan_GiangVien_GiangVienHuongDanId",
                        column: x => x.GiangVienHuongDanId,
                        principalTable: "GiangVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeTaiKhoaLuan_HoiDongBaoVe_HoiDongBaoVeId",
                        column: x => x.HoiDongBaoVeId,
                        principalTable: "HoiDongBaoVe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeTaiKhoaLuan_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiemRenLuyen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    HocKyId = table.Column<int>(type: "int", nullable: false),
                    Diem = table.Column<int>(type: "int", nullable: false),
                    XepLoai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemRenLuyen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiemRenLuyen_HocKy_HocKyId",
                        column: x => x.HocKyId,
                        principalTable: "HocKy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiemRenLuyen_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HocBong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    HocKyId = table.Column<int>(type: "int", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocBong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HocBong_HocKy_HocKyId",
                        column: x => x.HocKyId,
                        principalTable: "HocKy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HocBong_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HocPhi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    HocKyId = table.Column<int>(type: "int", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DaThanhToan = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocPhi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HocPhi_HocKy_HocKyId",
                        column: x => x.HocKyId,
                        principalTable: "HocKy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HocPhi_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KhenThuongKyLuat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    Loai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SoQuyetDinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayQuyetDinh = table.Column<DateOnly>(type: "date", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhenThuongKyLuat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KhenThuongKyLuat_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MienGiam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    HocKyId = table.Column<int>(type: "int", nullable: false),
                    TyLe = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MienGiam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MienGiam_HocKy_HocKyId",
                        column: x => x.HocKyId,
                        principalTable: "HocKy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MienGiam_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThucTap",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    HocKyId = table.Column<int>(type: "int", nullable: false),
                    DonViThucTap = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GiangVienHuongDanId = table.Column<int>(type: "int", nullable: true),
                    Diem = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThucTap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThucTap_GiangVien_GiangVienHuongDanId",
                        column: x => x.GiangVienHuongDanId,
                        principalTable: "GiangVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThucTap_HocKy_HocKyId",
                        column: x => x.HocKyId,
                        principalTable: "HocKy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThucTap_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "XetTotNghiep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    NgayXet = table.Column<DateOnly>(type: "date", nullable: false),
                    TongTinChiTichLuy = table.Column<int>(type: "int", nullable: false),
                    DiemTrungBinhTichLuy = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    XepLoai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    KetQua = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XetTotNghiep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XetTotNghiep_SinhVien_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiemHocPhan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DangKyHocPhanId = table.Column<int>(type: "int", nullable: false),
                    DiemQuaTrinh = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemGiuaKy = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemCuoiKy = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemTongKet = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemChu = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    DaKhoa = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemHocPhan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiemHocPhan_DangKyHocPhan_DangKyHocPhanId",
                        column: x => x.DangKyHocPhanId,
                        principalTable: "DangKyHocPhan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhucKhao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiemHocPhanId = table.Column<int>(type: "int", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DiemCu = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    DiemMoi = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhucKhao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhucKhao_DiemHocPhan_DiemHocPhanId",
                        column: x => x.DiemHocPhanId,
                        principalTable: "DiemHocPhan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoMon_KhoaId",
                table: "BoMon",
                column: "KhoaId");

            migrationBuilder.CreateIndex(
                name: "IX_BoMon_Ma",
                table: "BoMon",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTao_Ma",
                table: "ChuongTrinhDaoTao",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTao_NganhId",
                table: "ChuongTrinhDaoTao",
                column: "NganhId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNganh_Ma",
                table: "ChuyenNganh",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNganh_NganhId",
                table: "ChuyenNganh",
                column: "NganhId");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyHocPhan_LopHocPhanId",
                table: "DangKyHocPhan",
                column: "LopHocPhanId");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyHocPhan_SinhVienId_LopHocPhanId",
                table: "DangKyHocPhan",
                columns: new[] { "SinhVienId", "LopHocPhanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeTaiKhoaLuan_GiangVienHuongDanId",
                table: "DeTaiKhoaLuan",
                column: "GiangVienHuongDanId");

            migrationBuilder.CreateIndex(
                name: "IX_DeTaiKhoaLuan_HoiDongBaoVeId",
                table: "DeTaiKhoaLuan",
                column: "HoiDongBaoVeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeTaiKhoaLuan_SinhVienId",
                table: "DeTaiKhoaLuan",
                column: "SinhVienId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemHocPhan_DangKyHocPhanId",
                table: "DiemHocPhan",
                column: "DangKyHocPhanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiemRenLuyen_HocKyId",
                table: "DiemRenLuyen",
                column: "HocKyId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemRenLuyen_SinhVienId_HocKyId",
                table: "DiemRenLuyen",
                columns: new[] { "SinhVienId", "HocKyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DotDangKy_HocKyId",
                table: "DotDangKy",
                column: "HocKyId");

            migrationBuilder.CreateIndex(
                name: "IX_GiangVien_BoMonId",
                table: "GiangVien",
                column: "BoMonId");

            migrationBuilder.CreateIndex(
                name: "IX_GiangVien_Ma",
                table: "GiangVien",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HocBong_HocKyId",
                table: "HocBong",
                column: "HocKyId");

            migrationBuilder.CreateIndex(
                name: "IX_HocBong_SinhVienId",
                table: "HocBong",
                column: "SinhVienId");

            migrationBuilder.CreateIndex(
                name: "IX_HocKy_NamHocId_SoThuTu",
                table: "HocKy",
                columns: new[] { "NamHocId", "SoThuTu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HocPhi_HocKyId",
                table: "HocPhi",
                column: "HocKyId");

            migrationBuilder.CreateIndex(
                name: "IX_HocPhi_SinhVienId_HocKyId",
                table: "HocPhi",
                columns: new[] { "SinhVienId", "HocKyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoiDongBaoVe_ChuTichId",
                table: "HoiDongBaoVe",
                column: "ChuTichId");

            migrationBuilder.CreateIndex(
                name: "IX_HoiDongBaoVe_ThuKyId",
                table: "HoiDongBaoVe",
                column: "ThuKyId");

            migrationBuilder.CreateIndex(
                name: "IX_KhenThuongKyLuat_SinhVienId",
                table: "KhenThuongKyLuat",
                column: "SinhVienId");

            migrationBuilder.CreateIndex(
                name: "IX_Khoa_Ma",
                table: "Khoa",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichHoc_LopHocPhanId",
                table: "LichHoc",
                column: "LopHocPhanId");

            migrationBuilder.CreateIndex(
                name: "IX_LichHoc_PhongHocId",
                table: "LichHoc",
                column: "PhongHocId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHanhChinh_CoVanId",
                table: "LopHanhChinh",
                column: "CoVanId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHanhChinh_Ma",
                table: "LopHanhChinh",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LopHanhChinh_NganhId",
                table: "LopHanhChinh",
                column: "NganhId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocPhan_GiangVienId",
                table: "LopHocPhan",
                column: "GiangVienId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocPhan_HocKyId",
                table: "LopHocPhan",
                column: "HocKyId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocPhan_Ma",
                table: "LopHocPhan",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LopHocPhan_MonHocId",
                table: "LopHocPhan",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_MienGiam_HocKyId",
                table: "MienGiam",
                column: "HocKyId");

            migrationBuilder.CreateIndex(
                name: "IX_MienGiam_SinhVienId",
                table: "MienGiam",
                column: "SinhVienId");

            migrationBuilder.CreateIndex(
                name: "IX_MonHoc_Ma",
                table: "MonHoc",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonHocTienQuyet_MonHocId_TienQuyetId",
                table: "MonHocTienQuyet",
                columns: new[] { "MonHocId", "TienQuyetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonHocTienQuyet_TienQuyetId",
                table: "MonHocTienQuyet",
                column: "TienQuyetId");

            migrationBuilder.CreateIndex(
                name: "IX_NamHoc_Ten",
                table: "NamHoc",
                column: "Ten",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nganh_KhoaId",
                table: "Nganh",
                column: "KhoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Nganh_Ma",
                table: "Nganh",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhongHoc_Ma",
                table: "PhongHoc",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhucKhao_DiemHocPhanId",
                table: "PhucKhao",
                column: "DiemHocPhanId");

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_LopHanhChinhId",
                table: "SinhVien",
                column: "LopHanhChinhId");

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_MSSV",
                table: "SinhVien",
                column: "MSSV",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_NganhId",
                table: "SinhVien",
                column: "NganhId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_TenDangNhap",
                table: "TaiKhoan",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_NguoiTaoId",
                table: "ThongBao",
                column: "NguoiTaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ThucTap_GiangVienHuongDanId",
                table: "ThucTap",
                column: "GiangVienHuongDanId");

            migrationBuilder.CreateIndex(
                name: "IX_ThucTap_HocKyId",
                table: "ThucTap",
                column: "HocKyId");

            migrationBuilder.CreateIndex(
                name: "IX_ThucTap_SinhVienId",
                table: "ThucTap",
                column: "SinhVienId");

            migrationBuilder.CreateIndex(
                name: "IX_XetTotNghiep_SinhVienId",
                table: "XetTotNghiep",
                column: "SinhVienId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChuongTrinhDaoTao");

            migrationBuilder.DropTable(
                name: "ChuyenNganh");

            migrationBuilder.DropTable(
                name: "DeTaiKhoaLuan");

            migrationBuilder.DropTable(
                name: "DiemRenLuyen");

            migrationBuilder.DropTable(
                name: "DotDangKy");

            migrationBuilder.DropTable(
                name: "HocBong");

            migrationBuilder.DropTable(
                name: "HocPhi");

            migrationBuilder.DropTable(
                name: "KhenThuongKyLuat");

            migrationBuilder.DropTable(
                name: "LichHoc");

            migrationBuilder.DropTable(
                name: "MienGiam");

            migrationBuilder.DropTable(
                name: "MonHocTienQuyet");

            migrationBuilder.DropTable(
                name: "PhucKhao");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "ThucTap");

            migrationBuilder.DropTable(
                name: "XetTotNghiep");

            migrationBuilder.DropTable(
                name: "HoiDongBaoVe");

            migrationBuilder.DropTable(
                name: "PhongHoc");

            migrationBuilder.DropTable(
                name: "DiemHocPhan");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "DangKyHocPhan");

            migrationBuilder.DropTable(
                name: "LopHocPhan");

            migrationBuilder.DropTable(
                name: "SinhVien");

            migrationBuilder.DropTable(
                name: "HocKy");

            migrationBuilder.DropTable(
                name: "MonHoc");

            migrationBuilder.DropTable(
                name: "LopHanhChinh");

            migrationBuilder.DropTable(
                name: "NamHoc");

            migrationBuilder.DropTable(
                name: "GiangVien");

            migrationBuilder.DropTable(
                name: "Nganh");

            migrationBuilder.DropTable(
                name: "BoMon");

            migrationBuilder.DropTable(
                name: "Khoa");
        }
    }
}
