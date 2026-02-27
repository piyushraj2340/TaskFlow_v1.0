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
        const timelineWrapper = document.querySelector('section.timeline-center .space-y-8, .relative.border-l-4.pl-6'); // Selector for container
        const sentinel = document.getElementById('infinite-scroll-sentinel');
        if (!timelineWrapper) return;
        
        // Expose state to window so Index.cshtml can reset it
        window.infiniteScrollPage = 2;
        window.infiniteScrollFinished = false;
        
        const pageSize = 20; 
        let loading = false;
        
        // MODIFIED: Access global filter state object if it exists
        function getFilterParams() {
             if (typeof currentFilters !== 'undefined') {
                 return currentFilters;
             }
             return { search: null, goalId: null, taskId: null };
        }

        // detect page type and params
        const goalId = sentinel.dataset.goalId ? parseInt(sentinel.dataset.goalId, 10) : null;
        const taskId = sentinel.dataset.taskId ? parseInt(sentinel.dataset.taskId, 10) : null;
        const isJournal = !goalId && !taskId;

        let lastDate = null;

        // MODIFIED: Combine date header logic into rendering
        function renderDateHeader(globalIndex, dateStr) {
            const headerText = dateHeaderText(dateStr);
            const headerHtml = `<div class="mb-2 flex items-center justify-center relative top-[-4px]">
                                    <div class="inline-flex items-center rounded-full bg-gray-500 px-3 py-1 text-xs font-semibold text-white">
                                        ${escapeHtml(headerText)}
                                    </div>
                                </div>`;
            timelineWrapper.insertAdjacentHTML('beforeend', headerHtml);
        }

        // MODIFIED: Include date grouping in renderer
        function renderNoteWithDateGroup(note, globalIndex) {
            const dt = note.timeStamp ? new Date(note.timeStamp) : null;
            const dtStr = dt ? dt.toDateString() : null;

            // Only render a new date header if the date has changed
            if (lastDate !== dtStr) {
                lastDate = dtStr;
                renderDateHeader(globalIndex, note.timeStamp);
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

        // --- MODIFIED: Adjust fetchNextPage to use global filter state ---
        async function fetchNextPage() {
            if (loading || window.infiniteScrollFinished) return;
            loading = true;

            const filters = getFilterParams();
            
            // Construct URL with filters
             const urlParams = new URLSearchParams({
                pageNumber: window.infiniteScrollPage,
                pageSize: pageSize
            });
            
            if(filters.search) urlParams.set('searchQuery', filters.search);
            if(filters.goalId) urlParams.set('filterGoalId', filters.goalId);
            if(filters.taskId) urlParams.set('filterTaskId', filters.taskId);
            
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
            if (lastDateParam) urlParams.set('lastDate', lastDateParam);

            const url = `/Notes/GetJournalNotesPartial?${urlParams.toString()}`;

            try {
                const res = await fetch(url, { headers: { 'Accept': 'text/html' } });
                const html = await res.text();
                
                if (!html || !html.trim()) {
                     window.infiniteScrollFinished = true;
                } else {
                     // Append logic
                     // Note: We need to append strictly to the internal container now
                     const container = document.getElementById('journalItemsContainer') || timelineWrapper;
                     container.insertAdjacentHTML('beforeend', html);
                     window.infiniteScrollPage++;
                     
                     // highlight if necessary
                     if (window.Prism) window.Prism.highlightAll();
                }
            } catch (err) {
                console.error(err);
                window.infiniteScrollFinished = true;
            } finally {
                loading = false;
            }
        }
        
        // --- Observer setup remains similar ---
        if(sentinel) {
            const observer = new IntersectionObserver((entries) => {
                if (entries.some(e => e.isIntersecting)) {
                    fetchNextPage();
                }
            }, { root: null, rootMargin: '400px' });
            observer.observe(sentinel);
        }

    } catch (err) {
        console.error('Infinite scroll init error:', err);
    }
})();