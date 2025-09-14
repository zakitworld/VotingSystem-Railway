// Custom JavaScript for Bootstrap dropdown initialization
window.initializeBootstrapDropdown = function() {
    // Initialize all dropdown toggles
    const dropdownToggles = document.querySelectorAll('[data-bs-toggle="dropdown"]');
    
    dropdownToggles.forEach(function(toggle) {
        // Remove any existing event listeners to prevent duplicates
        toggle.removeEventListener('click', handleDropdownClick);
        
        // Add click event listener
        toggle.addEventListener('click', handleDropdownClick);
    });
};

function handleDropdownClick(event) {
    event.preventDefault();
    event.stopPropagation();
    
    const toggle = event.currentTarget;
    const dropdown = toggle.closest('.dropdown');
    const menu = dropdown.querySelector('.dropdown-menu');
    
    if (!dropdown || !menu) return;
    
    // Close all other dropdowns first
    closeAllDropdowns();
    
    // Toggle current dropdown
    if (menu.classList.contains('show')) {
        closeDropdown(dropdown, menu);
    } else {
        openDropdown(dropdown, menu);
    }
}

function openDropdown(dropdown, menu) {
    menu.classList.add('show');
    dropdown.classList.add('show');
    
    // Add click outside listener
    document.addEventListener('click', handleClickOutside);
}

function closeDropdown(dropdown, menu) {
    menu.classList.remove('show');
    dropdown.classList.remove('show');
}

function closeAllDropdowns() {
    const openDropdowns = document.querySelectorAll('.dropdown.show');
    openDropdowns.forEach(function(dropdown) {
        const menu = dropdown.querySelector('.dropdown-menu');
        if (menu) {
            closeDropdown(dropdown, menu);
        }
    });
}

function handleClickOutside(event) {
    const dropdowns = document.querySelectorAll('.dropdown.show');
    let clickedInsideDropdown = false;
    
    dropdowns.forEach(function(dropdown) {
        if (dropdown.contains(event.target)) {
            clickedInsideDropdown = true;
        }
    });
    
    if (!clickedInsideDropdown) {
        closeAllDropdowns();
        document.removeEventListener('click', handleClickOutside);
    }
}

// Initialize dropdowns when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    window.initializeBootstrapDropdown();
});
