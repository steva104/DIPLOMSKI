
var dataTable;


$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
        dataTable = $('#tblUser').DataTable({
            "ajax": {
                url: '/admin/user/getall',
                "type": "GET"
            },
            "columns": [
            { data: 'name' ,"width":"15%"},
            { data: 'email' ,"width": "15%" },
            { data: 'phoneNumber', "width": "15%", "className": "text-center" },
            { data: 'company.name', "width": "15%" },
            { data: 'role', "width": "15%" },           
                {
                    data: { id:'id', lockoutEnd: 'lockoutEnd'},
                    "render": function (data) {
                        var today = new Date().getTime();
                        var lockout = new Date(data.lockoutEnd).getTime();

                        if (lockout > today) {
                            return `
                                    <div class="text-center">
                                    <a onclick=LockUnlock('${data.id}') class="btn btn-success text-light" style="cursor:pointer; width:100px;">
                                        <i class="bi bi-unlock-fill"></i> Unlock
                                    </a>
                                    <a href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-info text-light" style="cursor:pointer; width:100px;">
                                        <i class="bi bi-pencil-square"></i> Edit
                                    </a>
                                    </div >
                                    `
                        }
                        else {
                            return `
                                    <div class="text-center">
                                    <a onclick=LockUnlock('${data.id}') class="btn btn-danger text-light" style="cursor:pointer; width:100px;">
                                        <i class="bi bi-lock-fill"></i> Lock
                                    </a>
                                    <a href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-info text-light" style="cursor:pointer; width:100px;">
                                        <i class="bi bi-pencil-square"></i> Edit
                                    </a>
                                    </div >
                                    `
                        }

                    
                    },
                    "width": "20%"
            }
        ]
    });
}


function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                dataTable.ajax.reload();
                toastr.success(data.message);  
               
                              
            }
        }
    });



}

