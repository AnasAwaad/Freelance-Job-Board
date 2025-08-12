var proxyConnection = new signalR.HubConnectionBuilder()
	.withUrl("https://localhost:7000/notifyHub")
	.build();


proxyConnection.on("ReceiveNotification", function (sender, title, message) {
	console.log("Notification received:", sender, title, message);
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

