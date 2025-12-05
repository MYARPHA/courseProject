// Theme toggle and auth-aware request link handling
(function () {
    function isAuthenticatedClient() {
        return !!localStorage.getItem('authToken');
    }

    // Apply saved theme on load
    function applySavedTheme() {
        const t = localStorage.getItem('siteTheme');
        if (t === 'pink') document.body.classList.add('theme-pink');
        else document.body.classList.remove('theme-pink');
    }

    function toggleTheme() {
        if (document.body.classList.contains('theme-pink')) {
            document.body.classList.remove('theme-pink');
            localStorage.setItem('siteTheme', 'default');
        } else {
            document.body.classList.add('theme-pink');
            localStorage.setItem('siteTheme', 'pink');
        }
    }

    // Attach behavior to theme toggle button if present
    document.addEventListener('DOMContentLoaded', function () {
        applySavedTheme();

        const tb = document.getElementById('themeToggleBtn');
        if (tb) tb.addEventListener('click', (e) => { e.preventDefault(); toggleTheme(); tb.classList.toggle('active'); });

        // Intercept links that require auth and open auth modal for unauthenticated users
        document.querySelectorAll('a[data-requires-auth]').forEach(a => {
            a.addEventListener('click', function (ev) {
                try {
                    if (!isAuthenticatedClient()) {
                        ev.preventDefault();
                        const m = document.getElementById('authModal');
                        if (m) bootstrap.Modal.getOrCreateInstance(m).show();
                        return false;
                    }
                } catch (e) { /* ignore */ }
            });
        });
    });
})();
