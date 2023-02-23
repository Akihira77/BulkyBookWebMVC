let dataTable;

$(document).ready(function () {
	loadDataTable();
});

function loadDataTable() {
	dataTable = $('#tblData').DataTable({
		"ajax": {
			"url": "/Admin/Company/GetAll"
		},
		"columns": [
			{ "data": "name", "width": "20%" },
			{ "data": "streetAddress", "width": "20%" },
			{ "data": "city", "width": "15%" },
			{ "data": "state", "width": "10%" },
			//{ "data": "postalCode", "width": "12%" },
			{ "data": "phoneNumber", "width": "15%" },
			{
				"data": "id",
				"render": function (data) {
					return `
						<div class="w-75 btn-group d-flex mx-auto" role="group">
							<a href="/Admin/Company/Upsert?id=${data}" class="btn btn-info d-flex flex-column align-items-center" style="width: 4rem"><i class="bi bi-pencil-square"></i> Edit</a>
							<a onClick=Delete('/Admin/Company/Delete/'+${data}) class="btn btn-danger d-flex flex-column align-items-center" style="width: 4rem"><i class="bi bi-trash-fill"></i> Delete</a>
						</div>
						`
				},
				"width": "20%",
			},
		]
	});
}

function Delete(url) {
	Swal.fire({
		title: 'Are you sure?',
		text: "You won't be able to revert this!",
		icon: 'warning',
		showCancelButton: true,
		confirmButtonColor: '#3085d6',
		cancelButtonColor: '#d33',
		confirmButtonText: 'Yes, delete it!'
	}).then((result) => {
		if (result.isConfirmed) {
			$.ajax({
				url: url,
				type: 'DELETE',
				success: function (data) {
					if (data.success) {
						dataTable.ajax.reload();
						toastr.success(data.message);
					} else {
						toastr.error(data.message);
					}
				},
			})
		}
	})
}