/**
 * Lớp API của StuM: gọi backend StudentManagement thật (xem config.js).
 * Các hàm trả về đúng `data` của ApiResponse. Danh sách có phân trang trả về
 * { data, page, pageSize, total } để các trang dùng chung một dạng.
 *
 * Backend giới hạn PageSize tối đa 100; listAll dùng giới hạn này để lấy danh mục đầy đủ.
 */

import { ApiError, request, session, toSession } from './http.js';

export { ApiError, session };

const PAGE_SIZE_MAX = 100;

/**
 * @param {object} result PagedResult của backend
 * @returns {{ data: object[], page: number, pageSize: number, total: number }}
 */
function toPaged(result) {
  return {
    data: result.items,
    page: result.page,
    pageSize: result.pageSize,
    total: result.totalCount,
  };
}

/**
 * @param {string} path
 * @param {{ page?: number, pageSize?: number, search?: string } & Record<string, unknown>} [params]
 */
function listPaged(path, { page = 1, pageSize = 10, search, ...filters } = {}) {
  return request(path, { query: { Page: page, PageSize: pageSize, Search: search, ...filters } }).then(toPaged);
}

/**
 * Lấy toàn bộ danh mục (dùng cho ô chọn). Chỉ phù hợp với danh mục nhỏ.
 * @param {string} path
 * @param {Record<string, unknown>} [query]
 * @returns {Promise<object[]>}
 */
async function listAll(path, query = {}) {
  const result = await request(path, { query: { ...query, Page: 1, PageSize: PAGE_SIZE_MAX } });
  return result.items;
}

/**
 * CRUD ở một đường dẫn bất kỳ (dùng cho các danh mục có tiền tố như /tot-nghiep/...).
 * @param {string} path
 * @param {{ noDelete?: boolean }} [options]
 */
function crudAt(path, options = {}) {
  const base = {
    list: (params) => listPaged(path, params),
    get: (id) => request(`${path}/${id}`),
    create: (payload) => request(path, { method: 'POST', body: payload }),
    update: (id, payload) => request(`${path}/${id}`, { method: 'PUT', body: payload }),
  };
  return options.noDelete ? base : { ...base, remove: (id) => request(`${path}/${id}`, { method: 'DELETE' }) };
}

/**
 * CRUD chuẩn của một danh mục: danh sách phân trang, lấy đầy đủ, chi tiết, tạo, sửa, xóa.
 * @param {string} path
 */
function crud(path) {
  return {
    list: (params) => listPaged(path, params),
    getAll: () => listAll(path),
    get: (id) => request(`${path}/${id}`),
    create: (payload) => request(path, { method: 'POST', body: payload }),
    update: (id, payload) => request(`${path}/${id}`, { method: 'PUT', body: payload }),
    remove: (id) => request(`${path}/${id}`, { method: 'DELETE' }),
  };
}

