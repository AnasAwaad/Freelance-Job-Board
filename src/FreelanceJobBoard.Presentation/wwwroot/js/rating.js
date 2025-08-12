/**
 * Interactive Star Rating System
 * Handles all star rating interactions across the application
 */

class StarRating {
    constructor(element) {
        this.container = element;
        this.input = this.container.parentElement.querySelector('.rating-input-value');
        this.labels = this.container.querySelectorAll('.rating-label');
        this.currentValue = parseInt(this.input?.value) || 0;
        this.hoverValue = 0;
        this.disabled = false;
        
        this.init();
    }
    
    init() {
        if (!this.labels.length || !this.input) return;
        
        // Check if rating is disabled
        this.disabled = this.container.hasAttribute('data-disabled') || 
                       this.container.classList.contains('rating-disabled');
        
        // Set initial state
        this.updateDisplay(this.currentValue);
        
        if (!this.disabled) {
            this.addEventListeners();
        }
        
        console.log('StarRating initialized with value:', this.currentValue);
    }
    
    addEventListeners() {
        // Add event listeners
        this.labels.forEach((label, index) => {
            const value = parseInt(label.getAttribute('data-value'));
            
            // Click event
            label.addEventListener('click', (e) => {
                e.preventDefault();
                this.setValue(value);
                console.log('Star clicked, value set to:', value);
            });
            
            // Hover events
            label.addEventListener('mouseenter', () => {
                this.setHover(value);
            });
            
            label.addEventListener('mouseleave', () => {
                this.clearHover();
            });
            
            // Keyboard support
            label.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    this.setValue(value);
                } else if (e.key === 'ArrowLeft' || e.key === 'ArrowDown') {
                    e.preventDefault();
                    this.setValue(Math.max(1, value - 1));
                } else if (e.key === 'ArrowRight' || e.key === 'ArrowUp') {
                    e.preventDefault();
                    this.setValue(Math.min(5, value + 1));
                }
            });
            
            // Make focusable for accessibility
            label.setAttribute('tabindex', '0');
            label.setAttribute('role', 'button');
            label.setAttribute('aria-label', `Rate ${value} star${value === 1 ? '' : 's'}`);
        });
        
        // Container leave event
        this.container.addEventListener('mouseleave', () => {
            this.clearHover();
        });
    }
    
    setValue(value) {
        if (this.disabled) return;
        
        this.currentValue = value;
        this.input.value = value;
        this.updateDisplay(value);
        
        // Trigger change event for any listeners
        this.input.dispatchEvent(new Event('change', { bubbles: true }));
        
        // Add animation effect
        this.animateStars(value);
        
        // Update accessibility
        this.updateAriaLabel();
        
        // Trigger custom event
        this.container.dispatchEvent(new CustomEvent('ratingSet', { 
            detail: { value, rating: this },
            bubbles: true 
        }));
        
        console.log('Rating set to:', value);
    }
    
    setHover(value) {
        if (this.disabled) return;
        
        this.hoverValue = value;
        this.updateDisplay(value, true);
    }
    
    clearHover() {
        if (this.disabled) return;
        
        this.hoverValue = 0;
        this.updateDisplay(this.currentValue);
    }
    
    updateDisplay(value, isHover = false) {
        this.labels.forEach((label, index) => {
            const starValue = parseInt(label.getAttribute('data-value'));
            const isActive = starValue <= value;
            
            // Remove all state classes
            label.classList.remove('checked', 'hover');
            
            // Add appropriate class
            if (isActive) {
                label.classList.add(isHover ? 'hover' : 'checked');
            }
            
            // Handle both icon fonts and Unicode stars
            const starIcon = label.querySelector('.star-icon');
            const iconElement = label.querySelector('i');
            
            if (starIcon) {
                // Unicode star handling
                if (isActive) {
                    starIcon.style.color = '#ffc107';
                } else {
                    starIcon.style.color = '#e4e6ef';
                }
            }
            
            if (iconElement) {
                // Icon font handling
                if (isActive) {
                    iconElement.style.color = '#ffc107';
                } else {
                    iconElement.style.color = '#e4e6ef';
                }
            }
        });
    }
    
    animateStars(value) {
        this.labels.forEach((label, index) => {
            const starValue = parseInt(label.getAttribute('data-value'));
            if (starValue <= value) {
                label.classList.add('star-pop');
                setTimeout(() => {
                    label.classList.remove('star-pop');
                }, 400);
            }
        });
    }
    
    updateAriaLabel() {
        const text = this.currentValue === 0 ? 
            'No rating selected' : 
            `${this.currentValue} star${this.currentValue === 1 ? '' : 's'} selected`;
        this.container.setAttribute('aria-label', text);
    }
    
    // Public methods
    getValue() {
        return this.currentValue;
    }
    
    reset() {
        this.setValue(0);
    }
    
    disable() {
        this.disabled = true;
        this.container.classList.add('rating-disabled');
        this.labels.forEach(label => {
            label.setAttribute('tabindex', '-1');
            label.style.cursor = 'default';
        });
    }
    
    enable() {
        this.disabled = false;
        this.container.classList.remove('rating-disabled');
        this.labels.forEach(label => {
            label.setAttribute('tabindex', '0');
            label.style.cursor = 'pointer';
        });
    }
}

