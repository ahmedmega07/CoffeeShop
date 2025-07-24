// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web ~.

// Write your JavaScript code.

// Add item to cart via AJAX
function addToCartAjax(productId, buttonElement) {
    // Disable button and show loading state
    const $button = $(buttonElement);
    const originalText = $button.html();
    $button.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status"></span> Adding...');

    $.ajax({
        url: '/ShoppingCart/AddToShoppingCartAjax',
        type: 'POST',
        data: { pId: productId },
        success: function (response) {
            if (response.success) {
                // Update cart count in navigation
                updateCartCount(response.cartCount);
                
                // Show success message
                showNotification(response.message, 'success');
                
                // Update button text temporarily
                $button.html('<i class="mbrib-checkmark mbr-iconfont mbr-iconfont-btn"></i> Added!');
                $button.removeClass('btn-primary').addClass('btn-success');
                
                // Reset button after 2 seconds
                setTimeout(function() {
                    $button.html(originalText);
                    $button.removeClass('btn-success').addClass('btn-primary');
                    $button.prop('disabled', false);
                }, 2000);
            } else {
                showNotification(response.message, 'error');
                $button.prop('disabled', false).html(originalText);
            }
        },
        error: function () {
            showNotification('An error occurred while adding item to cart.', 'error');
            $button.prop('disabled', false).html(originalText);
        }
    });
}

// Remove item from cart via AJAX
function removeFromCartAjax(productId, elementToUpdate) {
    $.ajax({
        url: '/ShoppingCart/RemoveFromShoppingCartAjax',
        type: 'POST',
        data: { pId: productId },
        success: function (response) {
            if (response.success) {
                updateCartCount(response.cartCount);
                showNotification(response.message, 'success');
                
                // Remove the item row if we're on the cart page
                if (elementToUpdate) {
                    $(elementToUpdate).closest('.row[data-product-id]').fadeOut(500, function() {
                        $(this).next('hr').remove(); // Remove the associated hr element
                        $(this).remove();
                        updateCartTotal(response.cartTotal);
                        updateCartItemCount(response.cartCount);
                        
                        // If cart is empty, reload the page to show empty cart message
                        if (response.cartCount === 0) {
                            setTimeout(function() {
                                location.reload();
                            }, 1000);
                        }
                    });
                }
            } else {
                showNotification(response.message, 'error');
            }
        },
        error: function () {
            showNotification('An error occurred while removing item from cart.', 'error');
        }
    });
}

// Increase quantity via AJAX
function increaseQuantityAjax(productId, quantityElement) {
    $.ajax({
        url: '/ShoppingCart/IncreaseQuantityAjax',
        type: 'POST',
        data: { pId: productId },
        success: function (response) {
            if (response.success) {
                updateCartCount(response.cartCount);
                $(quantityElement).text(response.newQuantity);
                
                // Update item total if element exists
                const itemTotalElement = $(quantityElement).closest('.row').find('.item-total');
                if (itemTotalElement.length) {
                    itemTotalElement.text(response.itemTotal);
                }
                
                updateCartTotal(response.cartTotal);
                updateCartItemCount(response.cartCount);
            } else {
                showNotification(response.message, 'error');
            }
        },
        error: function () {
            showNotification('An error occurred while updating quantity.', 'error');
        }
    });
}

// Decrease quantity via AJAX
function decreaseQuantityAjax(productId, quantityElement) {
    $.ajax({
        url: '/ShoppingCart/DecreaseQuantityAjax',
        type: 'POST',
        data: { pId: productId },
        success: function (response) {
            if (response.success) {
                updateCartCount(response.cartCount);
                $(quantityElement).text(response.newQuantity);
                
                // Update item total if element exists
                const itemTotalElement = $(quantityElement).closest('.row').find('.item-total');
                if (itemTotalElement.length) {
                    itemTotalElement.text(response.itemTotal);
                }
                
                updateCartTotal(response.cartTotal);
                updateCartItemCount(response.cartCount);
            } else {
                showNotification(response.message, 'error');
            }
        },
        error: function () {
            showNotification('An error occurred while updating quantity.', 'error');
        }
    });
}

// Update quantity via AJAX
function updateQuantityAjax(productId, quantity, quantityElement) {
    $.ajax({
        url: '/ShoppingCart/UpdateQuantityAjax',
        type: 'POST',
        data: { pId: productId, quantity: quantity },
        success: function (response) {
            if (response.success) {
                updateCartCount(response.cartCount);
                $(quantityElement).val(response.newQuantity);
                
                // Update item total if element exists
                const itemTotalElement = $(quantityElement).closest('.row').find('.item-total');
                if (itemTotalElement.length) {
                    itemTotalElement.text(response.itemTotal);
                }
                
                updateCartTotal(response.cartTotal);
                updateCartItemCount(response.cartCount);
            } else {
                showNotification(response.message, 'error');
            }
        },
        error: function () {
            showNotification('An error occurred while updating quantity.', 'error');
        }
    });
}

// Update cart count in navigation
function updateCartCount(count) {
    $('.cart-count').text('(' + count + ')');
}

// Update cart total
function updateCartTotal(total) {
    $('.cart-total').text(total);
}

// Update cart item count on cart page
function updateCartItemCount(count) {
    $('.cart-item-count').text(count + ' items');
}

// Show notification messages
function showNotification(message, type) {
    // Remove existing notifications
    $('.toast-notification').remove();
    
    // Create notification element
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    const iconClass = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle';
    
    const notification = $(`
        <div class="toast-notification alert ${alertClass} alert-dismissible fade show position-fixed" 
             style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
            <i class="fas ${iconClass} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);
    
    // Add to page
    $('body').append(notification);
    
    // Auto remove after 5 seconds
    setTimeout(function() {
        notification.fadeOut(500, function() {
            $(this).remove();
        });
    }, 5000);
}

// Initialize page-specific functionality
$(document).ready(function() {
    // Handle quantity input changes on cart page
    $('.quantity-input').on('change', function() {
        const productId = $(this).data('product-id');
        const quantity = parseInt($(this).val());
        
        if (quantity > 0) {
            updateQuantityAjax(productId, quantity, this);
        } else {
            $(this).val(1);
            showNotification('Quantity must be at least 1.', 'error');
        }
    });
    
    // Handle Enter key on quantity inputs
    $('.quantity-input').on('keypress', function(e) {
        if (e.which === 13) {
            $(this).trigger('change');
        }
    });
    
    // Add smooth animations for cart updates
    $('.item-total, .cart-total').on('DOMSubtreeModified', function() {
        $(this).addClass('highlight-update');
        setTimeout(() => {
            $(this).removeClass('highlight-update');
        }, 1000);
    });
});

// Add CSS for highlighting updates (inject into head)
$(document).ready(function() {
    $('<style>')
        .prop('type', 'text/css')
        .html(`
            .highlight-update {
                background-color: #fff3cd !important;
                transition: background-color 1s ease;
            }
            .toast-notification {
                animation: slideInRight 0.5s ease-out;
            }
            @keyframes slideInRight {
                from {
                    transform: translateX(100%);
                    opacity: 0;
                }
                to {
                    transform: translateX(0);
                    opacity: 1;
                }
            }
        `)
        .appendTo('head');
});
