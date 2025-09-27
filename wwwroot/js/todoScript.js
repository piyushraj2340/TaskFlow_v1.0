$(document).ready(function () {

    const todoStatusEnum = Object.freeze({
        notStarted: 0,
        running: 1,
        completed: 2,
        ended: 3
    });


    const pageLengthValue = Object.freeze({
        runningTodo: "RUNNING_TODO_PAGE_LENGTH",
        completedTodo: "COMPLETED_TODO_PAGE_LENGTH",
    });

    const defaultPageLength = 5;

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


    const dataTableObject = (url, renderCallBack, pageLengthKey) => {

        const pageLength = localStorage.getItem(pageLengthKey);

        const taskId = getTaskIdFromTaskDetailPage();

        if (taskId) {
            url = url + "?taskId=" + taskId;
        }

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
                                const days = row.task.repeatWeekList.map(d => dayMap[d]).join(', ');
                                return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-400 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max" title="${days}">Weekly</span>`;
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
            lengthMenu: [[5, 10, 50, 100, 250, 500, -1], [5, 10, 50, 100, 250, 500, "All"]],
            pageLength: pageLength ? pageLength : defaultPageLength
        }
    }

    function loadRunningTaskData() {

        let url = "/Todo/GetRunningTodo";

        function renderCallBack(data, type, row) {
            return `
                <a href="/Tasks/Edit/${row?.taskId}" data-id="${row?.taskId}" class="editTaskBtn my-1 me-1 rounded bg-yellow-500 px-3 py-1 text-sm text-white hover:bg-yellow-600">
                    <i class="fas fa-edit"></i> Edit
                </a>
                <button data-id="${row.id}" class="markAsCompleteToDoBtn my-1 me-1 rounded bg-green-500 px-3 py-1 text-sm text-white hover:bg-green-600">
                        <i class="fas fa-check"></i> Mark as Complete
                </button>
            `;
        }

        if (todoRunningDataTable?.length) {

            todoRunningDataTable.DataTable(dataTableObject(url, renderCallBack, pageLengthValue.runningTodo));

            todoRunningDataTable.on('length.dt', function (e, settings, len) {
                console.log('todoRunningDataTable page length: ' + len);

                localStorage.setItem(pageLengthValue.runningTodo, len);
            });
        }
    }

    function loadCompletedTaskData() {

        let url = "/Todo/GetCompletedTodo";

        function renderCallBack(data, type, row) {
            return `
                <a href="/Tasks/Edit/${row?.taskId}" data-id="${row?.taskId}" class="editTaskBtn my-1 me-1 rounded bg-yellow-500 px-3 py-1 text-sm text-white hover:bg-yellow-600">
                    <i class="fas fa-edit"></i> Edit
                </a>
                <button data-id="${row.id}" class="moveToRunningToDoBtn my-1 me-1 rounded bg-blue-500 px-3 py-1 text-sm text-white hover:bg-blue-600">
                        <i class="fas fa-check"></i> Move to Running
                </button>
            `;
        }

        if (todoCompletedDataTable?.length) {

            todoCompletedDataTable.DataTable(dataTableObject(url, renderCallBack, pageLengthValue.completedTodo));

            todoCompletedDataTable.on('length.dt', function (e, settings, len) {
                localStorage.setItem(pageLengthValue.completedTodo, len);
            });
        }
    }

    loadRunningTaskData();
    loadCompletedTaskData();

    function formatDate(date, formatType = "long") {
        const today = new Date();
        const inputDate = new Date(date);

        // Reset hours, minutes, seconds, ms to compare only the date
        today.setHours(0, 0, 0, 0);
        inputDate.setHours(0, 0, 0, 0);

        const dayDifference = Math.floor((today - inputDate) / (1000 * 60 * 60 * 24));

        if (formatType === "short") {
            return inputDate.toISOString().split('T')[0];
        }

        if (dayDifference === 0) return "Today";
        if (dayDifference === 1) return "Yesterday";
        if (dayDifference < 7) {
            return inputDate.toLocaleDateString('en-US', { weekday: 'long' });
        }

        return inputDate.toLocaleDateString('en-US', {
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
            $("#CompletedTodoHeadId").html("Today's Completed Tasks");
            $("#RunningOrMissingHeadId").html("Today's Tasks");
            $("#todoSections").removeClass("flex-col-reverse");
            return nextBtn.prop("disabled", true).removeClass("bg-indigo-600").addClass("bg-gray-500");
        }

        $("#CompletedTodoHeadId").html("Completed Tasks");
        $("#RunningOrMissingHeadId").html("Missed Tasks");
        $("#todoSections").addClass("flex-col-reverse");

        nextBtn.prop("disabled", false).addClass("bg-indigo-600").removeClass("bg-gray-500");
    }

    function handelUpdateTodoProductivity(date) {

        var url = `/Todo/GetTodoProgressAnalyses?forDate=${date}`;

        const taskId = getTaskIdFromTaskDetailPage();

        if (taskId) {
            url = url + "&taskId=" + taskId;
        }


        $.ajax({
            url,
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
        const taskId = getTaskIdFromTaskDetailPage();
        if (taskId) {
            $("#viewRunningTodoTableData").DataTable().ajax.url(`/Todo/GetRunningTodo?selectDate=${date}&taskId=${taskId}`).load();
            $("#viewCompletedTodoTableData").DataTable().ajax.url(`/Todo/GetCompletedTodo?selectDate=${date}&taskId=${taskId}`).load();
        } else {
            $("#viewRunningTodoTableData").DataTable().ajax.url(`/Todo/GetRunningTodo?selectDate=${date}`).load();
            $("#viewCompletedTodoTableData").DataTable().ajax.url(`/Todo/GetCompletedTodo?selectDate=${date}`).load();
        }
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

    function getTaskIdFromTaskDetailPage() {
        var path = window.location.pathname || "";
        var parts = path.split("/").filter(Boolean); // remove empty segments

        var detailsIndex = parts.indexOf("Details");

        if (detailsIndex !== -1 && parts.length > detailsIndex + 1) {
            var idCandidate = parts[detailsIndex + 1];

            if (/^\d+$/.test(idCandidate)) {
                return idCandidate; // return as string
            }
        }

        return ""; // fallback if not found
    }
});