class QuickRating {
    constructor(element) {
        this.container = element;
        this.labels = this.container.querySelectorAll('.rating-label-quick');
        this.currentValue = parseInt(this.container.getAttribute('data-selected-rating')) || 0;
        this.disabled = false;
        
        this.init();
    }
    
    init() {
        if (!this.labels.length) return;
        
        // Check if rating is disabled
        this.disabled = this.container.hasAttribute('data-disabled') || 
                       this.container.classList.contains('rating-disabled');
        
        // Set initial state
        this.updateDisplay(this.currentValue);
        
        if (!this.disabled) {
            this.addEventListeners();
        }
    }
    
    addEventListeners() {
        this.labels.forEach((label, index) => {
            const value = parseInt(label.getAttribute('data-value'));
            
            // Click event
            label.addEventListener('click', (e) => {
                e.preventDefault();
                this.setValue(value);
            });
            
            // Hover events
            label.addEventListener('mouseenter', () => {
                this.setHover(value);
            });
            
            label.addEventListener('mouseleave', () => {
                this.clearHover();
            });
            
            // Keyboard support
            label.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    this.setValue(value);
                } else if (e.key === 'ArrowLeft' || e.key === 'ArrowDown') {
                    e.preventDefault();
                    this.setValue(Math.max(1, value - 1));
                } else if (e.key === 'ArrowRight' || e.key === 'ArrowUp') {
                    e.preventDefault();
                    this.setValue(Math.min(5, value + 1));
                }
            });
            
