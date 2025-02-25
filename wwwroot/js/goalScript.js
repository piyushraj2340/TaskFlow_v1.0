$(document).ready(function () {
    // JavaScript to toggle goalModal visibility
    const openModalButton = $("#openModalButton");
 
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

    // Not Started Task...
    const deletedGoalDataTable = $('#viewDeletedTableData');

    // Open goalModal for create
    openModalButton.on("click", function () {
        modal.openModal(GoalModelComponent)
    });

    // Open goalModal for edit
    $(document).on("click", ".editGoalBtn", async function () {
        modal.openModal(GoalModelComponent, $(this).data("id"));
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
                    
                },
                function (error) { // Error callback
                    console.error('Error deleting goal:', error);
                }
            );
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



function GoalModelComponent({ getData, setData}) {
    let isEditMode = false;
    let isComponentInit = false;

    this.init = function () {
        if (!isComponentInit) {
            setData({
                id: "",
                goalStatus: 0,
                name: "",
                endDate: "",
                priority: 0,
                description: ""
            });

            isEditMode = false;
            isComponentInit = true;
        }

        return getData();
    }

    this.initAsync = function (id) {
        if (!isComponentInit) {
            return new Promise((resolve, reject) => {
                isEditMode = true;
                isComponentInit = true;
                try {
                    $.ajax({
                        url: `/Goals/GetById/${id}`,
                        method: "GET",
                        success: function (response) {
                            if (response.status) {
                                setData(response.data);
                                console.log(getData());
                                return resolve(response.data);
                            }
                            else {
                                setErrorData(response?.data)
                                throw new Error(response.message);
                            }
                        },
                        error: function (xhr, status, error) {
                            setErrorData(error);
                            throw error;
                        }
                    });
                }
                catch (error) {
                    console.error("Error:", error);
                    return reject(error);
                }
            });
        } else {
            return getData();
        }
    };

    const reRender = function (parrentElement) {
        const contentWithData = `
            
            <h2 id="form-label" class="mb-3 text-2xl font-semibold text-gray-700"></h2>

            <div id="goalMessageDisplay" class="hidden">
                <span class="text-xs text-red-500"></span>
            </div>

            <!-- Modal Form -->
            <form id="goalForm">
                <!-- Goal Id Field -->
                <div id="goalIdContainer" class="mb-4 ${isEditMode ? '' : 'hidden'}">
                    <label for="goalId" class="block text-sm font-medium text-gray-600">Goal Id</label>
                    <input type="text" id="goalId" name="Id" class="mt-1 w-full rounded-md border border-gray-300 px-4 py-2" value="${getData()?.id}" onchange="onChangeGoalInput(this)" placeholder="Enter your goal id" disabled>
                    <span class="hidden text-xs text-red-500" id="goalIdError">
                        Goal Id is required!
                    </span>
                </div>

                <!-- Goal Name Field -->
                <div class="mb-4">
                    <label for="goalName" class="block text-sm font-medium text-gray-600">Goal Name</label>
                    <input type="text" id="goalName" name="name" class="mt-1 w-full rounded-md border border-gray-300 px-4 py-2" value="${getData()?.name}" onchange="onChangeGoalInput(this)" required minlength="3" maxlength="50" placeholder="Enter your goal name" />
                    <span class="hidden text-xs text-red-500" id="goalNameError">Goal name must be between 3 and 50 characters.</span>
                </div>

                <!-- Task Description Input -->
                <div class="mb-4">
                    <label for="goalDescription" class="block text-sm font-medium text-gray-600">Goal Descriptions</label>
                    <textarea id="goalDescription" name="description" rows="4" class="mt-2 w-full rounded-lg border border-gray-300 px-4 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500" onchange="onChangeGoalInput(this)" required>${getData()?.description}</textarea>
                    <span id="goalDescriptionError" class="hidden text-sm text-red-500">Description field is required.</span>
                </div>


                <!-- Priority Dropdown -->
                <div class="mb-4">
                    <label for="goalPriority" class="block text-sm font-medium text-gray-600">Goal Priority</label>
                    <select id="goalPriority" name="priority" class="mt-2 w-full rounded-lg border border-gray-300 px-4 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500" value="${getData()?.priority}" onchange="onChangeGoalInput(this)">
                        <option value="0">Low</option>
                        <option value="1">Medium</option>
                        <option value="2">High</option>
                    </select>
                    <span id="goalPriorityError" class="hidden text-sm text-red-500">Priority field is required.</span>
                </div>


                <!-- End Date Field -->
                <div class="mb-4">
                    <label for="endDate" class="block text-sm font-medium text-gray-600">End Date</label>
                    <input type="datetime-local" id="endDate" name="endDate" class="mt-1 w-full rounded-md border border-gray-300 px-4 py-2" value="${getData()?.endDate}" onchange="onChangeGoalInput(this)" placeholder="Select Due Date." required />
                    <span class="hidden text-xs text-red-500" id="goalDueDateError">The selected date and time must be in the future.</span>
                </div>

                <!-- Goal Status Field -->
                <div id="goalStatusContainer" class="mb-4 ${isEditMode ? "" : 'hidden'}">
                    <label for="goalStatus" class="block text-sm font-medium text-gray-600">Goal Status</label>
                    <select id="goalStatus" name="goalStatus" class="mt-1 w-full rounded-md border border-gray-300 px-4 py-2" value="${getData()?.goalStatus}" onchange="onChangeGoalInput(this)">
                        <option value="0">Not Started</option>
                        <option value="1">Running</option>
                        <option value="2">Completed</option>
                        <option value="3">End</option>
                    </select>
                    <span id="goalStatusError" class="hidden text-sm text-red-500">GoalStatus field is required.</span>
                </div>


                <!-- Submit Button -->
                <div class="flex justify-end">
                    <button id="submit-btn" type="submit" class="rounded-md bg-blue-500 px-4 py-2 text-white hover:bg-blue-600">${isEditMode? 'Edit': 'Save'} Goal</button>
                </div>
            </form>
            
        `;

        const content = getData() ? contentWithData : noContentHtml;
        parrentElement.html(`
            <div class="custom-scrollbar relative h-full sm:max-h-[90vh] w-full max-w-xl overflow-auto rounded-lg bg-white shadow-xl sm:w-4/5 lg:w-1/2 xl:w-1/3">
                <div class="flex items-center justify-between bg-indigo-600 px-6 py-4 text-white">
                    <h2 id="model-title" class="text-xl font-semibold tracking-wide">
                        <i class="fa-solid fa-clipboard-list"></i> ${isEditMode? 'Update': 'Create'} Your Progress Record in Goal
                    </h2>
                    <button class="text-2xl text-white hover:text-gray-200" id="closeModal">
                        <i class="fa-solid fa-xmark"></i>
                    </button>
                </div>
                <div class="p-6">
                    ${content}
                </div>
            </div>
        `);

        window.onChangeGoalInput = function (e) {
            setData({ ...getData(), [e.name]: e.value, ["isModified"]: true  });
            console.log(getData());
        }

        $("#goalForm").on('submit', function (e) {
            e.preventDefault();
            try {
                $.ajax({
                    url: isEditMode ? "/Goals/Edit" : "/Goals/Create",
                    method: isEditMode ? "PUT" : "POST", 
                    data: getData(),
                    success: function (response) {
                        if (response.status) {
                            setTimeout(() => {
                                modal.closeModal();
                            }, 500)
                            showSuccessNotification(response.message || "Notes Saved with Goal!");

                            console.log(response.message || "Notes Saved with Goal!")

                        } else {
                            throw new Error(response.message);
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error("error", error)
                        throw error;
                    }
                });
            } catch (error) {
                showErrorNotification(error.message || "Notes Saved Failed!");
                console.error("Error:", error);
            }
        })

    }

    this.load = function (selector) {
        reRender(selector);
    }
}