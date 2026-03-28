window.themeManager = (function () {
    var STORAGE_KEY = 'app-theme';

    function getEffective(mode) {
        if (mode === 'system') {
            return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        }
        return mode;
    }

    function apply(mode) {
        var effective = getEffective(mode);
        document.documentElement.setAttribute('data-bs-theme', effective);
    }

    // Re-apply when system preference changes while in "system" mode
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function () {
        var current = localStorage.getItem(STORAGE_KEY) || 'system';
        if (current === 'system') {
            apply('system');
        }
    });

    return {
        init: function () {
            var saved = localStorage.getItem(STORAGE_KEY) || 'system';
            apply(saved);
            return saved;
        },
        setTheme: function (mode) {
            localStorage.setItem(STORAGE_KEY, mode);
            apply(mode);
        },
        getTheme: function () {
            return localStorage.getItem(STORAGE_KEY) || 'system';
        }
    };
})();
