// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Modern HRMS JavaScript Enhancement
// Professional Enterprise UI Interactions

document.addEventListener('DOMContentLoaded', function() {
    initializeModernUI();
    initializeSidebar();
    initializeAlerts();
    initializeCards();
    initializeTables();
    initializeTooltips();
});

// Main UI Initialization
function initializeModernUI() {
    // Add smooth scroll behavior
    document.documentElement.style.scrollBehavior = 'smooth';
    
    // Initialize page animations
    animatePageLoad();
    
    // Add keyboard navigation support
    initializeKeyboardNavigation();
}

// Sidebar Management
function initializeSidebar() {
    const sidebarToggle = document.querySelector('.navbar-toggler');
    const sidebar = document.querySelector('.sidebar');
    const overlay = createOverlay();
    
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('show');
            if (sidebar.classList.contains('show')) {
                document.body.appendChild(overlay);
                overlay.classList.add('show');
                document.body.style.overflow = 'hidden';
            } else {
                hideOverlay();
            }
        });
        
        // Close sidebar when clicking overlay
        overlay.addEventListener('click', hideOverlay);
        
        // Close sidebar on window resize if mobile
        window.addEventListener('resize', function() {
            if (window.innerWidth >= 992) {
                hideOverlay();
            }
        });
    }
    
    function createOverlay() {
        const overlay = document.createElement('div');
        overlay.className = 'sidebar-overlay';
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.5);
            z-index: 1020;
            opacity: 0;
            visibility: hidden;
            transition: all 0.3s ease;
        `;
        return overlay;
    }
    
    function hideOverlay() {
        sidebar?.classList.remove('show');
        overlay?.classList.remove('show');
        document.body.style.overflow = '';
        setTimeout(() => {
            if (overlay?.parentNode) {
                overlay.parentNode.removeChild(overlay);
            }
        }, 300);
    }
}

// Alert Management
function initializeAlerts() {
    // Auto-dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        if (!alert.querySelector('.btn-close')) return;
        
        setTimeout(() => {
            if (alert.parentNode) {
                alert.style.opacity = '0';
                alert.style.transform = 'translateY(-20px)';
                setTimeout(() => {
                    alert.remove();
                }, 300);
            }
        }, 5000);
    });
    
    // Add close animation
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('btn-close')) {
            const alert = e.target.closest('.alert');
            if (alert) {
                e.preventDefault();
                alert.style.opacity = '0';
                alert.style.transform = 'translateY(-20px)';
                setTimeout(() => alert.remove(), 300);
            }
        }
    });
}

// Card Enhancements
function initializeCards() {
    const cards = document.querySelectorAll('.card');
    cards.forEach(card => {
        // Add hover effect for interactive cards
        if (card.querySelector('a') || card.classList.contains('employee-card')) {
            card.style.cursor = 'pointer';
            
            card.addEventListener('mouseenter', function() {
                this.style.transform = 'translateY(-4px)';
                this.style.boxShadow = '0 8px 25px rgba(0, 0, 0, 0.15)';
            });
            
            card.addEventListener('mouseleave', function() {
                this.style.transform = 'translateY(0)';
                this.style.boxShadow = '';
            });
        }
    });
    
    // Stats cards animation
    const statsCards = document.querySelectorAll('.stats-card');
    statsCards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
        card.classList.add('animate-fade-in-up');
    });
}

// Table Enhancements
function initializeTables() {
    const tables = document.querySelectorAll('.table');
    tables.forEach(table => {
        // Add row hover effects
        const rows = table.querySelectorAll('tbody tr');
        rows.forEach(row => {
            row.addEventListener('mouseenter', function() {
                this.style.backgroundColor = 'rgba(52, 152, 219, 0.06)';
                this.style.transform = 'scale(1.01)';
            });
            
            row.addEventListener('mouseleave', function() {
                this.style.backgroundColor = '';
                this.style.transform = 'scale(1)';
            });
        });
        
        // Add loading state for form submissions in tables
        const forms = table.querySelectorAll('form');
        forms.forEach(form => {
            form.addEventListener('submit', function() {
                const button = form.querySelector('button[type="submit"]');
                if (button) {
                    button.disabled = true;
                    button.innerHTML = '<i class="bi bi-arrow-clockwise spin"></i>';
                    button.classList.add('loading');
                }
            });
        });
    });
}

// Tooltip Initialization
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"], [title]'));
    tooltipTriggerList.map(function(tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Page Load Animations
function animatePageLoad() {
    // Fade in main content
    const mainContent = document.querySelector('.main-content');
    if (mainContent) {
        mainContent.style.opacity = '0';
        mainContent.style.transform = 'translateY(20px)';
        setTimeout(() => {
            mainContent.style.transition = 'all 0.6s ease';
            mainContent.style.opacity = '1';
            mainContent.style.transform = 'translateY(0)';
        }, 100);
    }
    
    // Animate cards sequentially
    const cards = document.querySelectorAll('.card');
    cards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(30px)';
        setTimeout(() => {
            card.style.transition = 'all 0.5s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, 200 + (index * 100));
    });
}

// Keyboard Navigation
function initializeKeyboardNavigation() {
    document.addEventListener('keydown', function(e) {
        // ESC key to close modals, dropdowns, etc.
        if (e.key === 'Escape') {
            const dropdowns = document.querySelectorAll('.dropdown-menu.show');
            dropdowns.forEach(dropdown => {
                dropdown.classList.remove('show');
            });
        }
        
        // Ctrl+/ for search (if search exists)
        if (e.ctrlKey && e.key === '/') {
            e.preventDefault();
            const searchInput = document.querySelector('input[type="search"], input[placeholder*="search"]');
            if (searchInput) {
                searchInput.focus();
            }
        }
    });
}

// Form Enhancement Utilities
function enhanceForm(formSelector) {
    const form = document.querySelector(formSelector);
    if (!form) return;
    
    // Add loading state to submit buttons
    form.addEventListener('submit', function(e) {
        const submitBtn = form.querySelector('button[type="submit"], input[type="submit"]');
        if (submitBtn) {
            submitBtn.disabled = true;
            const originalText = submitBtn.textContent;
            submitBtn.innerHTML = '<i class="bi bi-arrow-clockwise spin me-2"></i>Processing...';
            
            // Re-enable after 5 seconds as fallback
            setTimeout(() => {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }, 5000);
        }
    });
    
    // Enhanced input focus effects
    const inputs = form.querySelectorAll('input, select, textarea');
    inputs.forEach(input => {
        input.addEventListener('focus', function() {
            this.parentElement.classList.add('focused');
        });
        
        input.addEventListener('blur', function() {
            this.parentElement.classList.remove('focused');
        });
    });
}

// Button Loading State
function setButtonLoading(button, isLoading = true) {
    if (isLoading) {
        button.disabled = true;
        button.dataset.originalText = button.innerHTML;
        button.innerHTML = '<i class="bi bi-arrow-clockwise spin me-2"></i>Loading...';
        button.classList.add('loading');
    } else {
        button.disabled = false;
        button.innerHTML = button.dataset.originalText || button.innerHTML;
        button.classList.remove('loading');
    }
}

// Smooth Page Transitions
function navigateTo(url) {
    document.body.style.opacity = '0.8';
    setTimeout(() => {
        window.location.href = url;
    }, 150);
}

// Notification System
function showNotification(message, type = 'info', duration = 3000) {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} notification`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        max-width: 350px;
        opacity: 0;
        transform: translateX(100%);
        transition: all 0.3s ease;
    `;
    
    const icon = {
        success: 'check-circle-fill',
        error: 'exclamation-circle-fill',
        warning: 'exclamation-triangle-fill',
        info: 'info-circle-fill'
    };
    
    notification.innerHTML = `
        <i class="bi bi-${icon[type]} me-2"></i>
        ${message}
        <button type="button" class="btn-close" onclick="this.parentElement.remove()"></button>
    `;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.style.opacity = '1';
        notification.style.transform = 'translateX(0)';
    }, 100);
    
    setTimeout(() => {
        notification.style.opacity = '0';
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => notification.remove(), 300);
    }, duration);
}

// Utility function for confirming actions
function confirmAction(message, callback) {
    if (confirm(message)) {
        callback();
    }
}

// CSS Animation Classes
const animationCSS = `
    .animate-fade-in-up {
        animation: fadeInUp 0.6s ease forwards;
    }
    
    .spin {
        animation: spin 1s linear infinite;
    }
    
    .loading {
        opacity: 0.7;
        pointer-events: none;
    }
    
    @keyframes fadeInUp {
        from {
            opacity: 0;
            transform: translateY(30px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
    
    @keyframes spin {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
    }
    
    .sidebar-overlay.show {
        opacity: 1 !important;
        visibility: visible !important;
    }
    
    .notification {
        animation: slideInRight 0.3s ease;
    }
    
    @keyframes slideInRight {
        from {
            opacity: 0;
            transform: translateX(100%);
        }
        to {
            opacity: 1;
            transform: translateX(0);
        }
    }
`;

// Inject animation CSS
if (!document.getElementById('modern-animations')) {
    const style = document.createElement('style');
    style.id = 'modern-animations';
    style.textContent = animationCSS;
    document.head.appendChild(style);
}

// Export functions for global use
window.HRMS = {
    enhanceForm,
    setButtonLoading,
    navigateTo,
    showNotification,
    confirmAction
};
