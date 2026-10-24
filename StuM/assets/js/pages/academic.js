/**
 * Trang Danh mục học vụ (chỉ Admin và Phòng đào tạo): Khoa, Bộ môn, Ngành, Chuyên ngành, Năm học,
 * Học kỳ, Quy chế, Chương trình đào tạo. Mỗi danh mục là một tab; bảng, form và bộ lọc dựng từ
 * academic-config.js. Chương trình đào tạo có thêm hộp thoại quản lý môn học trong CTĐT.
 */

import { api } from '../core/api.js';
import { renderAsync } from '../core/async-view.js';
import { debounce, escapeHtml, nextId } from '../core/dom.js';
import { Badge } from '../components/badge.js';
import { Button } from '../components/button.js';
import { EmptyState } from '../components/empty-state.js';
import { Input } from '../components/input.js';
import { Modal } from '../components/modal.js';
import { Pagination } from '../components/pagination.js';
import { Select } from '../components/select.js';
import { Skeleton } from '../components/skeleton.js';
import { Table } from '../components/table.js';
import { Toast } from '../components/toast.js';
import { renderPageHeader } from '../layout/shell.js';
import { DANH_MUC, toPayloadValue } from './academic-config.js';

const state = { key: 'khoa', page: 1, pageSize: 10, search: '', filters: {}, rows: [] };
let refs = {};

/** Tải các danh mục dùng làm lựa chọn (khóa ngoại) trong form và bộ lọc. */
/** Sinh viên để phân vào lớp hành chính (tải cùng danh mục tham chiếu). */
let students = [];

async function loadRefs() {
  const [khoa, nganh, namHoc, monHoc, boMon, giangVien, sv] = await Promise.all([
    api.khoa.getAll(),
    api.nganh.getAll(),
    api.namHoc.getAll(),
    api.monHoc.getAll(),
    api.boMon.getAll(),
    api.giangVien.getAll(),
    api.students.list({ pageSize: 100 }).then((r) => r.data),
  ]);
  const withLabel = (items, label) => items.map((item) => ({ value: item.id, label: label(item) }));
  refs = {
    khoa: withLabel(khoa, (i) => `${i.ma} - ${i.ten}`),
    nganh: withLabel(nganh, (i) => `${i.ma} - ${i.ten}`),
    namHoc: withLabel(namHoc, (i) => i.ten),
    monHoc: withLabel(monHoc, (i) => `${i.ma} - ${i.ten}`),
    boMon: withLabel(boMon, (i) => `${i.ma} - ${i.ten}`),
    giangVien: withLabel(giangVien, (i) => `${i.ma} - ${i.hoTen}`),
  };
  students = sv;
}

const current = () => DANH_MUC[state.key];

function tabsMarkup() {
  return `<nav class="tabs" role="tablist" aria-label="Danh mục học vụ">
    ${Object.entries(DANH_MUC)
      .map(([key, def]) => `<button type="button" class="tab" role="tab" data-tab="${key}" aria-selected="${key === state.key}">${escapeHtml(def.label)}</button>`)
      .join('')}
  </nav>`;
}

function filtersMarkup() {
  const def = current();
  const filterFields = def.filters.map((filter) =>
    Select.render({
      name: filter.name,
      label: filter.label,
      options: [{ value: '', label: `Tất cả ${filter.label.toLowerCase()}` }, ...(refs[filter.refKey] ?? [])],
      value: state.filters[filter.name] ?? '',
    }),
  );
  return `<div class="filter-bar" role="search">
    ${Input.render({ name: 'q', label: 'Tìm kiếm', value: state.search, placeholder: 'Mã hoặc tên', iconLeft: 'search' })}
    ${filterFields.join('')}
    ${Button.render({ label: 'Đặt lại', variant: 'ghost', action: 'reset' })}
  </div>`;
}

function rowActions() {
  const actions = [
    { action: 'edit', label: 'Chỉnh sửa', icon: 'pencil' },
    ...(current().extraRowActions ?? []),
    { divider: true },
    { action: 'delete', label: 'Xóa', icon: 'trash-2', danger: true },
  ];
  return actions;
}

