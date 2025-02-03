$(document).ready(function () {

    const todoRunningDataTable = $("#viewRunningTodoTableData");
    let runningdataTableReload = null;

    const todoCompletedDataTable = $("#viewCompletedTodoTableData");
    let completedDataTableReload = null;

    const runningTodoTaskCount = $("#runningTodoTaskCount");
    const completedTodoTaskCount = $("#completedTodoTaskCount");
    const totalTodoTaskCount = $("#totalTodoTaskCount");
    const productivityTotoTaskPercentage = $("#productivityPercentageTodo");


    $(document).on("click", ".markAsCompleteToDoBtn", async function () {
        const id = $(this).data("id");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        try {

            const res = await $.ajax({
                url: "/Todo/ChangeTodoStatus",
                method: "POST",
                data: {
                    Id: id,
                    Status: 2 // 0 is for NotStarted, 1 is for Running, 2 is for Completed, 3 is for End
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
                handelUpdateTodoProductivity();
            } else {
                showErrorNotification(res.message || "Error: while changing the status of Goal with Id: " + id);
            }
        }
        catch (error) {
            console.log(error);
            showErrorNotification(error.message || "Error: while changing the status of Goal with Id: " + id);
        }
    })

    $(document).on("click", ".moveToRunningToDoBtn", async function () {
        const id = $(this).data("id");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        try {

            const res = await $.ajax({
                url: "/Todo/ChangeTodoStatus",
                method: "POST",
                data: {
                    Id: id,
                    Status: 1 // 0 is for NotStarted, 1 is for Running, 2 is for Completed, 3 is for End
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
                handelUpdateTodoProductivity();
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
        runningdataTableReload = todoRunningDataTable.DataTable({
            ajax: {
                url: "/Todo/GetRunningTodo",
                type: "POST"
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data: "id", name: "id" },
                {
                    data: "task.name",
                    name: "name",
                    render: function (data, type, row) {
                        return `<a href="/Tasks/Details/${row?.task?.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
                    }
                },
                {
                    data: "task.priority",
                    name: "priority",
                    render: function (data, type, row) {
                        switch (data) {
                            case 0:
                                return '<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-red-500 rounded-full">High</span>';
                            case 1:
                                return '<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-green-500 rounded-full">Medium</span>';
                            case 2:
                                return '<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-gray-500 rounded-full">Low</span>';
                            default:
                                return '<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-yellow-500 rounded-full">Unknown Type</span>';
                        }

                    }
                },
                {
                    data: "task.repeat",
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
                    data: "status",
                    name: "Status",
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
                            <button data-id="${row.id}" class="markAsCompleteToDoBtn my-1 me-1 rounded bg-green-500 px-3 py-1 text-sm text-white hover:bg-green-600">
                                 <i class="fas fa-check"></i> Mark as Complete
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
        completedDataTableReload = todoCompletedDataTable.DataTable({
            ajax: {
                url: "/Todo/GetCompletedTodo",
                type: "POST"
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data: "id", name: "id" },
                {
                    data: "task.name",
                    name: "name",
                    render: function (data, type, row) {
                        return `<a href="/Tasks/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
                    }
                },
                {
                    data: "task.priority",
                    name: "priority",
                    render: function (data, type, row) {
                        switch (data) {
                            case 0:
                                return '<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-red-500 rounded-full">High</span>';
                            case 1:
                                return '<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-green-500 rounded-full">Medium</span>';
                            case 2:
                                return '<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-gray-500 rounded-full">Low</span>';
                            default:
                                return '<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-yellow-500 rounded-full">Unknown Type</span>';
                        }

                    }
                },
                {
                    data: "task.repeat",
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
                    data: "status",
                    name: "Status",
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
                            <button data-id="${row.id}" class="moveToRunningToDoBtn my-1 me-1 rounded bg-blue-500 px-3 py-1 text-sm text-white hover:bg-blue-600">
                                 <i class="fas fa-check"></i> Move to Running
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

    async function handelUpdateTodoProductivity() {
        try {
            const res = await $.ajax({
                url: "/Todo/GetTodoProgressForTodays",
                method: "POST",
                success: function (data) {
                    if (!data.status) {
                        throw new Error(data.message || "Unable to fetch the todays progress...");
                    }

                    const { completedTask, productivity, runningTask, totalTask } = data.data;

                    completedTodoTaskCount.html(completedTask);
                    runningTodoTaskCount.html(runningTask);
                    totalTodoTaskCount.html(totalTask);
                    productivityTotoTaskPercentage.html(productivity+ "%");

                },
                error: function (error) {
                    throw error;
                }
            })
        } catch (error) {
            console.error(error);
        }
    }
})