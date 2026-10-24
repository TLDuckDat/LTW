/**
 * Lớp gọi API thật: gắn JWT, tự làm mới phiên khi access token hết hạn (401),
 * và đọc định dạng ApiResponse { success, message, data } của backend.
 *
 * Refresh token của backend chỉ dùng được một lần, nên chỉ có một lần làm mới đang chạy tại một thời điểm;
 * các request khác chờ cùng kết quả (xem refreshSession).
 *
 * Thêm ?error vào URL để mọi request báo lỗi máy chủ (dùng để kiểm thử trạng thái lỗi).
 */

import { API_BASE_URL } from './config.js';
import { Storage } from './storage.js';

const SESSION_KEY = 'session';

export class ApiError extends Error {
  /**
   * @param {string} message
   * @param {number} [status=500]
   */
  constructor(message, status = 500) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

export const session = {
  /** @returns {{ token: string, refreshToken: string, name: string, vaiTro: string, tenDangNhap: string } | null} */
  get() {
    return Storage.get(SESSION_KEY, null);
  },
  set(value) {
    Storage.set(SESSION_KEY, value);
  },
  clear() {
    Storage.remove(SESSION_KEY);
  },
};

/**
 * Chuyển LoginResponse của backend sang dạng lưu trong trình duyệt.
 * @param {object} login
 */
export function toSession(login) {
  return {
    token: login.token,
    refreshToken: login.refreshToken,
    expiresAt: login.expiresAt,
    refreshTokenExpiresAt: login.refreshTokenExpiresAt,
    taiKhoanId: login.taiKhoanId,
    tenDangNhap: login.tenDangNhap,
    name: login.hoTen,
    vaiTro: login.vaiTro,
  };
}

function buildUrl(path, query) {
  const params = new URLSearchParams();
  Object.entries(query ?? {}).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') params.set(key, String(value));
  });
  const search = params.toString();
  return `${API_BASE_URL}${path}${search ? `?${search}` : ''}`;
}

/**
 * Gửi một request và trả về `data` của ApiResponse. Ném ApiError khi thất bại.
 */
async function send(path, { method, query, body, token }) {
  const headers = { Accept: 'application/json' };
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  if (token) headers.Authorization = `Bearer ${token}`;

  let response;
  try {
    response = await fetch(buildUrl(path, query), {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  } catch {
    throw new ApiError(`Không kết nối được máy chủ (${API_BASE_URL}). Kiểm tra backend đã chạy chưa.`, 0);
  }

  const payload = await response.json().catch(() => null);
  if (!response.ok || payload?.success === false) {
    throw new ApiError(payload?.message ?? 'Yêu cầu không thành công', response.status);
  }
  return payload?.data ?? null;
}

let refreshing = null;

/**
 * Làm mới phiên bằng refresh token. Nhiều request cùng gặp 401 vẫn chỉ gọi refresh một lần.
 * @returns {Promise<void>}
 */
function refreshSession() {
  if (!refreshing) {
    refreshing = (async () => {
      const current = session.get();
      if (!current?.refreshToken) throw new ApiError('Phiên đăng nhập đã hết hạn.', 401);
      const data = await send('/auth/refresh', {
        method: 'POST',
        body: { refreshToken: current.refreshToken },
      });
      session.set(toSession(data));
    })().finally(() => {
      refreshing = null;
    });
  }
  return refreshing;
}

function redirectToLogin() {
  session.clear();
  window.location.replace('index.html');
}

/**
 * @param {string} path đường dẫn sau /api, ví dụ '/sinh-vien'
 * @param {{ method?: string, query?: Record<string, unknown>, body?: unknown, auth?: boolean }} [options]
 * @returns {Promise<any>} data của ApiResponse
 */
export async function request(path, { method = 'GET', query, body, auth = true } = {}) {
  if (new URLSearchParams(window.location.search).has('error')) {
    throw new ApiError('Không kết nối được máy chủ (chế độ kiểm thử ?error).', 503);
  }
  if (!auth) return send(path, { method, query, body });

  const token = session.get()?.token;
  try {
    return await send(path, { method, query, body, token });
  } catch (error) {
    if (error.status !== 401) throw error;
    try {
      await refreshSession();
    } catch {
      redirectToLogin();
      throw new ApiError('Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.', 401);
    }
    return send(path, { method, query, body, token: session.get()?.token });
  }
}