function columnsOf(def) {
  return def.columns.map((column) => ({
    ...column,
    render: column.render ?? ((row) => escapeHtml(row[column.key] ?? '—')),
  }));
}

function loadList() {
  const def = current();
  return renderAsync(document.getElementById('list-container'), {
    load: async () => {
      const filters = {};
      def.filters.forEach((filter) => {
        if (state.filters[filter.name]) filters[filter.param] = state.filters[filter.name];
      });
      const result = await def.api().list({ page: state.page, pageSize: state.pageSize, search: state.search, ...filters });
      state.rows = result.data;
      return result;
    },
    skeleton: () => Skeleton.table(5, 4),
    isEmpty: (result) => result.total === 0,
    empty: () =>
      EmptyState.render({
        icon: 'folder-open',
        title: `Chưa có ${def.label.toLowerCase()} nào`,
        description: 'Thêm mục đầu tiên hoặc thay đổi bộ lọc.',
        actionText: `Thêm ${def.label.toLowerCase()}`,
        actionId: 'btn-add-empty',
        action: 'add',
      }),
    render: (result) =>
      Table.render({
        columns: columnsOf(def),
        rows: result.data,
        actions: rowActions(),
        actionLabel: (row) => `Thao tác cho ${row[def.columns[0].key] ?? row.id}`,
        caption: def.title,
        footer: Pagination.render({ page: result.page, pageSize: result.pageSize, total: result.total }),
      }),
    errorTitle: `Không tải được ${def.label.toLowerCase()}`,
  });
}

