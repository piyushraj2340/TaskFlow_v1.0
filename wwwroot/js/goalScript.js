$(document).ready(function () {

    /* ----------------------- Constants & Enums ----------------------- */
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

    let initialFormState = ""; // Stores the stringified version of the form
    let checkDirtyEnabled = false; // Prevents tracking before the form is ready

    // Captures all current form values into a single string for comparison
    const getFormSnapshot = () => {
        // Get standard form fields (serialized)
        let state = $goalForm.serialize();

        // Get TinyMCE content if it exists
        if (typeof tinymce !== "undefined" && tinymce.get("goalDescription")) {
            state += tinymce.get("goalDescription").getContent();
        }

        // 3. Get Select2 specific values
        state += $('#parentGoalId').val() || "";

        return state;
    };

    const startTrackingChanges = () => {
        // Small timeout to ensure Select2 and TinyMCE are fully rendered
        setTimeout(() => {
            initialFormState = getFormSnapshot();
            checkDirtyEnabled = true;
        }, 500);
    };

    const stopTrackingChanges = () => {
        checkDirtyEnabled = false;
        initialFormState = "";
    };

    const isActuallyDirty = () => {
        if (!checkDirtyEnabled) return false;
        return initialFormState !== getFormSnapshot();
    };

    // Table Selectors
    const runningGoalDataTable = $('#treeTableRunning');
    const completedGoalDataTable = $('#treeTableCompleted');
    const notStartedGoalDataTable = $('#treeTableNotStarted');
    const endedGoalDataTable = $('#treeTableEnded');

    // Modal Selectors
    const $goalModal = $("#addEditGoalModal");
    const $goalForm = $("#goalForm");
    const $parentGoalSelect = $('#parentGoalId');
    const $existingGoalSearch = $('#existingGoalSearch');
    const $subGoalModeContainer = $('#subGoalModeContainer');
    const $existingSearchContainer = $('#existingGoalSearchContainer');
    const $closeModalButton = $("#closeModalButton");
    const $cancelModalButton = $("#cancelModalButton");


    /* ----------------------- Initialization ----------------------- */

    // Parent Goal Search (Standard)
    if ($parentGoalSelect.length) {
        $parentGoalSelect.select2({
            dropdownParent: $goalModal,
            placeholder: 'Search for a parent goal...',
            allowClear: true,
            ajax: {
                url: '/Goals/SearchGoalNameByName',
                method: "get",
                dataType: 'json',
                delay: 250,
                data: (params) => ({ searchQuery: params.term }),
                processResults: (response) => ({
                    results: response.data.map(item => ({ id: item.id, text: item.name }))
                }),
                cache: true
            },
            minimumInputLength: 1
        });
    }

    // Existing Goal Search (For linking sub-goals)
    if ($existingGoalSearch.length) {
        $existingGoalSearch.select2({
            dropdownParent: $goalModal,
            placeholder: 'Select a goal to link as sub-goal...',
            allowClear: true,
            ajax: {
                url: '/Goals/SearchGoalNameByName',
                dataType: 'json',
                delay: 250,
                data: (params) => ({ searchQuery: params.term }),
                processResults: (response) => ({
                    results: response.data.map(item => ({ id: item.id, text: item.name }))
                }),
                cache: true
            },
            minimumInputLength: 1
        });
    }

    /* --- Closure Prevention --- */

    // Prevent Modal Close (via X button or Cancel)
    $(document).on('click', '#closeModalButton, #cancelModalButton', function (e) {
        if (isActuallyDirty()) {
            Swal.fire({
                title: 'Unsaved Changes',
                text: "You have unsaved changes. Are you sure you want to close?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#4f46e5', // Indigo
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Yes, discard changes',
                cancelButtonText: 'No, stay here'
            }).then((result) => {
                if (result.isConfirmed) {
                    stopTrackingChanges(); // Reset flag
                    $goalModal.fadeOut();
                }
            });
        } else {
            stopTrackingChanges();
            $goalModal.fadeOut();
        }
    });

    // Prevent Modal Close (via Clicking outside/Overlay)
    $goalModal.on('click', function (e) {
        if (e.target === this) {
            if (isActuallyDirty()) {
                // Trigger the same SweetAlert as above or just stop the click
                $('#closeModalButton').trigger('click');
            } else {
                $(this).fadeOut();
            }
        }
    });

    // Prevent Page Reload / Navigation / Tab Close
    window.addEventListener('beforeunload', function (e) {
        if (isActuallyDirty()) {
            // Standard browser prompt (Custom text is ignored by modern browsers for security)
            e.preventDefault();
            e.returnValue = '';
        }
    });


    $(document).on("click", "#openModalButton, #addSubGoalButton", function () {
        resetModalState();
        startTrackingChanges();
    });

    /* ----------------------- Modal Trigger Logic ----------------------- */

    // Standard "Add New Goal" (Dashboard)
    $(document).on("click", "#openModalButton", function () {
        resetModalState();
        $goalForm.data('action-goal-form', 'create');
        $("#form-label").html('Add New Goal');
        $subGoalModeContainer.addClass('hidden');
        $existingSearchContainer.addClass('hidden');
        $parentGoalSelect.prop('disabled', false).val(null).trigger('change');
        $goalModal.removeClass("hidden").fadeIn();
        handleModalOpen();
    });

    // "Edit Goal" (Table Rows - Delegated)
    $(document).on("click", ".editGoalBtn", function () {
        const id = $(this).data("id");
        if (!id) return showErrorNotification("Missing Id parameters!");

        resetModalState();
        $goalForm.data('action-goal-form', 'edit').data('goalid', id);
        $("#form-label").html('Edit Goal');
        $("#goalIdContainer").removeClass('hidden');
        $subGoalModeContainer.addClass('hidden');
        $parentGoalSelect.prop('disabled', false);

        $goalModal.removeClass("hidden").fadeIn();
        handleModalOpen();

        $.get(`/Goals/GetById/${id}`, function (response) {
            if (response?.status) {
                populateGoalForm(response.data);

                startTrackingChanges();
            };
        });
    });

    // "Add Sub-Goal" (Goal Details Page)
    $(document).on('click', '#addSubGoalButton', function () {
        const parentId = $(this).data('parent-id');
        const parentName = $(this).data('parent-name');

        resetModalState();
        $goalForm.data('action-goal-form', 'create');
        $("#form-label").html('Add Sub-goal to: ' + parentName);
        $subGoalModeContainer.removeClass('hidden');
        $('input[name="subGoalMode"][value="new"]').prop('checked', true).trigger('change');

        if ($parentGoalSelect.length) {
            var newOption = new Option(parentName, parentId, true, true);
            $parentGoalSelect.empty().append(newOption).trigger('change');
            $parentGoalSelect.prop('disabled', true);
        }

        $goalModal.removeClass("hidden").fadeIn();
        handleModalOpen();
    });

    /* ----------------------- Sub-Goal Logic ----------------------- */

    $(document).on('change', 'input[name="subGoalMode"]', function () {
        const mode = $(this).val();
        if (mode === 'existing') {
            $existingSearchContainer.removeClass('hidden');
            $goalForm.data('action-goal-form', 'edit');
            $('#goalName').prop('readonly', true).addClass('bg-gray-100');
        } else {
            $existingSearchContainer.addClass('hidden');
            $goalForm.data('action-goal-form', 'create');
            $('#goalName').prop('readonly', false).removeClass('bg-gray-100');

            // Keep Parent Locked
            const pId = $parentGoalSelect.val();
            const pText = $parentGoalSelect.find("option:selected").text();
            $goalForm[0].reset();
            $('input[name="subGoalMode"][value="new"]').prop('checked', true);
            if (pId) {
                var newOption = new Option(pText, pId, true, true);
                $parentGoalSelect.empty().append(newOption).trigger('change');
            }
        }
    });

    $existingGoalSearch.on('select2:select', function (e) {
        const goalId = e.params.data.id;
        $goalForm.data('goalid', goalId);
        $.get(`/Goals/GetById/${goalId}`, function (response) {
            if (response?.status) populateGoalForm(response.data);
        });
    });

    /* ----------------------- DataTables Actions & Renderers ----------------------- */

    const runningActions = (row) => `
        <div class="flex flex-nowrap items-center gap-1 justify-start">
            <a href="/Tasks/Create?goalId=${row.id}" class="group flex h-8 w-8 items-center justify-center rounded bg-purple-100 text-purple-600 hover:bg-purple-600 hover:text-white transition" title="Add Task"><i class="fas fa-plus text-sm"></i></a>
            <button data-id="${row.id}" class="editGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-amber-100 text-amber-600 hover:bg-amber-500 hover:text-white transition" title="Edit Goal"><i class="fas fa-edit text-sm"></i></button>
            <button data-id="${row.id}" class="markAsComplete group flex h-8 w-8 items-center justify-center rounded bg-green-100 text-green-600 hover:bg-green-600 hover:text-white transition" title="Mark as Complete"><i class="fas fa-check text-sm"></i></button>
            <button data-id="${row.id}" class="deleteGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-red-100 text-red-600 hover:bg-red-600 hover:text-white transition" title="Delete Goal"><i class="fas fa-trash text-sm"></i></button>
        </div>`;

    const notStartedActions = (row) => `
        <div class="flex flex-nowrap items-center gap-1 justify-start">
            <a href="/Tasks/Create?goalId=${row.id}" class="group flex h-8 w-8 items-center justify-center rounded bg-purple-100 text-purple-600 hover:bg-purple-600 hover:text-white transition" title="Add Task"><i class="fas fa-plus text-sm"></i></a>
            <button data-id="${row.id}" class="editGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-amber-100 text-amber-600 hover:bg-amber-500 hover:text-white transition" title="Edit Goal"><i class="fas fa-edit text-sm"></i></button>
            <button data-id="${row.id}" class="moveToRunning group flex h-8 w-8 items-center justify-center rounded bg-blue-100 text-blue-600 hover:bg-blue-600 hover:text-white transition" title="Start Goal"><i class="fas fa-play text-xs pl-0.5"></i></button>
            <button data-id="${row.id}" class="deleteGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-red-100 text-red-600 hover:bg-red-600 hover:text-white transition" title="Delete Goal"><i class="fas fa-trash text-sm"></i></button>
        </div>`;

    const archiveActions = (row) => `
        <div class="flex flex-nowrap items-center gap-1 justify-start">
            <button data-id="${row.id}" class="editGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-amber-100 text-amber-600 hover:bg-amber-500 hover:text-white transition" title="Edit Goal"><i class="fas fa-edit text-sm"></i></button>
            <button data-id="${row.id}" class="moveToRunning group flex h-8 w-8 items-center justify-center rounded bg-blue-100 text-blue-600 hover:bg-blue-600 hover:text-white transition" title="Move to Running"><i class="fas fa-play text-xs pl-0.5"></i></button>
            <button data-id="${row.id}" class="deleteGoalBtn group flex h-8 w-8 items-center justify-center rounded bg-red-100 text-red-600 hover:bg-red-600 hover:text-white transition" title="Delete Goal"><i class="fas fa-trash text-sm"></i></button>
        </div>`;

    const returnStatusBadge = (status) => {
        let classes = "";
        let text = "";

        switch (parseInt(status)) {
            case goalStatusEnum.notStarted:
                classes = "bg-slate-100 text-slate-600 border-slate-200";
                text = "Not Started";
                break;
            case goalStatusEnum.running:
                classes = "bg-blue-100 text-blue-700 border-blue-200";
                text = "Running";
                break;
            case goalStatusEnum.completed:
                classes = "bg-green-100 text-green-700 border-green-200";
                text = "Completed";
                break;
            case goalStatusEnum.ended:
                classes = "bg-red-100 text-red-700 border-red-200";
                text = "Ended";
                break;
            default:
                return "";
        }

        return `<span class="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium border ${classes}">${text}</span>`;
    };

    const dataTableObject = (url, actionRenderer, pageLengthKey) => {
        const pageLength = localStorage.getItem(pageLengthKey);
        return {
            ajax: { url, type: "POST" },
            responsive: true, processing: true, serverSide: true, filter: true,
            columns: [
                {
                    data: "name", name: "Name",
                    render: function (data, type, row) {
                        const hasChildren = row.subGoalsCount > 0;
                        const toggle = hasChildren ? `<i class="fas fa-chevron-right tree-toggle mr-2 text-gray-400 cursor-pointer" data-id="${row.id}" data-status="${row.goalStatus}"></i>` : `<span class="mr-6"></span>`;

                        const countBadge = row.subGoalsCount > 0 ? ` <span class="text-gray-400 text-sm">(${row.subGoalsCount})</span>` : '';

                        return `
                            <div class="flex items-center">
                                ${toggle}
                                <a href="/Goals/Details/${row.id}" class="text-blue-600 hover:underline font-medium">${data}</a>
                                ${countBadge}
                            </div>`;
                    }
                },
                { data: "endDate", render: (data) => `<span class="inline-flex items-center px-3 py-1 text-xs font-semibold text-white bg-gray-700 rounded-full">${formatShortDate(data)}</span>` },
                { data: "priority", render: (data) => returnPriorityBadge(data) },
                { data: null, orderable: false, render: (data, type, row) => actionRenderer(row) }
            ],
            "order": [[0, 'asc']],
            lengthMenu: [[5, 10, 50, 100, -1], [5, 10, 50, 100, "All"]],
            pageLength: pageLength ? pageLength : defaultPageLength,
            drawCallback: () => $('.dataTables_paginate').addClass('mt-4')
        };
    };

    /* ----------------------- Tree Table Expansion ----------------------- */

    function renderGoalRow(goal, level = 0, parentStatus = null) {
        const hasChildren = goal.subGoalsCount > 0;
        const paddingClass = `level-${Math.min(level, 4)}`;
        const toggleIcon = hasChildren ? `<i class="fas fa-chevron-right tree-toggle mr-2 text-gray-400 hover:text-gray-600 cursor-pointer" data-status="${parentStatus}" data-id="${goal.id}" data-level="${level}"></i>` : `<span class="mr-6"></span>`;
        let statusBadge = "";

        if (level > 0 && parentStatus !== null && parseInt(goal.goalStatus) !== parseInt(parentStatus)) {
            statusBadge = returnStatusBadge(goal.goalStatus);
        }

        let actions = "";
        if (goal.goalStatus == goalStatusEnum.running) actions = runningActions(goal);
        else if (goal.goalStatus == goalStatusEnum.notStarted) actions = notStartedActions(goal);
        else actions = archiveActions(goal);

        return `
            <tr data-id="${goal.id}" data-level="${level}" class="bg-white border-b border-gray-100 hover:bg-gray-50 transition-colors">
                <td class="px-6 py-4 whitespace-nowrap ${paddingClass}">
                    <div class="flex items-center">
                        ${toggleIcon}
                        <a href="/Goals/Details/${goal.id}" class="text-blue-600 hover:underline font-medium">${goal.name}</a>
                        ${statusBadge}
                    </div>            
                </td>
                <td class="px-6 py-4 whitespace-nowrap"><span class="inline-flex items-center px-3 py-1 text-xs font-semibold text-white bg-gray-700 rounded-full">${formatShortDate(goal.endDate)}</span></td>
                <td class="px-6 py-4 whitespace-nowrap">${returnPriorityBadge(goal.priority)}</td>
                <td class="px-6 py-4 whitespace-nowrap">${actions}</td>
            </tr>`;
    }

    $(document).on('click', '.tree-toggle', function (e) {
        e.preventDefault();
        const $icon = $(this);
        const $row = $icon.closest('tr');
        const parentId = $icon.data('id');
        const currentLevel = parseInt($row.data('level') || 0);
        const parentStatus = $icon.data('status');

        if ($icon.hasClass('fa-chevron-down')) {
            $icon.removeClass('fa-chevron-down').addClass('fa-chevron-right');
            let $nextRow = $row.next();
            while ($nextRow.length && parseInt($nextRow.data('level')) > currentLevel) {
                let $temp = $nextRow.next(); $nextRow.remove(); $nextRow = $temp;
            }
        } else {
            $icon.removeClass('fa-chevron-right').addClass('fa-chevron-down');
            $.get(`/Goals/GetChildGoals?parentId=${parentId}`, function (response) {
                if (response?.data) {
                    let html = '';
                    response.data.forEach(child => html += renderGoalRow(child, currentLevel + 1, parentStatus));
                    $row.after(html);
                }
            });
        }
    });

    /* ----------------------- Form Helpers & Submission ----------------------- */

    function resetModalState() {
        $goalForm[0].reset();
        $goalForm.removeData('goalid');
        $("#goalId").val("");
        $("#goalIdContainer").addClass('hidden');
        $('#goalName').prop('readonly', false).removeClass('bg-gray-100');
        if (typeof tinymce !== "undefined") tinymce.get("goalDescription")?.setContent("");
    }

    function populateGoalForm(data) {
        $("#goalId").val(data.id || "");
        $("#goalName").val(data.name || "");
        $("#goalPriority").val(data.priority ?? 0);
        $("#startDate").val(data.startDate || "");
        $("#endDate").val(data.endDate || "");
        $("#goalStatus").val(data.goalStatus ?? goalStatusEnum.notStarted);
        $("#goalDescription").val(data.description || "");

        // --- FIX: Handle Parent Goal for Select2 ---
        if ($parentGoalSelect.length) {
            if (data.parentId && data.parentName) {
                // Create a new option and select it
                var newOption = new Option(data.parentName, data.parentId, true, true);
                $parentGoalSelect.empty().append(newOption).trigger('change');
            } else {
                // Clear if there is no parent
                $parentGoalSelect.val(null).trigger('change');
            }
        }
        // -------------------------------------------

        const startOpt = data.startOptionType ?? startOptionValues.manual;
        $goalModal.find(`input[name="goalStart"][value="${startOpt}"]`).prop('checked', true);
        $("#startDateContainer").toggleClass('hidden', +startOpt !== startOptionValues.scheduled);

        if (typeof tinymce !== "undefined") {
            setTimeout(() => {
                tinymce.get("goalDescription")?.setContent(data.description || "");
            }, 200);
        }
    }

    $goalForm.on('submit', function (e) {
        e.preventDefault();

        // 1. DISABLE TRACKING IMMEDIATELY
        // This stops the "Unsaved Changes" prompt from triggering during save
        stopTrackingChanges();

        if (typeof tinymce !== "undefined") tinymce.triggerSave();

        // Temporarily enable for serialization
        $parentGoalSelect.prop('disabled', false);

        const action = $(this).data('action-goal-form') || 'create';
        const data = {
            Id: $("#goalId").val() || $(this).data('goalid'),
            Name: $("#goalName").val(),
            Description: $("#goalDescription").val(),
            Priority: $("#goalPriority").val(),
            StartOptionType: $("input[name='goalStart']:checked").val(),
            StartDate: $("#startDate").val(),
            EndDate: $("#endDate").val(),
            GoalStatus: $("#goalStatus").val(),
            ParentId: $parentGoalSelect.val()
        };

        const url = action.toLowerCase() === 'create' ? "/Goals/Create" : "/Goals/Edit";
        const method = action.toLowerCase() === 'create' ? "POST" : "PUT";

        $.ajax({
            url: url, method: method, data: data,
            success: function (response) {
                if (response?.status) {
                    showSuccessNotification(response.message);

                    // 2. Remove the browser listener so refresh doesn't prompt
                    window.removeEventListener('beforeunload', function (e) {
                        e.preventDefault();
                        e.returnValue = '';
                    });

                    setTimeout(() => location.reload(), 500);
                } else {
                    showErrorNotification(response?.message);
                    
                    // 3. RE-ENABLE TRACKING IF SAVE FAILED
                    // If there's a validation error, we want to keep tracking 
                    // so the user doesn't lose their corrections!
                    if (!$subGoalModeContainer.hasClass('hidden')) $parentGoalSelect.prop('disabled', true);
                    startTrackingChanges();                }
            }
        });
    });

    /* -----------------------  UI Utilities ----------------------- */

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
        setup: function (editor) {
            editor.on('Change KeyUp', function (e) {
                setFormDirty();
            });
            editor.on('init', function () {
                // Ensure initial content doesn't trigger dirty flag
                editor.setContent($('#goalDescription').val() || "");
            });
        }
    };


    const handleModalOpen = (initialContent = "") => {
        // 1. Give the DOM a moment to be visible
        setTimeout(() => {
            setupTinyMCE('textarea[name="goalDescriptions"]', tinymceConfig, initialContent);
        }, 100);
    };
    function setupTinyMCE(selector, config, initialContent) {
        const $target = $(selector);
        if (!$target.length) return;

        let editorId = $target.attr('id') || 'tmce-' + Math.random().toString(36).substring(2, 9);
        $target.attr('id', editorId);

        const editor = tinymce.get(editorId);

        if (editor) {
            // 2. If editor exists, just update the content and reset dirty flag
            editor.setContent(initialContent || "");
            editor.undoManager.clear(); // Prevents "Undo" from bringing back old data
        } else {
            // 3. If editor doesn't exist, initialize it
            tinymce.init({
                ...config,
                selector: `#${editorId}`,
                setup: (ed) => {
                    ed.on('init', () => {
                        ed.setContent(initialContent || $target.val() || "");
                    });
                    // Ensure the dirty check logic works with TinyMCE
                    ed.on('Change KeyUp', function () {
                        tinymce.triggerSave(); // Keep the hidden textarea in sync
                    });
                }
            });
        }
    }

    function initTable(tableSelector, url, actionRenderer, storageKey) {
        if (tableSelector?.length) {
            tableSelector.DataTable(dataTableObject(url, actionRenderer, storageKey));
            tableSelector.on('length.dt', (e, s, len) => localStorage.setItem(storageKey, len));
        }
    }

    // Initialize Root Tables
    initTable(runningGoalDataTable, "/Goals/GetAllRunningGoals", runningActions, pageLengthValue.runningGoal);
    initTable(completedGoalDataTable, "/Goals/GetAllCompletedGoals", archiveActions, pageLengthValue.completedGoal);
    initTable(notStartedGoalDataTable, "/Goals/GetAllNotStartedGoals", notStartedActions, pageLengthValue.notStartedGoal);
    initTable(endedGoalDataTable, "/Goals/GetAllEndedGoals", archiveActions, pageLengthValue.endedGoal);


    /* ----------------------- Status & Delete Action Handlers ----------------------- */

    // 1. Handle Delete Goal
    $(document).on("click", ".deleteGoalBtn", function () {
        const id = $(this).data("id");

        Swal.fire({
            title: 'Are you sure?',
            text: "This will permanently delete the goal and its sub-goals!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#ef4444', // Red
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: `/Goals/DeleteGoal/${id}`,
                    method: "DELETE",
                    success: function (response) {
                        if (response?.status) {
                            showSuccessNotification(response.message);
                            setTimeout(() => location.reload(), 500);
                        } else {
                            showErrorNotification(response?.message || "Delete failed.");
                        }
                    },
                    error: () => showErrorNotification("An error occurred while deleting the goal.")
                });
            }
        });
    });

    // 2. Handle Status Changes (Mark as Complete / Move to Running)
    // 1. Handle Move to Running (Start Goal)
    $(document).on("click", ".moveToRunning", function () {
        const goalId = $(this).data("id");

        // Check for child goals before proceeding
        $.get(`/Goals/GetChildGoals?parentId=${goalId}`, function (response) {
            const subGoals = (response && response.status) ? response.data : [];

            if (subGoals.length > 0) {
                handleMultiLevelStatusChange(goalId, subGoals, goalStatusEnum.running, "Running");
            } else {
                // Standard single goal update
                updateSingleGoalStatus(goalId, goalStatusEnum.running, "Goal started successfully!");
            }
        });
    });

    // 2. Main Logic Handler for Multi-level Status Transitions
    async function handleMultiLevelStatusChange(parentId, subGoals, targetStatus, statusName) {
        const isRunning = targetStatus === goalStatusEnum.running;
        const actionVerb = isRunning ? "start" : "complete";

        // QUESTION 1: Update all sub-goals?
        const result1 = await Swal.fire({
            title: `Multi-level Goal: ${statusName}`,
            text: `Do you want to ${actionVerb} all sub-goals as well?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: `Yes, ${actionVerb} all`,
            cancelButtonText: 'No',
            confirmButtonColor: isRunning ? '#2563eb' : '#10b981', // Blue for Running, Green for Complete
            cancelButtonColor: '#6b7280'
        });

        if (result1.isConfirmed) {
            // ACTION: Update all via Promise.all
            Swal.showLoading();
            const promises = subGoals.map(sg =>
                $.ajax({
                    url: `/Goals/ChangeGoalStatus/${sg.id}`,
                    method: "POST",
                    data: { Id: sg.id, GoalStatus: targetStatus }
                })
            );
            promises.push($.ajax({
                url: `/Goals/ChangeGoalStatus/${parentId}`,
                method: "POST",
                data: { Id: parentId, GoalStatus: targetStatus }
            }));

            await Promise.all(promises);
            showSuccessNotification(`Parent and all sub-goals are now ${statusName}.`);
            setTimeout(() => location.reload(), 500);

        } else if (result1.dismiss === Swal.DismissReason.cancel) {

            // QUESTION 2: De-attach sub-goals?
            const result2 = await Swal.fire({
                title: 'Sub-goal Relationship',
                text: "Do you want to de-attach these sub-goals (make them root goals) and only update this parent?",
                icon: 'info',
                showCancelButton: true,
                confirmButtonText: 'Yes, de-attach and update',
                cancelButtonText: `No, just update parent`,
                confirmButtonColor: '#4f46e5',
                cancelButtonColor: '#6b7280'
            });

            if (result2.isConfirmed) {
                // ACTION: Bulk De-attach using the new API
                Swal.showLoading();

                const goalIdsToDeattach = subGoals.map(sg => sg.id);

                $.ajax({
                    url: `/Goals/DeattachSubGoals`,
                    method: "POST",
                    contentType: "application/json",
                    data: JSON.stringify(goalIdsToDeattach),
                    success: async function (response) {
                        if (response.status) {
                            // After de-attaching, complete the parent
                            updateSingleGoalStatus(parentId, targetStatus, `Sub-goals de-attached. Parent is now ${statusName}.`);
                        } else {
                            showErrorNotification("Failed to de-attach sub-goals.");
                        }
                    }
                });
            } else if (result2.dismiss === Swal.DismissReason.cancel) {
                // ACTION: Just update parent status, leave sub-goals attached
                updateSingleGoalStatus(parentId, targetStatus, `Parent goal moved to ${statusName}.`);
            }
        }
    }


    $(document).on("click", ".markAsComplete", function () {
        const goalId = $(this).data("id");

        // 1. First, fetch child goals to see if this is a multi-level goal
        $.get(`/Goals/GetChildGoals?parentId=${goalId}`, function (response) {
            const subGoals = (response && response.status) ? response.data : [];

            if (subGoals.length > 0) {
                handleMultiLevelCompletion(goalId, subGoals, "Goal completed successfully!");
            } else {
                // Standard single goal completion
                updateSingleGoalStatus(goalId, goalStatusEnum.completed);
            }
        });
    });

    async function handleMultiLevelCompletion(parentId, subGoals) {
        // QUESTION 1: Mark all as complete?
        const result1 = await Swal.fire({
            title: 'Multi-level Goal Detected',
            text: "Do you want to mark all sub-goals as complete as well?",
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, complete all',
            cancelButtonText: 'No',
            confirmButtonColor: '#10b981', // Green
            cancelButtonColor: '#6b7280'
        });

        if (result1.isConfirmed) {
            // ACTION: Mark all complete (Loop + Promise.all)
            const promises = subGoals.map(sg =>
                $.ajax({
                    url: `/Goals/ChangeGoalStatus/${sg.id}`,
                    method: "POST",
                    data: { Id: sg.id, GoalStatus: goalStatusEnum.completed }
                })
            );
            // Add the parent to the list
            promises.push($.ajax({
                url: `/Goals/ChangeGoalStatus/${parentId}`,
                method: "POST",
                data: { Id: parentId, GoalStatus: goalStatusEnum.completed }
            }));

            await Promise.all(promises);
            showSuccessNotification("Parent and all sub-goals completed!");
            setTimeout(() => location.reload(), 500);

        } else if (result1.dismiss === Swal.DismissReason.cancel) {

            // QUESTION 2: De-attach sub-goals?
            const result2 = await Swal.fire({
                title: 'Sub-goal Handling',
                text: "Do you want to de-attach these sub-goals (make them root goals) before completing the parent?",
                icon: 'info',
                showCancelButton: true,
                confirmButtonText: 'Yes, de-attach and complete',
                cancelButtonText: 'No, just complete parent',
                confirmButtonColor: '#4f46e5', // Indigo
                cancelButtonColor: '#6b7280'
            });

            if (result2.isConfirmed) {
                // ACTION: Bulk De-attach using the new API
                Swal.showLoading();

                const goalIdsToDeattach = subGoals.map(sg => sg.id);

                $.ajax({
                    url: `/Goals/DeattachSubGoals`,
                    method: "POST",
                    contentType: "application/json",
                    data: JSON.stringify(goalIdsToDeattach),
                    success: async function (response) {
                        if (response.status) {
                            // After de-attaching, complete the parent
                            updateSingleGoalStatus(parentId, targetStatus, `Sub-goals de-attached. Parent is now ${statusName}.`);
                        } else {
                            showErrorNotification("Failed to de-attach sub-goals.");
                        }
                    }
                });
            } else if (result2.dismiss === Swal.DismissReason.cancel) {
                // ACTION: Just update parent status
                updateSingleGoalStatus(parentId, goalStatusEnum.completed);
            }
        }
    }

    // Helper to handle simple status updates
    function updateSingleGoalStatus(id, status, customMessage) {
        $.ajax({
            url: `/Goals/ChangeGoalStatus/${id}`,
            method: "POST",
            data: { Id: id, GoalStatus: status },
            success: function (response) {
                if (response?.status) {
                    showSuccessNotification(customMessage || "Status updated successfully!");
                    setTimeout(() => location.reload(), 500);
                } else {
                    showErrorNotification(response?.message);
                }
            }
        });
    }

});