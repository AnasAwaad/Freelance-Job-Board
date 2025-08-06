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

    KTDatatables.init();



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
                //$.validator.unobtrusive.parse(myModal);
            },
            error: function (err) {
                showErrorMessage(err);
            }
        })


        myModal.modal('show');
    })

    $('.js-modal-save').on('click', function () {
        $('#ModalForm').submit();
    })
})

function onModalFormSuccess(newRow) {
    $('.js-tbody').append(newRow);
    // add new row to datatable.
    if (newRow != undefined)
        datatable.row.add($(newRow)).draw();

    if (updatedRow != undefined) {
        // remove existing row from datatable
        datatable.row(updatedRow).remove().draw();
        updatedRow = undefined;

        message = "Category updated successfully";
    } else {

        message = "Category created successfully";
    }

    $('#myModal').modal('hide');

    showSuccessMessage("Category created successfully");

}

function onModalFormFailure(res) {
    $('#myModal').modal('hide');
    showErrorMessage("An error happen while creating category");
}


// Handle change status checkbox
$('.js-change-status').on('click', function () {
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
                $.post({
                    url,
                    data: {
                        "__RequestVerificationToken": $("input[name='__RequestVerificationToken']").val()
                    },
                    success: function (response) {
                        // toastify alert
                        showSuccessMessage("item updated successfully");

                        // change time of last updated on
                        btn.parents('tr').find('.js-last-updated-on').html(response.lastUpdatedOn);

                        // change status badge text and style
                        var statusItem = btn.parents('tr').find('.js-status');
                        if (btn.data('status') !== undefined) {
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
                })
            }
        }
    });


})

function showSuccessMessage(message) {
    //Toastify({
    //    text: message,
    //    duration: 3000,
    //    newWindow: true,
    //    close: true,
    //    gravity: "top",
    //    position: "right",
    //    stopOnFocus: true,
    //    style: {
    //        background: "linear-gradient(to right, #00b09b, #96c93d)",
    //    },
    //}).showToast();
}

function showErrorMessage(message) {
    //Toastify({
    //    text: message,
    //    duration: 3000,
    //    newWindow: true,
    //    close: true,
    //    gravity: "top",
    //    position: "right",
    //    stopOnFocus: true,
    //    style: {
    //        background: "linear-gradient(to right, #ff5f6d, #ffc371)", // red-orange gradient
    //        color: "#fff"
    //    },
    //    icon: "❌"
    //}).showToast();
}


var KTDatatables = function () {
    // Shared variables
    var table;

    // Private functions
    var initDatatable = function () {

        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'order': [],
            'pageLength': 10,
        });
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = $(table).data("title");
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle
                }
            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Public methods
    return {
        init: function () {
            table = document.querySelector('#datatable');

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
        }
    };
}();