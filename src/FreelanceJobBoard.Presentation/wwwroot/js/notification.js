// Hub Connection
var proxyConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5102/notifyHub")
    .build();

// Start Connection
proxyConnection.start().then(function () {
    console.log("start connection")
}).catch(function (error) {
    console.log(error)
});


proxyConnection.on("ReceiveNotification", function (title, message) {
    $('#Notifications').append(`<div class="d-flex flex-stack py-4">
										<div class="d-flex align-items-center">
											<div class="symbol symbol-35px me-4">
												<span class="symbol-label bg-light-primary">
													<i class="ki-duotone ki-abstract-28 fs-2 text-primary">
														<span class="path1"></span>
														<span class="path2"></span>
													</i>
												</span>
											</div>
											<div class="mb-0 me-2">
												<a href="#" class="fs-6 text-gray-800 text-hover-primary fw-bold">${title}</a>
												<div class="text-gray-400 fs-7">${message}</div>
											</div>
										</div>
										<span class="badge badge-light fs-8">1 hr</span>
									</div>`)
})



// receive all old Notifications
const userId = "@User.FindFirst(ClaimTypes.NameIdentifier)?.Value";

$.ajax({
    url: "/Notifications/GetAllNotifications",
    method: "GET",
    success: function (data) {
        console.log(data);
        data.forEach(n => {
            $('#Notifications').append(`<div class="d-flex flex-stack py-4">
										<div class="d-flex align-items-center">
											<div class="symbol symbol-35px me-4">
												<span class="symbol-label bg-light-primary">
													<i class="ki-duotone ki-abstract-28 fs-2 text-primary">
														<span class="path1"></span>
														<span class="path2"></span>
													</i>
												</span>
											</div>
											<div class="mb-0 me-2">
												<a href="#" class="fs-6 text-gray-800 text-hover-primary fw-bold">${n.title}</a>
												<div class="text-gray-400 fs-7">${n.message}</div>
											</div>
										</div>
										<span class="badge badge-light fs-8">1 hr</span>
									</div>`)
        });

    },
    error: function (err) {
        console.error("Error fetching notifications:", err);
    }
});
