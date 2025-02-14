$(document).ready(function () {

    // running data table 
    const goalRunningDataTable = $("#viewRunningTaskTableData");
    let runningdataTableReload = null;


    // completed data table 
    const goalCompletedDataTable = $("#viewCompletedTaskTableData");
    let completedDataTableReload = null;

    // running data table 
    const goalNotStartedDataTable = $("#viewNotStartedTaskTableData");
    let notStartedDataTableReload = null;


    // completed data table 
    const goalEndedDataTable = $("#viewEndedTaskTableData");
    let endedDataTableReload = null;


    // completed data table 
    const goalDeletedDataTable = $("#viewDeletedTaskTableData");
    let deletedDataTableReload = null;

    $(document).on("click", ".deleteTaskBtn", async function () {
        const id = $(this).data("id");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        try {
            showConfirmationDialog(
                {
                    title: 'Are you sure?',
                    text: 'Do you really want to delete this goal? This action cannot be undone.',
                },
                {
                    url: `/Tasks/DeleteTask`,
                    type: 'DELETE',
                    data: { Id: id }
                },
                function (response) {

                    //if (runningdataTableReload !== null) {
                    //    runningdataTableReload.draw();
                    //}

                    //if (notStartedDataTableReload !== null) {
                    //    notStartedDataTableReload.draw();
                    //}

                    //if (completedDataTableReload !== null) {
                    //    completedDataTableReload.draw();
                    //}

                    //if (endedDataTableReload !== null) {
                    //    endedDataTableReload.draw();
                    //}

                    //if (deletedDataTableReload !== null) {
                    //    deletedDataTableReload.draw();
                    //}
                },
                function (error) { // Error callback
                    console.error('Error deleting task:', error);
                }
            );
        } catch (error) {
            showErrorNotification(error.message || "Error: while deleting the Task with Id: " + id)
            console.error(error);
        }
    })

    $(document).on("click", ".markAsCompleteTaskBtn", async function () {
        const id = $(this).data("id");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        try {

            const res = await $.ajax({
                url: "/Tasks/ChangeTaskStatus",
                method: "POST",
                data: {
                    Id: id,
                    TaskStatus: 2 // 0 is for NotStarted, 1 is for Running, 2 is for Completed, 3 is for End
                }
            })

            if (res.status) {

                if (runningdataTableReload !== null) {
                    runningdataTableReload.draw();
                }

                if (completedDataTableReload !== null) {
                    completedDataTableReload.draw();
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

    $(document).on("click", ".moveToRunningTaskBtn", async function () {
        const id = $(this).data("id");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        try {

            const res = await $.ajax({
                url: "/Tasks/ChangeTaskStatus",
                method: "POST",
                data: {
                    Id: id,
                    TaskStatus: 1 // 0 is for NotStarted, 1 is for Running, 2 is for Completed, 3 is for End
                }
            })

            if (res.status) {

                if (runningdataTableReload !== null) {
                    runningdataTableReload.draw();
                }

                if (completedDataTableReload !== null) {
                    completedDataTableReload.draw();
                }

                if (notStartedDataTableReload !== null) {
                    notStartedDataTableReload.draw();
                }

                if (endedDataTableReload !== null) {
                    endedDataTableReload.draw();
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

    function loadRunningTaskData() {
        runningdataTableReload = goalRunningDataTable.DataTable({
            ajax: {
                url: "/Tasks/GetAllRunningTaskList",
                type: "POST"
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data: "id", name: "id" },
                {
                    data: "name",
                    name: "name",
                    render: function (data, type, row) {
                        return `<a href="/Tasks/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
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
                    data: "repeat",
                    name: "repeat",
                    render: function (data, type, row) {
                        switch (data) {
                            case 0:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-red-500 rounded-full shadow-md hover:bg-red-600 transition duration-300 min-w-max">RunOnce</span>';
                            case 1:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-green-500 rounded-full shadow-md hover:bg-green-600 transition duration-300 min-w-max">Daily</span>';
                            case 2:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-400 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">Weekly</span>';
                            default:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-yellow-400 rounded-full shadow-md hover:bg-yellow-500 transition duration-300 min-w-max">Unknown Type</span>';
                        }
                    }
                },
                {
                    data: "taskStatus",
                    name: "taskStatus",
                    render: function (data, type, row) {
                        return returnStatusBadge(data);
                    }

                },
                {
                    data: "endDate",
                    name: "endDate",
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: null,
                    name: "Action",
                    defaultContent: "",
                    render: function (data, type, row) {
                        return `
                            <a href="/Tasks/Edit/${row.id}" data-id="${row.id}" class="editTaskBtn my-1 me-1 rounded bg-yellow-500 px-3 py-1 text-sm text-white hover:bg-yellow-600">
                                <i class="fas fa-edit"></i> Edit
                            </a>
                            <button data-id="${row.id}" class="markAsCompleteTaskBtn my-1 me-1 rounded bg-green-500 px-3 py-1 text-sm text-white hover:bg-green-600">
                                 <i class="fas fa-check"></i> Mark as Complete
                            </button>
                            <button data-id="${row.id}" class="deleteTaskBtn my-1 rounded bg-red-500 px-3 py-1 text-sm text-white hover:bg-red-600">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        `;
                    }
                }
            ],
            order: [[0, 'asc']],
            info: true,
            pageLength: 5
        })
    }

    function loadCompletedTaskData() {
        completedDataTableReload = goalCompletedDataTable.DataTable({
            ajax: {
                url: "/Tasks/GetAllCompletedTaskList",
                type: "POST"
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data: "id", name: "id" },
                {
                    data: "name",
                    name: "name",
                    render: function (data, type, row) {
                        return `<a href="/Tasks/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
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
                    data: "repeat",
                    name: "repeat",
                    render: function (data, type, row) {
                        switch (data) {
                            case 0:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-red-500 rounded-full shadow-md hover:bg-red-600 transition duration-300 min-w-max">RunOnce</span>';
                            case 1:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-green-500 rounded-full shadow-md hover:bg-green-600 transition duration-300 min-w-max">Daily</span>';
                            case 2:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-400 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">Weekly</span>';
                            default:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-yellow-400 rounded-full shadow-md hover:bg-yellow-500 transition duration-300 min-w-max">Unknown Type</span>';
                        }
                    }
                },
                {
                    data: "taskStatus",
                    name: "taskStatus",
                    render: function (data, type, row) {
                        return returnStatusBadge(data);
                    }

                },
                {
                    data: "endDate",
                    name: "endDate",
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: null,
                    name: "Action",
                    defaultContent: "",
                    render: function (data, type, row) {
                        return `
                            <a href="/Tasks/Edit/${row.id}" data-id="${row.id}" class="editTaskBtn my-1 me-1 rounded bg-yellow-500 px-3 py-1 text-sm text-white hover:bg-yellow-600">
                                <i class="fas fa-edit"></i> Edit
                            </a>
                            <button data-id="${row.id}" class="moveToRunningTaskBtn my-1 me-1 rounded bg-blue-500 px-3 py-1 text-sm text-white hover:bg-blue-600">
                                 <i class="fas fa-check"></i> Move To Running
                            </button>
                            <button data-id="${row.id}" class="deleteTaskBtn my-1 rounded bg-red-500 px-3 py-1 text-sm text-white hover:bg-red-600">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        `;
                    }
                }
            ],
            order: [[0, 'asc']],
            info: true,
            pageLength: 5
        })
    }

    function loadNotStartedTaskData() {
        notStartedDataTableReload = goalNotStartedDataTable.DataTable({
            ajax: {
                url: "/Tasks/GetAllNotStartedTaskList",
                type: "POST"
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data: "id", name: "id" },
                {
                    data: "name",
                    name: "name",
                    render: function (data, type, row) {
                        return `<a href="/Tasks/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
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
                    data: "repeat",
                    name: "repeat",
                    render: function (data, type, row) {
                        switch (data) {
                            case 0:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-red-500 rounded-full shadow-md hover:bg-red-600 transition duration-300 min-w-max">RunOnce</span>';
                            case 1:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-green-500 rounded-full shadow-md hover:bg-green-600 transition duration-300 min-w-max">Daily</span>';
                            case 2:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-400 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">Weekly</span>';
                            default:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-yellow-400 rounded-full shadow-md hover:bg-yellow-500 transition duration-300 min-w-max">Unknown Type</span>';
                        }
                    }
                },
                {
                    data: "taskStatus",
                    name: "taskStatus",
                    render: function (data, type, row) {
                        return returnStatusBadge(data);
                    }

                },
                {
                    data: "endDate",
                    name: "endDate",
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: null,
                    name: "Action",
                    defaultContent: "",
                    render: function (data, type, row) {
                        return `
                            <a href="/Tasks/Edit/${row.id}" data-id="${row.id}" class="editTaskBtn my-1 me-1 rounded bg-yellow-500 px-3 py-1 text-sm text-white hover:bg-yellow-600">
                                <i class="fas fa-edit"></i> Edit
                            </a>
                            <button data-id="${row.id}" class="moveToRunningTaskBtn my-1 me-1 rounded bg-blue-500 px-3 py-1 text-sm text-white hover:bg-blue-600">
                                 <i class="fas fa-check"></i> Move To Running
                            </button>
                            <button data-id="${row.id}" class="deleteTaskBtn my-1 rounded bg-red-500 px-3 py-1 text-sm text-white hover:bg-red-600">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        `;
                    }
                }
            ],
            order: [[0, 'asc']],
            info: true,
            pageLength: 5
        })
    }

    function loadEndedTaskData() {
        endedDataTableReload = goalEndedDataTable.DataTable({
            ajax: {
                url: "/Tasks/GetAllEndedTaskList",
                type: "POST"
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data: "id", name: "id" },
                {
                    data: "name",
                    name: "name",
                    render: function (data, type, row) {
                        return `<a href="/Tasks/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
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
                    data: "repeat",
                    name: "repeat",
                    render: function (data, type, row) {
                        switch (data) {
                            case 0:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-red-500 rounded-full shadow-md hover:bg-red-600 transition duration-300 min-w-max">RunOnce</span>';
                            case 1:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-green-500 rounded-full shadow-md hover:bg-green-600 transition duration-300 min-w-max">Daily</span>';
                            case 2:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-400 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">Weekly</span>';
                            default:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-yellow-400 rounded-full shadow-md hover:bg-yellow-500 transition duration-300 min-w-max">Unknown Type</span>';
                        }
                    }
                },
                {
                    data: "taskStatus",
                    name: "taskStatus",
                    render: function (data, type, row) {
                        return returnStatusBadge(data);
                    }

                },
                {
                    data: "endDate",
                    name: "endDate",
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: null,
                    name: "Action",
                    defaultContent: "",
                    render: function (data, type, row) {
                        return `
                            <a href="/Tasks/Edit/${row.id}" data-id="${row.id}" class="editTaskBtn my-1 me-1 rounded bg-yellow-500 px-3 py-1 text-sm text-white hover:bg-yellow-600">
                                <i class="fas fa-edit"></i> Edit
                            </a>
                            <button data-id="${row.id}" class="moveToRunningTaskBtn my-1 me-1 rounded bg-blue-500 px-3 py-1 text-sm text-white hover:bg-blue-600">
                                 <i class="fas fa-check"></i> Move To Running
                            </button>
                            <button data-id="${row.id}" class="deleteTaskBtn my-1 rounded bg-red-500 px-3 py-1 text-sm text-white hover:bg-red-600">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        `;
                    }
                }
            ],
            order: [[0, 'asc']],
            info: true,
            pageLength: 5
        })
    }

    function loadDeletedTaskData() {
        deletedDataTableReload = goalDeletedDataTable.DataTable({
            ajax: {
                url: "/Tasks/GetAllDeletedTaskList",
                type: "POST"
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data: "id", name: "id" },
                {
                    data: "name",
                    name: "name",
                    render: function (data, type, row) {
                        return `<a href="/Tasks/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
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
                    data: "repeat",
                    name: "repeat",
                    render: function (data, type, row) {
                        switch (data) {
                            case 0:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-red-500 rounded-full shadow-md hover:bg-red-600 transition duration-300 min-w-max">RunOnce</span>';
                            case 1:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-green-500 rounded-full shadow-md hover:bg-green-600 transition duration-300 min-w-max">Daily</span>';
                            case 2:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-400 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">Weekly</span>';
                            default:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-yellow-400 rounded-full shadow-md hover:bg-yellow-500 transition duration-300 min-w-max">Unknown Type</span>';
                        }
                    }
                },
                {
                    data: "taskStatus",
                    name: "taskStatus",
                    render: function (data, type, row) {
                        return returnStatusBadge(data);
                    }

                },
                {
                    data: "endDate",
                    name: "endDate",
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: null,
                    name: "Action",
                    defaultContent: "",
                    render: function (data, type, row) {
                        return `
                            <button data-id="${row.id}" class="moveToRunningTaskBtn my-1 me-1 rounded bg-blue-500 px-3 py-1 text-sm text-white hover:bg-blue-600">
                                 <i class="fas fa-check"></i> Move To Running
                            </button>
                            <button data-id="${row.id}" class="deleteTaskBtn my-1 rounded bg-red-500 px-3 py-1 text-sm text-white hover:bg-red-600">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        `;
                    }
                }
            ],
            order: [[0, 'asc']],
            info: true,
            pageLength: 5
        })
    }

    loadRunningTaskData();
    loadCompletedTaskData();
    loadNotStartedTaskData();
    loadEndedTaskData();
    //loadDeletedTaskData();
})