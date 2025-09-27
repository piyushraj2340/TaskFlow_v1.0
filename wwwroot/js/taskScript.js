$(document).ready(function () {

    const taskStatusEnum = Object.freeze({
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

    const pageLengthValue = Object.freeze({
        runningTask:"RUNNING_TASK_PAGE_LENGTH",
        completedTask:"COMPLETED_TASK_PAGE_LENGTH",
        notStartedTask:"NOT_STARTED_TASK_PAGE_LENGTH",
        endedTask:"ENDED_TASK_PAGE_LENGTH",
    });

    const dayMap = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

    const defaultPageLength = 5;

    // running data table 
    const taskRunningDataTable = $("#viewRunningTaskTableData");

    // completed data table 
    const taskCompletedDataTable = $("#viewCompletedTaskTableData");

    // running data table 
    const taskNotStartedDataTable = $("#viewNotStartedTaskTableData");

    // completed data table 
    const taskEndedDataTable = $("#viewEndedTaskTableData");

    // completed data table 
    const taskDeletedDataTable = $("#viewDeletedTaskTableData");

    $(document).on("click",".deleteTaskBtn", async function () {
        const id = $(this).data("id");
        const parrentTable = $(this).closest("table");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        try {
            showConfirmationDialog(
                {
                    title: 'Are you sure?',
                    text: 'Do you really want to delete this task? This action cannot be undone.',
                },
                {
                    url: `/Tasks/DeleteTask`,
                    type: 'DELETE',
                    data: { Id: id }
                },
                function (response) {
                    parrentTable?.DataTable().ajax.reload();
                },
                function (error) { // Error callback
                    console.error('Error deleting task:', error);
                }
            );
        } catch (error) {
            showErrorNotification(error.message ||"Error: while deleting the Task with Id:" + id)
            console.error(error);
        }
    })

    $(document).on("click",".markAsCompleteTaskBtn", async function () {
        const id = $(this).data("id");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        $.ajax({
            url:"/Tasks/ChangeTaskStatus",
            method:"POST",
            data: {
                Id: id,
                TaskStatus: taskStatusEnum.completed
            },
            success: function (response) {
                if (response.status) {
                    showSuccessNotification(response.message);
                    setTimeout(() => location.reload(), 1000);
                }
                else {
                    showErrorNotification(response.message ||"Error: while changing the status of Goal with Id:" + id);
                    console.error(response.message ||"Error: while changing the status of Goal with Id:" + id)
                }
            },
            error: function (xhr, status, error) {
                showErrorNotification(error ||"Error: while changing the status of Goal with Id:" + id);
                console.error("Error:", error);
            }
        })


    })

    $(document).on("click",".moveToRunningTaskBtn", async function () {
        const id = $(this).data("id");
        let taskStatus = $(this).closest("table").data("task-status");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        $.ajax({
            url:"/Tasks/ChangeTaskStatus",
            method:"POST",
            data: {
                Id: id,
                TaskStatus: taskStatusEnum.running
            },
            success: function (response) {
                if (response.status) {
                    showSuccessNotification(response.message);
                    setTimeout(() => location.reload(), 1000);
                }
                else {
                    showErrorNotification(response.message ||"Error: while changing the status of Goal with Id:" + id);
                    console.error(response.message ||"Error: while changing the status of Goal with Id:" + id)
                }
            },
            error: function (xhr, status, error) {
                showErrorNotification(error ||"Error: while changing the status of Goal with Id:" + id);
                console.error("Error:", error);
            }
        })
    })

    const dataTableObject = (url, renderCallBack, pageLengthKey) => {
        const pageLength = localStorage.getItem(pageLengthKey);

        const goalId = getGoalIdFromGoalDetailPage();

        if (goalId) {
            url = url +"?goalId=" + goalId;
        }

        return {
            ajax: {
                url,
                type:"POST"
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data:"id", name:"Id" },
                {
                    data:"name",
                    name:"Name",
                    render: function (data, type, row) {
                        return `<a href="/Tasks/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
                    }
                },
                {
                    data:"priority",
                    name:"Priority",
                    render: function (data, type, row) {
                        return returnPriorityBadge(data);
                    }
                },
                {
                    data:"repeat",
                    name:"Repeat",
                    render: function (data, type, row) {
                        
                        switch (data) {
                            case 0:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-red-500 rounded-full shadow-md hover:bg-red-600 transition duration-300 min-w-max">RunOnce</span>';
                            case 1:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-green-500 rounded-full shadow-md hover:bg-green-600 transition duration-300 min-w-max">Daily</span>';
                            case 2:
                                const days = row.repeatWeekList.map(d => dayMap[d]).join(', ');
                                return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-400 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max" title="${days}">Weekly</span>`;
                            case 3:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gradient-to-r from-indigo-500 via-purple-500 to-pink-500 rounded-full shadow-md hover:opacity-90 transition duration-300 min-w-max">No Repeat</span>';
                            default:
                                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-yellow-400 rounded-full shadow-md hover:bg-yellow-500 transition duration-300 min-w-max">Unknown Type</span>';
                        }
                    }
                },
                {
                    data:"endDate",
                    name:"EndDate",
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: null,
                    name:"Action",
                    defaultContent:"",
                    render: renderCallBack
                }
            ],
            order: [[0, 'asc']],
            info: true,
            lengthMenu: [[5, 10, 50, 100, 250, 500, -1], [5, 10, 50, 100, 250, 500,"All"]],
            pageLength: pageLength ? pageLength : defaultPageLength
        }
    }

    function loadRunningTaskData() {

        let url ="/Tasks/GetAllRunningTaskList";

        function renderCallBack(data, type, row) {
            return `
                <div class="flex flex-wrap gap-2 items-center">
                    
                    <a href="/Tasks/Edit/${row.id}" data-id="${row.id}" class="editTaskBtn my-1 rounded bg-yellow-600 px-4 py-2 text-sm text-white hover:bg-yellow-700 focus:ring-2 focus:ring-yellow-300 transition-all duration-300 transform hover:scale-105 shadow-md w-full sm:w-auto" aria-label="Edit Task">
                        <i class="fas fa-edit"></i> 
                        <span class="ml-2">Edit</span>
                    </a>

                    
                    <button data-id="${row.id}" class="markAsCompleteTaskBtn my-1 rounded bg-green-600 px-4 py-2 text-sm text-white hover:bg-green-700 focus:ring-2 focus:ring-green-300 transition-all duration-300 transform hover:scale-105 shadow-md w-full sm:w-auto" aria-label="Mark as Complete">
                        <i class="fas fa-check"></i> 
                        <span class="ml-2">Mark as Complete</span>
                    </button>

                    
                    <button data-id="${row.id}" class="deleteTaskBtn my-1 rounded bg-red-600 px-4 py-2 text-sm text-white hover:bg-red-700 focus:ring-2 focus:ring-red-300 transition-all duration-300 transform hover:scale-105 shadow-md w-full sm:w-auto" aria-label="Delete Task">
                        <i class="fas fa-trash"></i> 
                        <span class="ml-2">Delete</span>
                    </button>
                </div>

            `;
        }

        if (taskRunningDataTable?.length) {

            taskRunningDataTable.DataTable(dataTableObject(url, renderCallBack, pageLengthValue.runningTask));

            taskRunningDataTable.on('length.dt', function (e, settings, len) {
                console.log('taskRunningDataTable page length: ' + len);

                localStorage.setItem(pageLengthValue.runningTask, len);
            });

        }
    }

    function loadCompletedTaskData() {
        let url ="/Tasks/GetAllCompletedTaskList";

        function renderCallBack(data, type, row) {
            return `
                <div class="flex flex-wrap gap-2 items-center">
                    
                    <a href="/Tasks/Edit/${row.id}" data-id="${row.id}" class="editTaskBtn my-1 rounded bg-yellow-600 px-4 py-2 text-sm text-white hover:bg-yellow-700 focus:ring-2 focus:ring-yellow-300 transition-all duration-300 transform hover:scale-105 shadow-md w-full sm:w-auto" aria-label="Edit Task">
                        <i class="fas fa-edit"></i> 
                        <span class="ml-2">Edit</span>
                    </a>

                    <!-- Move to Running Task Button -->
                    <button data-id="${row.id}" class="moveToRunningTaskBtn my-1 rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700 focus:ring-2 focus:ring-blue-300 transition-all duration-300 transform hover:scale-105 shadow-md w-full sm:w-auto" aria-label="Move to Running">
                        <i class="fas fa-play-circle"></i> 
                        <span class="ml-2">Move To Running</span>
                    </button>

                    
                    <button data-id="${row.id}" class="deleteTaskBtn my-1 rounded bg-red-600 px-4 py-2 text-sm text-white hover:bg-red-700 focus:ring-2 focus:ring-red-300 transition-all duration-300 transform hover:scale-105 shadow-md w-full sm:w-auto" aria-label="Delete Task">
                        <i class="fas fa-trash"></i> 
                        <span class="ml-2">Delete</span>
                    </button>
                </div>
            `;
        }


        if (taskCompletedDataTable?.length) {

            taskCompletedDataTable.DataTable(dataTableObject(url, renderCallBack, pageLengthValue.completedTask));

            taskCompletedDataTable.on('length.dt', function (e, settings, len) {
                console.log('taskCompletedDataTable page length: ' + len);

                localStorage.setItem(pageLengthValue.completedTask, len);
            });

        }
    }

    function loadNotStartedTaskData() {
        let url ="/Tasks/GetAllNotStartedTaskList";

        function renderCallBack(data, type, row) {
            return `
                <div class="flex flex-wrap gap-2 items-center">
                    
                    <a href="/Tasks/Edit/${row.id}" 
                       data-id="${row.id}" 
                       class="editTaskBtn my-1 rounded bg-yellow-600 px-4 py-2 text-sm text-white hover:bg-yellow-700 focus:ring-2 focus:ring-yellow-300 transition-all duration-300 transform hover:scale-105 shadow-md w-full sm:w-auto" 
                       aria-label="Edit Task">
                        <i class="fas fa-edit"></i>
                        <span class="ml-2">Edit</span>
                    </a>

                    <!-- Move to Running Button -->
                    <button data-id="${row.id}" 
                            class="moveToRunningTaskBtn my-1 rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700 focus:ring-2 focus:ring-blue-300 transition-all duration-300 transform hover:scale-105 shadow-md w-full sm:w-auto" 
                            aria-label="Move to Running">
                        <i class="fas fa-play"></i>
                        <span class="ml-2">Move To Running</span>
                    </button>

                    
                    <button data-id="${row.id}" 
                            class="deleteTaskBtn my-1 rounded bg-red-600 px-4 py-2 text-sm text-white hover:bg-red-700 focus:ring-2 focus:ring-red-300 transition-all duration-300 transform hover:scale-105 shadow-md w-full sm:w-auto" 
                            aria-label="Delete Task">
                        <i class="fas fa-trash"></i>
                        <span class="ml-2">Delete</span>
                    </button>
                </div>

            `;
        }

        if (taskNotStartedDataTable?.length) {

            taskNotStartedDataTable.DataTable(dataTableObject(url, renderCallBack, pageLengthValue.notStartedTask));

            taskNotStartedDataTable.on('length.dt', function (e, settings, len) {
                console.log('taskNotStartedDataTable page length: ' + len);

                localStorage.setItem(pageLengthValue.notStartedTask, len);
            });
        }

    }

    function loadEndedTaskData() {
        let url ="/Tasks/GetAllEndedTaskList";

        function renderCallBack(data, type, row) {
            return `
                <div class="flex flex-wrap gap-2 items-center">
                    
                    <a href="/Tasks/Edit/${row.id}" 
                        data-id="${row.id}" 
                        class="editTaskBtn my-1 rounded bg-yellow-600 px-4 py-2 text-sm text-white hover:bg-yellow-700 focus:outline-none focus:ring-2 focus:ring-yellow-300 transition-all duration-300 transform hover:scale-105 shadow-md w-full sm:w-auto" 
                        aria-label="Edit Task">
                        <i class="fas fa-edit"></i>
                        <span class="ml-2">Edit</span>
                    </a>

                    <!-- Move to Running Button -->
                    <button data-id="${row.id}" 
                            class="moveToRunningTaskBtn my-1 rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-300 transition-all duration-300 transform hover:scale-105 shadow-md w-full sm:w-auto" 
                            aria-label="Move To Running">
                        <i class="fas fa-play"></i>
                        <span class="ml-2">Move To Running</span>
                    </button>

                    
                    <button data-id="${row.id}" 
                            class="deleteTaskBtn my-1 rounded bg-red-600 px-4 py-2 text-sm text-white hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-300 transition-all duration-300 transform hover:scale-105 shadow-md w-full sm:w-auto" 
                            aria-label="Delete Task">
                        <i class="fas fa-trash"></i>
                        <span class="ml-2">Delete</span>
                    </button>
                </div>

            `;
        }

        if (taskEndedDataTable?.length) {

            taskEndedDataTable.DataTable(dataTableObject(url, renderCallBack, pageLengthValue.endedTask));

            taskEndedDataTable.on('length.dt', function (e, settings, len) {
                console.log('taskEndedDataTable page length: ' + len);

                localStorage.setItem(pageLengthValue.endedTask, len);
            });
        }
    }

    function loadDeletedTaskData() {
        deletedDataTableReload = taskDeletedDataTable.DataTable({
            ajax: {
                url:"/Tasks/GetAllDeletedTaskList",
                type:"POST"
            },
            responsive: true,
            processing: true,
            serverSide: true,
            filter: true,
            columns: [
                { data:"id", name:"id" },
                {
                    data:"name",
                    name:"name",
                    render: function (data, type, row) {
                        return `<a href="/Tasks/Details/${row.id}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
                    }
                },
                {
                    data:"priority",
                    name:"priority",
                    render: function (data, type, row) {
                        return returnPriorityBadge(data);
                    }
                },
                {
                    data:"repeat",
                    name:"repeat",
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
                    data:"taskStatus",
                    name:"taskStatus",
                    render: function (data, type, row) {
                        return returnStatusBadge(data);
                    }

                },
                {
                    data:"endDate",
                    name:"endDate",
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: null,
                    name:"Action",
                    defaultContent:"",
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

    taskRunningDataTable?.length && loadRunningTaskData();
    taskCompletedDataTable?.length && loadCompletedTaskData();
    taskNotStartedDataTable?.length && loadNotStartedTaskData();
    taskNotStartedDataTable?.length && loadEndedTaskData();
    //loadDeletedTaskData();


    $('#Repeat').on('change', function () {
        if ($(this).val() ==="2") {
            $('#RepeatWeekList').removeClass('hidden');
        } else {
            $('#RepeatWeekList').addClass('hidden');
        }
    });


    $('input[name="StartOptionType"]').on('change', function () {
        // Get the selected value
        var selectedValue = $('input[name="StartOptionType"]:checked').val();

        if (+selectedValue === startOptionValues.scheduled) {
            $('#startDateContainer').removeClass('hidden');
        } else {
            $('#startDateContainer').addClass('hidden');
            $('#startDate').val('');
            $('#startDateError').addClass('hidden');
        }
    });

    function getGoalIdFromGoalDetailPage() {
        var path = window.location.pathname ||"";
        var parts = path.split("/").filter(Boolean); // remove empty segments

        var detailsIndex = parts.indexOf("Details");

        if (detailsIndex !== -1 && parts.length > detailsIndex + 1) {
            var idCandidate = parts[detailsIndex + 1];

            if (/^\d+$/.test(idCandidate)) {
                return idCandidate; // return as string
            }
        }

        return""; // fallback if not found
    }
})