/** Form thêm/sửa theo cấu hình của danh mục. */
function openForm(record = null) {
  const def = current();
  const formId = nextId('acad-form');
  const isEdit = Boolean(record);
  const fieldsMarkup = def.fields
    .map((field) => {
      const value = record?.[field.name] ?? '';
      if (field.kind === 'ref') {
        return Select.render({ name: field.name, label: field.label, options: refs[field.refKey] ?? [], value, placeholder: 'Chọn', required: field.required });
      }
      if (field.kind === 'bool') {
        const current = record ? String(Boolean(record[field.name])) : 'false';
        return Select.render({ name: field.name, label: field.label, value: current, options: [{ value: 'true', label: 'Có' }, { value: 'false', label: 'Không' }] });
      }
      const type = field.kind === 'date' ? 'date' : field.kind === 'int' || field.kind === 'decimal' ? 'number' : 'text';
      return Input.render({ name: field.name, label: field.label, type, value: type === 'date' && value ? String(value).slice(0, 10) : value, required: field.required, hint: field.hint, step: field.kind === 'decimal' ? 0.01 : undefined });
    })
    .join('');

  const handle = Modal.open({
    title: `${isEdit ? 'Sửa' : 'Thêm'} ${def.label.toLowerCase()}`,
    size: 'lg',
    content: `<form class="modal-form" id="${formId}" novalidate><div class="form-grid">${fieldsMarkup}</div></form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: isEdit ? 'Lưu thay đổi' : 'Thêm', variant: 'primary', action: 'save' }),
    ].join(''),
  });
  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);

  saveButton.addEventListener('click', async () => {
    let valid = true;
    def.fields.forEach((field) => {
      const input = form.elements.namedItem(field.name);
      if (field.required && input.value.trim() === '') {
        Input.setError(input, 'Vui lòng nhập thông tin này');
        valid = false;
      } else {
        Input.clearError(input);
      }
    });
    if (!valid) return;

    const payload = {};
    def.fields.forEach((field) => {
      payload[field.name] = toPayloadValue(field, form.elements.namedItem(field.name).value);
    });
    if (def.fields.some((field) => field.kind === 'bool')) {
      def.fields.filter((field) => field.kind === 'bool').forEach((field) => {
        payload[field.name] = form.elements.namedItem(field.name).value === 'true';
      });
    }

    Button.setLoading(saveButton, true);
    try {
      if (isEdit) await def.api().update(record.id, payload);
      else await def.api().create(payload);
      Toast.success(isEdit ? 'Đã cập nhật' : 'Đã thêm');
      handle.close();
      refreshList();
    } catch (error) {
      Button.setLoading(saveButton, false);
      Toast.error(error.message);
    }
  });
}

function confirmDelete(record) {
  const def = current();
  Modal.confirm({
    title: `Xóa ${def.label.toLowerCase()}`,
    message: `Xóa "${record.ten ?? record.ma}"? Backend sẽ từ chối nếu mục đang được tham chiếu.`,
    confirmText: 'Xóa',
    danger: true,
    onConfirm: async () => {
      try {
        await def.api().remove(record.id);
        Toast.success('Đã xóa');
        refreshList();
      } catch (error) {
        Toast.error(error.message);
        throw error;
      }
    },
  }).catch(() => {});
}

function refreshList() {
  return loadList();
}

/**
 * Môn học trong chương trình đào tạo: liệt kê, thêm, sửa, bỏ.
 * @param {object} ctdt
 */
async function openCtdtSubjects(ctdt) {
  const formId = nextId('ctdt-form');
  const handle = Modal.open({
    title: `Môn học trong ${ctdt.ma}`,
    size: 'lg',
    content: `<div class="modal-form">
      <div data-ctdt-list><p class="form-hint">Đang tải...</p></div>
      <form class="form-grid" id="${formId}" novalidate>
        ${Select.render({ name: 'monHocId', label: 'Môn học', options: refs.monHoc, placeholder: 'Chọn môn', searchable: true, required: true })}
        ${Input.render({ name: 'hocKyGoiY', type: 'number', label: 'Học kỳ gợi ý', min: 1, step: 1, value: 1 })}
        ${Select.render({ name: 'batBuoc', label: 'Loại', value: 'true', options: [{ value: 'true', label: 'Bắt buộc' }, { value: 'false', label: 'Tự chọn' }] })}
        ${Input.render({ name: 'ghiChu', label: 'Ghi chú' })}
      </form>
    </div>`,
    footer: [
      Button.render({ label: 'Đóng', variant: 'outline', action: 'close' }),
      Button.render({ label: 'Thêm môn', variant: 'primary', action: 'add-subject' }),
    ].join(''),
  });
  const listEl = handle.element.querySelector('[data-ctdt-list]');
  const form = handle.element.querySelector(`#${formId}`);
  handle.element.querySelector('[data-action="close"]').addEventListener('click', handle.close);

  const reload = async () => {
    const items = await api.chuongTrinhDaoTao.monHoc(ctdt.id);
    listEl.innerHTML = items.length
      ? Table.render({
          columns: [
            { key: 'maMonHoc', label: 'Mã', primary: true },
            { key: 'tenMonHoc', label: 'Môn học' },
            { key: 'soTinChi', label: 'TC', align: 'right' },
            { key: 'hocKyGoiY', label: 'HK gợi ý', align: 'right' },
            { key: 'batBuoc', label: 'Loại', render: (row) => Badge.render({ label: row.batBuoc ? 'Bắt buộc' : 'Tự chọn', variant: row.batBuoc ? 'primary' : 'neutral' }) },
          ],
          rows: items,
          rowKey: 'monHocId',
          actions: [
            { action: 'edit-subject', label: 'Sửa', icon: 'pencil' },
            { action: 'remove-subject', label: 'Bỏ khỏi CTĐT', icon: 'trash-2', danger: true },
          ],
          actionLabel: (row) => `Thao tác cho ${row.maMonHoc}`,
          caption: 'Môn học trong chương trình',
        })
      : '<p class="form-hint">Chương trình chưa có môn học nào.</p>';
    return items;
  };

  handle.element.addEventListener('click', async (event) => {
    const button = event.target.closest('[data-action="add-subject"]');
    if (!button) return;
    const field = (name) => form.elements.namedItem(name).value;
    if (field('monHocId') === '') {
      Toast.warning('Chọn môn học trước khi thêm');
      return;
    }
    try {
      await api.chuongTrinhDaoTao.themMonHoc(ctdt.id, {
        monHocId: Number(field('monHocId')),
        hocKyGoiY: field('hocKyGoiY') === '' ? null : Number(field('hocKyGoiY')),
        batBuoc: field('batBuoc') === 'true',
        ghiChu: field('ghiChu').trim() || null,
      });
      Toast.success('Đã thêm môn vào chương trình');
      reload();
    } catch (error) {
      Toast.error(error.message);
    }
  });

  Table.init(listEl, {
    onAction: async (action, rowId) => {
      const row = (await api.chuongTrinhDaoTao.monHoc(ctdt.id)).find((item) => String(item.monHocId) === rowId);
      if (!row) return;
      if (action === 'edit-subject') editCtdtSubject(ctdt, row, reload);
      if (action === 'remove-subject') {
        try {
          await api.chuongTrinhDaoTao.boMonHoc(ctdt.id, row.monHocId);
          Toast.success('Đã bỏ môn khỏi chương trình');
          reload();
        } catch (error) {
          Toast.error(error.message);
        }
      }
    },
  });

  try {
    await reload();
  } catch (error) {
    listEl.innerHTML = `<p class="form-error">${escapeHtml(error.message)}</p>`;
  }
}

/** Sửa học kỳ gợi ý, loại và ghi chú của một môn trong CTĐT. */
function editCtdtSubject(ctdt, row, onDone) {
  const formId = nextId('edit-subject');
  const handle = Modal.open({
    title: `Sửa ${row.maMonHoc} trong ${ctdt.ma}`,
    content: `<form class="modal-form" id="${formId}" novalidate>
      ${Input.render({ name: 'hocKyGoiY', type: 'number', label: 'Học kỳ gợi ý', min: 1, step: 1, value: row.hocKyGoiY ?? '' })}
      ${Select.render({ name: 'batBuoc', label: 'Loại', value: String(row.batBuoc), options: [{ value: 'true', label: 'Bắt buộc' }, { value: 'false', label: 'Tự chọn' }] })}
      ${Input.render({ name: 'ghiChu', label: 'Ghi chú', value: row.ghiChu ?? '' })}
    </form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: 'Lưu thay đổi', variant: 'primary', action: 'save' }),
    ].join(''),
  });
  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);
  saveButton.addEventListener('click', async () => {
    const field = (name) => form.elements.namedItem(name).value;
    Button.setLoading(saveButton, true);
    try {
      await api.chuongTrinhDaoTao.capNhatMonHoc(ctdt.id, row.monHocId, {
        hocKyGoiY: field('hocKyGoiY') === '' ? null : Number(field('hocKyGoiY')),
        batBuoc: field('batBuoc') === 'true',
        ghiChu: field('ghiChu').trim() || null,
      });
      Toast.success('Đã cập nhật môn trong chương trình');
      handle.close();
      onDone();
    } catch (error) {
      Button.setLoading(saveButton, false);
      Toast.error(error.message);
    }
  });
}

/** Phân sinh viên vào lớp hành chính bằng cách đánh dấu danh sách. */
function openAssignStudents(lop) {
  const formId = nextId('assign-students');
  const handle = Modal.open({
    title: `Phân sinh viên vào ${lop.ma}`,
    size: 'lg',
    content: `<form class="modal-form" id="${formId}">
      ${students.map((sv) => `<label class="checkbox-row"><input type="checkbox" name="sinhVien" value="${sv.id}"> <span>${escapeHtml(sv.mssv)} - ${escapeHtml(sv.hoTen)}</span></label>`).join('') || '<p class="form-hint">Chưa có sinh viên.</p>'}
    </form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: 'Phân lớp', variant: 'primary', action: 'save' }),
    ].join(''),
  });
  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);
  saveButton.addEventListener('click', async () => {
    const ids = [...form.querySelectorAll('input[name="sinhVien"]:checked')].map((input) => Number(input.value));
    if (ids.length === 0) {
      Toast.warning('Chọn ít nhất một sinh viên');
      return;
    }
    Button.setLoading(saveButton, true);
    try {
      await api.lopHanhChinh.phanLop(lop.id, ids);
      Toast.success(`Đã phân ${ids.length} sinh viên vào lớp`);
      handle.close();
      loadList();
    } catch (error) {
      Button.setLoading(saveButton, false);
      Toast.error(error.message);
    }
  });
}

