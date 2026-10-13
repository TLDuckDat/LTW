using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Entities;
using StudentManagement.Modules.Students.Entities;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Registration.Entities;

namespace StudentManagement.Modules.StudentAffairs.Services;

public class StudentAffairsService(AppDbContext db)
    : IStudentAffairsService
{
    private readonly AppDbContext _db = db;

    // SA-01: NHẬP ĐIỂM RÈN LUYỆN
    public async Task<object?> NhapDiemRenLuyenAsync(
        DiemRenLuyenRequest request)
    {
        // 1. Kiểm tra sinh viên có tồn tại trong hệ thống không
        var sinhVien = await _db.Set<SinhVien>()
            .FirstOrDefaultAsync(x => x.Id == request.SinhVienId);

        if (sinhVien == null)
            throw new KeyNotFoundException("Không tìm thấy sinh viên.");

        // 2. Chặn sinh viên không còn học (thôi học, bảo lưu, tốt nghiệp) nhập điểm rèn luyện
        if (sinhVien.TrangThai != TrangThaiSinhVien.DangHoc)
            throw new InvalidOperationException(
                $"Sinh viên đang ở trạng thái '{sinhVien.TrangThai}', không thể nhập điểm rèn luyện.");

        // 3. Kiểm tra học kỳ có tồn tại không
        var hocKy = await _db.Set<HocKy>()
            .FirstOrDefaultAsync(x => x.Id == request.HocKyId);

        if (hocKy == null)
            throw new KeyNotFoundException("Không tìm thấy học kỳ.");

        // 4. Kiểm tra cận điểm hợp lệ trong khoảng 0 đến 100
        if (request.Diem < 0 || request.Diem > 100)
            throw new ArgumentException(
                "Điểm rèn luyện phải từ 0 đến 100.");

        // 5. Chặn nhập trùng: Một sinh viên chỉ có duy nhất 1 điểm rèn luyện trong 1 học kỳ
        var daTonTai = await _db.Set<DiemRenLuyen>()
            .AnyAsync(x =>
                x.SinhVienId == request.SinhVienId &&
                x.HocKyId == request.HocKyId);

        if (daTonTai)
            throw new InvalidOperationException(
                "Sinh viên đã có điểm trong học kỳ này.");

        // 6. Tự động quy đổi điểm số sang xếp loại rèn luyện
        string xepLoai = request.Diem switch
        {
            >= 90 => "Xuất sắc",
            >= 80 => "Tốt",
            >= 65 => "Khá",
            >= 50 => "Trung bình",
            _ => "Yếu"
        };

        // 7. Tạo bản ghi điểm mới và lưu vào CSDL
        var diem = new DiemRenLuyen
        {
            SinhVienId = request.SinhVienId,
            HocKyId = request.HocKyId,
            Diem = request.Diem,
            XepLoai = xepLoai
        };

        _db.Set<DiemRenLuyen>().Add(diem);
        await _db.SaveChangesAsync();

        return new
        {
            diem.Id,
            diem.SinhVienId,
            HoTen = sinhVien.HoTen,
            diem.HocKyId,
            TenHocKy = hocKy.Ten,
            diem.Diem,
            diem.XepLoai,
            diem.CreatedAt
        };
    }

    // Xem danh sách điểm, có thể lọc theo sinh viên hoặc học kỳ
    public async Task<object?> GetDiemRenLuyenAsync(
        int? sinhVienId,
        int? hocKyId)
    {
        var query =
            from d in _db.Set<DiemRenLuyen>().AsNoTracking()
            join sv in _db.Set<SinhVien>().AsNoTracking()
                on d.SinhVienId equals sv.Id
            join hk in _db.Set<HocKy>().AsNoTracking()
                on d.HocKyId equals hk.Id
            select new
            {
                d.Id,
                d.SinhVienId,
                sv.MSSV,
                sv.HoTen,
                d.HocKyId,
                TenHocKy = hk.Ten,
                d.Diem,
                d.XepLoai,
                d.CreatedAt
            };

        if (sinhVienId.HasValue)
            query = query.Where(
                x => x.SinhVienId == sinhVienId.Value);

        if (hocKyId.HasValue)
            query = query.Where(
                x => x.HocKyId == hocKyId.Value);

        return await query
            .OrderBy(x => x.MSSV)
            .ToListAsync();
    }

    // SA-02: TÍNH HỌC PHÍ THEO HỌC KỲ (Tích hợp Miễn giảm & Đăng ký môn học)
    public async Task<object?> TinhHocPhiAsync(int hocKyId)
    {
        const decimal DonGiaTinChi = 600000m;

        // 1. Kiểm tra học kỳ có tồn tại không
        var hocKy = await _db.Set<HocKy>()
            .FirstOrDefaultAsync(x => x.Id == hocKyId);

        if (hocKy == null)
            throw new KeyNotFoundException("Không tìm thấy học kỳ.");

        // 2. Lấy trước danh sách tỷ lệ miễn giảm trong kỳ (tra cứu O(1) qua Dictionary)
        var mienGiamDict = await _db.Set<MienGiam>()
            .Where(x => x.HocKyId == hocKyId)
            .ToDictionaryAsync(x => x.SinhVienId, x => x.TyLe);

        // 3. Quét các lớp học phần sinh viên đã đăng ký hợp lệ (TrangThai == DaDangKy) và tính tổng số tín chỉ
        var danhSachTinChi = await (
            from dk in _db.Set<DangKyHocPhan>()
            join lhp in _db.Set<LopHocPhan>()
                on dk.LopHocPhanId equals lhp.Id
            join mh in _db.Set<MonHoc>()
                on lhp.MonHocId equals mh.Id
            where lhp.HocKyId == hocKyId
                  && dk.TrangThai == TrangThaiDangKy.DaDangKy
            group mh by dk.SinhVienId into g
            select new
            {
                SinhVienId = g.Key,
                TongTinChi = g.Sum(x => x.SoTinChi)
            }
        ).ToListAsync();

        var sinhVienCoTinChi = danhSachTinChi.Select(x => x.SinhVienId).ToHashSet();

        // 4. Xử lý sinh viên rút hết môn học (0 tín chỉ): đưa học phí cũ về 0đ và đánh dấu đã thanh toán
        var hocPhiHuyMon = await _db.Set<HocPhi>()
            .Where(x => x.HocKyId == hocKyId && !sinhVienCoTinChi.Contains(x.SinhVienId) && x.SoTien > 0)
            .ToListAsync();

        foreach (var hp in hocPhiHuyMon)
        {
            hp.SoTien = 0m;
            hp.DaThanhToan = true;
        }

        // Nếu cả học kỳ chưa có ai đăng ký học phần
        if (danhSachTinChi.Count == 0)
        {
            await _db.SaveChangesAsync();
            return new
            {
                HocKyId = hocKyId,
                TenHocKy = hocKy.Ten,
                DonGiaTinChi,
                SoSinhVien = 0,
                ThongBao = "Chưa có sinh viên đăng ký học phần trong học kỳ này."
            };
        }

        var ketQua = new List<object>();

        // 5. Tính toán học phí cho từng sinh viên
        foreach (var item in danhSachTinChi)
        {
            decimal tongTienGoc = item.TongTinChi * DonGiaTinChi;
            decimal tyLeGiam = mienGiamDict.GetValueOrDefault(item.SinhVienId, 0m);

            // Công thức trừ tiền theo tỷ lệ miễn giảm: TienGoc * (1 - TyLe/100)
            decimal soTienPhaiNop = Math.Round(tongTienGoc * (1m - (tyLeGiam / 100m)), 2);

            // Kiểm tra sinh viên đã có bản ghi học phí trong kỳ chưa
            var hocPhi = await _db.Set<HocPhi>()
                .FirstOrDefaultAsync(x =>
                    x.SinhVienId == item.SinhVienId &&
                    x.HocKyId == hocKyId);

            if (hocPhi == null)
            {
                hocPhi = new HocPhi
                {
                    SinhVienId = item.SinhVienId,
                    HocKyId = hocKyId,
                    SoTien = soTienPhaiNop,
                    // Nếu số tiền bằng 0 (miễn 100%) thì tự động coi như đã thanh toán
                    DaThanhToan = soTienPhaiNop == 0m
                };

                _db.Set<HocPhi>().Add(hocPhi);
            }
            else
            {
                // Nếu sinh viên đăng ký thêm môn làm tiền tăng lên -> bắt buộc chuyển lại DaThanhToan = false để đóng bù
                if (soTienPhaiNop > hocPhi.SoTien)
                {
                    hocPhi.DaThanhToan = false;
                }
                else if (soTienPhaiNop == 0m)
                {
                    hocPhi.DaThanhToan = true;
                }

                hocPhi.SoTien = soTienPhaiNop;
            }

            ketQua.Add(new
            {
                item.SinhVienId,
                item.TongTinChi,
                DonGiaTinChi,
                TongTienGoc = tongTienGoc,
                TyLeMienGiam = tyLeGiam,
                SoTienHocPhi = soTienPhaiNop,
                hocPhi.DaThanhToan
            });
        }

        await _db.SaveChangesAsync();

        return new
        {
            HocKyId = hocKyId,
            TenHocKy = hocKy.Ten,
            DonGiaTinChi,
            SoSinhVien = danhSachTinChi.Count,
            DanhSach = ketQua
        };
    }

    // Xem danh sách học phí, có thể lọc theo sinh viên hoặc học kỳ
    public async Task<object?> GetHocPhiAsync(
        int? sinhVienId,
        int? hocKyId)
    {
        var query =
            from hp in _db.Set<HocPhi>().AsNoTracking()
            join sv in _db.Set<SinhVien>().AsNoTracking()
                on hp.SinhVienId equals sv.Id
            join hk in _db.Set<HocKy>().AsNoTracking()
                on hp.HocKyId equals hk.Id
            select new
            {
                hp.Id,
                hp.SinhVienId,
                sv.MSSV,
                sv.HoTen,
                hp.HocKyId,
                TenHocKy = hk.Ten,
                hp.SoTien,
                hp.DaThanhToan
            };

        if (sinhVienId.HasValue)
        {
            query = query.Where(
                x => x.SinhVienId == sinhVienId.Value);
        }

        if (hocKyId.HasValue)
        {
            query = query.Where(
                x => x.HocKyId == hocKyId.Value);
        }

        return await query
            .OrderBy(x => x.MSSV)
            .ThenBy(x => x.HocKyId)
            .ToListAsync();
    }

    // SA-03: XÁC NHẬN THANH TOÁN HỌC PHÍ
    public async Task<object?> ThanhToanAsync(int id)
    {
        // 1. Kiểm tra bản ghi học phí có tồn tại không
        var hocPhi = await _db.Set<HocPhi>()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (hocPhi == null)
            throw new KeyNotFoundException(
                "Không tìm thấy bản ghi học phí.");

        // 2. Chặn thanh toán trùng nếu học phí đã được đóng rồi
        if (hocPhi.DaThanhToan)
            throw new InvalidOperationException(
                "Học phí này đã được thanh toán.");

        // 3. Chặn thanh toán hóa đơn 0đ (do được miễn 100% hoặc không có tín chỉ)
        if (hocPhi.SoTien <= 0)
            throw new InvalidOperationException(
                "Học phí bằng 0đ (được miễn 100% hoặc không có tín chỉ), không cần thực hiện thanh toán.");

        // 4. Xác nhận thanh toán thành công
        hocPhi.DaThanhToan = true;

        await _db.SaveChangesAsync();

        return new
        {
            hocPhi.Id,
            hocPhi.SinhVienId,
            hocPhi.HocKyId,
            hocPhi.SoTien,
            hocPhi.DaThanhToan,
            ThongBao = "Xác nhận thanh toán học phí thành công."
        };
    }

    // SA-04: TẠO MIỄN GIẢM HỌC PHÍ
    public async Task<object?> CreateMienGiamAsync(
        MienGiamRequest request)
    {
        // 1. Kiểm tra ID hợp lệ (> 0)
        if (request.SinhVienId <= 0 || request.HocKyId <= 0)
            throw new ArgumentException(
                "SinhVienId và HocKyId phải lớn hơn 0.");

        // 2. Kiểm tra tỷ lệ miễn giảm trong khoảng [0..100]%
        if (request.TyLe < 0 || request.TyLe > 100)
            throw new ArgumentException(
                "Tỷ lệ miễn giảm phải từ 0 đến 100.");

        // 3. Kiểm tra sinh viên và học kỳ có tồn tại không
        var sinhVien = await _db.Set<SinhVien>()
            .FirstOrDefaultAsync(x => x.Id == request.SinhVienId);

        if (sinhVien == null)
            throw new KeyNotFoundException(
                "Không tìm thấy sinh viên.");

        var hocKy = await _db.Set<HocKy>()
            .FirstOrDefaultAsync(x => x.Id == request.HocKyId);

        if (hocKy == null)
            throw new KeyNotFoundException(
                "Không tìm thấy học kỳ.");

        // 4. Chặn tạo trùng: Mỗi sinh viên chỉ có tối đa 1 mức miễn giảm trong cùng một học kỳ
        var daTonTai = await _db.Set<MienGiam>()
            .AnyAsync(x => x.SinhVienId == request.SinhVienId && x.HocKyId == request.HocKyId);

        if (daTonTai)
            throw new InvalidOperationException(
                "Sinh viên đã có hồ sơ miễn giảm trong học kỳ này.");

        // 5. Lưu hồ sơ miễn giảm
        var mienGiam = new MienGiam
        {
            SinhVienId = request.SinhVienId,
            HocKyId = request.HocKyId,
            TyLe = request.TyLe,
            LyDo = request.LyDo
        };

        _db.Set<MienGiam>().Add(mienGiam);
        await _db.SaveChangesAsync();

        return new
        {
            mienGiam.Id,
            mienGiam.SinhVienId,
            HoTen = sinhVien.HoTen,
            mienGiam.HocKyId,
            TenHocKy = hocKy.Ten,
            mienGiam.TyLe,
            mienGiam.LyDo
        };
    }

    // SA-05: XÉT HỌC BỔNG THEO ĐIỂM RÈN LUYỆN (Loại trừ sinh viên bị kỷ luật)
    public async Task<object?> XetHocBongAsync(int hocKyId)
    {
        const int DiemToiThieu = 80;
        const decimal SoTienHocBong = 1_000_000m;

        // 1. Kiểm tra học kỳ có tồn tại không
        var hocKy = await _db.Set<HocKy>()
            .FirstOrDefaultAsync(x => x.Id == hocKyId);

        if (hocKy == null)
            throw new KeyNotFoundException("Không tìm thấy học kỳ.");

        // 2. Lấy danh sách ID sinh viên bị kỷ luật trong hệ thống để loại trừ
        var danhSachKyLuatIds = await _db.Set<KhenThuongKyLuat>()
            .Where(x => x.Loai == LoaiKhenThuongKyLuat.KyLuat)
            .Select(x => x.SinhVienId)
            .Distinct()
            .ToListAsync();

        // 3. Lọc danh sách đủ điều kiện:
        //    - Điểm rèn luyện trong kỳ >= 80
        //    - Sinh viên đang theo học (DangHoc)
        //    - Không nằm trong danh sách bị kỷ luật (!Contains)
        var danhSachDuDieuKien = await (
            from d in _db.Set<DiemRenLuyen>()
            join sv in _db.Set<SinhVien>()
                on d.SinhVienId equals sv.Id
            where d.HocKyId == hocKyId
                  && d.Diem >= DiemToiThieu
                  && sv.TrangThai == TrangThaiSinhVien.DangHoc
                  && !danhSachKyLuatIds.Contains(d.SinhVienId)
            select d
        ).ToListAsync();

        var ketQua = new List<object>();

        // 4. Cấp học bổng cho các sinh viên đủ điều kiện
        foreach (var diem in danhSachDuDieuKien)
        {
            // Chặn cấp trùng: Bỏ qua nếu sinh viên đã được cấp học bổng trong kỳ này rồi
            var daCoHocBong = await _db.Set<HocBong>()
                .AnyAsync(x =>
                    x.SinhVienId == diem.SinhVienId &&
                    x.HocKyId == hocKyId);

            if (daCoHocBong)
                continue;

            var hocBong = new HocBong
            {
                SinhVienId = diem.SinhVienId,
                HocKyId = hocKyId,
                Ten = "Học bổng khuyến khích",
                SoTien = SoTienHocBong
            };

            _db.Set<HocBong>().Add(hocBong);

            ketQua.Add(new
            {
                diem.SinhVienId,
                diem.Diem,
                hocBong.Ten,
                hocBong.SoTien
            });
        }

        await _db.SaveChangesAsync();

        return new
        {
            HocKyId = hocKyId,
            TenHocKy = hocKy.Ten,
            DiemToiThieu,
            SoTienHocBong,
            SoLuongHocBong = ketQua.Count,
            DanhSach = ketQua
        };
    }

    // SA-06: KHEN THƯỞNG / KỶ LUẬT
    public async Task<object?> CreateKhenThuongKyLuatAsync(
    KhenThuongKyLuatRequest request)
    {
        // 1. Kiểm tra ID sinh viên > 0
        if (request.SinhVienId <= 0)
            throw new ArgumentException("SinhVienId phải lớn hơn 0.");

        // 2. Kiểm tra nội dung không được để trống
        if (string.IsNullOrWhiteSpace(request.NoiDung))
            throw new ArgumentException("Nội dung không được để trống.");

        // 3. Giới hạn độ dài chuỗi theo đúng cấu hình CSDL để chống crash lỗi 500 (String truncation)
        if (request.NoiDung.Length > 1000)
            throw new ArgumentException("Nội dung quyết định không được vượt quá 1000 ký tự.");

        if (request.SoQuyetDinh?.Length > 50)
            throw new ArgumentException("Số quyết định không được vượt quá 50 ký tự.");

        // 4. Kiểm tra loại quyết định phải thuộc Enum (KhenThuong hoặc KyLuat)
        if (!Enum.IsDefined(request.Loai))
            throw new ArgumentException("Loại quyết định không hợp lệ.");

        // 5. Kiểm tra sinh viên có tồn tại trong hệ thống không
        var sinhVien = await _db.Set<SinhVien>()
            .FirstOrDefaultAsync(x => x.Id == request.SinhVienId);

        if (sinhVien == null)
            throw new KeyNotFoundException("Không tìm thấy sinh viên.");

        // 6. Lưu quyết định khen thưởng / kỷ luật
        var quyetDinh = new KhenThuongKyLuat
        {
            SinhVienId = request.SinhVienId,
            Loai = request.Loai,
            SoQuyetDinh = request.SoQuyetDinh,
            NgayQuyetDinh = request.NgayQuyetDinh,
            NoiDung = request.NoiDung.Trim()
        };

        _db.Set<KhenThuongKyLuat>().Add(quyetDinh);
        await _db.SaveChangesAsync();

        return new
        {
            quyetDinh.Id,
            quyetDinh.SinhVienId,
            HoTen = sinhVien.HoTen,
            Loai = quyetDinh.Loai.ToString(),
            quyetDinh.SoQuyetDinh,
            quyetDinh.NgayQuyetDinh,
            quyetDinh.NoiDung
        };
    }
}
