/**
 * Wrapper quanh localStorage: tự động prefix key, serialize JSON,
 * và không làm crash app khi storage bị chặn (private mode, hết quota...).
 */

const PREFIX = 'stum:';

export const Storage = {
  /**
   * Đọc giá trị đã lưu.
   * @param {string} key
   * @param {unknown} [fallback=null] giá trị trả về nếu không có hoặc lỗi
   * @returns {unknown}
   */
  get(key, fallback = null) {
    try {
      const raw = window.localStorage.getItem(PREFIX + key);
      return raw === null ? fallback : JSON.parse(raw);
    } catch (error) {
      console.warn(`[Storage] Không đọc được key "${key}"`, error);
      return fallback;
    }
  },

  /**
   * Lưu giá trị (serialize JSON).
   * @param {string} key
   * @param {unknown} value
   * @returns {boolean} true nếu lưu thành công
   */
  set(key, value) {
    try {
      window.localStorage.setItem(PREFIX + key, JSON.stringify(value));
      return true;
    } catch (error) {
      console.warn(`[Storage] Không ghi được key "${key}"`, error);
      return false;
    }
  },

  /**
   * Xóa một key.
   * @param {string} key
   */
  remove(key) {
    try {
      window.localStorage.removeItem(PREFIX + key);
    } catch (error) {
      console.warn(`[Storage] Không xóa được key "${key}"`, error);
    }
  },

  /**
   * Kiểm tra key có tồn tại hay không.
   * @param {string} key
   * @returns {boolean}
   */
  has(key) {
    try {
      return window.localStorage.getItem(PREFIX + key) !== null;
    } catch {
      return false;
    }
  },

  /** Xóa toàn bộ key thuộc StuM (không đụng key của app khác trên cùng origin). */
  clear() {
    try {
      Object.keys(window.localStorage)
        .filter((key) => key.startsWith(PREFIX))
        .forEach((key) => window.localStorage.removeItem(key));
    } catch (error) {
      console.warn('[Storage] Không xóa được dữ liệu StuM', error);
    }
  },
};
