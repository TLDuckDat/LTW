/**
 * Badge: 5 màu semantic (success, warning, destructive, info, primary) + neutral.
 * Chấm màu và nền nhạt mang màu semantic; chữ luôn dùng màu foreground để đạt tương phản.
 * - render({ label, variant }) -> HTML string
 */

import { escapeHtml } from '../core/dom.js';

const VARIANTS = ['success', 'warning', 'destructive', 'info', 'primary', 'neutral'];

/**
 * @param {{ label: string, variant?: 'success'|'warning'|'destructive'|'info'|'primary'|'neutral' }} props
 * @returns {string}
 */
function render({ label, variant = 'neutral' }) {
  const safeVariant = VARIANTS.includes(variant) ? variant : 'neutral';
  const modifier = safeVariant === 'neutral' ? '' : ` badge-${safeVariant}`;
  return `<span class="badge${modifier}">${escapeHtml(label)}</span>`;
}

export const Badge = { render };
