(function(){
    async function fetchJson(url, opts) {
        try {
            const res = await fetch(url, opts);
            if (!res.ok) throw new Error('HTTP ' + res.status);
            return await res.json();
        } catch (e) {
            console.error('fetchJson error', e, url);
            throw e;
        }
    }

    async function init() {
        let defs = [];
        try {
            const res = await fetch('/api/requests/statuses/definitions');
            if (res.ok) defs = await res.json();
        } catch (e) { console.warn('Could not load status definitions', e); }

        // apply colored badges to all status badges (use defs.color or fallback mapping)
        function statusColor(name) {
            if (!name) return '';
            const n = name.toLowerCase();
            if (n.includes('готов') || n.includes('заверш')) return '#28a745'; // green
            if (n.includes('в процессе') || n.includes('в обработ')) return '#fd7e14'; // orange
            if (n.includes('отклон') || n.includes('отмен')) return '#dc3545'; // red
            return '';
        }

        document.querySelectorAll('.status-badge').forEach(el => {
            const name = el.getAttribute('data-status') || el.textContent || '';
            const def = defs.find(d => d.name === name);
            const color = def ? def.color : statusColor(name);
            if (color) {
                el.style.background = color;
                el.style.color = '#fff';
                el.style.padding = '6px 10px';
                el.style.borderRadius = '999px';
                el.style.fontWeight = '600';
                el.style.display = 'inline-flex';
                el.style.alignItems = 'center';
                el.style.gap = '6px';
                if (def && def.icon) el.innerHTML = def.icon + ' ' + el.textContent;
            } else {
                el.classList.add('badge','bg-secondary');
            }
        });

        const rows = document.querySelectorAll('#profileRequestsTable tbody tr[data-request-id]');
        rows.forEach(row => {
            const id = row.getAttribute('data-request-id');
            const cell = row.querySelector('.admin-actions-cell');
            if (!cell) return;

            // Clear
            cell.innerHTML = '';
            // Create a select + save button for admin to change status
            const select = document.createElement('select');
            select.className = 'form-select form-select-sm d-inline-block me-1';
            select.style.width = 'auto';
            defs.forEach(d => {
                const o = document.createElement('option'); o.value = d.name; o.text = d.name; select.appendChild(o);
            });
            // set current value to badge data-status if present
            const badge = row.querySelector('.status-badge');
            if (badge) {
                const cur = badge.getAttribute('data-status') || badge.textContent || '';
                for (let i = 0; i < select.options.length; i++) {
                    if (select.options[i].value === cur) { select.selectedIndex = i; break; }
                }
            }

            const saveBtn = document.createElement('button');
            saveBtn.className = 'btn btn-sm btn-secondary';
            saveBtn.innerText = 'Применить';
            saveBtn.addEventListener('click', async () => {
                const chosen = select.value;
                if (!chosen) return;
                saveBtn.disabled = true;
                try {
                    const res = await fetchWithAuth(`/api/requests/${id}/status`, {
                        method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ Status: chosen })
                    });
                    if (!res.ok) throw new Error('HTTP ' + res.status);
                    // update badge with icon + color
                    const def = defs.find(x => x.name === chosen) || { name: chosen, color: '' };
                    if (badge) {
                        badge.setAttribute('data-status', def.name);
                        badge.innerHTML = def.icon ? (def.icon + ' ' + def.name) : def.name;
                        const color = def.color || statusColor(def.name);
                        if (color) { badge.style.background = color; badge.style.color = '#fff'; } else { badge.style.background = ''; badge.style.color = ''; }
                    }
                } catch (e) {
                    console.error('Failed to save status', e);
                    alert('Не удалось сохранить статус.');
                } finally { saveBtn.disabled = false; }
            });

            cell.appendChild(select);
            cell.appendChild(saveBtn);
        });

        // Export visible button
        const exportBtn = document.getElementById('adminExportVisible');
        if (exportBtn) {
            exportBtn.addEventListener('click', async () => {
                exportBtn.disabled = true;
                try {
                    // Currently call the server export endpoint (returns full dataset).
                    // If you later add a server endpoint that accepts a list of IDs, update this logic to POST that list.
                    const res = await fetchWithAuth('/Requests/ExportExcel');
                    if (!res.ok) throw new Error('HTTP ' + res.status);
                    const blob = await res.blob();
                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = 'admin-requests.xlsx';
                    document.body.appendChild(a);
                    a.click();
                    a.remove();
                    window.URL.revokeObjectURL(url);
                } catch (e) {
                    console.error('Export failed', e);
                    alert('Не удалось экспортировать таблицу.');
                } finally { exportBtn.disabled = false; }
            });
        }
    }

    // Wait for DOM
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();
})();