export const api = {
  auth: {
    /**
     * @param {string} tenDangNhap
     * @param {string} matKhau
     */
    async login(tenDangNhap, matKhau) {
      const data = await request('/auth/login', {
        method: 'POST',
        body: { tenDangNhap, matKhau },
        auth: false,
      });
      session.set(toSession(data));
      return data;
    },

    /** Đăng xuất phiên hiện tại ở server (nếu được) rồi xóa phiên cục bộ. */
    async logout() {
      const current = session.get();
      if (current?.refreshToken) {
        try {
          await request('/auth/logout', { method: 'POST', body: { refreshToken: current.refreshToken } });
        } catch {
          // Phiên có thể đã hết hạn ở server; vẫn xóa phiên cục bộ bên dưới
        }
      }
      session.clear();
    },

    getSession: () => session.get(),
  },

  baoCao: {
    /** Số liệu tổng quan cho Dashboard (RPT-05). */
    tongQuan: () => request('/bao-cao/tong-quan'),
    /** Thống kê sinh viên theo khoa, ngành và trạng thái (RPT-01). */
    sinhVien: () => request('/bao-cao/sinh-vien'),
    /** Kết quả học tập của học kỳ: GPA, xếp loại, điểm chữ, theo môn (RPT-02). */
    ketQuaHocTap: (hocKyId) => request(`/bao-cao/ket-qua-hoc-tap/${hocKyId}`),
    /** Công nợ học phí của học kỳ, danh sách còn nợ có phân trang (RPT-03). */
    congNo: (hocKyId, params) => listPaged(`/bao-cao/cong-no/${hocKyId}`, params),
    /** Sinh viên cảnh báo học vụ của học kỳ (RPT-04). */
    canhBao: (hocKyId, params) => listPaged(`/bao-cao/canh-bao-hoc-vu/${hocKyId}`, params),
  },

  thongBao: crud('/thong-bao'),

  khoa: crud('/khoa'),
  nganh: crud('/nganh'),
  lopHanhChinh: {
    ...crud('/lop-hanh-chinh'),
    /** Gán danh sách sinh viên vào lớp hành chính. */
    phanLop: (id, sinhVienIds) => request(`/lop-hanh-chinh/${id}/phan-lop`, { method: 'POST', body: { sinhVienIds } }),
    /** Gán cố vấn học tập cho lớp (coVanId null để bỏ). */
    coVan: (id, coVanId) => request(`/lop-hanh-chinh/${id}/co-van`, { method: 'PUT', body: { coVanId } }),
  },
  giangVien: crud('/giang-vien'),
  phongHoc: { getAll: () => listAll('/phong-hoc') },
  hocKy: crud('/hoc-ky'),
  boMon: crud('/bo-mon'),
  chuyenNganh: crud('/chuyen-nganh'),
  namHoc: crud('/nam-hoc'),
  quyCheHocVu: crud('/quy-che'),
  chuongTrinhDaoTao: {
    ...crud('/chuong-trinh-dao-tao'),
    monHoc: (id) => request(`/chuong-trinh-dao-tao/${id}/mon-hoc`),
    themMonHoc: (id, payload) => request(`/chuong-trinh-dao-tao/${id}/mon-hoc`, { method: 'POST', body: payload }),
    capNhatMonHoc: (id, monHocId, payload) =>
      request(`/chuong-trinh-dao-tao/${id}/mon-hoc/${monHocId}`, { method: 'PUT', body: payload }),
    boMonHoc: (id, monHocId) => request(`/chuong-trinh-dao-tao/${id}/mon-hoc/${monHocId}`, { method: 'DELETE' }),
  },

  students: {
    /**
     * @param {{ page?: number, pageSize?: number, search?: string, khoaId?: number, trangThai?: string }} [params]
     */
    list({ khoaId, trangThai, ...params } = {}) {
      return listPaged('/sinh-vien', { ...params, KhoaId: khoaId, TrangThai: trangThai });
    },
    get: (id) => request(`/sinh-vien/${id}`),
    /** Hồ sơ của chính sinh viên đang đăng nhập (vai trò SinhVien). */
    me: () => request('/sinh-vien/toi'),
    create: (payload) => request('/sinh-vien', { method: 'POST', body: payload }),
    update: (id, payload) => request(`/sinh-vien/${id}`, { method: 'PUT', body: payload }),
    remove: (id) => request(`/sinh-vien/${id}`, { method: 'DELETE' }),
    /**
     * Chuyển trạng thái học tập bằng hành động nghiệp vụ.
     * @param {number} id
     * @param {'bao-luu'|'thoi-hoc'|'quay-lai-hoc'} action
     * @param {object} [body]
     */
    changeStatus: (id, action, body = {}) =>
      request(`/sinh-vien/${id}/${action}`, { method: 'POST', body }),
  },

  monHoc: {
    list: (params) => listPaged('/mon-hoc', params),
    getAll: () => listAll('/mon-hoc'),
    create: (payload) => request('/mon-hoc', { method: 'POST', body: payload }),
    update: (id, payload) => request(`/mon-hoc/${id}`, { method: 'PUT', body: payload }),
    remove: (id) => request(`/mon-hoc/${id}`, { method: 'DELETE' }),
    tienQuyet: (id) => request(`/mon-hoc/${id}/tien-quyet`),
    themTienQuyet: (id, tienQuyetId) =>
      request(`/mon-hoc/${id}/tien-quyet`, { method: 'POST', body: { tienQuyetId } }),
    boTienQuyet: (id, tienQuyetId) =>
      request(`/mon-hoc/${id}/tien-quyet/${tienQuyetId}`, { method: 'DELETE' }),
  },

  lopHocPhan: {
    list({ hocKyId, monHocId, trangThai, ...params } = {}) {
      return listPaged('/lop-hoc-phan', {
        ...params,
        HocKyId: hocKyId,
        MonHocId: monHocId,
        TrangThai: trangThai,
      });
    },
    getAll: () => listAll('/lop-hoc-phan'),
    create: (payload) => request('/lop-hoc-phan', { method: 'POST', body: payload }),
    update: (id, payload) => request(`/lop-hoc-phan/${id}`, { method: 'PUT', body: payload }),
    remove: (id) => request(`/lop-hoc-phan/${id}`, { method: 'DELETE' }),
    /** @param {number|null} giangVienId null để bỏ phân công */
    phanCongGiangVien: (id, giangVienId) =>
      request(`/lop-hoc-phan/${id}/giang-vien`, { method: 'PUT', body: { giangVienId } }),
    batDauHoc: (id) => request(`/lop-hoc-phan/${id}/bat-dau-hoc`, { method: 'POST' }),
    ketThuc: (id) => request(`/lop-hoc-phan/${id}/ket-thuc`, { method: 'POST' }),
    huy: (id) => request(`/lop-hoc-phan/${id}/huy`, { method: 'POST' }),
    lichHoc: (id) => request(`/lop-hoc-phan/${id}/lich-hoc`),
    themLichHoc: (id, payload) => request(`/lop-hoc-phan/${id}/lich-hoc`, { method: 'POST', body: payload }),
    xoaLichHoc: (id, lichHocId) =>
      request(`/lop-hoc-phan/${id}/lich-hoc/${lichHocId}`, { method: 'DELETE' }),
  },

  /** Tốt nghiệp (module Graduation). */
  totNghiep: {
    dieuKien: (params) => listPaged('/tot-nghiep/dieu-kien', params),
    dieuKienCuaSinhVien: (sinhVienId) => request(`/tot-nghiep/dieu-kien/${sinhVienId}`),
    xetList: ({ ketQua, daTotNghiep, ...params } = {}) =>
      listPaged('/tot-nghiep/xet', { ...params, KetQua: ketQua, DaTotNghiep: daTotNghiep }),
    xetChiTiet: (sinhVienId) => request(`/tot-nghiep/xet/${sinhVienId}`),
    /** Chạy xét tốt nghiệp cho một sinh viên (ghi kết quả vào lịch sử). */
    xet: (sinhVienId) => request(`/tot-nghiep/xet/${sinhVienId}`, { method: 'POST' }),
    coSo: {
      ...crudAt('/tot-nghiep/co-so-thuc-tap'),
      trangThai: (id, dangTiepNhan) =>
        request(`/tot-nghiep/co-so-thuc-tap/${id}/trang-thai`, { method: 'PUT', body: { dangTiepNhan } }),
    },
    deTai: {
      ...crudAt('/tot-nghiep/de-tai', { noDelete: true }),
      dangKy: (id, sinhVienId) =>
        request(`/tot-nghiep/de-tai/${id}/dang-ky`, { method: 'POST', body: { sinhVienId } }),
      phanGiangVien: (id, giangVienHuongDanId) =>
        request(`/tot-nghiep/de-tai/${id}/giang-vien`, { method: 'PUT', body: { giangVienHuongDanId } }),
    },
    hoiDong: {
      ...crudAt('/tot-nghiep/hoi-dong', { noDelete: true }),
      deTai: (id, params) => listPaged(`/tot-nghiep/hoi-dong/${id}/de-tai`, params),
      phanDeTai: (id, deTaiId) => request(`/tot-nghiep/hoi-dong/${id}/de-tai/${deTaiId}`, { method: 'PUT' }),
    },
    thucTap: {
      ...crudAt('/tot-nghiep/thuc-tap', { noDelete: true }),
      capNhat: (id, payload) => request(`/tot-nghiep/thuc-tap/${id}`, { method: 'PUT', body: payload }),
      huy: (id) => request(`/tot-nghiep/thuc-tap/${id}/huy`, { method: 'POST' }),
      ketQua: (id) => request(`/tot-nghiep/thuc-tap/${id}/ket-qua`),
      nhapKetQua: (id, payload) => request(`/tot-nghiep/thuc-tap/${id}/ket-qua`, { method: 'PUT', body: payload }),
    },
    ketQuaBaoVe: {
      list: (params) => listPaged('/tot-nghiep/ket-qua-bao-ve', params),
      get: (deTaiId) => request(`/tot-nghiep/ket-qua-bao-ve/${deTaiId}`),
      nhap: (deTaiId, payload) => request(`/tot-nghiep/ket-qua-bao-ve/${deTaiId}`, { method: 'PUT', body: payload }),
    },
  },

  /** Công tác sinh viên (module StudentAffairs). */
  congTacSinhVien: {
    hocPhi: {
      list: ({ conNo, hocKyId, sinhVienId, ...params } = {}) =>
        listPaged('/cong-tac-sinh-vien/hoc-phi', { ...params, ConNo: conNo, HocKyId: hocKyId, SinhVienId: sinhVienId }),
      get: (id) => request(`/cong-tac-sinh-vien/hoc-phi/${id}`),
      /** Tính học phí cho cả học kỳ từ các lớp đã đăng ký. */
      tinh: (hocKyId) => request(`/cong-tac-sinh-vien/hoc-phi/tinh/${hocKyId}`, { method: 'POST' }),
      thanhToan: (id, payload) => request(`/cong-tac-sinh-vien/hoc-phi/${id}/thanh-toan`, { method: 'POST', body: payload }),
      lichSuThanhToan: (id) => request(`/cong-tac-sinh-vien/hoc-phi/${id}/thanh-toan`),
    },
    hocBong: {
      list: ({ hocKyId, sinhVienId, ...params } = {}) =>
        listPaged('/cong-tac-sinh-vien/hoc-bong', { ...params, HocKyId: hocKyId, SinhVienId: sinhVienId }),
      xet: (hocKyId) => request(`/cong-tac-sinh-vien/hoc-bong/xet/${hocKyId}`, { method: 'POST' }),
      remove: (id) => request(`/cong-tac-sinh-vien/hoc-bong/${id}`, { method: 'DELETE' }),
    },
    mienGiam: crudAt('/cong-tac-sinh-vien/mien-giam'),
    diemRenLuyen: crudAt('/cong-tac-sinh-vien/diem-ren-luyen'),
    khenThuongKyLuat: crudAt('/cong-tac-sinh-vien/khen-thuong-ky-luat'),
  },

  /** Phúc khảo điểm (module Grades). */
  phucKhao: {
    list: ({ trangThai, ...params } = {}) => listPaged('/phuc-khao', { ...params, trangThai }),
    get: (id) => request(`/phuc-khao/${id}`),
    create: (payload) => request('/phuc-khao', { method: 'POST', body: payload }),
    /** @param {{ chapNhan: boolean, diemMoi?: number }} payload */
    xuLy: (id, payload) => request(`/phuc-khao/${id}/xu-ly`, { method: 'POST', body: payload }),
  },

  /** Quản trị tài khoản (module Auth): chỉ Admin. */
  taiKhoan: {
    list: (params) => listPaged('/auth/accounts', params),
    get: (id) => request(`/auth/accounts/${id}`),
    create: (payload) => request('/auth/accounts', { method: 'POST', body: payload }),
    doiVaiTro: (id, vaiTro) => request(`/auth/accounts/${id}/role`, { method: 'PUT', body: { vaiTro } }),
    lienKetSinhVien: (id, sinhVienId) =>
      request(`/auth/accounts/${id}/student`, { method: 'PUT', body: { sinhVienId } }),
    lienKetGiangVien: (id, giangVienId) =>
      request(`/auth/accounts/${id}/lecturer`, { method: 'PUT', body: { giangVienId } }),
    khoa: (id) => request(`/auth/accounts/${id}/lock`, { method: 'POST' }),
    moKhoa: (id) => request(`/auth/accounts/${id}/unlock`, { method: 'POST' }),
    datLaiMatKhau: (id, matKhauMoi) =>
      request(`/auth/accounts/${id}/reset-password`, { method: 'POST', body: { matKhauMoi } }),
    /** Nhật ký hoạt động (chỉ Admin). Lọc: taiKhoanId, hanhDong, thanhCong, tuNgay, denNgay. */
    nhatKy({ taiKhoanId, hanhDong, thanhCong, tuNgay, denNgay, ...params } = {}) {
      return listPaged('/auth/activity-logs', {
        ...params,
        TaiKhoanId: taiKhoanId,
        HanhDong: hanhDong,
        ThanhCong: thanhCong,
        TuNgay: tuNgay,
        DenNgay: denNgay,
      });
    },
  },

  /** Tài khoản đang đăng nhập: đổi mật khẩu, đăng xuất mọi thiết bị. */
  phien: {
    me: () => request('/auth/me'),
    doiMatKhau: (matKhauCu, matKhauMoi) =>
      request('/auth/change-password', { method: 'POST', body: { matKhauCu, matKhauMoi } }),
    dangXuatTatCa: () => request('/auth/logout-all', { method: 'POST' }),
  },

  dotDangKy: {
    list({ hocKyId, trangThai, ...params } = {}) {
      return listPaged('/dot-dang-ky', { ...params, HocKyId: hocKyId, TrangThai: trangThai });
    },
    create: (payload) => request('/dot-dang-ky', { method: 'POST', body: payload }),
    update: (id, payload) => request(`/dot-dang-ky/${id}`, { method: 'PUT', body: payload }),
    mo: (id) => request(`/dot-dang-ky/${id}/mo`, { method: 'POST' }),
    dong: (id) => request(`/dot-dang-ky/${id}/dong`, { method: 'POST' }),
    remove: (id) => request(`/dot-dang-ky/${id}`, { method: 'DELETE' }),
  },

  dangKy: {
    /** Lớp mở đăng ký kèm điều kiện của sinh viên (LopMoDto). */
    lopMo({ sinhVienId, hocKyId, monHocId, search, page = 1, pageSize = 100 } = {}) {
      return listPaged('/dang-ky-hoc-phan/lop-mo', {
        page,
        pageSize,
        search,
        SinhVienId: sinhVienId,
        HocKyId: hocKyId,
        MonHocId: monHocId,
      });
    },
    /** Học phần đã đăng ký trong học kỳ (DaDangKyDto). */
    daDangKy: (sinhVienId, hocKyId) =>
      request('/dang-ky-hoc-phan/da-dang-ky', { query: { SinhVienId: sinhVienId, HocKyId: hocKyId } }),
    /** Thời khóa biểu của sinh viên trong học kỳ (ThoiKhoaBieuDto). */
    thoiKhoaBieu: (sinhVienId, hocKyId) =>
      request('/dang-ky-hoc-phan/thoi-khoa-bieu', { query: { SinhVienId: sinhVienId, HocKyId: hocKyId } }),
    /**
     * @param {{ sinhVienId?: number, lopHocPhanId: number, xacNhanHocCaiThien?: boolean }} payload
     * sinhVienId bắt buộc với nhân viên; sinh viên tự đăng ký thì bỏ qua.
     */
    dangKy: ({ sinhVienId, lopHocPhanId, xacNhanHocCaiThien = false }) =>
      request('/dang-ky-hoc-phan', {
        method: 'POST',
        body: { sinhVienId: sinhVienId ?? null, lopHocPhanId, xacNhanHocCaiThien },
      }),
    huy: (dangKyId) => request(`/dang-ky-hoc-phan/${dangKyId}`, { method: 'DELETE' }),
    /** Theo dõi sĩ số các lớp trong học kỳ (SiSoLopDto). */
    siSo({ hocKyId, monHocId, tinhTrang, page = 1, pageSize = 100, search } = {}) {
      return listPaged('/dang-ky-hoc-phan/si-so', {
        page,
        pageSize,
        search,
        HocKyId: hocKyId,
        MonHocId: monHocId,
        TinhTrang: tinhTrang,
      });
    },
    capNhatSiSo: (lopHocPhanId, siSoToiDa) =>
      request(`/dang-ky-hoc-phan/si-so/${lopHocPhanId}`, { method: 'PUT', body: { siSoToiDa } }),
  },

  /** Sinh viên trong một lớp học phần (module Grades). */
  sinhVienLopHocPhan: {
    list: (lopHocPhanId, params) => listPaged(`/diem/lop-hoc-phan/${lopHocPhanId}/sinh-vien`, params),
    them: (lopHocPhanId, sinhVienId) =>
      request(`/diem/lop-hoc-phan/${lopHocPhanId}/sinh-vien`, { method: 'POST', body: { sinhVienId } }),
    xoa: (lopHocPhanId, dangKyHocPhanId) =>
      request(`/diem/lop-hoc-phan/${lopHocPhanId}/sinh-vien/${dangKyHocPhanId}`, { method: 'DELETE' }),
    /** Chuyển sinh viên sang lớp học phần khác cùng học kỳ. */
    chuyenLop: (lopHocPhanId, dangKyHocPhanId, lopHocPhanMoiId) =>
      request(`/diem/lop-hoc-phan/${lopHocPhanId}/sinh-vien/${dangKyHocPhanId}/chuyen-lop`, {
        method: 'POST',
        body: { lopHocPhanMoiId },
      }),
  },

  /** Bảng điểm đầy đủ của một sinh viên theo học kỳ (BangDiemSinhVienDto). */
  bangDiemSinhVien: (sinhVienId) => request(`/diem/sinh-vien/${sinhVienId}`),

  diem: {
    /** Bảng điểm của một lớp học phần (BangDiemLopDto). */
    bangDiem: (lopHocPhanId) => request(`/diem/lop-hoc-phan/${lopHocPhanId}`),
    /** @param {Array<{ dangKyHocPhanId: number, diemQuaTrinh: number, diemGiuaKy: number, diemCuoiKy: number }>} danhSachDiem */
    nhapDiem: (lopHocPhanId, danhSachDiem) =>
      request(`/diem/lop-hoc-phan/${lopHocPhanId}`, { method: 'PUT', body: { danhSachDiem } }),
    tinhDiem: (lopHocPhanId) => request(`/diem/lop-hoc-phan/${lopHocPhanId}/tinh-diem`, { method: 'POST' }),
    khoaDiem: (lopHocPhanId) => request(`/diem/lop-hoc-phan/${lopHocPhanId}/khoa`, { method: 'POST' }),
  },
};
