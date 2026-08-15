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
