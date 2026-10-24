/**
 * Helper định dạng dữ liệu hiển thị: ngày tháng và số.
 */

const NUMBER_FORMATTER = new Intl.NumberFormat('en-US');
const ISO_DATE_PATTERN = /^(\d{4})-(\d{2})-(\d{2})$/;

/**
 * Chuyển giá trị bất kỳ thành Date hợp lệ, hoặc null.
 * Chuỗi dạng "YYYY-MM-DD" được đọc theo giờ địa phương để tránh lệch một ngày.
 * @param {Date|string|number|null|undefined} value
 * @returns {Date|null}
 */
function toDate(value) {
  if (value === null || value === undefined || value === '') return null;

  let date;
  if (typeof value === 'string' && ISO_DATE_PATTERN.test(value)) {
    const [, year, month, day] = value.match(ISO_DATE_PATTERN);
    date = new Date(Number(year), Number(month) - 1, Number(day));
  } else {
    date = new Date(value);
  }

  return Number.isNaN(date.getTime()) ? null : date;
}

/**
 * Định dạng ngày theo dd/MM/yyyy. Trả về chuỗi rỗng nếu giá trị không hợp lệ.
 * @param {Date|string|number|null|undefined} value
 * @returns {string}
 */
export function formatDate(value) {
  const date = toDate(value);
  if (!date) return '';

  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  return `${day}/${month}/${date.getFullYear()}`;
}

/**
 * Định dạng số có dấu phân cách hàng nghìn (vd: 1,420).
 * Có thể giới hạn số chữ số thập phân khi truyền decimals.
 * @param {number|string|null|undefined} value
 * @param {number} [decimals]
 * @returns {string}
 */
export function formatNumber(value, decimals) {
  const number = Number(value);
  if (value === null || value === undefined || value === '' || Number.isNaN(number)) return '';

  if (decimals === undefined) return NUMBER_FORMATTER.format(number);

  return new Intl.NumberFormat('en-US', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(number);
}
