const SWEET_ALERT_Z_INDEX_CLASS = 'tma-swal-zindex';

// Ensure SweetAlert container always sits above high-z-index modals.
(function ensureSweetAlertZIndexStyle() {
    if (document.getElementById('tma-swal-zindex-style')) {
        return;
    }

    const style = document.createElement('style');
    style.id = 'tma-swal-zindex-style';
    style.textContent = `.${SWEET_ALERT_Z_INDEX_CLASS}{z-index:9999999999 !important;}`;
    document.head.appendChild(style);
})();

function withSweetAlertZIndex(options) {
    const safeOptions = options || {};
    const existingContainerClass = safeOptions.customClass && safeOptions.customClass.container
        ? safeOptions.customClass.container
        : '';

    return {
        ...safeOptions,
        customClass: {
            ...(safeOptions.customClass || {}),
            container: `${existingContainerClass} ${SWEET_ALERT_Z_INDEX_CLASS}`.trim()
        }
    };
}

// Success notification
function showSuccessNotification(message) {
    Swal.fire(withSweetAlertZIndex({
        icon: 'success',
        title: 'Success!',
        text: message,
        position: 'top-end',  // Position at the top right
        showConfirmButton: false,  // Hide the confirm button
        timer: 1000,  // Close after 1 second
        toast: true,  // Make it a toast-style notification
        timerProgressBar: true,  // Show a progress bar as it counts down
    }));
}

// Error notification
function showErrorNotification(message) {
    Swal.fire(withSweetAlertZIndex({
        icon: 'error',
        title: 'Error!',
        text: message || "Error: While processing your request.",
        position: 'top-end',  // Position at the top right
        showConfirmButton: false,  // Hide the confirm button
        timer: 1000,  // Close after 1 second
        toast: true,  // Make it a toast-style notification
        timerProgressBar: true,  // Show a progress bar as it counts down
    }));
}


function showConfirmationDialog(options, ajaxConfig, successCallback, errorCallback) {
    // Set default SweetAlert2 options
    const defaultOptions = {
        title: 'Are you sure?',
        text: 'Do you want to proceed with this action?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, do it!',
        cancelButtonText: 'Cancel',
        buttonsStyling: false,
        customClass: {
            confirmButton: 'bg-red-500 hover:bg-red-600 text-white font-bold py-2 px-4 rounded mr-2',
            cancelButton: 'bg-gray-300 hover:bg-gray-400 text-black font-bold py-2 px-4 rounded',
        }
    };

    // Merge user-provided options with defaults
    const swalOptions = {
        ...defaultOptions,
        ...options,
        customClass: {
            ...defaultOptions.customClass,
            ...((options && options.customClass) || {})
        }
    };

    // Show SweetAlert2 dialog
    Swal.fire(withSweetAlertZIndex(swalOptions)).then((result) => {
        if (result.isConfirmed) {
            // Perform AJAX request if user confirms
            $.ajax({
                ...ajaxConfig, // Pass AJAX configuration
                success: function (response) {
                    Swal.fire(withSweetAlertZIndex({
                        title: 'Success!',
                        text: 'The action was completed successfully.',
                        icon: 'success'
                    }));
                    if (successCallback) successCallback(response); // Call success callback if provided
                },
                error: function (error) {
                    Swal.fire(withSweetAlertZIndex({
                        title: 'Error!',
                        text: 'An error occurred while performing the action.',
                        icon: 'error'
                    }));
                    if (errorCallback) errorCallback(error); // Call error callback if provided
                }
            });
        } else if (result.isDismissed) {
            Swal.fire(withSweetAlertZIndex({
                title: 'Cancelled',
                text: 'The action was cancelled.',
                icon: 'info'
            }));
        }
    });
}


function showModelCloseAlert(resetCallBack) {
    // Set default SweetAlert2 options
    const defaultOptions = {
        title: 'Are you sure?',
        text: 'Do you want to close the modal? You will lose all the changes!!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Close it!',
        cancelButtonText: 'Cancel',
        buttonsStyling: false,
        customClass: {
            confirmButton: 'bg-red-500 hover:bg-red-600 text-white font-bold py-2 px-4 rounded mr-2',
            cancelButton: 'bg-gray-300 hover:bg-gray-400 text-black font-bold py-2 px-4 rounded',
        }
    };

    // Show SweetAlert2 dialog
    Swal.fire(withSweetAlertZIndex(defaultOptions)).then((result) => {
        if (result.isConfirmed) {
            resetCallBack();
        }
    });
}