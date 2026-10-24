/**
 * DOM helpers dùng chung cho toàn bộ ứng dụng StuM.
 * Không phụ thuộc thư viện ngoài.
 */

/**
 * Lấy phần tử đầu tiên khớp selector.
 * @param {string} selector
 * @param {ParentNode} [root=document]
 * @returns {Element|null}
 */
export function $(selector, root = document) {
  return root.querySelector(selector);
}

/**
 * Lấy toàn bộ phần tử khớp selector dưới dạng mảng.
 * @param {string} selector
 * @param {ParentNode} [root=document]
 * @returns {Element[]}
 */
export function $$(selector, root = document) {
  return Array.from(root.querySelectorAll(selector));
}

const HTML_ESCAPE_MAP = {
  '&': '&amp;',
  '<': '&lt;',
  '>': '&gt;',
  '"': '&quot;',
  "'": '&#39;',
};

/**
 * Escape chuỗi trước khi chèn vào HTML. BẮT BUỘC dùng cho mọi dữ liệu người dùng.
 * @param {unknown} value
 * @returns {string}
 */
export function escapeHtml(value) {
  if (value === null || value === undefined) return '';
  return String(value).replace(/[&<>"']/g, (char) => HTML_ESCAPE_MAP[char]);
}

/**
 * Tạo phần tử DOM theo cú pháp khai báo.
 * - attrs: thuộc tính HTML; key "class" nhận chuỗi hoặc mảng; key "dataset" nhận object;
 *   key bắt đầu bằng "on" (vd: onclick) được gắn làm event listener.
 * - children: Node, chuỗi (sẽ thành text node, không parse HTML) hoặc mảng các giá trị trên.
 * @param {string} tag
 * @param {Record<string, unknown>} [attrs={}]
 * @param {Array<Node|string|null|undefined|false>|Node|string} [children=[]]
 * @returns {HTMLElement}
 */
export function el(tag, attrs = {}, children = []) {
  const node = document.createElement(tag);

  Object.entries(attrs).forEach(([key, value]) => {
    if (value === null || value === undefined || value === false) return;

    if (key === 'class') {
      node.className = Array.isArray(value) ? value.filter(Boolean).join(' ') : String(value);
    } else if (key === 'dataset') {
      Object.entries(value).forEach(([dataKey, dataValue]) => {
        node.dataset[dataKey] = String(dataValue);
      });
    } else if (key.startsWith('on') && typeof value === 'function') {
      node.addEventListener(key.slice(2).toLowerCase(), value);
    } else {
      node.setAttribute(key, value === true ? '' : String(value));
    }
  });

  const list = Array.isArray(children) ? children : [children];
  list.forEach((child) => {
    if (child === null || child === undefined || child === false) return;
    node.append(child instanceof Node ? child : document.createTextNode(String(child)));
  });

  return node;
}
