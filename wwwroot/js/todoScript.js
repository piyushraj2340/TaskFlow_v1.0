$(document).ready(function () {

    const todoStatusEnum = Object.freeze({
        notStarted: 0,
        running: 1,
        completed: 2,
        ended: 3
    });


    const todoRunningDataTable = $("#viewRunningTodoTableData");

    const todoCompletedDataTable = $("#viewCompletedTodoTableData");

    const runningTodoTaskCount = $("#runningTodoTaskCount");
    const completedTodoTaskCount = $("#completedTodoTaskCount");
    const totalTodoTaskCount = $("#totalTodoTaskCount");
    const productivityTotoTaskPercentage = $("#productivityPercentageTodo");


    let selectedDate = new Date();

    const nextBtn = $("#nextDateBtn");
    const prevBtn = $("#prevDateBtn");

    const inputDatePicker = $('#taskDatePicker');

    const datePicker = flatpickr(inputDatePicker, {
        dateFormat: "Y-m-d",
        defaultDate: new Date(),
        maxDate: new Date(),
        onChange: function (selectedDates, dateStr) {
            selectedDate = new Date(dateStr);
            updateDateDisplay();
            fetchTasksByDate(dateStr);
        }
    });

    function handelStatusChange(data) {
        $.ajax({
            url: "/Todo/ChangeTodoStatus",
            method: "POST",
            data,
            success: function (data) {
                if (data.status) {
                    todoRunningDataTable?.length && todoRunningDataTable.DataTable().ajax.reload();
                    todoCompletedDataTable?.length && todoCompletedDataTable.DataTable().ajax.reload();
                    showSuccessNotification(data.message);
                    handelUpdateTodoProductivity(selectedDate.toLocaleDateString("en-CA", { timeZone: "Asia/Kolkata" }));
                } else {
                    showErrorNotification(data.message || "Error: while changing the status of Goal with Id: " + data.Id);
                    console.error(data.message || "Failed while changing the status of Goal with Id: " + data.Id);
                }
            },
            error: function (xhr, status, error) {
                showErrorNotification(error || "Error: while changing the status of Goal with Id: " + data.Id);
                console.error(error || "Failed while changing the status of Goal with Id: " + data.Id);
            }
        })
    }

    todoRunningDataTable?.length && todoRunningDataTable.on("click", ".markAsCompleteToDoBtn", async function () {
        const id = $(this).data("id");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        handelStatusChange({
            Id: id,
            Status: todoStatusEnum.completed
        });
    })

    todoCompletedDataTable?.length && todoCompletedDataTable.on("click", ".moveToRunningToDoBtn", async function () {
        const id = $(this).data("id");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        handelStatusChange({
            Id: id,
            Status: todoStatusEnum.running
        });
    })


    const dataTableObject = (url, renderCallBack) => {
        return {
            ajax: {
                url,
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
                        return `<a href="/Tasks/Details/${row?.taskId}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
                    }
                },
                {
                    data: "task.priority",
                    name: "priority",
                    render: function (data, type, row) {
                        return returnPriorityBadge(data);
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
                    render: renderCallBack
                }
            ],
            order: [[0, 'asc']],
            info: true,
            pageLength: 5
        }
    }

    function loadRunningTaskData() {

        let url = "/Todo/GetRunningTodo";

        function renderCallBack(data, type, row) {
            return `
                <a href="/Tasks/Edit/${row.id}" data-id="${row.id}" class="editTaskBtn my-1 me-1 rounded bg-yellow-500 px-3 py-1 text-sm text-white hover:bg-yellow-600">
                    <i class="fas fa-edit"></i> Edit
                </a>
                <button data-id="${row.id}" class="markAsCompleteToDoBtn my-1 me-1 rounded bg-green-500 px-3 py-1 text-sm text-white hover:bg-green-600">
                        <i class="fas fa-check"></i> Mark as Complete
                </button>
            `;
        }
        todoRunningDataTable?.length && todoRunningDataTable.DataTable(dataTableObject(url, renderCallBack));
    }

    function loadCompletedTaskData() {

        let url = "/Todo/GetCompletedTodo";

        function renderCallBack(data, type, row) {
            return `
                <a href="/Tasks/Edit/${row.id}" data-id="${row.id}" class="editTaskBtn my-1 me-1 rounded bg-yellow-500 px-3 py-1 text-sm text-white hover:bg-yellow-600">
                    <i class="fas fa-edit"></i> Edit
                </a>
                <button data-id="${row.id}" class="moveToRunningToDoBtn my-1 me-1 rounded bg-blue-500 px-3 py-1 text-sm text-white hover:bg-blue-600">
                        <i class="fas fa-check"></i> Move to Running
                </button>
            `;
        }
        todoCompletedDataTable?.length && todoCompletedDataTable.DataTable(dataTableObject(url, renderCallBack));
    }

    loadRunningTaskData();
    loadCompletedTaskData();

    function formatDate(date, formatType = "long") {
        let today = new Date();
        let yesterday = new Date();
        yesterday.setDate(today.getDate() - 1);

        let dayDifference = Math.floor((today - date) / (1000 * 60 * 60 * 24));

        if (formatType === "short") {
            return date.toISOString().split('T')[0];
        }

        if (dayDifference === 0) return "Today";
        if (dayDifference === 1) return "Yesterday";
        if (dayDifference < 7) {
            return date.toLocaleDateString('en-US', { weekday: 'long' });
        }

        return date.toLocaleDateString('en-US', {
            weekday: 'long',
            day: "numeric",
            month: "long",
            year: "numeric"
        });
    }


    function updateDateDisplay() {
        $("#selectedDate").text(formatDate(selectedDate));

        datePicker.setDate(selectedDate, false);

        if (formatDate(selectedDate) == "Today") {
            return nextBtn.prop("disabled", true).removeClass("bg-indigo-600").addClass("bg-gray-500");
        }

        nextBtn.prop("disabled", false).addClass("bg-indigo-600").removeClass("bg-gray-500");
    }

    function handelUpdateTodoProductivity(date) {
        $.ajax({
            url: `/Todo/GetTodoProgressAnalyses?forDate=${date}`,
            method: "POST",
            success: function (response) {
                if (response.status) {
                    const { productivityForDay, totalCompletedTodo, totalMissedTodo, totalTodo } = response.data;

                    completedTodoTaskCount.html(totalCompletedTodo);
                    runningTodoTaskCount.html(totalMissedTodo);
                    totalTodoTaskCount.html(totalTodo);
                    productivityTotoTaskPercentage.html(productivityForDay + "%");
                }
                else {
                    completedTodoTaskCount.html(0);
                    runningTodoTaskCount.html(0);
                    totalTodoTaskCount.html(0);
                    productivityTotoTaskPercentage.html(0 + "%");

                    console.error(response.message);
                }
            },
            error: function (error) {
                completedTodoTaskCount.html(0);
                runningTodoTaskCount.html(0);
                totalTodoTaskCount.html(0);
                productivityTotoTaskPercentage.html(0 + "%");
                console.error(error);
            }
        })
    }

    function fetchTasksByDate(date) {
        $("#viewRunningTodoTableData").DataTable().ajax.url(`/Todo/GetRunningTodo?selectDate=${date}`).load();
        $("#viewCompletedTodoTableData").DataTable().ajax.url(`/Todo/GetCompletedTodo?selectDate=${date}`).load();
        handelUpdateTodoProductivity(date);
    }

    prevBtn.click(function () {
        selectedDate.setDate(selectedDate.getDate() - 1);
        updateDateDisplay();
        fetchTasksByDate(selectedDate.toLocaleDateString("en-CA", { timeZone: "Asia/Kolkata" }));
    });

    nextBtn.click(function () {
        selectedDate.setDate(selectedDate.getDate() + 1);
        updateDateDisplay();
        fetchTasksByDate(selectedDate.toLocaleDateString("en-CA", { timeZone: "Asia/Kolkata" }));

    });

    // Initialize display on load
    updateDateDisplay();
});