/** Gán cố vấn học tập cho lớp hành chính. */
function openAssignAdvisor(lop) {
  const formId = nextId('advisor');
  const handle = Modal.open({
    title: `Cố vấn học tập của ${lop.ma}`,
    content: `<form class="modal-form" id="${formId}">${Select.render({ name: 'coVanId', label: 'Giảng viên', options: refs.giangVien, value: lop.coVanId ?? '', placeholder: 'Chưa gán', searchable: true })}</form>`,
    footer: [
      Button.render({ label: 'Hủy', variant: 'outline', action: 'cancel' }),
      Button.render({ label: 'Lưu', variant: 'primary', action: 'save' }),
    ].join(''),
  });
  const form = handle.element.querySelector(`#${formId}`);
  const saveButton = handle.element.querySelector('[data-action="save"]');
  handle.element.querySelector('[data-action="cancel"]').addEventListener('click', handle.close);
  saveButton.addEventListener('click', async () => {
    const value = Select.getValue(form.elements.namedItem('coVanId'), refs.giangVien);
    Button.setLoading(saveButton, true);
    try {
      await api.lopHanhChinh.coVan(lop.id, value === '' ? null : Number(value));
      Toast.success('Đã cập nhật cố vấn');
      handle.close();
      loadList();
    } catch (error) {
      Button.setLoading(saveButton, false);
      Toast.error(error.message);
    }
  });
}

