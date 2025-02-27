$(document).ready(function () {

    const goalStatusEnum = Object.freeze({
        notStarted: 0,
        running: 1,
        completed: 2,
        ended: 3
    });

    const startOptionValues = Object.freeze({
        manual: 0,
        scheduled: 1,
        immediate: 2
    });

    // Running task...
    const runningGoalDataTable = $('#viewRunningGoalTableData');
    // Compleated Task...
    const completedGoalDataTable = $('#viewCompletedGoalTableData');
    // Not Started Task...
    const notStartedGoalDataTable = $('#viewNotStartedGoalTableData');
    // Not Started Task...
    const endedGoalDataTable = $('#viewEndedGoalTableData');
    // Not Started Task...
    const deletedGoalDataTable = $('#viewDeletedTableData');

    // JavaScript to toggle goalModal visibility
    const openModalButton = $("#openModalButton");

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
        let parentTable = $(this).closest("table");

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
                    parentTable?.DataTable().ajax.reload();
                },
                function (error) { // Error callback
                    console.error('Error deleting goal:', error);
                }
            );
        }
        catch (error) {
            showErrorNotification(error.message || "Error: while deleting Goal with Id: " + id);
            console.error(error);
        }
    })

    // Mark goal as completed...
    $(document).on("click", ".markAsComplete", async function () {
        const id = $(this).data("id");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        $.ajax({
            url: "/Goals/ChangeGoalStatus",
            method: "POST",
            data: {
                Id: id,
                GoalStatus: goalStatus.completed 
            },
            success: function (response) {
                if (response.status) {

                    runningGoalDataTable?.DataTable().ajax.reload();

                    completedGoalDataTable?.DataTable().ajax.reload();

                    showSuccessNotification(response.message);
                } else {
                    throw new Error(response.message || "Error: while changing the status of Goal with Id: " + id);
                }
            },
            error: function (xhr, status, error) {
                showErrorNotification(error || "Error: while changing the status of Goal with Id: " + id);
                console.error("Error:", error);
            }
        })
    })

    // Move to runnings
    $(document).on("click", ".moveToRunning", async function () {
        const id = $(this).data("id");
        let goalStatus = $(this).closest("table").data("goal-status");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        $.ajax({
            url: "/Goals/ChangeGoalStatus",
            method: "POST",
            data: {
                Id: id,
                GoalStatus: goalStatusEnum.running 
            },
            success: function (response) {
                if (response.status) {

                    runningGoalDataTable.DataTable().ajax.reload(); 

                    goalStatus === "notStarted" && notStartedGoalDataTable?.DataTable().ajax.reload();

                    goalStatus === "completed" && completedGoalDataTable?.DataTable().ajax.reload();

                    goalStatus === "ended" && endedGoalDataTable?.DataTable().ajax.reload();


                    showSuccessNotification(response.message);
                } else {
                    showErrorNotification(response.message || "Error: while changing the status of Goal with Id: " + id);
                }
            },
            error: function (xhr, status, error) {
                showErrorNotification(error || "Error: while changing the status of Goal with Id: " + id);
                console.error("Error:", error);
            }
        })
    })

    const dataTableObject = (url, renderCallBack) => {
        return {
            ajax: {
                url,
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
                    data: null, // This column will not contain data directly
                    name: "Action",
                    defaultContent: "",
                    render: renderCallBack
                }
            ],
            "order": [[0, 'asc']],
            info: true,
            pageLength: 5
        }
    }

    // Running Goal Data...
    function loadRunningGoalData() {
        let url = "/Goals/GetAllRunningGoals";

        function renderCallBack(data, type, row) {
            return `
                <a href="/Tasks/Create?goalId=${row.id}" class="my-1 rounded me-1 bg-purple-500 px-3 py-1 text-sm text-white hover:bg-purple-600">
                    <i class="fas fa-plus"></i> Add Task
                </a>
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

         runningGoalDataTable?.length && runningGoalDataTable.DataTable(dataTableObject(url, renderCallBack));
    }

    // Completed Goal data....
    function loadCompletedGoalData() {
        let url = "/Goals/GetAllCompletedGoals";

        function renderCallBack(data, type, row) {
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


        completedGoalDataTable?.length && completedGoalDataTable.DataTable(dataTableObject(url, renderCallBack));
    }

    // Completed Goal data....
    function loadNotStartedGoalData() {

        let url = "/Goals/GetAllNotStartedGoals";

        function renderCallBack(data, type, row) {
            return `
                <a href="/Tasks/Create?goalId=${row.id}" class="my-1 rounded me-1 bg-purple-500 px-3 py-1 text-sm text-white hover:bg-purple-600">
                    <i class="fas fa-plus"></i> Add Task
                </a>
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

        notStartedGoalDataTable.length && notStartedGoalDataTable.DataTable(dataTableObject(url, renderCallBack));
    }

    // Completed Goal data....
    function loadEndedGoalData() {
        let url = "/Goals/GetAllEndedGoals";

        function renderCallBack(data, type, row) {
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

        endedGoalDataTable.length && endedGoalDataTable.DataTable(dataTableObject(url, renderCallBack));
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

    runningGoalDataTable?.length && loadRunningGoalData();
    completedGoalDataTable?.length && loadCompletedGoalData();
    notStartedGoalDataTable?.length && loadNotStartedGoalData();
    endedGoalDataTable?.length && loadEndedGoalData();
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


    function GoalModelComponent({ getData, setData }) {
        let isEditMode = false;
        let isComponentInit = false;
        let parrentElementModel;

        const [isLoading, setIsLoading] = useDataState(false);

        let oldGoalStatus = -1;

        this.init = function () {
            if (!isComponentInit) {
                setData({
                    id: "",
                    goalStatus: goalStatusEnum.notStarted,
                    name: "",
                    endDate: "",
                    priority: 0,
                    description: "",
                    startOptionType: startOptionValues.manual,
                    startDate: ""
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

                    $.ajax({
                        url: `/Goals/GetById/${id}`,
                        method: "GET",
                        success: function (response) {
                            if (response.status) {
                                setData(response.data);
                                oldGoalStatus = response.data.goalStatus; //Storeing the Old status and check for if the status changes or not then need to re-draw the table
                                return resolve(response.data);
                            }
                            else {
                                showErrorNotification(response.message || "Failed to load goal data!")
                                return reject(new Error(response.message || "Failed to load goal data!"));
                            }
                        },
                        error: function (xhr, status, error) {
                            showErrorNotification(error || "Failed to load goal data!")
                            return reject(new Error(error || "Failed to load goal data!"));
                        }
                    });

                });
            } else {
                return getData();
            }
        };

        const reRender = function (parrentElement) {
            parrentElementModel = parrentElement;
            const contentWithData = `
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

                <!-- Goal Start Options -->
                <div id="startOptionsModeContainer" class="mb-4">
                    <label class="block text-sm font-medium text-gray-600">Goal Start</label>
                    <div class="mt-2 flex flex-col space-y-2 md:flex-row md:space-y-0 md:space-x-4">
                        <label class="flex items-center space-x-2">
                            <input type="radio" name="startOptionType" value="0" onchange="onChangeGoalInput(this)"  class="goalStartRadio text-indigo-600 focus:ring-indigo-500" ${+getData()?.startOptionType === startOptionValues.manual ? "checked" : ""}>
                            <span>Manual</span>
                        </label>
                        <label class="flex items-center space-x-2">
                            <input type="radio" name="startOptionType" value="1" onchange="onChangeGoalInput(this)" class="goalStartRadio text-indigo-600 focus:ring-indigo-500" ${+getData()?.startOptionType === startOptionValues.scheduled ? "checked" : ""}>
                            <span>Scheduled</span>
                        </label>
                        <label class="flex items-center space-x-2">
                            <input type="radio" name="startOptionType" value="2" onchange="onChangeGoalInput(this)" class="goalStartRadio text-indigo-600 focus:ring-indigo-500" ${+getData()?.startOptionType === startOptionValues.immediate ? "checked" : ""}>
                            <span>Start Immediately</span>
                        </label>
                    </div>
                </div>

                <!-- Scheduled Start Date Field -->
                <div id="startDateContainer" class="mb-4 ${+getData()?.startOptionType === startOptionValues.scheduled ? '' : 'hidden'}">
                    <label for="startDate" class="block text-sm font-medium text-gray-600">Start Date</label>
                    <input type="datetime-local" id="startDate" name="startDate" onchange="onChangeGoalInput(this)" value="${getData()?.startDate}" class="mt-1 w-full rounded-md border border-gray-300 px-4 py-2">
                    <span class="hidden text-xs text-red-500" id="startDateError">Start date is required when scheduling.</span>
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
                        <option value="0" ${getData()?.goalStatus == 0 ? "selected" : ""}>Not Started</option>
                        <option value="1" ${getData()?.goalStatus == 1 ? "selected" : ""}>Running</option>
                        <option value="2" ${getData()?.goalStatus == 2 ? "selected" : ""}>Completed</option>
                        <option value="3" ${getData()?.goalStatus == 3 ? "selected" : ""}>End</option>
                    </select>
                    <span id="goalStatusError" class="hidden text-sm text-red-500">GoalStatus field is required.</span>
                </div>


                <!-- Submit Button -->
                <div class="flex justify-end">
                    <button id="goal-submit-btn" type="submit" ${isLoading() ? 'disabled' : ''} class="flex items-center justify-center gap-2 rounded-md bg-blue-500 px-4 py-2 text-white hover:bg-blue-600 disabled:bg-blue-300 disabled:cursor-not-allowed">
                        <i class="icon fa-solid ${isEditMode ? 'fa-edit' : 'fa-save'}"></i>
                        <span class="button-text">${isEditMode ? 'Edit' : 'Save'} Goal</span>
                    </button>
                </div>
            </form>
        `;

            const content = getData() ? contentWithData : noContentHtml;
            parrentElementModel.html(`
            <div class="custom-scrollbar relative w-full sm:w-4/5 lg:w-1/2 xl:w-1/3 h-full sm:h-auto sm:max-h-[90vh] overflow-auto rounded-lg bg-white shadow-xl">
                <div class="flex items-center justify-between bg-indigo-600 px-6 py-4 text-white">
                    <h2 id="model-title" class="text-xl font-semibold tracking-wide">
                        <i class="fa-solid fa-clipboard-list"></i> ${isEditMode ? 'Update' : 'Create'} Your Progress Record in Goal
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
                setData({ ...getData(), [e.name]: e.value, ["isModified"]: true }, () => reRender(parrentElementModel));
            }

            $("#goalForm").on('submit', function (e) {
                e.preventDefault();
                setIsLoading(true, () => reRender(parrentElementModel));
                $.ajax({
                    url: isEditMode ? "/Goals/Edit" : "/Goals/Create",
                    method: isEditMode ? "PUT" : "POST",
                    data: getData(),
                    success: function (response) {
                        if (response.status) {
                            const { goalStatus } = getData();

                            (goalStatusEnum.notStarted === +goalStatus || goalStatusEnum.notStarted === +oldGoalStatus) && notStartedGoalDataTable?.DataTable().ajax.reload();

                            (goalStatusEnum.running === +goalStatus || goalStatusEnum.running === +oldGoalStatus) && runningGoalDataTable?.DataTable().ajax.reload();

                            (goalStatusEnum.completed === +goalStatus || goalStatusEnum.completed === +oldGoalStatus) && completedGoalDataTable?.DataTable().ajax.reload();

                            (goalStatusEnum.ended === +goalStatus || goalStatusEnum.ended === +oldGoalStatus) && endedGoalDataTable?.DataTable().ajax.reload();


                            setTimeout(() => {
                                modal.closeModal();
                            }, 500)
                            showSuccessNotification(response.message || "Notes Saved with Goal!");

                        } else {
                            setIsLoading(false, () => reRender(parrentElementModel));
                            showErrorNotification(response.message || "Notes Saved Failed!");
                            console.error(response.message);
                        }
                    },
                    error: function (xhr, status, error) {
                        setIsLoading(false, () => reRender(parrentElementModel));
                        showErrorNotification(error || "Notes Saved Failed!");
                        console.error("Error:", error);
                    }
                });
            })
        }

        this.load = function (selector) {
            reRender(selector);
        }
    }

});



