(function () {
    const sidebar = document.getElementById('appSidebar');
    const toggle = document.getElementById('sidebarToggle');
    const backdrop = document.getElementById('sidebarBackdrop');

    if (!sidebar || !toggle || !backdrop) {
        return;
    }

    function closeSidebar() {
        sidebar.classList.remove('show');
        backdrop.classList.remove('show');
    }

    function openSidebar() {
        sidebar.classList.add('show');
        backdrop.classList.add('show');
    }

    toggle.addEventListener('click', function () {
        if (sidebar.classList.contains('show')) {
            closeSidebar();
        } else {
            openSidebar();
        }
    });

    backdrop.addEventListener('click', closeSidebar);

    sidebar.querySelectorAll('.sidebar-link').forEach(function (link) {
        link.addEventListener('click', function () {
            if (window.innerWidth < 992) {
                closeSidebar();
            }
        });
    });
})();

(function () {
    document.querySelectorAll('input[type="password"]').forEach(function (input) {
        if (input.closest('.password-toggle')) {
            return;
        }

        const wrap = document.createElement('div');
        wrap.className = 'password-toggle';
        input.parentNode.insertBefore(wrap, input);
        wrap.appendChild(input);

        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'password-toggle-btn';
        button.setAttribute('aria-label', 'Show password');
        button.innerHTML = '<i class="bi bi-eye" aria-hidden="true"></i>';
        wrap.appendChild(button);

        button.addEventListener('click', function () {
            const hidden = input.type === 'password';
            input.type = hidden ? 'text' : 'password';
            button.setAttribute('aria-label', hidden ? 'Hide password' : 'Show password');
            button.innerHTML = hidden
                ? '<i class="bi bi-eye-slash" aria-hidden="true"></i>'
                : '<i class="bi bi-eye" aria-hidden="true"></i>';
        });
    });
})();

(function () {
    const main = document.querySelector('.app-content');
    if (!main) {
        return;
    }

    main.querySelectorAll('table.table').forEach(function (table) {
        if (table.classList.contains('js-no-filter') || table.closest('.js-no-filter') || table.closest('.js-paged-table')) {
            return;
        }

        const tbody = table.tBodies[0];
        if (!tbody) {
            return;
        }

        const dataRows = Array.from(tbody.rows).filter(function (row) {
            return row.cells.length > 1;
        });
        if (dataRows.length === 0) {
            return;
        }

        const bar = document.createElement('div');
        bar.className = 'table-filter-bar';
        const input = document.createElement('input');
        input.type = 'search';
        input.className = 'form-control table-filter-input';
        input.placeholder = 'Filter table…';
        input.setAttribute('aria-label', 'Filter table');
        bar.appendChild(input);

        const wrapper = table.closest('.table-responsive') || table;
        wrapper.parentNode.insertBefore(bar, wrapper);

        input.addEventListener('input', function () {
            const q = input.value.trim().toLowerCase();
            tbody.querySelectorAll('tr').forEach(function (row) {
                if (row.cells.length <= 1) {
                    return;
                }
                row.hidden = q.length > 0 && row.textContent.toLowerCase().indexOf(q) === -1;
            });
        });
    });
})();
