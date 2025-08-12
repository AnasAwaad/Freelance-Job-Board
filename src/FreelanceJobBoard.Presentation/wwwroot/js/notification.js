// Hub Connection

$(document).ready(function () {

    var proxyConnection = new signalR.HubConnectionBuilder()
        .withUrl("/notifyHub")
        .build();

    // Start Connection
    proxyConnection.start().then(function () {
        console.log("start connection")
    }).catch(function (error) {
        console.log(error)
    });

    proxyConnection.on("ReceiveNotification", function (sender, title, message) {
        console.log("Notification received:", sender, title, message);
        $('#Notifications').prepend(`
                                      <div class="d-flex flex-stack py-4 notification-item">
                                        <div class="d-flex align-items-center">
                                          <div class="symbol symbol-35px me-4">
                                            <span class="symbol-label bg-light-primary">
                                              <i class="ki-duotone ki-briefcase fs-2 text-primary">
                                                <span class="path1"></span>
                                                <span class="path2"></span>
                                              </i>
                                            </span>
                                          </div>
                                          <div class="mb-0 me-2">
                                            <a href="/Admin/Jobs/Review" class="fs-6 text-gray-800 text-hover-primary fw-bold">
                                              New Job Pending Approval
                                            </a>
                                            <div class="text-gray-400 fs-7">
                                              <strong>${title}</strong> submitted by <em>${sender}</em>
                                            </div>
                                          </div>
                                        </div>
                                        <a href="/Admin/Jobs/Review" class="btn btn-sm btn-primary">Review</a>
                                      </div>
                                    `);

    })
})



// receive all old Notifications

$.ajax({
    url: "/Notifications/GetAllNotifications",
    method: "GET",
    success: function (data) {
        console.log(data);
        data.forEach(n => {
            $('#Notifications').append(`
                                      <div class="d-flex flex-stack py-4 notification-item">
                                        <div class="d-flex align-items-center">
                                          <div class="symbol symbol-35px me-4">
                                            <span class="symbol-label bg-light-primary">
                                              <i class="ki-duotone ki-briefcase fs-2 text-primary">
                                                <span class="path1"></span>
                                                <span class="path2"></span>
                                              </i>
                                            </span>
                                          </div>
                                          <div class="mb-0 me-2">
                                            <a href="/Admin/Jobs/Review" class="fs-6 text-gray-800 text-hover-primary fw-bold">
                                              New Job Pending Approval
                                            </a>
                                            <div class="text-gray-400 fs-7">
                                              <strong>${n.title}</strong> submitted by <em>${n.message}</em>
                                            </div>
                                          </div>
                                        </div>
                                        <a href="/Admin/Jobs/Review" class="btn btn-sm btn-primary">Review</a>
                                      </div>
                                    `);
        });

    },
    error: function (err) {
        console.error("Error fetching notifications:", err);
    }
});
