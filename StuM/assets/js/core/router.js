/**
 * Router đơn giản cho StuM.
 * Mỗi trang là một file HTML riêng (theo cấu trúc dự án), nên router chỉ lo:
 * - điều hướng giữa các trang bằng tên route
 * - đọc và cập nhật query string (ví dụ ?q=...&khoa=...) để giữ trạng thái lọc khi tải lại
 */

export const ROUTES = {
  login: 'index.html',
  dashboard: 'dashboard.html',
  students: 'students.html',
  courses: 'courses.html',
  classes: 'classes.html',
  grades: 'grades.html',
  registration: 'registration.html',
  accounts: 'accounts.html',
  academic: 'academic.html',
  'ui-kit': 'ui-kit.html',
};

export const router = {
  /** @returns {string} tên trang hiện tại, lấy từ data-page của <body> */
  current() {
    return document.body.dataset.page || 'dashboard';
  },

  /**
   * @param {string} route
   * @param {Record<string, string>} [query]
   */
  navigate(route, query = {}) {
    const search = new URLSearchParams(query).toString();
    window.location.href = ROUTES[route] + (search ? `?${search}` : '');
  },

  /** @returns {Record<string, string>} */
  query() {
    return Object.fromEntries(new URLSearchParams(window.location.search));
  },

  /**
   * Ghi query string mà không tải lại trang. Giá trị rỗng/null sẽ bị bỏ.
   * @param {Record<string, string|number|null|undefined>} values
   */
  setQuery(values) {
    const params = new URLSearchParams(window.location.search);
    Object.entries(values).forEach(([key, value]) => {
      if (value === null || value === undefined || value === '') {
        params.delete(key);
      } else {
        params.set(key, String(value));
      }
    });
    const search = params.toString();
    window.history.replaceState(null, '', window.location.pathname + (search ? `?${search}` : ''));
  },
};