            // Make focusable for accessibility
            label.setAttribute('tabindex', '0');
            label.setAttribute('role', 'button');
            label.setAttribute('aria-label', `Select ${value} star${value === 1 ? '' : 's'}`);
        });
        
        // Container leave event
        this.container.addEventListener('mouseleave', () => {
            this.clearHover();
        });
    }
    
    setValue(value) {
        if (this.disabled) return;
        
        this.currentValue = value;
        this.container.setAttribute('data-selected-rating', value);
        this.updateDisplay(value);
        
        // Trigger custom event
        this.container.dispatchEvent(new CustomEvent('ratingChanged', { 
            detail: { value, rating: this },
            bubbles: true 
        }));
    }
    
    setHover(value) {
        if (this.disabled) return;
        this.updateDisplay(value, true);
    }
    
    clearHover() {
        if (this.disabled) return;
        this.updateDisplay(this.currentValue);
    }
    
    updateDisplay(value, isHover = false) {
        this.labels.forEach((label, index) => {
            const starValue = parseInt(label.getAttribute('data-value'));
            const isActive = starValue <= value;
            
            // Remove all state classes
            label.classList.remove('active', 'hover');
            
            // Add appropriate class
            if (isActive) {
                label.classList.add(isHover ? 'hover' : 'active');
            }
            
            // Handle both icon fonts and Unicode stars
            const starIcon = label.querySelector('.star-icon');
            const iconElement = label.querySelector('i');
            
            if (starIcon) {
                // Unicode star handling
                if (isActive) {
                    starIcon.style.color = '#ffc107';
                } else {
                    starIcon.style.color = '#e4e6ef';
                }
            }
            
            if (iconElement) {
                // Icon font handling
                if (isActive) {
                    iconElement.style.color = '#ffc107';
                } else {
                    iconElement.style.color = '#e4e6ef';
                }
            }
        });
    }
    
    getValue() {
        return this.currentValue;
    }
    
    reset() {
        this.setValue(0);
    }
    
    disable() {
        this.disabled = true;
        this.container.classList.add('rating-disabled');
        this.labels.forEach(label => {
            label.setAttribute('tabindex', '-1');
            label.style.cursor = 'default';
        });
    }
    
    enable() {
        this.disabled = false;
        this.container.classList.remove('rating-disabled');
        this.labels.forEach(label => {
            label.setAttribute('tabindex', '0');
            label.style.cursor = 'pointer';
        });
    }
}

// Rating validation utilities
const RatingValidation = {
    validateRating: function(value, min = 1, max = 5) {
        const rating = parseInt(value);
        return !isNaN(rating) && rating >= min && rating <= max;
    },
    
    showError: function(element, message) {
        if (typeof toastr !== 'undefined') {
            toastr.error(message);
        } else {
            console.error(message);
        }
        
        // Add visual error state
        if (element) {
            element.classList.add('rating-error');
            setTimeout(() => {
                element.classList.remove('rating-error');
            }, 1000);
        }
    },
    
    showSuccess: function(element, message) {
        if (typeof toastr !== 'undefined') {
            toastr.success(message);
        } else {
            console.log(message);
        }
        
        // Add visual success state
        if (element) {
            element.classList.add('rating-success');
            setTimeout(() => {
                element.classList.remove('rating-success');
            }, 1000);
        }
    }
};

// Initialize rating systems when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    console.log('Rating system: DOM loaded, initializing...');
    initializeRatings();
});

// Function to initialize all rating components
function initializeRatings() {
    console.log('Rating system: Initializing all rating components...');
    
    // Initialize main rating components
    const ratingContainers = document.querySelectorAll('[data-kt-rating="true"]');
    console.log('Found', ratingContainers.length, 'rating containers');
    
    ratingContainers.forEach((container, index) => {
        if (!container.starRating) {
            container.starRating = new StarRating(container);
            console.log(`Initialized rating container ${index + 1}/${ratingContainers.length}`);
        }
    });
    
    // Initialize quick rating components
    const quickRatingContainers = document.querySelectorAll('.rating-quick');
    console.log('Found', quickRatingContainers.length, 'quick rating containers');
    
    quickRatingContainers.forEach((container, index) => {
        if (!container.quickRating) {
            container.quickRating = new QuickRating(container);
            console.log(`Initialized quick rating container ${index + 1}/${quickRatingContainers.length}`);
        }
    });
    
    // Add global event listeners
    addGlobalRatingEventListeners();
    
    console.log('Rating system: Initialization complete');
}

// Add global event listeners for rating feedback
function addGlobalRatingEventListeners() {
    // Listen for rating changes to provide feedback
    document.addEventListener('ratingSet', function(e) {
        console.log('Global ratingSet event:', e.detail);
        const { value } = e.detail;
        const feedbackElement = e.target.parentElement.querySelector('.rating-feedback');
        
        if (feedbackElement) {
            updateRatingFeedback(feedbackElement, value);
        }
    });
    
    document.addEventListener('ratingChanged', function(e) {
        console.log('Global ratingChanged event:', e.detail);
        const { value } = e.detail;
        const feedbackElement = e.target.parentElement.querySelector('.quick-rating-feedback');
        
        if (feedbackElement) {
            updateQuickRatingFeedback(feedbackElement, value);
        }
    });
}