function handleRowAction(action, rowId) {
  const record = state.rows.find((row) => String(row.id) === rowId);
  if (!record) return;
  if (action === 'edit') openForm(record);
  if (action === 'delete') confirmDelete(record);
  if (action === 'monHoc') openCtdtSubjects(record);
  if (action === 'phanLop') openAssignStudents(record);
  if (action === 'coVan') openAssignAdvisor(record);
}

function switchTab(root, key) {
  state.key = key;
  state.page = 1;
  state.search = '';
  state.filters = {};
  root.querySelector('#tabs-slot').innerHTML = tabsMarkup();
  root.querySelector('#filters-slot').innerHTML = filtersMarkup();
  root.querySelector('#card-title').textContent = current().title;
  loadList();
}

function bindEvents(root) {
  const debouncedSearch = debounce(() => {
    state.search = root.querySelector('[name="q"]').value.trim();
    state.page = 1;
    loadList();
  }, 300);

  Input.init(root, {
    onInput: (input) => {
      if (input.name === 'q') debouncedSearch();
    },
  });

  Select.init(root, {
    onChange: (element) => {
      if (!current().filters.some((filter) => filter.name === element.name)) return;
      state.filters[element.name] = element.value;
      state.page = 1;
      loadList();
    },
  });

  root.addEventListener('click', (event) => {
    const tab = event.target.closest('[data-tab]');
    if (tab && tab.dataset.tab !== state.key) switchTab(root, tab.dataset.tab);
  });

  Button.init(root, {
    onAction: (action) => {
      if (action === 'add') openForm(null);
      if (action === 'reset') {
        state.search = '';
        state.filters = {};
        state.page = 1;
        root.querySelector('#filters-slot').innerHTML = filtersMarkup();
        loadList();
      }
    },
  });

  Table.init(root, { onAction: (action, rowId) => handleRowAction(action, rowId) });

  Pagination.init(root.querySelector('#list-container'), {
    onChange: ({ page, pageSize }) => {
      state.page = page;
      state.pageSize = pageSize;
      loadList();
    },
  });
}

/**
 * @param {HTMLElement} root
 */
export async function mount(root) {
  root.innerHTML = `${renderPageHeader({
    title: 'Danh mục học vụ',
    description: 'Khoa, bộ môn, ngành, chuyên ngành, năm học, học kỳ, quy chế và chương trình đào tạo.',
    actions: Button.render({ label: 'Thêm mới', variant: 'primary', icon: 'plus', action: 'add' }),
  })}
  <div id="tabs-slot">${tabsMarkup()}</div>
  <section class="card" aria-labelledby="card-title">
    <header class="card-header"><div class="card-header-text"><h2 class="card-title" id="card-title">${escapeHtml(current().title)}</h2></div></header>
    <div id="filters-slot">${filtersMarkup()}</div>
    <div class="card-body is-flush" id="list-container"></div>
  </section>`;

  bindEvents(root);
  await loadRefs();
  root.querySelector('#filters-slot').innerHTML = filtersMarkup();
  await loadList();
}
