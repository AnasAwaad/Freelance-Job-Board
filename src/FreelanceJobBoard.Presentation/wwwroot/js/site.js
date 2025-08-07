// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var updatedRow;
var datatable;


$(document).ready(function () {
    var message = $('.js-success-message').text();
    if (message != '') {
        showSuccessMessage(message);
    }

    // Initialize DataTables for pages that need it
    initializeDataTable();

    // Handle Global Modal 
    $('.js-render-modal').on('click', function () {
        var btn = $(this);
        var myModal = $('#myModal');
        var title = btn.data('title');
        var url = btn.data('url');

        if (btn.data('update') != undefined) {
            updatedRow = btn.parents('tr');
        }

        myModal.find('.modal-title').text(title);

        // call ajax request to render form inside modal
        $.get({
            url,
            success: function (form) {
                myModal.find('.modal-body').html(form);
                // Re-enable client-side validation for the new form
                $.validator.unobtrusive.parse(myModal);
            },
            error: function (err) {
                showErrorMessage("Failed to load form. Please try again.");
            }
        });

        myModal.modal('show');
    });

    $('.js-modal-save').on('click', function () {
        $('#ModalForm').submit();
    });


});

document.addEventListener("DOMContentLoaded", function () {
    const menuItems = document.querySelectorAll(".menu-item.menu-accordion");

    menuItems.forEach(item => {
        const link = item.querySelector(".menu-link");

        link.addEventListener("click", function () {
            menuItems.forEach(i => {
                if (i !== item) {
                    i.classList.remove("show");
                }
            });

            item.classList.toggle("show");
        });
    });
});
function initializeDataTable() {
    // Check if DataTable element exists and is not already initialized
    if ($('#datatable').length && !$.fn.DataTable.isDataTable('#datatable')) {
        // Only initialize if there are rows with data
        if ($('#datatable tbody tr').length > 0) {
            datatable = $('#datatable').DataTable({
                responsive: true,
                pageLength: 10,
                order: [[0, 'asc']],
                columnDefs: [
                    { orderable: false, targets: -1 } // Disable sorting on last column (Actions)
                ],
                language: {
                    emptyTable: "No data available",
                    info: "Showing _START_ to _END_ of _TOTAL_ entries",
                    infoEmpty: "Showing 0 to 0 of 0 entries",
                    infoFiltered: "(filtered from _MAX_ total entries)",
                    lengthMenu: "Show _MENU_ entries",
                    loadingRecords: "Loading...",
                    processing: "Processing...",
                    search: "Search:",
                    zeroRecords: "No matching records found"
                }
            });
        }
    }
}

function destroyDataTable() {
    // Destroy existing DataTable if it exists
    if ($.fn.DataTable.isDataTable('#datatable')) {
        $('#datatable').DataTable().destroy();
    }
}

function onModalFormSuccess(newRow) {
    if (newRow != undefined) {
        if (updatedRow != undefined) {
            // Update existing row
            updatedRow.replaceWith(newRow);
            message = "Item updated successfully";
        } else {
            // Add new row
            $('.js-tbody').append(newRow);
            message = "Item created successfully";
        }

        // Reinitialize DataTable
        destroyDataTable();
        initializeDataTable();
    }

    updatedRow = undefined;
    $('#myModal').modal('hide');
    showSuccessMessage(message);
}

function onModalFormFailure(res) {
    $('#myModal').modal('hide');
    showErrorMessage("An error occurred while processing your request");
}

// Handle change status checkbox
$(document).on('click', '.js-change-status', function () {
    var btn = $(this);
    var url = btn.data('url');

    bootbox.confirm({
        title: "Change Status Alert",
        message: "Do you want to change status of this item?",
        buttons: {
            cancel: {
                label: '<i class="fa fa-times"></i> Cancel',
                className: 'btn btn-secondary'
            },
            confirm: {
                label: '<i class="fa fa-check"></i> Confirm',
                className: 'btn btn-danger',
            }
        },
        callback: function (result) {
            if (result) {
                $.ajax({
                    url: url,
                    type: 'POST',
                    data: {
                        "__RequestVerificationToken": $("input[name='__RequestVerificationToken']").val()
                    },
                    success: function (response) {
                        showSuccessMessage("Status updated successfully");

                        // Update last updated time if exists
                        btn.parents('tr').find('.js-last-updated-on').html(response.lastUpdatedOn);

                        // Update status badge
                        var statusItem = btn.parents('tr').find('.js-status');
                        if (response.isActive !== undefined) {
                            if (response.isActive) {
                                statusItem.html('<span class="badge badge-success">Active</span>');
                            } else {
                                statusItem.html('<span class="badge badge-warning">Inactive</span>');
                            }
                        }
                    },
                    error: function () {
                        showErrorMessage("An error occurred while changing status.");
                    }
                });
            }
        }
    });
});

function showSuccessMessage(message) {
    // Create and show success toast
    const toast = $(`
        <div class="alert alert-success alert-dismissible fade show position-fixed" 
             style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;" role="alert">
            <i class="ki-duotone ki-check fs-3 me-2">
                <span class="path1"></span>
                <span class="path2"></span>
            </i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);

    $('body').append(toast);

    // Auto remove after 3 seconds
    setTimeout(function () {
        toast.alert('close');
    }, 3000);
}

function showErrorMessage(message) {
    // Create and show error toast
    const toast = $(`
        <div class="alert alert-danger alert-dismissible fade show position-fixed" 
             style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;" role="alert">
            <i class="ki-duotone ki-cross fs-3 me-2">
                <span class="path1"></span>
                <span class="path2"></span>
            </i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);

    $('body').append(toast);

    // Auto remove after 5 seconds
    setTimeout(function () {
        toast.alert('close');
    }, 5000);
}

// Remove the old KTDatatables object as it's causing conflicts