// Rating feedback messages
const RATING_MESSAGES = {
    0: "Please select a rating",
    1: "Poor - Did not meet expectations",
    2: "Fair - Met some expectations",
    3: "Good - Met expectations",
    4: "Very Good - Exceeded expectations",
    5: "Excellent - Far exceeded expectations"
};

function updateRatingFeedback(feedbackElement, value) {
    const textElement = feedbackElement.querySelector('.rating-text');
    if (textElement) {
        textElement.textContent = RATING_MESSAGES[value] || RATING_MESSAGES[0];
        textElement.className = `rating-text fs-7 ${getRatingColorClass(value)}`;
    }
}

function updateQuickRatingFeedback(feedbackElement, value) {
    const textElement = feedbackElement.querySelector('.rating-text');
    if (textElement) {
        textElement.textContent = RATING_MESSAGES[value] || RATING_MESSAGES[0];
        textElement.className = `rating-text fs-8 ${getRatingColorClass(value)}`;
    }
}

function getRatingColorClass(value) {
    if (value >= 4) return 'text-success';
    if (value >= 3) return 'text-warning';
    if (value >= 1) return 'text-danger';
    return 'text-muted';
}

// Function to initialize ratings in dynamically loaded content (like modals)
function initializeModalRatings() {
    console.log('Rating system: Initializing modal ratings...');
    // Small delay to ensure modal content is fully rendered
    setTimeout(() => {
        initializeRatings();
    }, 100);
}

// Enhanced rating utilities for better UX
const RatingUtils = {
    // Animate all stars up to a certain value
    animateToValue: function(container, value) {
        const labels = container.querySelectorAll('.rating-label');
        labels.forEach((label, index) => {
            const starValue = parseInt(label.getAttribute('data-value'));
            if (starValue <= value) {
                setTimeout(() => {
                    label.classList.add('star-pop');
                    setTimeout(() => {
                        label.classList.remove('star-pop');
                    }, 400);
                }, index * 100); // Stagger the animation
            }
        });
    },
    
    // Get rating summary text
    getRatingSummary: function(value) {
        return RATING_MESSAGES[value] || RATING_MESSAGES[0];
    },
    
    // Create a read-only rating display
    createReadOnlyRating: function(container, value, maxStars = 5) {
        container.innerHTML = '';
        for (let i = 1; i <= maxStars; i++) {
            const label = document.createElement('div');
            label.className = 'rating-label rating-readonly';
            label.innerHTML = '<span class="star-icon">?</span>';
            
            if (i <= value) {
                label.classList.add('checked');
            }
            
            container.appendChild(label);
        }
    }
};

// Utility functions for external use
window.StarRatingUtils = {
    initialize: initializeRatings,
    initializeModal: initializeModalRatings,
    createRating: (element) => new StarRating(element),
    createQuickRating: (element) => new QuickRating(element),
    validation: RatingValidation,
    messages: RATING_MESSAGES,
    utils: RatingUtils
};

// Handle dynamic content loading (like modals)
const observer = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
        if (mutation.type === 'childList') {
            mutation.addedNodes.forEach((node) => {
                if (node.nodeType === Node.ELEMENT_NODE) {
                    // Check if the added node contains rating elements
                    if (node.querySelector && (
                        node.querySelector('[data-kt-rating="true"]') || 
                        node.querySelector('.rating-quick')
                    )) {
                        console.log('Rating system: Dynamic content detected, reinitializing...');
                        // Delay to ensure content is fully rendered
                        setTimeout(initializeRatings, 50);
                    }
                }
            });
        }
    });
});

// Start observing for dynamically added content
observer.observe(document.body, {
    childList: true,
    subtree: true
});

// Export for module systems (if needed)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { StarRating, QuickRating, RatingValidation, RatingUtils };
}