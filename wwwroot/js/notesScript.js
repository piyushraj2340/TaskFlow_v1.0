$(document).ready(function () {
    const notesActionType = Object.freeze({
        All: 0, // Independent/Journal
        Goal: 1,
        Task: 2
    });

    // --- NEW: Helper function to update the modal title ---
    function updateModalTitle(type) {
        let title = '';
        switch (parseInt(type, 10)) {
            case notesActionType.Goal:
                title = '<i class="fa-solid fa-clipboard-list"></i> Create Progress Record for Goal';
                break;
            case notesActionType.Task:
                title = '<i class="fa-solid fa-clipboard-list"></i> Create Progress Record for Task';
                break;
            case notesActionType.All:
            default:
                title = '<i class="fa-solid fa-clipboard-list"></i> Create New Journal Note';
                break;
        }
        $('#note-title').html(title);
    }

    // --- NEW: Helper to ensure hidden fields exist for form submission ---
    function ensureAttachedFields(type, id) {
        let $attachedTypeInput = $('#attachedType');
        let $attachedIdInput = $('#attachedId');
        const $noteForm = $('#noteForm');

        if (!$attachedTypeInput.length) {
            $attachedTypeInput = $('<input type="hidden" id="attachedType" name="attachedType">');
            $noteForm.prepend($attachedTypeInput);
        }
        if (!$attachedIdInput.length) {
            $attachedIdInput = $('<input type="hidden" id="attachedId" name="attachedId">');
            $noteForm.prepend($attachedIdInput);
        }

        $attachedTypeInput.val(type);
        $attachedIdInput.val(id);
    }

    // Tabs changes (KEEPING AS IS)
    $(".tab-link").on('click', function (e) {
        e.preventDefault();

        if (typeof tinymce !== "undefined") {
            tinymce.triggerSave(); // updates #content with editor value
        }

        history.pushState(null, null, this.href);

        // Remove active class from all tabs
        $(".tab-link").removeClass("text-blue-600 border-blue-600 font-semibold").addClass("text-gray-600 border-gray-600");

        // Add active class to the clicked tab
        $(this).addClass("text-blue-600 border-blue-600 font-semibold").removeClass("text-gray-600 border-gray-600");

        // Hide all tab contents
        $(".tab-content").addClass("hidden");

        // Show the selected tab
        $("#" + $(this).data("tab")).removeClass("hidden");
    });

    // --- MODIFIED: Form Submission Handler to use Dynamic URL ---
    $("#noteForm").on('submit', function (e) {
        e.preventDefault();

        // Ensure TinyMCE content is saved back to the textarea before submission
        if (typeof tinymce !== "undefined") {
            tinymce.triggerSave();
        }

        const action = $(this).data('action-note-form');
        const noteId = $(this).data("noteid");

        const data = {
            Title: $("#title").val(),
            Content: $("#content").val(),
            Tags: $("#tags").val(),
            IsPinned: $("#IsPinned").prop('checked'),
            // NEW: Include attached type/id from the hidden fields
            AttachedType: parseInt($('#attachedType').val() || '0', 10),
            AttachedId: parseInt($('#attachedId').val() || '0', 10)
        }

        if (action.toString().toLowerCase() === 'create') {

            // Check for attachment requirement if radio button selected Goal or Task
            if ((data.AttachedType === notesActionType.Goal || data.AttachedType === notesActionType.Task) && data.AttachedId === 0) {
                showErrorNotification('Please select a ' + (data.AttachedType === notesActionType.Goal ? 'Goal' : 'Task') + ' to attach this note.');
                return;
            }

            // --- FIX: Dynamic URL generation ---
            const url = `/Notes/Create/${data.AttachedId}/${data.AttachedType}`;

            $.ajax({
                url: url,
                method: "POST",
                data: data,
                success: function (response) {
                    showSuccessNotification(response.message || `Note Saved!`);

                    setTimeout(() => {
                        $("#noteModal").fadeOut();
                        location.reload();
                    }, 500)
                },
                error: function (xhr, status, error) {
                    showErrorNotification(error.message || "Notes Save Failed!");
                    console.error("Error:", error);
                }
            });
        } else if (action.toString().toLowerCase() === 'edit' && noteId) {
            // Edit logic remains mostly the same, ensuring 'id' is sent
            data.id = noteId;

            $.ajax({
                url: `/Notes/Edit/${noteId}`,
                method: "POST",
                data: data,
                success: function (response) {
                    showSuccessNotification(response.message || "Notes Saved!");

                    setTimeout(() => {
                        $("#noteModal").fadeOut();
                        location.reload();
                    }, 500)
                },
                error: function (xhr, status, error) {
                    showErrorNotification(error.message || "Notes Save Failed!");
                    console.error("Error:", error);
                }
            });
        } else {
            showErrorNotification("Invalid Action!")
        }
    });

    // --- MODIFIED: Open Modal Handler ---
    $("#openAddNotesModal").on('click', function () {
        // Get context from button (if available)
        const actionTypeStr = $(this).data('action-note-form-type')?.toString().toLowerCase() || 'all';
        const actionTypeId = $(this).data('action-note-form-type-id') || 0;

        let initialAttachType = notesActionType.All;
        if (actionTypeStr === 'goal') {
            initialAttachType = notesActionType.Goal;
        } else if (actionTypeStr === 'task') {
            initialAttachType = notesActionType.Task;
        } else {
            initialAttachType = notesActionType.All;
            $("#AddNotesWithMultiFeature").removeClass("hidden");
        }

        // Reset all form elements
        $("#noteForm")[0].reset();
        if (typeof tinymce !== "undefined" && tinymce.get("content")) {
            tinymce.get("content").setContent('');
        }
        $('#attachSelected').empty();
        $('#attachSearchResults').empty().hide();
        $('#attachSearchArea').addClass('hidden');

        // 1. Set title dynamically (Journal for 'all')
        updateModalTitle(initialAttachType);

        // 2. Hide ID field for new creation
        $("#note-id-field").addClass("hidden");

        // 3. Set form action
        $("#noteForm").data('action-note-form', 'create');

        // 4. Set initial radio button and hidden fields based on button context
        $(`#noteForm input[name="attachType"][value="${initialAttachType}"]`).prop('checked', true).trigger('change');

        if (initialAttachType !== notesActionType.All && parseInt(actionTypeId, 10) > 0) {
            // If opened from a Goal/Task button, pre-select it
            ensureAttachedFields(initialAttachType, actionTypeId);
            const typeName = initialAttachType === notesActionType.Goal ? 'Goal' : 'Task';
            $('#attachSelected').html(`<div class="rounded-md bg-indigo-50 px-3 py-2 text-sm text-indigo-800">Attached to: <strong>${typeName} #${actionTypeId}</strong></div>`);
            // Hide the search bar since an item is already selected
            $('#attachSearchArea').addClass('hidden');
        } else {
            // Independent/Journal or Goal/Task without ID, so keep default 0 and hide search
            ensureAttachedFields(notesActionType.All, 0);
        }

        // Show modal
        $("#noteModal").removeClass("hidden").fadeIn().css('display', 'flex');
    });

    // --- MODIFIED: Edit Modal Handler (Title fix) ---
    $(".edit-notes").on('click', function (e) {
        // Title update for Edit mode
        $("#noteModal").removeClass("hidden").fadeIn();
        $("#note-title").html('<i class="fa-solid fa-edit"></i> Edit Your Progress Record'); // Changed 'in Goal' to more generic 'Record'
        $("#noteForm").data('action-note-form', 'edit');

        $("#note-id-field").removeClass("hidden").fadeIn();

        const id = $(this).data("noteeditid");

        $("#noteForm").data("noteid", id);

        $.ajax({
            url: `/Notes/Details/${id}`,
            method: "POST",
            success: function (response) {
                $("#noteModal").removeClass("hidden").fadeIn();

                const { data } = response;

                $("#noteid").val(data.id);
                $("#title").val(data.title);
                // Note: The original code used data.content for both, which is fine
                // but if using a TinyMCE editor we should call setContent.
                // Keeping the original field-setting for non-tinymce fallback:
                $("#content").val(data.content);
                $("#tags").val(data.tags);
                $("#IsPinned").prop("checked", data.isPinned);

                if (typeof tinymce !== "undefined") {
                    tinymce.get("content").setContent(data.content || "");
                }

                // NEW: Handle pre-selection of attached item for editing if needed
                if (data.attachedType) {
                    $(`#noteForm input[name="attachType"][value="${data.attachedType}"]`).prop('checked', true).trigger('change');
                    ensureAttachedFields(data.attachedType, data.attachedId || 0);

                    if (data.attachedId && data.attachedName) {
                        const typeName = data.attachedType === notesActionType.Goal ? 'Goal' : data.attachedType === notesActionType.Task ? 'Task' : 'Independent';
                        $('#attachSelected').html(`<div class="rounded-md bg-indigo-50 px-3 py-2 text-sm text-indigo-800">Attached to: <strong>${typeName} #${data.attachedId} - ${data.attachedName}</strong></div>`);
                    }
                }

            },
            error: function (xhr, status, error) {
                showErrorNotification(error.message || "Failed To Load data!");
                console.error("Error:", error);
            }
        });
    });

    // --- NEW: Attach-type radio change handler (pulled from the second script block) ---
    $('#noteModal').on('change', 'input[name="attachType"]', function () {
        const val = parseInt($(this).val(), 10);

        // Update hidden fields
        ensureAttachedFields(val, 0); // Reset ID on type change
        $('#attachSelected').empty();
        $('#attachSearchResults').empty().hide();

        // Update Modal Title (replaces the hardcoded title in the original openAddNotesModal function)
        updateModalTitle(val);

        if (val === notesActionType.Goal || val === notesActionType.Task) {
            $('#attachSearchLabel').text(val === notesActionType.Goal ? 'Search Goals' : 'Search Tasks');
            $('#attachSearchInput').attr('placeholder', val === notesActionType.Goal ? 'Type goal name...' : 'Type task name...');
            $('#attachSearchArea').removeClass('hidden').show();
            $('#attachSearchInput').focus();
        } else {
            $('#attachSearchArea').addClass('hidden').hide();
        }
    });

    // --- NEW: Attach Search Logic (pulled from the second script block) ---
    let attachTimer = null;
    $('#attachSearchInput').on('input', function () {
        const q = $(this).val()?.toString().trim() || '';
        clearTimeout(attachTimer);
        if (q.length < 2) {
            $('#attachSearchResults').empty().hide();
            return;
        }
        attachTimer = setTimeout(async () => {
            const type = parseInt($('#attachedType').val(), 10);
            try {
                let resp;
                if (type === notesActionType.Goal) {
                    resp = await $.post('/Goals/SearchGoalNameByName', { searchQuery: q });
                } else if (type === notesActionType.Task) {
                    resp = await $.get('/Tasks/SearchTasks', { query: q });
                } else {
                    return; // Should not happen if logic is correct
                }

                if (resp && resp.status && Array.isArray(resp.data)) {
                    const html = resp.data.map(item => {
                        const typeName = type === notesActionType.Goal ? 'goal' : 'task';
                        return `<div class="px-3 py-2 hover:bg-gray-100 cursor-pointer attach-item" data-id="${item.id}" data-type="${typeName}">
                                    <div class="font-medium">${item.name}</div>
                                    <div class="text-xs text-gray-500">${item.description || item.goalStatus || ''}</div>
                                </div>`;
                    }).join('');
                    $('#attachSearchResults').html(html).show();
                } else {
                    $('#attachSearchResults').html('<div class="p-3 text-sm text-gray-500">No items found</div>').show();
                }
            } catch (err) {
                console.error('Attach search error', err);
                $('#attachSearchResults').empty().hide();
            }
        }, 300);
    });

    // --- NEW: Select attach item (pulled from the second script block) ---
    $('#attachSearchResults').on('click', '.attach-item', function () {
        const id = $(this).data('id');
        const title = $(this).find('.font-medium').text();
        ensureAttachedFields(parseInt($('#attachedType').val(), 10), id);
        $('#attachSelected').html(`<div class="rounded-md bg-indigo-50 px-3 py-2 text-sm text-indigo-800">Attached to: <strong>${title}</strong></div>`);
        $('#attachSearchResults').hide();
    });

    // Close Modal (KEEPING AS IS)
    $("#closeModal").on('click', function () {
        $("#noteModal").fadeOut();
    });

    $("#noteModal").on('click', function (e) {
        if (e.target === this) {
            $(this).fadeOut();
        }
    })

    // Handle Form Submission (kept for `#saveBtn` fallback, though #noteForm is primary)
    $("#saveBtn").on('click', function () {
        $("#noteForm").submit();
    });

    // Delete Notes (KEEPING AS IS)
    $(".delete-notes").on("click", async function () {
        const id = $(this).data("noteeditid");

        if (!id) {
            showErrorNotification("Missing Id parameters!");
            return;
        }

        try {
            // Call reusable confirmation function (assuming this is defined elsewhere)
            showConfirmationDialog(
                {
                    title: 'Are you sure?',
                    text: 'Do you really want to delete this goal? This action cannot be undone.',
                },
                {
                    url: `/Notes/Delete`,
                    type: 'DELETE',
                    data: { Id: id }
                },
                function (response) { // Success callback
                    location.reload();
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
});

// -----------------------------
// Infinite scroll (IntersectionObserver)
// -----------------------------
(function initInfiniteScroll() {
    try {
        const timelineWrapper = document.querySelector('section.timeline-center .space-y-8, .relative.border-l-4.pl-6');
        const sentinel = document.getElementById('infinite-scroll-sentinel');
        if (!timelineWrapper || !sentinel) return;

        const pageSize = 20; // server default
        let pageNumber = 2; // assume page 1 was server-rendered
        let loading = false;
        let finished = false;
        let globalIndex = timelineWrapper.querySelectorAll('article, .relative.mb-6').length || 0;
        let lastDate = null;

        // detect page type and params
        const goalId = sentinel.dataset.goalId ? parseInt(sentinel.dataset.goalId, 10) : null;
        const taskId = sentinel.dataset.taskId ? parseInt(sentinel.dataset.taskId, 10) : null;
        const isJournal = !goalId && !taskId;

        if (globalIndex === 0) pageNumber = 1;
        else if (globalIndex < pageSize) finished = true;

        function setLoadingIndicator(show) {
            sentinel.innerHTML = show ? '<div class="py-6 flex justify-center"><svg class="animate-spin h-6 w-6 text-indigo-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z"></path></svg></div>' : '';
        }

        function dateHeaderText(dt) {
            if (!dt) return '';
            const d = new Date(dt);
            const today = new Date();
            if (d.toDateString() === today.toDateString()) return 'Today';
            const yesterday = new Date(); yesterday.setDate(today.getDate() - 1);
            if (d.toDateString() === yesterday.toDateString()) return 'Yesterday';
            const weekAgo = new Date(); weekAgo.setDate(today.getDate() - 7);
            if (d >= weekAgo) {
                return d.toLocaleDateString(undefined, { weekday: 'long' });
            }
            return d.toLocaleDateString(undefined, { month: 'long', day: 'numeric', year: 'numeric' });
        }

        function escapeHtml(unsafe) {
            if (!unsafe) return '';
            return unsafe
                .replace(/&/g, "&amp;")
                .replace(/</g, "&lt;")
                .replace(/>/g, "&gt;")
                .replace(/"/g, "&quot;")
                .replace(/'/g, "&#039;");
        }

        function formatTimestamp(ts) {
            if (!ts) return '';
            const d = new Date(ts);
            return d.toLocaleString(undefined, { month: 'short', day: 'numeric', year: 'numeric', hour: 'numeric', minute: '2-digit' });
        }

        function renderNoteHtml(note, idx) {
            const isLeft = (idx % 2) === 0;
            const sideClass = isLeft ? 'md:w-1/2 md:text-right md:order-1' : 'md:w-1/2 md:order-3';
            const articleParts = [];

            articleParts.push(`<article class="relative md:flex md:items-start md:justify-between md:gap-6">`);

            // left block (goal/task preview) - placed on proper side
            if (isLeft) {
                articleParts.push(`<div class="${sideClass}"><div class="inline-block md:ml-auto md:max-w-md">`);
                if (note.goal) {
                    articleParts.push(`<h3 class="text-lg font-semibold text-slate-900">Goal: #${note.goal.id} - ${escapeHtml(note.goal.name || '')}</h3>`);
                    articleParts.push(`<div class="relative group inline-block w-full">`);
                    articleParts.push(`<div class="tooltip-preview richtext line-clamp-2 overflow-hidden cursor-help text-left">${note.goal.description ? note.goal.description : ''}</div>`);
                    articleParts.push(`<div class="tooltip-panel hidden z-[9999]"><div class="tooltip-content bg-white p-3 rounded-lg shadow-xl border border-slate-300 text-sm max-h-[400px] overflow-auto text-left">${note.goal.description ? note.goal.description : ''}</div></div>`);
                    articleParts.push(`</div>`);
                }
                if (note.task) {
                    articleParts.push(`<h3 class="text-lg font-semibold text-slate-900 mt-4">Task: #${note.task.id} - ${escapeHtml(note.task.name || '')}</h3>`);
                    articleParts.push(`<div class="relative group inline-block w-full">`);
                    articleParts.push(`<div class="tooltip-preview richtext line-clamp-2 overflow-hidden cursor-help text-left">${note.task.description ? note.task.description : ''}</div>`);
                    articleParts.push(`<div class="tooltip-panel hidden z-[9999]"><div class="tooltip-content bg-white p-3 rounded-lg shadow-xl border border-slate-300 text-sm max-h-[400px] overflow-auto text-left">${note.task.description ? note.task.description : ''}</div></div>`);
                    articleParts.push(`</div>`);
                }
                articleParts.push(`<div class="mt-3 text-xs text-slate-400 flex gap-3 justify-end"><time datetime="${note.timeStamp}">Created: ${formatTimestamp(note.timeStamp)}</time>`);
                if (note.modifiedOn) {
                    articleParts.push(`<span>•</span><time datetime="${note.modifiedOn}">Last modified: ${formatTimestamp(note.modifiedOn)}</time>`);
                }
                articleParts.push(`</div></div></div>`);
            }

            // center marker
            articleParts.push(`<div class="absolute md:static left-0 md:left-auto md:mx-0 md:order-2 md:flex md:items-center md:justify-center md:w-0">`);
            articleParts.push(`<div class="flex items-center md:flex-col md:justify-center md:gap-2">`);
            articleParts.push(`<div class="timeline-dot ${isLeft ? 'bg-indigo-400' : 'bg-slate-400'} border-2 border-white shadow-md" aria-hidden></div>`);
            articleParts.push(`<div class="hidden md:block w-px bg-slate-200 h-12"></div>`);
            articleParts.push(`</div></div>`);

            // right block (content and metadata)
            if (!isLeft) {
                articleParts.push(`<div class="${sideClass}"><div class="inline-block md:ml-auto">`);
                if (note.goal) {
                    articleParts.push(`<h3 class="text-lg font-semibold text-slate-900">Goal: #${note.goal.id} - ${escapeHtml(note.goal.name || '')}</h3>`);
                    articleParts.push(`<div class="relative group inline-block w-full">`);
                    articleParts.push(`<div class="tooltip-preview richtext line-clamp-2 overflow-hidden cursor-help text-left">${note.goal.description ? note.goal.description : ''}</div>`);
                    articleParts.push(`<div class="tooltip-panel hidden z-[9999]"><div class="tooltip-content bg-white p-3 rounded-lg shadow-xl border border-slate-300 text-sm max-h-[400px] overflow-auto text-left">${note.goal.description ? note.goal.description : ''}</div></div>`);
                    articleParts.push(`</div>`);
                }
                if (note.task) {
                    articleParts.push(`<h3 class="text-lg font-semibold text-slate-900 mt-4">Task: #${note.task.id} - ${escapeHtml(note.task.name || '')}</h3>`);
                    articleParts.push(`<div class="relative group inline-block w-full">`);
                    articleParts.push(`<div class="tooltip-preview richtext line-clamp-2 overflow-hidden cursor-help text-left">${note.task.description ? note.task.description : ''}</div>`);
                    articleParts.push(`<div class="tooltip-panel hidden z-[9999]"><div class="tooltip-content bg-white p-3 rounded-lg shadow-xl border border-slate-300 text-sm max-h-[400px] overflow-auto text-left">${note.task.description ? note.task.description : ''}</div></div>`);
                    articleParts.push(`</div>`);
                }
                articleParts.push(`<div class="mt-3 text-xs text-slate-400 flex gap-3 justify-end"><time datetime="${note.timeStamp}">Created: ${formatTimestamp(note.timeStamp)}</time>`);
                if (note.modifiedOn) {
                    articleParts.push(`<span>•</span><time datetime="${note.modifiedOn}">Last modified: ${formatTimestamp(note.modifiedOn)}</time>`);
                }
                articleParts.push(`</div></div></div>`);
            } else {
                // for left side, we still render right content area (using existing layout pattern)
                articleParts.push(`<div class="md:w-1/2 md:order-3">`);
                // reuse a minimal content area placeholder to keep layout parity
                articleParts.push(`<div class="rounded-lg bg-white p-3 shadow-sm">`);
                articleParts.push(`<h3 class="text-md font-semibold text-gray-700">${escapeHtml(note.title || '')}</h3>`);
                articleParts.push(`<div class="richtext line-clamp-3 mt-2 text-gray-700">${note.content ? note.content : ''}</div>`);
                articleParts.push(`</div></div>`);
            }

            // for right side we already included both blocks; for left we added simplified content.
            articleParts.push(`</article>`);

            return articleParts.join('');
        }

        // renderer for journal (uses article left/right layout)
        function renderJournalNoteHtml(note, idx) {
            // reuse existing renderNoteHtml implementation (kept minimal here)
            // Use the same function body you had earlier (omitted for brevity) - ensure consistent left/right layout.
            return renderNoteHtml(note, idx); // existing function defined earlier in file
        }

        // renderer for goal/task listing (simple linear timeline item used in Goal/Task details)
        function renderGoalTaskNoteHtml(note) {
            const modified = note.modifiedOn ? `<div class="mt-2 text-xs text-gray-600"><i class="fas fa-calendar-alt mr-2 text-sm"></i>${formatTimestamp(note.modifiedOn)} | <span class="text-rose-700">Modified</span></div>` :
                `<div class="mt-2 text-xs text-gray-600"><i class="fas fa-calendar-alt mr-2 text-sm"></i>${formatTimestamp(note.timeStamp)} | <span class="text-green-700">Published</span></div>`;

            const actions = `<div class="mt-3 flex items-center justify-between">
                                    <div class="flex space-x-2">
                                        <button class="edit-notes text-gray-700 hover:text-blue-500" data-noteeditid="${note.id}"><i class="fas fa-edit"></i> Edit</button>
                                        <button class="delete-notes text-gray-700 hover:text-red-500" data-noteeditid="${note.id}"><i class="fas fa-trash"></i> Delete</button>
                                    </div>
                                    ${note.isPinned ? `<button class="text-yellow-600"><i class="fas fa-thumbtack"></i></button>` : ''}
                                 </div>`;

            return `<div class="relative mb-6">
                            <div class="absolute -left-[20px] top-4 h-0.5 w-2 bg-gray-300"></div>
                            <div class="absolute -left-[11px] top-2 flex h-5 w-5 items-center justify-center rounded-full border-4 border-white bg-blue-600"></div>
                            <div class="rounded-lg bg-blue-100 p-4 shadow">
                                <h3 class="flex items-center font-semibold text-blue-700">${escapeHtml(note.title || '')}</h3>
                                <div class="richtext w-full">${note.content || ''}</div>
                                <div class="relative mb-4"><div class="flex py-2 text-sm text-gray-500"><i class="fas fa-link mr-2"></i> Attached Files</div></div>
                                ${modified}
                                ${actions}
                            </div>
                        </div>`;
        }

        async function fetchNextPage() {
            if (loading || finished) return;
            loading = true;
            setLoadingIndicator(true);

            let url;
            if (isJournal) {
                // compute lastDate to avoid duplicate headers: get last displayed article time datetime (if any)
                let lastDateParam = null;
                try {
                    const timeEls = timelineWrapper.querySelectorAll('time[datetime]');
                    if (timeEls.length > 0) {
                        const lastTime = timeEls[timeEls.length - 1].getAttribute('datetime');
                        if (lastTime) {
                            // send yyyy-mm-dd so server can compare easily
                            const d = new Date(lastTime);
                            lastDateParam = d.toISOString().slice(0, 10);
                        }
                    }
                } catch (err) {
                    lastDateParam = null;
                }

                // request HTML partial
                const urlParams = new URLSearchParams({
                    pageNumber: pageNumber,
                    pageSize: pageSize
                });
                if (lastDateParam) urlParams.set('lastDate', lastDateParam);

                url = `/Notes/GetJournalNotesPartial?${urlParams.toString()}`;
            } else if (goalId) {
                url = `/Goals/GetNotesByGoal?goalId=${goalId}&pageNumber=${pageNumber}&pageSize=${pageSize}`;
            } else if (taskId) {
                url = `/Tasks/GetNotesByTask?taskId=${taskId}&pageNumber=${pageNumber}&pageSize=${pageSize}`;
            } else {
                finished = true;
                setLoadingIndicator(false);
                loading = false;
                return;
            }

            try {
                if (isJournal) {
                    // fetch HTML partial
                    const res = await fetch(url, { headers: { 'Accept': 'text/html' } });
                    if (!res.ok) throw new Error('Network response was not ok');
                    const html = await res.text();
                    if (!html || !html.trim()) {
                        finished = true;
                        return;
                    }

                    // determine how many article nodes were returned so pagination can stop correctly
                    const temp = document.createElement('div');
                    temp.innerHTML = html;
                    const addedArticles = temp.querySelectorAll('article').length;

                    // append server-rendered HTML directly
                    timelineWrapper.insertAdjacentHTML('beforeend', html);

                    if (addedArticles === 0) {
                        finished = true;
                    } else if (addedArticles < pageSize) {
                        finished = true;
                        globalIndex += addedArticles;
                    } else {
                        pageNumber++;
                        globalIndex += addedArticles;
                    }

                    // keep lastDate updated for subsequent requests
                    const appendedTimes = temp.querySelectorAll('time[datetime]');
                    if (appendedTimes.length > 0) {
                        const lastAppended = appendedTimes[appendedTimes.length - 1].getAttribute('datetime');
                        if (lastAppended) {
                            lastDate = new Date(lastAppended).toDateString();
                        }
                    }

                    // highlight if necessary
                    if (window.Prism && typeof window.Prism.highlightAll === 'function') {
                        window.Prism.highlightAll();
                    }
                } else {
                    const res = await fetch(url, { headers: { 'Accept': 'application/json' } });
                    if (!res.ok) throw new Error('Network response was not ok');
                    const json = await res.json();
                    if (!json || !json.status) {
                        finished = true;
                        return;
                    }

                    const data = json.data || [];
                    if (!Array.isArray(data) || data.length === 0) {
                        finished = true;
                        return;
                    }

                    for (let i = 0; i < data.length; i++) {
                        const note = data[i];

                        const dt = note.timeStamp ? new Date(note.timeStamp) : null;
                        const dtStr = dt ? dt.toDateString() : null;
                        if (dtStr !== lastDate) {
                            lastDate = dtStr;
                            const headerText = dateHeaderText(note.timeStamp);
                            const headerHtml = `<div class="mb-2 flex items-center justify-center relative top-[-4px]"><div class="inline-flex items-center rounded-full bg-gray-500 px-3 py-1 text-xs font-semibold text-white">${escapeHtml(headerText)}</div></div>`;
                            timelineWrapper.insertAdjacentHTML('beforeend', headerHtml);
                        }

                        let html;
                        if (isJournal) {
                            html = renderJournalNoteHtml(note, globalIndex);
                        } else {
                            html = renderGoalTaskNoteHtml(note);
                        }

                        timelineWrapper.insertAdjacentHTML('beforeend', html);
                        globalIndex++;
                    }

                    if (data.length < pageSize) finished = true;
                    else pageNumber++;

                    if (window.Prism && typeof window.Prism.highlightAll === 'function') {
                        window.Prism.highlightAll();
                    }
                }
            } catch (err) {
                console.error('Error loading notes page:', err);
                finished = true;
            } finally {
                setLoadingIndicator(false);
                loading = false;
            }
        }

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    fetchNextPage();
                }
            });
        }, { root: null, rootMargin: '400px', threshold: 0.1 });

        observer.observe(sentinel);
    } catch (err) {
        console.error('Infinite scroll init error:', err);
    }
})();