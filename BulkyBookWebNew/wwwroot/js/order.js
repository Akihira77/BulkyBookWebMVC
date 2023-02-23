let dataTable;

$(document).ready(function () {
	let url = window.location.search;
	if (url.includes("inprocess")) {
		loadDataTable("inprocess");
	} else if (url.includes("completed")) {
		loadDataTable("completed");
	} else if (url.includes("pending")) {
		loadDataTable("pending");
	} else if (url.includes("approved")) {
		loadDataTable("approved");
	} else {
		loadDataTable("all");
	}
	//loadDataTable();
});

function loadDataTable(status) {
	dataTable = $('#tblData').DataTable({
		"ajax": {
			"url":"/Admin/Order/GetAll?status=" + status
		},
		"columns": [
			{ "data": "id", "width": "5%" },
			{ "data": "name", "width": "20%" },
			{ "data": "phoneNumber", "width": "10%" },
			{ "data": "applicationUser.email", "width": "20%" },
			{ "data": "orderStatus", "width": "10%" },
			{ "data": "orderTotal", "width": "10%" },
			{
				"data": "id",
				"render": function (data) {
					return `
						<div class="w-75 btn-group d-flex mx-auto" role="group">
							<a href="/Admin/Order/Details?orderId=${data}" class="btn btn-info d-flex flex-column align-items-center" style="width: 4rem"><i class="bi bi-pencil-square"></i></a>
						</div>
						`
				},
				"width": "5%",
			},
		]
	});
}