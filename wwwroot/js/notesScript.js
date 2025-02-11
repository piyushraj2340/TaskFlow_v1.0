$(document).ready(function () {
    // Tabs
    $(".tab-link").on('click', function (e) {
        e.preventDefault();

        history.pushState(null, null, this.href);

        // Remove active class from all tabs
        $(".tab-link").removeClass("text-blue-600 border-blue-600 font-semibold").addClass("text-gray-600 border-gray-600");

        // Add active class to the clicked tab
        $(this).addClass("text-blue-600 border-blue-600 font-semibold").removeClass("text-gray-600 border-gray-600");

        // Hide all tab contents
        $(".tab-content").addClass("hidden");

        // Show the selected tab
        $("#" + $(this).data("tab")).removeClass("hidden");
    });

    // Handel Create Notes...
    $("#noteForm").on('submit', function (e) {
        e.preventDefault();
        const data = {
            Title: $("#title").val(),
            Content: $("#content").val(),
            Tags: $("#tags").val(),
            IsPinned: true
        }

        const goalId = $("#noteForm").data("goalid");

        $.ajax({
            url: `/Notes/Create/${goalId}`,
            method: "POST",
            data: data,
            success: function (response) {
                showSuccessNotification(response.message || "Notes Saved with Goal!");

                setTimeout(() => {
                    $("#goalModal").fadeOut();
                }, 500)
            },
            error: function (xhr, status, error) {
                showErrorNotification(error.message || "Notes Saved Failed!");
                console.error("Error:", error);
            }
        });

    });

    $("#openAddNotesModal").on('click', function () {
        $("#goalModal").removeClass("hidden").fadeIn();
    });

    $(".edit-notes").on('click', function () {
        //$("#goalModal").removeClass("hidden").fadeIn();

        alert($(this).data("noteId"));
    })

    // Close Modal
    $("#closeModal").on('click',function () {
        $("#goalModal").fadeOut();
    });

    // Display File Name
    //$("#attachments").on('change', function () {
    //    let fileName = $(this).val().split("\\").pop();
    //    $("#fileName").text(fileName);
    //});

    // Handle Form Submission
    $("#saveBtn").on('click', function () {
        $("#noteForm").submit();
    });
})