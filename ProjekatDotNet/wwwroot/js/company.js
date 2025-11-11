
var dataTable;


$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblCompany').DataTable({
        "processing": true,
        "serverSide": true,
            "ajax": {
                url: '/admin/company/getall',
                "type": "GET"
            },
            "columns": [
            { data: 'name' ,"name":"name","width":"15%"},
            { data: 'streetAddress', "name": "streetAddress" ,"width": "15%" },
            { data: 'city', "name": "city", "width": "15%", "className": "text-center" },
            { data: 'country', "name": "country", "width": "15%" },
            { data: 'postalCode', "name": "postalCode", "width": "15%" },
            { data: 'phoneNumber', "name": "phoneNumber", "width": "15%", "className": "text-center" },
            {
                    data: 'id',
                    "render" : function (data) {
                        return `<div class="w-75 btn-group role="group">
                        <a href="/admin/company/upsert?id=${data}" class="btn btn-outline-primary btn-sm mx-2">  <i class="bi bi-pencil-square"></i>Edit</a>
                         <a onClick=Delete('/admin/company/delete/${data}') class="btn btn-outline-danger btn-sm mx-2"> <i class="bi bi-trash"></i>Delete</a>
                        </div >`
                    },
                    "width": "20%"
            }
        ]
    });
}


function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    });
}

