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

    const pageLengthValue = Object.freeze({
        runningGoal: "RUNNING_GOAL_PAGE_LENGTH",
        completedGoal: "COMPLETED_GOAL_PAGE_LENGTH",
        notStartedGoal: "NOT_STARTED_GOAL_PAGE_LENGTH",
        endedGoal: "ENDED_GOAL_PAGE_LENGTH",
    });

    const defaultPageLength = 5;

    // Running task...
    const runningGoalDataTable = $('#viewRunningGoalTableData');
    // Completed Task...
    const completedGoalDataTable = $('#viewCompletedGoalTableData');
    // Not Started Task...
    const notStartedGoalDataTable = $('#viewNotStartedGoalTableData');
    // Ended Task...
    const endedGoalDataTable = $('#viewEndedGoalTableData');
    // Deleted Task...
    const deletedGoalDataTable = $('#viewDeletedTableData');

    // Partial modal selectors (from Views/Shared/AddUpdateGoals.cshtml)
    const $goalModal = $("#addEditGoalModal");
    const $openModalButton = $("#openModalButton");
    const $closeModalButton = $("#closeModalButton");
    const $goalForm = $("#goalForm");

    // Open goalModal for create (use partial view already rendered in layout/page)
    $openModalButton.on("click", function () {
        // reset form and state
        $goalForm[0].reset();
        $goalForm.data('action-goal-form', 'create');
        $goalForm.removeData('goalid');

        $("#form-label").html('Add Goal');
        $("#goalIdContainer").addClass('hidden');

        // show modal
        $goalModal.removeClass("hidden").fadeIn();

        // initialize editor after modal is visible
        handleModalOpen();
    });

    const handleModalOpen = function () {
        // wait a bit for modal to be visible then init/refresh editor
        setTimeout(() => {
            setupTinyMCE('textarea[name="goalDescriptions"]', tinymceConfig);
        }, 100);
    }

    // Open goalModal for edit
    $(document).on("click", ".editGoalBtn", function () {
        const id = $(this).data("id");
        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        $("#form-label").html('Edit Goal');
        $("#goalIdContainer").removeClass('hidden');
        $goalForm.data('action-goal-form', 'edit');
        $goalForm.data('goalid', id);

        // show modal
        $goalModal.removeClass("hidden").fadeIn();

        // ensure tinymce exists for the textarea
        handleModalOpen();

        // load data and populate fields
        $.ajax({
            url: `/Goals/GetById/${id}`,
            method: "GET",
            success: function (response) {
                if (!response || !response.status) {
                    showErrorNotification(response?.message || "Failed to load goal data!");
                    return;
                }

                const data = response.data || {};

                $("#goalId").val(data.id || "");
                $("#goalName").val(data.name || "");
                $("#goalDescription").val(data.description || "");
                $("#goalPriority").val(data.priority ?? 0);
                $("#startDate").val(data.startDate || "");
                $("#endDate").val(data.endDate || "");
                $("#goalStatus").val(data.goalStatus ?? goalStatusEnum.notStarted);

                // set radio for start option
                const startOption = data.startOptionType ?? startOptionValues.manual;
                $goalModal.find(`input[name="goalStart"][value="${startOption}"]`).prop('checked', true);

                // show/hide start date container based on value
                $("#startDateContainer").toggleClass('hidden', +startOption !== startOptionValues.scheduled);

                // set tinymce content (if editor initialized)
                if (typeof tinymce !== "undefined") {
                    // ensure editor is initialized for the textarea id
                    setTimeout(() => {
                        const editor = tinymce.get($("#goalDescription").attr('id'));
                        if (editor) {
                            editor.setContent(data.description || "");
                        } else {
                            // attempt to find any editor and set content
                            const inst = tinymce.editors && tinymce.editors[0];
                            if (inst) inst.setContent(data.description || "");
                        }
                    }, 200);
                }
            },
            error: function (xhr, status, error) {
                showErrorNotification(error || "Failed to load goal data!");
                console.error("Error:", error);
            }
        });
    });

    // Close modal (button)
    $closeModalButton.on('click', function () {
        $goalModal.fadeOut();
    });

    // Close when clicking overlay
    $goalModal.on('click', function (e) {
        if (e.target === this) {
            $(this).fadeOut();
        }
    });

    // Toggle startDateContainer when start option changes
    $(document).on('change', 'input[name="goalStart"]', function () {
        const val = $(this).val();
        $("#startDateContainer").toggleClass('hidden', +val !== startOptionValues.scheduled);
    });

    // Handle form submission using AJAX (use same pattern as notesScript.js)
    $goalForm.on('submit', function (e) {
        e.preventDefault();

        // if tinymce active, ensure textarea is updated
        if (typeof tinymce !== "undefined") {
            try { tinymce.triggerSave(); } catch (ex) { /* ignore */ }
        }

        const action = $(this).data('action-goal-form') || 'create';
        const goalId = $(this).data('goalid');

        const data = {
            Id: $("#goalId").val(),
            Name: $("#goalName").val(),
            Description: $("#goalDescription").val(),
            Priority: $("#goalPriority").val(),
            StartOptionType: $("input[name='goalStart']:checked").val(),
            StartDate: $("#startDate").val(),
            EndDate: $("#endDate").val(),
            GoalStatus: $("#goalStatus").val()
        };

        if (action.toString().toLowerCase() === 'create') {
            $.ajax({
                url: "/Goals/Create",
                method: "POST",
                data: data,
                success: function (response) {
                    if (response?.status) {
                        showSuccessNotification(response.message || "Goal created successfully!");
                        setTimeout(() => {
                            $goalModal.fadeOut();
                            location.reload();
                        }, 500);
                    } else {
                        showErrorNotification(response?.message || "Goal creation failed!");
                    }
                },
                error: function (xhr, status, error) {
                    showErrorNotification(error || "Goal creation failed!");
                    console.error("Error:", error);
                }
            });
        } else if (action.toString().toLowerCase() === 'edit' && goalId) {
            data.Id = goalId;
            $.ajax({
                url: `/Goals/Edit`,
                method: "PUT",
                data: data,
                success: function (response) {
                    if (response?.status) {
                        showSuccessNotification(response.message || "Goal updated successfully!");
                        setTimeout(() => {
                            $goalModal.fadeOut();
                            location.reload();
                        }, 500);
                    } else {
                        showErrorNotification(response?.message || "Goal update failed!");
                    }
                },
                error: function (xhr, status, error) {
                    showErrorNotification(error || "Goal update failed!");
                    console.error("Error:", error);
                }
            });
        } else {
            showErrorNotification("Invalid Action!");
        }
    });

    const dataTableObject = (url, renderCallBack, pageLengthKey) => {

        const pageLength = localStorage.getItem(pageLengthKey);


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
                    name: "Priority",
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
            lengthMenu: [[5, 10, 50, 100, 250, 500, -1], [5, 10, 50, 100, 250, 500, "All"]],
            pageLength: pageLength ? pageLength : defaultPageLength
        }
    }

    // Running Goal Data...
    function loadRunningGoalData() {
        let url = "/Goals/GetAllRunningGoals";

        function renderCallBack(data, type, row) {
            return `
             <div class="flex flex-nowrap items-center gap-1 justify-start sm:justify-center">

                <a href="/Tasks/Create?goalId=${row.id}" 
                   class="group flex h-8 w-8 items-center justify-center rounded bg-purple-100 text-purple-600 hover:bg-purple-600 hover:text-white transition-colors duration-200"
                   title="Add Task">
                    <i class="fas fa-plus text-sm"></i>
                </a>

                <button data-id="${row.id}" 
                        class="editGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-amber-100 text-amber-600 hover:bg-amber-500 hover:text-white transition-colors duration-200"
                        title="Edit Goal">
                    <i class="fas fa-edit text-sm"></i>
                </button>

                <button data-id="${row.id}" 
                        class="markAsComplete group flex h-8 w-8 items-center justify-center rounded bg-green-100 text-green-600 hover:bg-green-600 hover:text-white transition-colors duration-200"
                        title="Mark as Complete">
                    <i class="fas fa-check text-sm"></i>
                </button>

                <button data-id="${row.id}" 
                        class="deleteGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-red-100 text-red-600 hover:bg-red-600 hover:text-white transition-colors duration-200"
                        title="Delete Goal">
                    <i class="fas fa-trash text-sm"></i>
                </button>

            </div>
            `;
        }

        if (runningGoalDataTable?.length) {

            runningGoalDataTable.DataTable(dataTableObject(url, renderCallBack, pageLengthValue.runningGoal));

            runningGoalDataTable.on('length.dt', function (e, settings, len) {
                localStorage.setItem(pageLengthValue.runningGoal, len);
            });

        }
    }

    // Completed Goal data....
    function loadCompletedGoalData() {
        let url = "/Goals/GetAllCompletedGoals";

        function renderCallBack(data, type, row) {
            return `
                <div class="flex flex-nowrap items-center gap-1 justify-start sm:justify-center">

                    <button data-id="${row.id}" 
                            class="editGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-amber-100 text-amber-600 hover:bg-amber-500 hover:text-white transition-colors duration-200"
                            title="Edit Goal">
                        <i class="fas fa-edit text-sm"></i>
                    </button>

                    <button data-id="${row.id}" 
                            class="moveToRunning group flex h-8 w-8 items-center justify-center rounded bg-blue-100 text-blue-600 hover:bg-blue-600 hover:text-white transition-colors duration-200"
                            title="Start / Move to Running">
                        <i class="fas fa-play text-xs pl-0.5"></i>
                    </button>

                    <button data-id="${row.id}" 
                            class="deleteGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-red-100 text-red-600 hover:bg-red-600 hover:text-white transition-colors duration-200"
                            title="Delete Goal">
                        <i class="fas fa-trash text-sm"></i>
                    </button>

                </div>

            `;
        }

        if (completedGoalDataTable?.length) {

            completedGoalDataTable.DataTable(dataTableObject(url, renderCallBack, pageLengthValue.completedGoal));

            completedGoalDataTable.on('length.dt', function (e, settings, len) {
                localStorage.setItem(pageLengthValue.completedGoal, len);
            });

        }
    }

    // NotStarted
    function loadNotStartedGoalData() {
        let url = "/Goals/GetAllNotStartedGoals";

        function renderCallBack(data, type, row) {
            return `
                <div class="flex flex-nowrap items-center gap-1 justify-start sm:justify-center">

                    <a href="/Tasks/Create?goalId=${row.id}" 
                       class="group flex h-8 w-8 items-center justify-center rounded bg-purple-100 text-purple-600 hover:bg-purple-600 hover:text-white transition-colors duration-200"
                       title="Add Task">
                        <i class="fas fa-plus text-sm"></i>
                    </a>

                    <button data-id="${row.id}" 
                            class="editGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-amber-100 text-amber-600 hover:bg-amber-500 hover:text-white transition-colors duration-200"
                            title="Edit Goal">
                        <i class="fas fa-edit text-sm"></i>
                    </button>

                    <button data-id="${row.id}" 
                            class="moveToRunning group flex h-8 w-8 items-center justify-center rounded bg-blue-100 text-blue-600 hover:bg-blue-600 hover:text-white transition-colors duration-200"
                            title="Start / Move to Running">
                        <i class="fas fa-play text-xs pl-0.5"></i>
                    </button>

                    <button data-id="${row.id}" 
                            class="deleteGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-red-100 text-red-600 hover:bg-red-600 hover:text-white transition-colors duration-200"
                            title="Delete Goal">
                        <i class="fas fa-trash text-sm"></i>
                    </button>

                </div>

            `;
        }

        if (notStartedGoalDataTable?.length) {

            notStartedGoalDataTable.DataTable(dataTableObject(url, renderCallBack, pageLengthValue.notStartedGoal));

            notStartedGoalDataTable.on('length.dt', function (e, settings, len) {
                localStorage.setItem(pageLengthValue.notStartedGoal, len);
            });

        }
    }

    // Ended
    function loadEndedGoalData() {
        let url = "/Goals/GetAllEndedGoals";

        function renderCallBack(data, type, row) {
            return `
               <div class="flex flex-nowrap items-center gap-1 justify-start sm:justify-center">

                    <button data-id="${row.id}" 
                            class="editGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-amber-100 text-amber-600 hover:bg-amber-500 hover:text-white transition-colors duration-200"
                            title="Edit Goal">
                        <i class="fas fa-edit text-sm"></i>
                    </button>

                    <button data-id="${row.id}" 
                            class="moveToRunning group flex h-8 w-8 items-center justify-center rounded bg-blue-100 text-blue-600 hover:bg-blue-600 hover:text-white transition-colors duration-200"
                            title="Move to Running">
                        <i class="fas fa-play text-xs pl-0.5"></i>
                    </button>

                    <button data-id="${row.id}" 
                            class="deleteGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-red-100 text-red-600 hover:bg-red-600 hover:text-white transition-colors duration-200"
                            title="Delete Goal">
                        <i class="fas fa-trash text-sm"></i>
                    </button>

                </div>
            `;
        }

        if (endedGoalDataTable?.length) {

            endedGoalDataTable.DataTable(dataTableObject(url, renderCallBack, pageLengthValue.endedGoal));

            endedGoalDataTable.on('length.dt', function (e, settings, len) {
                localStorage.setItem(pageLengthValue.endedGoal, len);
            });

        }
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
                GoalStatus: goalStatusEnum.completed
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

    /* ----------------------- TinyMCE config + helper ----------------------- */

    const tinymceConfig = {
        menubar: false,
        height: 800,
        plugins: 'link lists code codesample media image table autoresize',
        toolbar: [
            'undo redo | blocks fontfamily fontsize | bold italic underline forecolor backcolor | blockquote codesample code',
            '| alignleft aligncenter alignright | bullist numlist',
            '| link image media | table | removeformat'
        ].join(' '),

        font_family_formats: `
      Arial=arial,helvetica,sans-serif;
      Courier New=courier new,courier,monospace;
      Georgia=georgia,palatino;
      Times New Roman=times new roman,times;
      Tahoma=tahoma,arial,helvetica,sans-serif;
      Verdana=verdana,geneva;
      Roboto=roboto,sans-serif
    `,
        fontsize_formats: '10px 12px 14px 16px 18px 24px 36px 48px',
        content_style: `
          body { font-family: "Segoe UI", sans-serif; font-size:14px; line-height:1.6; color:#333; }
          h1,h2,h3,h4,h5,h6 { font-weight:600; margin:1em 0 0.5em; }
          blockquote { border-left:3px solid #ccc; padding-left:10px; color:#666; font-style:italic; }
          pre, code { background:#f4f4f4; border-radius:4px; padding:4px 6px; font-family:"Fira Code", monospace; }
          pre { padding:10px; overflow:auto; }
          table { border-collapse: collapse; width:100%; margin:1em 0; }
          table, th, td { border:1px solid #ddd; }
          th, td { padding:8px; text-align:left; }
          a { color:#2563eb; text-decoration:underline; }
          img { max-width:100%; height:auto; border-radius:6px; }
        `,
        automatic_uploads: true,
        images_upload_url: '/uploads/image',
        file_picker_types: 'image file media',
        codesample_global_prismjs: true,
        codesample_languages: [
            { text: 'JavaScript', value: 'javascript' },
        ],
        file_picker_callback: function (cb, value, meta) {
            const input = document.createElement('input');
            input.setAttribute('type', 'file');

            if (meta.filetype === 'image') {
                input.setAttribute('accept', 'image/*');
            }

            input.onchange = function () {
                const file = this.files[0];
                const formData = new FormData();
                formData.append('file', file);

                const targetUrl =
                    meta.filetype === 'image' ? '/uploads/image' :
                        meta.filetype === 'media' ? '/uploads/media' :
                            '/uploads/file';

                fetch(targetUrl, {
                    method: 'POST',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    body: formData
                })
                    .then(res => res.ok ? res.json() : Promise.reject(res))
                    .then(json => {
                        cb(json.location, { text: file.name, title: file.name, alt: file.name });
                    })
                    .catch(() => alert('Upload failed. Please try again.'));
            };
            input.click();
        },
        convert_urls: false,
        media_live_embeds: true,
    };

    function setupTinyMCE(selector, config) {
        const $targetElement = $(selector);
        if (!$targetElement.length) {
            console.warn(`TinyMCE: Target element not found for selector: ${selector}`);
            return;
        }

        let editorId = $targetElement.attr('id');
        if (!editorId) {
            editorId = 'tinymce-dynamic-' + Math.random().toString(36).substring(2, 9);
            $targetElement.attr('id', editorId);
        }

        const existingEditor = tinymce.get(editorId);

        if (existingEditor) {
            try {
                existingEditor.execCommand('mceFocus', false, editorId);
                // deprecated repaint but harmless if available
                try { existingEditor.execCommand('mceRepaint'); } catch (e) { }
                const content = existingEditor.getContent();
                existingEditor.setContent(content);
                existingEditor.fire && existingEditor.fire('ResizeEditor');
            } catch (e) {
                console.error("TinyMCE refresh failed:", e);
            }
        } else {
            const fullConfig = {
                ...config,
                selector: `#${editorId}`,
                setup: function (editor) {
                    editor.on('init', function () {
                        console.log(`TinyMCE instance '${editorId}' initialized.`);
                    });
                    $(document).on('focusin', function (e) {
                        if ($(e.target).closest('.tox-tinymce, .tox-tinymce-aux').length) {
                            e.stopImmediatePropagation();
                        }
                    });
                }
            };
            tinymce.init(fullConfig);
        }
    }

});