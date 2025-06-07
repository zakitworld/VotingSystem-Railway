window.initializeBootstrapDropdown = () => {
    const initialize = () => {
        if (typeof bootstrap !== 'undefined') {
            var dropdownElementList = [].slice.call(document.querySelectorAll('[data-bs-toggle="dropdown"]'))
            var dropdownList = dropdownElementList.map(function (dropdownToggleEl) {
                return new bootstrap.Dropdown(dropdownToggleEl)
            })
        } else {
            // Bootstrap not yet loaded, retry after a short delay
            setTimeout(initialize, 50);
        }
    };
    initialize();
}; 