
var dataTable;


$(document).ready(function () {
    var url = window.location.search;
    var statusLoaded = false;

    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    } else if (url.includes("completed")) {
        loadDataTable("completed");
    } else if (url.includes("approved")) {
        loadDataTable("approved");
    } else if (url.includes("cancelled")) {
        loadDataTable("cancelled");
    } 
    else {
        loadDataTable("all"); // Samo ako nijedan status nije pronađen
    }
});
function loadDataTable(status) {
    dataTable = $('#tblOrder').DataTable({
        "processing": true,
        "serverSide": true,
        "ajax": {
            "url": '/admin/order/getall?status=' + status,
            "type": "GET"
        },
        "columns": [
            { data: 'id', "name": "id", "width": "5%", "className": "text-center" },
            { data: 'name', "name": "name", "width": "20%" },
            { data: 'phoneNumber', "name": "phoneNumber", "width": "15%", "className": "text-center" },
            { data: 'email', "name": "email", "width": "20%" },
            { data: 'orderStatus', "name": "orderStatus", "width": "15%", "className": "text-center" },
            { data: 'orderTotal', "name": "orderTotal", "width": "10%", "className": "text-center" },
            {
               "data": "id",
               "name": "id",
                "render": function (data) {
                    return `<div class="w-75 btn-group role="group">
                        <a href="/admin/order/details?orderId=${data}" class="btn btn-outline-primary btn-sm mx-2">
                        <i class="bi bi-pencil-square"></i>Manage</a></div>`;
                },
                "width": "20%",
                "orderable": false
            }
        ]
    });
}


