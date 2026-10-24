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
  },

  thongBao: {
    list: (params) => listPaged('/thong-bao', params),
  },

  khoa: { getAll: () => listAll('/khoa') },
  nganh: { getAll: () => listAll('/nganh') },
  lopHanhChinh: { getAll: () => listAll('/lop-hanh-chinh') },
  giangVien: { getAll: () => listAll('/giang-vien') },
  phongHoc: { getAll: () => listAll('/phong-hoc') },
  hocKy: { getAll: () => listAll('/hoc-ky') },

  students: {
    /**
     * @param {{ page?: number, pageSize?: number, search?: string, khoaId?: number, trangThai?: string }} [params]
     */
    list({ khoaId, trangThai, ...params } = {}) {
      return listPaged('/sinh-vien', { ...params, KhoaId: khoaId, TrangThai: trangThai });
    },
    get: (id) => request(`/sinh-vien/${id}`),
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
