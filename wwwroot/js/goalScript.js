$(document).ready(function () {
    // JavaScript to toggle modal visibility
    const openModalButton = $("#openModalButton");
    const modal = $("#addEditGoalModal");

    // Goal Id Input Field and Error Handling..
    const goalIdInput = $("#goalId");
    const goalIdError = $("#goalIdError");

    // Name Input Field and Error Handling..
    const goalNameInput = $("#goalName");
    const goalNameError = $("#goalNameError");

    // Description Input Field and error handling...
    const goalDescriptionInput = $("#goalDescription");
    const goalDescriptionError = $("#goalDescriptionError");

    const goalPriorityInput = $("#goalPriority");
    const goalPriorityError = $("#goalPriorityError");

    const goalStatusInput = $("#goalStatus");
    const goalStatusError = $("#goalStatusError");

    // End Date Input Field and Errror Handling..
    const goalEndDateInput = $("#endDate");
    const goalEndDateError = $("#goalDueDateError");

    const goalForm = $("#goalForm");
    const submitBtn = $("#submit-btn");

    // Running task...
    const runningGoalDataTable = $('#viewRunningGoalTableData');
    let goalRunningDataTableReload = null;

    // Compleated Task...
    const completedGoalDataTable = $('#viewCompletedGoalTableData');
    let goalCompletedDataTableReload = null;

    // Not Started Task...
    const notStartedGoalDataTable = $('#viewNotStartedGoalTableData');
    let goalNotStartedDataTableReload = null;

    // Not Started Task...
    const endedGoalDataTable = $('#viewEndedGoalTableData');
    let goalEndedDataTableReload = null;

    // Not Started Task...
    const deletedGoalDataTable = $('#viewDeletedTableData');
    let goalDeletedDataTableReload = null;

    let totalGoal = 0; // completed + end
    let completedGoal = 0;

    let isEditMode = false;  // Flag to track if it's edit mode or create mode
    let currentGoalId = null;  // Store the ID of the goal being edited

    // Open modal for create
    openModalButton.on("click", function () {
        isEditMode = false;  // Set to create mode
        currentGoalId = null;  // Clear the current goal ID
        openModal();
    });

    // Close modal
    $("#closeModalButton").on("click", closeModal);

    // Handle form submission
    goalForm.on("submit", async function (e) {
        e.preventDefault(); // Prevent default form submission

        const goalData = {
            name: goalNameInput.val(),
            endDate: $("#endDate").val(),
            priority: $("#goalPriority").val(),
            description: $("#goalDescription").val()
        };

        if (isEditMode) {
            // Edit mode: Include goal ID
            goalData.id = currentGoalId;
            goalData.goalStatus = $("#goalStatus").val();  // Include status for edit
        }

        try {
            const url = isEditMode ? "/Goals/Edit" : "/Goals/Create";
            const method = isEditMode ? "PUT" : "POST";

            validateData(isEditMode ? 'edit' : 'add');

            // Send request to create or edit goal
            const res = await $.ajax({
                url: url,
                method: method,
                data: goalData
            });

            if (res.status) {
                // If successful, reload the goal data
                /*runningGoalDataTable.DataTable({ responsive: true }).ajax.reload();*/

                if (goalRunningDataTableReload !== null) {
                    goalRunningDataTableReload.draw();
                }

                if (goalNotStartedDataTableReload !== null) {
                    goalNotStartedDataTableReload.draw();
                }

                if (goalCompletedDataTableReload !== null) {
                    goalCompletedDataTableReload.draw();
                }

                if (goalEndedDataTableReload !== null) {
                    goalEndedDataTableReload.draw();
                }

                //loadRunningGoalData();
                closeModal();

                //Show Alert message of success...
                showSuccessNotification(res.message);
            } else {
                //Show Alert message of success...
                showErrorNotification(res.message);
            }
        } catch (error) {
            //Show Alert message of success...
            showErrorNotification(error.message);
            console.log(error);
        }
    });

    // Modal background click close
    modal.on("click", function (e) {
        if (e.target === this) {
            closeModal();
        }
    });

    // Open modal for edit
    $(document).on("click", ".editGoalBtn", async function () {
        isEditMode = true;  // Set to edit mode
        currentGoalId = $(this).data("id");

        // Fetch goal data
        const res = await $.ajax({
            url: `/Goals/GetById/${currentGoalId}`,
            method: "GET",
        });

        if (res.status) {
            // Load the data into the form for editing
            openModal();
            loadDataIntoForm(res.data);

        } else {
            //Show Alert message of success...
            showErrorNotification(res.message);
        }
    });

    // delete goal
    $(document).on("click", ".deleteGoalBtn", async function () {
        const id = $(this).data("id");

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
                    url: `/Goals/DeleteGoal`,
                    type: 'DELETE',
                    data: { Id: id }
                },
                function (response) { // Success callback
                    $(`button[data-id='${goalId}']`).parent().remove();
                    //if (goalRunningDataTableReload !== null) {
                    //    goalRunningDataTableReload.draw();
                    //}

                    //if (goalCompletedDataTableReload !== null) {
                    //    goalCompletedDataTableReload.draw();
                    //}

                    //if (goalNotStartedDataTableReload !== null) {
                    //    goalNotStartedDataTableReload.draw();
                    //}

                    //if (goalEndedDataTableReload !== null) {
                    //    goalEndedDataTableReload.draw();
                    //}

                    //if (goalDeletedDataTableReload !== null) {
                    //    goalDeletedDataTableReload.draw();
                    //}
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

    // Mark goal as completed...
    $(document).on("click", ".markAsComplete", async function () {
        const id = $(this).data("id");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        try {

            const res = await $.ajax({
                url: "/Goals/ChangeGoalStatus",
                method: "POST",
                data: {
                    Id: id,
                    GoalStatus: 2 // 0 is for NotStarted, 1 is for Running, 2 is for Completed, 3 is for End
                }
            })

            if (res.status) {

                if (goalRunningDataTableReload !== null) {
                    goalRunningDataTableReload.draw();
                }

                if (goalCompletedDataTableReload !== null) {
                    goalCompletedDataTableReload.draw();
                }

                showSuccessNotification(res.message);
            } else {
                showErrorNotification(res.message || "Error: while changing the status of Goal with Id: " + id);
            }
        }
        catch (error) {
            console.log(error);
            showErrorNotification(error.message || "Error: while changing the status of Goal with Id: " + id);
        }
    })

    // Move to runnings
    $(document).on("click", ".moveToRunning", async function () {
        const id = $(this).data("id");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        try {

            const res = await $.ajax({
                url: "/Goals/ChangeGoalStatus",
                method: "POST",
                data: {
                    Id: id,
                    GoalStatus: 1 // 0 is for NotStarted, 1 is for Running, 2 is for Completed, 3 is for End
                }
            })

            if (res.status) {

                if (goalRunningDataTableReload !== null) {
                    goalRunningDataTableReload.draw();
                }

                if (goalCompletedDataTableReload !== null) {
                    goalCompletedDataTableReload.draw();
                }

                if (goalNotStartedDataTableReload !== null) {
                    goalNotStartedDataTableReload.draw();
                }

                showSuccessNotification(res.message);
            } else {
                showErrorNotification(res.message || "Error: while changing the status of Goal with Id: " + id);
            }
        }
        catch (error) {
            console.log(error);
            showErrorNotification(error.message || "Error: while changing the status of Goal with Id: " + id);
        }
    })

    // Function to open modal
    function openModal() {
        modal.removeClass("hidden");
        modal.addClass("flex");

        // Change submit button text based on the mode
        if (isEditMode) {
            submitBtn.text("Edit Goal");
        } else {
            submitBtn.text("Save Goal");
            $("#goalIdContainer").addClass("hidden");
            $("#goalStatusContainer").addClass("hidden");
        }

        // Reset the form
        goalForm[0].reset();
    }

    // Close modal and reset form
    function closeModal() {
        // Hide the validations error...
        goalIdError.addClass("hidden");
        goalNameError.addClass("hidden");
        goalEndDateError.addClass("hidden");


        modal.addClass("hidden");
        goalForm[0].reset();
    }

    // Function to load goal data into the form for editing
    function loadDataIntoForm(goal) {
        // Load data into the form fields for editing

        if (isEditMode) {
            $("#goalIdContainer").removeClass("hidden");
            goalIdInput.val(goal.id);
            goalNameInput.val(goal.name);
            goalEndDateInput.val(goal.endDate);
            goalStatusInput.val(goal.goalStatus);
            goalPriorityInput.val(goal.priority);
            goalDescriptionInput.val(goal.description);
            $("#goalStatusContainer").removeClass("hidden");
        }
    }

    // Load goal data into DataTable

    // Running Goal Data...
    function loadRunningGoalData() {
        goalRunningDataTableReload = runningGoalDataTable.DataTable({
            ajax: {
                url: "/Goals/GetAllRunningGoals",
                type: "POST",
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data: "id", name: "Id" },
                {
                    data: "name",
                    name: "Name",
                    render: function (data, type, row) {
                        return `<a href="/Goals/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
                    }
                },
                {
                    data: "endDate",
                    name: "Due Date",
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: "priority",
                    name: "priority",
                    render: function (data, type, row) {
                        return returnPriorityBadge(data);
                    }
                },
                {
                    data: "goalStatus",
                    name: "Status",
                    render: function (data, type, row) {
                        return returnStatusBadge(data);
                    }
                },
                {
                    data: null, // This column will not contain data directly
                    name: "Action",
                    defaultContent: "",
                    render: function (data, type, row) {
                        return `
                            <button data-id="${row.id}" class="editGoalBtn my-1 me-1 rounded bg-yellow-500 px-3 py-1 text-sm text-white hover:bg-yellow-600">
                                <i class="fas fa-edit"></i> Edit
                            </button>
                            <button data-id="${row.id}" class="markAsComplete my-1 me-1 rounded bg-green-500 px-3 py-1 text-sm text-white hover:bg-green-600">
                                 <i class="fas fa-check"></i> Mark as Complete
                            </button>
                            <button data-id="${row.id}" class="deleteGoalBtn my-1 rounded bg-red-500 px-3 py-1 text-sm text-white hover:bg-red-600">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        `;
                    }
                }
            ],
            "order": [[0, 'asc']],
            info: true,
            pageLength: 5
        });
    }

    // Completed Goal data....
    function loadCompletedGoalData() {
        goalCompletedDataTableReload = completedGoalDataTable.DataTable({
            ajax: {
                url: "/Goals/GetAllCompletedGoals",
                type: "POST",
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data: "id", name: "Id" },
                {
                    data: "name",
                    name: "Name",
                    render: function (data, type, row) {
                        return `<a href="/Goals/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
                    }
                },
                {
                    data: "endDate",
                    name: "Due Date",
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: "priority",
                    name: "priority",
                    render: function (data, type, row) {
                        return returnPriorityBadge(data);
                    }
                },
                {
                    data: "goalStatus",
                    name: "Status",
                    render: function (data, type, row) {
                        return returnStatusBadge(data);
                    }
                },
                {
                    data: null, // This column will not contain data directly
                    name: "Action",
                    defaultContent: "",
                    render: function (data, type, row) {
                        return `
                            <button data-id="${row.id}" class="editGoalBtn my-1 me-1 rounded bg-yellow-500 px-3 py-1 text-sm text-white hover:bg-yellow-600">
                                <i class="fas fa-edit"></i> Edit
                            </button>
                            <button data-id="${row.id}" class="moveToRunning my-1 me-1 rounded bg-blue-500 px-3 py-1 text-sm text-white hover:bg-blue-600">
                                 <i class="fas fa-check"></i> Move to Running
                            </button>
                            <button data-id="${row.id}" class="deleteGoalBtn my-1 rounded bg-red-500 px-3 py-1 text-sm text-white hover:bg-red-600">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        `;
                    }
                }
            ],
            "order": [[0, 'asc']],
            info: true,
            pageLength: 5
        });
    }

    // Completed Goal data....
    function loadNotStartedGoalData() {
        goalNotStartedDataTableReload = notStartedGoalDataTable.DataTable({
            ajax: {
                url: "/Goals/GetAllNotStartedGoals",
                type: "POST",
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data: "id", name: "Id" },
                {
                    data: "name",
                    name: "Name",
                    render: function (data, type, row) {
                        return `<a href="/Goals/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
                    }
                },
                {
                    data: "endDate",
                    name: "Due Date",
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: "priority",
                    name: "priority",
                    render: function (data, type, row) {
                        return returnPriorityBadge(data);
                    }
                },
                {
                    data: "goalStatus",
                    name: "Status",
                    render: function (data, type, row) {
                        return returnStatusBadge(data);
                    }
                },
                {
                    data: null, // This column will not contain data directly
                    name: "Action",
                    defaultContent: "",
                    render: function (data, type, row) {
                        return `
                            <button data-id="${row.id}" class="editGoalBtn my-1 me-1 rounded bg-yellow-500 px-3 py-1 text-sm text-white hover:bg-yellow-600">
                                <i class="fas fa-edit"></i> Edit
                            </button>
                            <button data-id="${row.id}" class="moveToRunning my-1 me-1 rounded bg-blue-500 px-3 py-1 text-sm text-white hover:bg-blue-600">
                                 <i class="fas fa-check"></i> Move to Running
                            </button>
                            <button data-id="${row.id}" class="deleteGoalBtn my-1 rounded bg-red-500 px-3 py-1 text-sm text-white hover:bg-red-600">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        `;
                    }
                }
            ],
            "order": [[0, 'asc']],
            info: true,
            pageLength: 5
        });
    }

    // Completed Goal data....
    function loadEndedGoalData() {
        goalEndedDataTableReload = endedGoalDataTable.DataTable({
            ajax: {
                url: "/Goals/GetAllEndedGoals",
                type: "POST",
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data: "id", name: "Id" },
                {
                    data: "name",
                    name: "Name",
                    render: function (data, type, row) {
                        return `<a href="/Goals/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
                    }
                },
                {
                    data: "endDate",
                    name: "Due Date",
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: "priority",
                    name: "Priority",
                    render: function (data, type, row) {
                        return returnPriorityBadge(data);
                    }
                },
                {
                    data: "goalStatus",
                    name: "Status",
                    render: function (data, type, row) {
                        return returnStatusBadge(data);
                    }
                },
                {
                    data: null, // This column will not contain data directly
                    name: "Action",
                    defaultContent: "",
                    render: function (data, type, row) {
                        return `
                            <button data-id="${row.id}" class="editGoalBtn my-1 me-1 rounded bg-yellow-500 px-3 py-1 text-sm text-white hover:bg-yellow-600">
                                <i class="fas fa-edit"></i> Edit
                            </button>
                            <button data-id="${row.id}" class="moveToRunning my-1 me-1 rounded bg-blue-500 px-3 py-1 text-sm text-white hover:bg-blue-600">
                                 <i class="fas fa-check"></i> Move to Running
                            </button>
                            <button data-id="${row.id}" class="deleteGoalBtn my-1 rounded bg-red-500 px-3 py-1 text-sm text-white hover:bg-red-600">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        `;
                    }
                }
            ],
            "order": [[0, 'asc']],
            info: true,
            pageLength: 5
        });
    }

    function loadDeletedGoalData() {
        goalDeletedDataTableReload = deletedGoalDataTable.DataTable({
            ajax: {
                url: "/Goals/GetAllDeletedGoals",
                type: "POST",
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data: "id", name: "Id" },
                {
                    data: "name",
                    name: "Name",
                    render: function (data, type, row) {
                        return `<a href="/Goals/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
                    }
                },
                {
                    data: "endDate",
                    name: "Due Date",
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: "priority",
                    name: "priority",
                    render: function (data, type, row) {
                        return returnPriorityBadge(data);
                    }
                },
                {
                    data: "goalStatus",
                    name: "Status",
                    render: function (data, type, row) {
                        return returnStatusBadge(data);
                    }
                },
                {
                    data: null, // This column will not contain data directly
                    name: "Action",
                    defaultContent: "",
                    render: function (data, type, row) {
                        return `
                            <button data-id="${row.id}" class="moveToRunning my-1 me-1 rounded bg-blue-500 px-3 py-1 text-sm text-white hover:bg-blue-600">
                                 <i class="fas fa-check"></i> Move to Running
                            </button>
                            <button data-id="${row.id}" class="deleteGoalBtn my-1 rounded bg-red-500 px-3 py-1 text-sm text-white hover:bg-red-600">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        `;
                    }
                }
            ],
            "order": [[0, 'asc']],
            info: true,
            pageLength: 5
        });
    }

    // client-side-validations...
    function validateData(formAction) { // formAction: 'add' || 'edit' 
        // Common....
        // Validate goal name
        if (goalNameInput.val().length < 3 || goalNameInput.val().length > 50) {
            goalNameError.removeClass("hidden");
            throw new Error("Invalid Goal Name input field!...");
        }
        goalNameError.addClass("hidden");

        // Validate Goal End Error...
        const endDate = new Date(goalEndDateInput.val());
        if (isNaN(endDate) || endDate <= Date.now()) {
            goalEndDateError.removeClass("hidden");
            throw new Error("Invalid Goal DueDate input field!...");
        }
        goalEndDateError.addClass("hidden");

        if (goalDescriptionInput.val().trim() === "") {
            goalDescriptionError.removeClass("hidden");
            throw new Error("Invalid Goal Descriptions input field!...");
        }
        goalDescriptionError.addClass("hidden");

        if (!goalPriorityInput.val()) {
            goalPriorityError.removeClass("hidden");
            throw new Error("Invalid Goal Priority input field!...");
        }
        goalPriorityError.addClass("hidden");

        if (formAction === 'edit') {
            // validations for the IdGoalField...
            if (goalIdInput.val().length === 0) {
                goalIdError.removeClass("hidden");
                throw new Error("Editing Model, Missing Id...");
            }
            goalIdError.addClass("hidden");
        }
    }

    loadRunningGoalData();
    loadCompletedGoalData();
    loadNotStartedGoalData();
    loadEndedGoalData();
    //loadDeletedGoalData();


    //async function calculateProductivity() {
    //    const res = await $.ajax({
    //        url: "/Goals/GetProductivity",
    //        method: 'GET',
    //    });

    //    if (res.status) {
    //        $("#productivityPercentage").html(`${res.data.productivity}%`);
    //        $("#runningGoalCount").html(res.data.runningGoal);
    //        $("#completedGoalCount").html(res.data.completedGoal);
    //        $("#growthCalculations").html(`${Number(res.data.growthPercentage).toFixed(2) > 0 ? '+': '-'}${Number(res.data.growthPercentage).toFixed(2)} this month`);
    //    }
    //}

    //calculateProductivity();

});
