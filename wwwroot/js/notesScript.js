$(document).ready(function () {
    // Tabs changes
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

    $("#noteForm").on('submit', function (e) {
        e.preventDefault();
        const data = {
            Title: $("#title").val(),
            Content: $("#content").val(),
            Tags: $("#tags").val(),
            IsPinned: $("#IsPinned").prop('checked'),
        }

        const action = $(this).data('action-note-form');
        const goalId = $(this).data("goalid");
        const noteId = $(this).data("noteid");

        if (action.toString().toLowerCase() === 'create') {
            $.ajax({
                url: `/Notes/Create/${goalId}`,
                method: "POST",
                data: data,
                success: function (response) {
                    showSuccessNotification(response.message || "Notes Saved with Goal!");

                    setTimeout(() => {
                        $("#noteModal").fadeOut();
                    }, 500)
                },
                error: function (xhr, status, error) {
                    showErrorNotification(error.message || "Notes Saved Failed!");
                    console.error("Error:", error);
                }
            });
        } else if (action.toString().toLowerCase() === 'edit' && noteId) {
            data.id = noteId;

            $.ajax({
                url: `/Notes/Edit/${noteId}`,
                method: "POST",
                data: data,
                success: function (response) {
                    showSuccessNotification(response.message || "Notes Saved with Goal!");

                    setTimeout(() => {
                        $("#noteModal").fadeOut();
                    }, 500)
                },
                error: function (xhr, status, error) {
                    showErrorNotification(error.message || "Notes Saved Failed!");
                    console.error("Error:", error);
                }
            });
        } else {
            showErrorNotification("Invalid Action!")
        }
    });

    $("#openAddNotesModal").on('click', function () {
        $("#noteModal").removeClass("hidden").fadeIn();
        $("#note-title").html('<i class="fa-solid fa-clipboard-list"></i> Create a Progress Record in Goal');
        $("#note-id-field").addClass("hidden");

        $("#title").val('');
        $("#content").val('');
        $("#tags").val('');
        $("#IsPinned").prop("checked", false);

    });

    $(".edit-notes").on('click', function (e) {
        
        $("#noteModal").removeClass("hidden").fadeIn();
        $("#note-title").html('<i class="fa-solid fa-edit"></i> Edit Your Progress Record in Goal');
        $("#noteForm").data('action-note-form', 'edit');
        
        $("#note-id-field").removeClass("hidden").fadeIn();

        const id = $(this).data("noteeditid");

        $("#noteForm").data("noteid", id);

        $.ajax({
            url: `/Notes/Details/${id}`,
            method: "POST",
            success: function (response) {
                $("#noteModal").removeClass("hidden").fadeIn();

                const { data } = response;

                $("#noteid").val(data.id);
                $("#title").val(data.title);
                $("#content").val(data.content);
                $("#tags").val(data.tags);
                $("#IsPinned").prop("checked", data.isPinned);

            },
            error: function (xhr, status, error) {
                showErrorNotification(error.message || "Failed To Load data!");
                console.error("Error:", error);
            }
        });
    })

    // Close Modal
    $("#closeModal").on('click',function () {
        $("#noteModal").fadeOut();
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


    $(".delete-notes").on("click",  async function () {
        const id = $(this).data("noteeditid");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        try {

            // Call reusable confirmation function
            showConfirmationDialog(
                {
                    title: 'Are you sure?',
                    text: 'Do you really want to delete this goal? This action cannot be undone.',
                },
                {
                    url: `/Notes/Delete`,
                    type: 'DELETE',
                    data: { Id: id }
                },
                function (response) { // Success callback
                    $(`button[data-id='${goalId}']`).parent().remove();

                },
                function (error) { // Error callback
                    console.error('Error deleting goal:', error);
                }
            );

            //const res = await $.ajax({
            //    url: "/Goals/DeleteGoal",
            //    method: "DELETE",
            //    data: { Id: id }
            //})

            //if (res.status) {

            //    if (goalRunningDataTableReload !== null) {
            //        goalRunningDataTableReload.draw();
            //    }

            //    showSuccessNotification(res.message);

            //} else {
            //    showErrorNotification(res.message || "Error: while deleting Goal with Id: " + id);
            //}
        }
        catch (error) {
            showErrorNotification(error.message || "Error: while deleting Goal with Id: " + id);
            console.log(error);
        }
    })
})