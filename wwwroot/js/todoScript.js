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

    function returnStatusBadge(status, title = '') {
        // Ensure the status is a valid number (not NaN, not undefined, etc.)
        const data = Number.parseInt(status);

        if (typeof title !== 'string') {
            title = '';
        }

        // Validate if 'status' is a valid number
        if (isNaN(data) || typeof data !== 'number') {
            return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-900 rounded-full shadow-md hover:bg-yellow-500 transition duration-300 min-w-max">Unknown Type</span>';
            // Return an error message or default badge if invalid
        }

        switch (data) {
            case 0:
                return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-yellow-400 rounded-full shadow-md hover:bg-yellow-500 transition duration-300 min-w-max" title="${title}">Not Started</span>`;
            case 1:
                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-blue-500 rounded-full shadow-md hover:bg-blue-600 transition duration-300 min-w-max">Running</span>';
            case 2:
                return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-green-500 rounded-full shadow-md hover:bg-green-600 transition duration-300 min-w-max" title="${title}">Completed</span>`;
            case 3:
                return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-400 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max" title="${title}">End</span>`;
            case 4:
                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-red-500 rounded-full shadow-md hover:bg-red-600 transition duration-300 min-w-max">Deleted</span>';
            default:
                return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-900 rounded-full shadow-md hover:bg-gray-950 transition duration-300 min-w-max">Unknown Type</span>';
        }
    }

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

    // Mark as complete with notes validation
    $(document).on("click", ".markAsCompleteToDoBtn", function () {
        const todoId = $(this).data("id");
        // Find the closest table row
        const $tr = $(this).closest("tr");
        let notes = "";

        // Try to get notes from the notes cell (by class)
        const $notesCell = $tr.find(".notes-cell");
        if ($notesCell.length) {
            notes = $notesCell.data("notes") || $notesCell.attr("data-notes") || "";
            if (typeof notes === "undefined") notes = "";
            notes = notes.trim();
        }

        // Fallback: get notes from DataTable row data if available
        if (!notes && $tr.length && $tr.closest("table").length) {
            const table = $tr.closest("table").DataTable();
            const rowData = table.row($tr).data();
            if (rowData && rowData.notes) {
                notes = rowData.notes.trim();
            }
        }

        if (!notes) {
            Swal.fire({
                title: "No notes added!",
                text: "Do you want to complete this todo without adding notes?",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Yes, complete",
                cancelButtonText: "Add Notes"
            }).then((result) => {
                if (result.isConfirmed) {
                    handelStatusChange({ Id: todoId, Status: todoStatusEnum.completed });
                } else {
                    // Open notes modal
                    $(".addOrUpdateNotesBtn[data-id='" + todoId + "']").click();
                }
            });
        } else {
            handelStatusChange({ Id: todoId, Status: todoStatusEnum.completed });
        }
    });

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
                { data: "id", name: "id", orderable: true, searchable: true },
                {
                    data: "task.name",
                    name: "name",
                    orderable: true,
                    searchable: true,
                    render: function (data, type, row) {
                        return `<a href="/Tasks/Details/${row?.taskId}" class="text-blue-500 hover:text-blue-700 hover:underline">${data}</a>`
                    }
                },
                {
                    data: "task.taskStatus",
                    name: "status",
                    orderable: true,
                    searchable: false,
                    render: function (data, type, row) {

                        let title = '';

                        if (row.isManualAdded) {
                            title = 'Manualy Added Todo.';


                            if (data == todoStatusEnum.notStarted) {
                                if (row.task.isScheduled) {
                                    title += ' Task is scheduled on date: ' + formatFullDate(row.task.startDate);
                                } else {
                                    title += ' Tash Need to Start Manualy';
                                }
                            } else if (data == todoStatusEnum.completed) {
                                title += ' Task is completed on date: ' + formatFullDate(row.task.completedOn);
                            } else if (data == todoStatusEnum.ended) {
                                title += ' Task is ended on date: ' + formatFullDate(row.task.endedOn);
                            }
                        }

                        return returnStatusBadge(data, title);
                    }
                },
                {
                    data: "task.priority",
                    name: "priority",
                    orderable: true,
                    searchable: false,
                    render: function (data, type, row) {
                        return returnPriorityBadge(data);
                    }
                },
                {
                    data: "task.repeat",
                    name: "repeat",
                    orderable: true,
                    searchable: false,
                    render: function (data, type, row) {
                        const days = row.task.repeatWeekList.map(d => dayMap[d]).join(', ');
                        return returnRepeatyBadge(data, days);
                    }
                },
                {
                    data: "endDate",
                    name: "endDate",
                    orderable: true,
                    searchable: false,
                    render: function (data, type, row) {
                        return `<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-700 rounded-full shadow-md hover:bg-gray-500 transition duration-300 min-w-max">${formatShortDate(data)}</span>`
                    }
                },
                {
                    data: "notes",
                    name: "notes",
                    orderable: true,
                    searchable: true,
                    render: function (data, type, row) {
                        if (!data) return "";

                        const noteHTML = data.replace(/\n/g, "<br />");
                        const noteText = data.replace(/\n/g, " ");

                        return `
                            <div data-notes="${data}" class="relative group inline-block max-w-xs notes-cell" 
                                 aria-label="Full Note: ${noteText}">
            
                                <div class="bg-yellow-100 text-yellow-800 p-1 rounded-md shadow-sm text-sm font-medium leading-snug line-clamp-3 overflow-hidden cursor-help border border-yellow-300 transition duration-150 ease-in-out hover:shadow-md">
                                    ${noteHTML}
                                </div>

                                <div class="absolute z-[9999] hidden group-hover:block top-1/2 left-[100%] -translate-y-1/2  -mt-2 w-96 max-w-lg">
                                    <div class="relative 
                                                 bg-yellow-50 text-yellow-800 
                                                 p-3 rounded-lg shadow-xl border border-yellow-300 
                                                 text-sm max-h-[200px] overflow-auto">

                                        ${noteHTML}
                                    </div>
                                </div>
                            </div>
                        `;
                    }

                },
                {
                    data: "taskProductivity",
                    name: "productivity",
                    orderable: true,
                    searchable: true,
                    render: function (data, type, row) {
                        if (!data || data.productivityForDay === undefined || data.productivityForDay === null) {
                            return `<span class="inline-flex items-center px-3 py-1 text-xs font-semibold text-gray-500 rounded-full">-</span>`;
                        }

                        // Data
                        const percent = Math.round(Number(data.productivityForDay));
                        const total = data.totalTodo ?? 0;
                        const completed = data.totalCompletedTodo ?? 0;
                        const missed = data.totalMissedTodo ?? 0;

                        // SVG Circular Meter
                        const size = 48;
                        const radius = 20;
                        const stroke = 5;
                        const normalizedPercent = Math.max(0, Math.min(100, percent));
                        const circumference = 2 * Math.PI * radius;
                        const offset = circumference - (normalizedPercent / 100) * circumference;

                        // Color
                        let color = "#ef4444"; // red
                        if (normalizedPercent >= 75) color = "#22c55e"; // green
                        else if (normalizedPercent >= 50) color = "#eab308"; // yellow

                        // Tooltip HTML
                        const tooltipHtml = `
                            <div class="circular-tooltip absolute z-50 left-[80%] top-1/2 mt-2 w-48 -translate-y-1/2 bg-white text-gray-800 rounded-lg shadow-lg border border-gray-200 p-3 text-xs hidden group-hover:block">
                                <div class="font-semibold text-sm mb-2">Productivity Details</div>
                                <div class="flex justify-between mb-1"><span>Total Tasks:</span><span>${total}</span></div>
                                <div class="flex justify-between mb-1"><span>Completed:</span><span>${completed}</span></div>
                                <div class="flex justify-between mb-1"><span>Missed:</span><span>${missed}</span></div>
                                <div class="flex justify-between"><span>Productivity:</span><span class="font-bold">${percent}%</span></div>
                            </div>
                        `;

                        // Main HTML
                        return `
                            <div class="relative flex flex-col items-center group cursor-help" style="min-width:${size}px;">
                                <svg width="${size}" height="${size}" viewBox="0 0 ${size} ${size}">
                                    <circle
                                        cx="${size / 2}" cy="${size / 2}" r="${radius}"
                                        stroke="#e5e7eb" stroke-width="${stroke}" fill="none"
                                    />
                                    <circle
                                        cx="${size / 2}" cy="${size / 2}" r="${radius}"
                                        stroke="${color}" stroke-width="${stroke}" fill="none"
                                        stroke-dasharray="${circumference}"
                                        stroke-dashoffset="${offset}"
                                        style="transition:stroke-dashoffset 0.6s;"
                                        stroke-linecap="round"
                                    />
                                    <text
                                        x="50%" y="54%" text-anchor="middle" dominant-baseline="middle"
                                        font-size="10" font-weight="bold" fill="#374151"
                                    >${percent}%</text>
                                </svg>
                                ${tooltipHtml}
                            </div>
                        `;
                    }
                },
                {
                    data: null,
                    name: "Action",
                    orderable: false,
                    searchable: false,
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
            const hasNotes = row.notes && row.notes.trim().length > 0;

            return `
               <div class="flex flex-nowrap items-center gap-1 justify-start sm:justify-center">

                    <button data-id="${row.id}" 
                            class="addOrUpdateNotesBtn group flex h-8 w-8 items-center justify-center rounded bg-purple-100 text-purple-600 hover:bg-purple-600 hover:text-white transition-colors duration-200"
                            title="${hasNotes ? 'Update Notes' : 'Add Notes'}">
                        <i class="fas fa-comment text-sm"></i>
                    </button>

                    <a href="/Tasks/Edit/${row?.taskId}" 
                       data-id="${row?.taskId}" 
                       class="editTaskBtn group flex h-8 w-8 items-center justify-center rounded bg-amber-100 text-amber-600 hover:bg-amber-500 hover:text-white transition-colors duration-200"
                       title="Edit Task">
                        <i class="fas fa-edit text-sm"></i>
                    </a>

                    <button data-id="${row.id}" 
                            class="markAsCompleteToDoBtn group flex h-8 w-8 items-center justify-center rounded bg-green-100 text-green-600 hover:bg-green-600 hover:text-white transition-colors duration-200"
                            title="Mark as Complete">
                        <i class="fas fa-check text-sm"></i>
                    </button>

                </div>
            `;
        }

        if (todoRunningDataTable?.length) {

            todoRunningDataTable.DataTable(dataTableObject(url, renderCallBack, pageLengthValue.runningTodo));

            todoRunningDataTable.on('length.dt', function (e, settings, len) {
                localStorage.setItem(pageLengthValue.runningTodo, len);
            });
        }
    }

    function loadCompletedTaskData() {

        let url = "/Todo/GetCompletedTodo";

        function renderCallBack(data, type, row) {
            const hasNotes = row.notes && row.notes.trim().length > 0;

            return `
               <div class="flex flex-nowrap items-center gap-1 justify-start sm:justify-center">

                <button data-id="${row.id}" 
                        class="addOrUpdateNotesBtn group flex h-8 w-8 items-center justify-center rounded bg-purple-100 text-purple-600 hover:bg-purple-600 hover:text-white transition-colors duration-200"
                        title="${hasNotes ? 'Update Notes' : 'Add Notes'}">
                    <i class="fas fa-comment text-sm"></i>
                </button>

                <a href="/Tasks/Edit/${row?.taskId}" 
                    data-id="${row?.taskId}" 
                    class="editTaskBtn group flex h-8 w-8 items-center justify-center rounded bg-amber-100 text-amber-600 hover:bg-amber-500 hover:text-white transition-colors duration-200"
                    title="Edit Task">
                    <i class="fas fa-edit text-sm"></i>
                </a>

                <button data-id="${row.id}" 
                        class="moveToRunningToDoBtn group flex h-8 w-8 items-center justify-center rounded bg-blue-100 text-blue-600 hover:bg-blue-600 hover:text-white transition-colors duration-200"
                        title="Move to Running">
                    <i class="fas fa-play-circle text-sm"></i>
                </button>

            </div>
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
    if ($("taskDatePicker").length) {
        updateDateDisplay();
    }

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


    // Modal logic
    $(document).on("click", ".addOrUpdateNotesBtn", function () {
        const todoId = $(this).data("id");
        // Load notes via AJAX and show modal
        $.get(`/Todo/GetTodoById?id=${todoId}`, function (response) {
            $("#todoNotesModal").data("todoid", todoId);
            $("#todoNotesTextarea").val(response.data.notes || "");
            $("#todoNotesModal").removeClass("hidden").fadeIn();
        });
    });

    // Save notes
    $("#saveTodoNotesBtn").on("click", function () {
        const todoId = $("#todoNotesModal").data("todoid");
        const notes = $("#todoNotesTextarea").val();
        $.post("/Todo/SaveTodoNotes", { todoId, notes }, function (response) {
            if (response.status) {
                todoRunningDataTable?.length && todoRunningDataTable.DataTable().ajax.reload();
                todoCompletedDataTable?.length && todoCompletedDataTable.DataTable().ajax.reload();
                $("#todoNotesModal").fadeOut();
            } else {
                alert(response.message);
            }
        });
    });

    // Close modal by button
    $("#closeTodoNotesModalBtn").on("click", function () {
        $("#todoNotesModal").fadeOut();
    });

    // Close modal by clicking outside the modal card
    $("#todoNotesModal").on("click", function (e) {
        // Only close if the click is on the overlay, not inside the modal card
        if (e.target === this) {
            $(this).fadeOut();
        }
    });

    // Optional: close modal on ESC key
    $(document).on("keydown", function (e) {
        if (e.key === "Escape") {
            $("#todoNotesModal").fadeOut();
        }
    });
});

function returnGoalBadge(goal) {
    const status = Number.parseInt(goal.goalStatus);

    if (isNaN(status)) {
        return `<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-gray-900 rounded-full shadow-md min-w-max">
              #${goal.id} - ${goal.name}
            </span>`;
    }

    switch (status) {
        case 0:
            return `<span title="Status - NotStarted" class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-yellow-400 rounded-full shadow-md min-w-max hover:bg-yellow-500 transition duration-300">
                #${goal.id} - ${goal.name}
              </span>`;
        case 1:
            return `<span title="Status - Running" class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-blue-500 rounded-full shadow-md min-w-max hover:bg-blue-600 transition duration-300">
                #${goal.id} - ${goal.name}
              </span>`;
        case 2:
            return `<span title="Status - Completed" class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-green-500 rounded-full shadow-md min-w-max hover:bg-green-600 transition duration-300">
                #${goal.id} - ${goal.name}
              </span>`;
        case 3:
            return `<span title="Status - Ended" class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-gray-400 rounded-full shadow-md min-w-max hover:bg-gray-500 transition duration-300">
                #${goal.id} - ${goal.name}
              </span>`;
        case 4:
            return `<span  title="Status - Deleted" class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-red-500 rounded-full shadow-md min-w-max hover:bg-red-600 transition duration-300">
                #${goal.id} - ${goal.name}
              </span>`;
        default:
            return `<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-gray-900 rounded-full shadow-md min-w-max">
                #${goal.id} - ${goal.name}
              </span>`;
    }
}

$(document).ready(function () {
    // --- ELEMENT SELECTORS ---
    const searchTaskModal = $("#searchTaskModal");
    const taskMultiSelect = $("#taskMultiSelect");
    const selectedTasksContainer = $("#selectedTasksContainer");
    const emptyState = $("#emptyState");
    const dropdownButton = $("#statusFilterDropdownButton");
    const dropdownMenu = $("#statusFilterDropdownMenu");
    const selectedStatusText = $("#selectedStatusText");

    // --- STATE MANAGEMENT ---
    let selectedTaskIds = [];
    let currentStatus = 4; // Default to 'All'

    // --- HELPER FUNCTION: CREATE TASK CARD HTML (Unchanged) ---
    function createTaskCardHTML(task, isSelectedTag = false) {
        // Safely map goal lists, providing a fallback message
        const goalBadges = task.goalLists?.map(goal => returnGoalBadge(goal)).join('')
            || '<span class="text-xs text-gray-500 italic">No associated goals</span>';

        const goalListHTML = `
            <div class="mt-3 pt-3 border-t border-gray-200">
                <span class="text-gray-500 text-xs font-semibold">GOALS</span>
                <div class="flex flex-wrap gap-2 mt-1">${goalBadges}</div>
            </div>
        `;

        // Conditionally add a remove button for selected tags
        const removeButton = isSelectedTag
            ? `<button class="remove-task-btn absolute top-2 right-2 transition" data-id="${task.id}" title="Remove"><i class="fas fa-times-circle"></i></button>`
            : '';

        return `
            <div class="task-card relative w-full p-5 border rounded-lg shadow-sm bg-white transition-all hover:shadow-md hover:border-indigo-300" data-task-id="${task.id}">
                ${removeButton}
                <div class="flex w-full justify-between items-start">
                    <div class="flex flex-col pr-4">
                        <span class="text-indigo-500 text-xs font-mono">TASK #${task.id}</span>
                        <div class="text-md font-semibold text-gray-800">${task.name}</div>
                    </div>
                    <div class="flex-shrink-0 mr-3">${returnStatusBadge(task.taskStatus)}</div>
                </div>
                ${task.goalLists && task.goalLists.length > 0 ? goalListHTML : ''}
            </div>
        `;
    }

    // --- MODAL & DATEPICKER LOGIC (Unchanged) ---
    function updateAddTodoButtonVisibility() {
        const today = new Date(); today.setHours(0, 0, 0, 0);
        const selectedDateStr = $("#taskDatePicker").val();
        const selectedDate = selectedDateStr ? new Date(selectedDateStr) : today; selectedDate.setHours(0, 0, 0, 0);
        $("#addTodoButton").toggleClass("hidden", selectedDate.getTime() !== today.getTime());
    }
    $("#taskDatePicker, #prevDateBtn, #nextDateBtn").on("change click", updateAddTodoButtonVisibility);
    updateAddTodoButtonVisibility();

    $("#addTodoButton").on("click", function () {
        searchTaskModal.removeClass("hidden");
        taskMultiSelect.val(null).trigger('change');
        selectedTasksContainer.empty().append(emptyState.show());
        dropdownMenu.find('.status-filter-option[data-status=4]').click();
    });

    $("#closeTodoModalButton, #cancelModalButton").on("click", () => searchTaskModal.addClass("hidden"));

    // --- DROPDOWN INTERACTION LOGIC ---
    dropdownButton.on("click", function () {
        dropdownMenu.toggleClass("hidden");
        $(this).find('i').toggleClass("rotate-180");
    });

    $(document).on("click", function (event) {
        if (!$(event.target).closest("#statusFilterDropdownButton, #statusFilterDropdownMenu").length) {
            dropdownMenu.addClass("hidden");
            dropdownButton.find('i').removeClass("rotate-180");
        }
    });

    dropdownMenu.on("click", ".status-filter-option", function (e) {
        e.preventDefault();
        const option = $(this);
        currentStatus = option.data("status");
        selectedStatusText.text(option.text());

        dropdownMenu.find(".status-filter-option").removeClass("bg-indigo-600 text-white font-semibold").addClass("hover:bg-gray-100");
        option.addClass("bg-indigo-600 text-white font-semibold").removeClass("hover:bg-gray-100");

        dropdownMenu.addClass("hidden");
        dropdownButton.find('i').removeClass("rotate-180");

        // --- ✨ KEY CHANGE HERE ---
        // We no longer clear the selection. This preserves the user's choices.
        // taskMultiSelect.val(null).trigger('change'); 

        // We still open the dropdown to encourage a new search with the new filter.
        taskMultiSelect.select2('open');
    });
    dropdownMenu.find('.status-filter-option[data-status=4]').addClass("bg-indigo-600 text-white font-semibold");

    // --- SELECT2 INITIALIZATION (Unchanged) ---
    taskMultiSelect.select2({
        dropdownParent: searchTaskModal,
        placeholder: "Search by task name or ID...",
        minimumInputLength: 1,
        ajax: {
            url: (params) => `/TaskSearch/SearchTasksWithGoals?query=${params.term}&status=${currentStatus}`,
            dataType: 'json',
            delay: 300,
            processResults: (response) => ({ results: response.status ? response.data.filter(task => !selectedTaskIds.includes(task.id.toString())).map(task => ({ id: task.id, text: `#${task.id} - ${task.name}`, task: task })) : [] }),
        },
        templateResult: (data) => data.loading ? $('<div class="p-3 text-gray-500">Searching...</div>') : (data.task ? $(createTaskCardHTML(data.task, false)) : data.text),
        templateSelection: (data) => data.text
    });

    // --- SELECTION & REMOVAL LOGIC (Unchanged) ---
    taskMultiSelect.on("change", function () {
        const selectedData = taskMultiSelect.select2('data');
        selectedTaskIds = selectedData.map(item => item.id.toString());
        selectedTasksContainer.empty();
        if (selectedData.length > 0) {
            selectedData.forEach(item => { if (item.task) selectedTasksContainer.append(createTaskCardHTML(item.task, true)); });
        } else {
            selectedTasksContainer.append(emptyState.show());
        }
    });
    selectedTasksContainer.on("click", ".remove-task-btn", function () {
        const idToRemove = $(this).data("id").toString();
        const newVals = (taskMultiSelect.val() || []).filter(id => id !== idToRemove);
        taskMultiSelect.val(newVals).trigger('change');
    });

    // --- CONFIRM & SUBMIT LOGIC (Unchanged) ---
    $("#confirmTasksButton").on("click", function () {
        const selectedData = taskMultiSelect.select2('data');
        if (selectedData.length === 0) { Swal.fire({ icon: "warning", title: "No Tasks Selected", text: "Please select at least one task." }); return; }
        const taskIds = selectedData.map(d => d.task.id);
        $.ajax({
            url: "/Todo/AddBulkTodos", method: "POST", contentType: "application/json", data: JSON.stringify({ taskIds, notes: "" }),
            success: (response) => {
                if (response.status) {
                    Swal.fire({ icon: "success", title: "Success!", text: response.message });
                    searchTaskModal.addClass("hidden");
                    $("#viewRunningTodoTableData").DataTable().ajax.reload(null, false);
                } else { Swal.fire({ icon: "error", title: "Error", text: response.message }); }
            },
            error: () => Swal.fire({ icon: "error", title: "Request Failed", text: "Could not create todos." })
        });
    });
});