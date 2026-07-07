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
    $(document).on('click', "#openAddNotesModal", function () {
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
    $(document).on('click', ".edit-notes", function (e) {
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
                    resp = await $.get('/Goals/SearchGoalNameByName', { searchQuery: q });
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
    $(document).on("click", ".delete-notes", async function () {
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
        const timelineWrapper = document.getElementById('journalItemsContainer') || document.getElementById('notes-container'); // Selector for container
        const sentinel = document.getElementById('infinite-scroll-sentinel');
        
        if (!timelineWrapper || !sentinel) return;
        
        // Expose state to window so Index.cshtml can reset it
        window.infiniteScrollPage = 2;
        window.infiniteScrollFinished = false;
        
        // Check if pageSize is defined in sentinel data attributes, otherwise default to 20
        const pageSize = sentinel.dataset.pageSize ? parseInt(sentinel.dataset.pageSize, 10) : 20; 
        let loading = false;
        
        // MODIFIED: Access global filter state object if it exists
        function getFilterParams() {
             if (typeof window.currentFilters !== 'undefined') {
                 return window.currentFilters;
             }
             // Fallback default
             return { search: null, goalIds: [], taskIds: [], includeGoalRelatedTasks: false };
        }

        // detect page type and params
        const goalId = sentinel.dataset.goalId ? parseInt(sentinel.dataset.goalId, 10) : 0;
        const taskId = sentinel.dataset.taskId ? parseInt(sentinel.dataset.taskId, 10) : 0;
        const filterPage = sentinel.dataset.filterPage || 'notes';
        
        const notesObjType = Object.freeze({
            notes: "notes",
            tasks: "tasks",
            goals: "goals"
        });
        
        const notesType = notesObjType[filterPage] || notesObjType.notes;

        const isJournal = !goalId && !taskId && notesType === notesObjType.notes;

        let lastDate = null;

        function renderDateHeader(globalIndex, dateStr) {
            const headerText = dateHeaderText(dateStr);
            const headerHtml = `<div class="mb-2 flex items-center justify-center relative top-[-4px]">
                                    <div class="inline-flex items-center rounded-full bg-gray-500 px-3 py-1 text-xs font-semibold text-white">
                                        ${escapeHtml(headerText)}
                                    </div>
                                </div>`;
            timelineWrapper.insertAdjacentHTML('beforeend', headerHtml);
        }

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

        async function fetchNextPage() {
    
            if (loading || window.infiniteScrollFinished) return;
            loading = true;

            // Show loading state in sentinel
            const s = document.getElementById('infinite-scroll-sentinel');
            if (s) {
                 s.innerHTML = '<div class="flex justify-center py-4"><i class="fas fa-circle-notch fa-spin text-indigo-500 text-xl"></i><span class="ml-2 text-gray-500 font-medium">Loading more notes...</span></div>';
            }

            const filters = getFilterParams();
            
            // Construct URL with filters
             const urlParams = new URLSearchParams({
                pageNumber: window.infiniteScrollPage,
                pageSize: pageSize
            });

            if(filters.search) urlParams.append('searchQuery', filters.search);
            
            // Handle Goal IDs (Array)
            // Can be single id (legacy) or array
            let hasGoalFilter = false;
            if(filters.goalIds && Array.isArray(filters.goalIds) && filters.goalIds.length > 0) {
                 filters.goalIds.forEach(id => urlParams.append('filterGoalIds', id));
                 hasGoalFilter = true;
            } else if (filters.goalId) { // Fallback for legacy single ID if somehow set
                 urlParams.append('filterGoalIds', filters.goalId);
                 hasGoalFilter = true;
            }

            // Fix: If on Goal Details page and no specific filter set, pass the context goalId
            if (!hasGoalFilter && goalId > 0 && notesType === notesObjType.goals) {
                urlParams.append('filterGoalIds', goalId);
            }

            // Handle Task IDs (Array)
            let hasTaskFilter = false;
            if(filters.taskIds && Array.isArray(filters.taskIds) && filters.taskIds.length > 0) {
                 filters.taskIds.forEach(id => urlParams.append('filterTaskIds', id));
                 hasTaskFilter = true;
            } else if (filters.taskId) { // Fallback
                 urlParams.append('filterTaskIds', filters.taskId);
                 hasTaskFilter = true;
            }

            // Fix: If on Task Details page and no specific filter set, pass the context taskId
            if (!hasTaskFilter && taskId > 0 && notesType === notesObjType.tasks) {
                 urlParams.append('filterTaskIds', taskId);
            }

            // Handle Boolean
            if(filters.includeGoalRelatedTasks) {
                urlParams.append('includeGoalRelatedTasks', 'true');
            }
            
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

            // MODIFIED: Use appropriate endpoint based on page context (Journal vs Goal/Task Details)
            let endpoint = '/Notes/GetJournalNotesPartial';
            if (notesType === notesObjType.goals || notesType === notesObjType.tasks) {
                endpoint = '/Notes/GetGoalTaskNotesPartial';
            }
            
            const url = `${endpoint}?${urlParams.toString()}`;

            try {
                const res = await fetch(url, { headers: { 'Accept': 'text/html' } });
                const html = await res.text();
                
                if (!html || !html.trim()) {
                     window.infiniteScrollFinished = true;
                     const s = document.getElementById('infinite-scroll-sentinel');
                     if (s) {
                         s.innerHTML = '<div class="text-slate-400 text-sm py-4 font-center text-center italic">No more history to scan.</div>';
                     }
                } else {
                     // Append logic
                     // Note: We need to append strictly to the internal container now
                     const container = document.getElementById('journalItemsContainer') || timelineWrapper;
                     // Instead of beforeend of container, we should append before the sentinel to keep structure valid if sentinel is inside
                     // Checking structure: usually sentinel is after container or inside at bottom. 
                     // In Razor files: <div id="notes-container"> ...items... <div id="sentinel"></div> </div>
                     // In Notes/Index: <div id="wrapper"> ...items... </div> <div id="sentinel"></div>
                     
                     // If sentinel is inside container, insert before sentinel. Else append to container.
                     if (container.contains(s)) {
                        s.insertAdjacentHTML('beforebegin', html);
                     } else {
                        container.insertAdjacentHTML('beforeend', html);
                     }

                     window.infiniteScrollPage++;
                     
                     // Reset sentinel text to empty/spacer
                     if (s && !window.infiniteScrollFinished) {
                        s.innerHTML = '';
                     }

                     // highlight if necessary
                     if (window.Prism) window.Prism.highlightAll();

                     // Ensure small pages fetch until screen is full or data ends
                    setTimeout(() => {
                         const s = document.getElementById('infinite-scroll-sentinel');
                         // Fix: Logic to prevent premature stopping if content is short but more exists
                         // Only fetch if sentinel is actually visible in viewport
                         if (s && s.getBoundingClientRect().top < window.innerHeight && !window.infiniteScrollFinished && !loading) {
                             fetchNextPage();
                         }
                     }, 200);
                }
            } catch (err) {
                console.error(err);
                // Don't finish infinite scroll on error, just allow retry
                // window.infiniteScrollFinished = true; 
                if (s) s.innerHTML = '<div class="text-red-400 text-sm text-center py-2" onclick="window.triggerInfiniteScroll()">Error loading. Click to retry.</div>';
            } finally {
                loading = false;
            }
        }
        
        window.triggerInfiniteScroll = fetchNextPage;
        
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

// -----------------------------
// Pinned Notes Carousel & Jump-to-Note Logic (Shared)
// -----------------------------
$(document).ready(function () {
    // Pinned Carousel Logic
    let currentPinnedIndex = 1;
    let totalPinned = 0;

    window.initPinnedCarousel = function () {
        const meta = $('#pinned-metadata');
        if (!meta.length) return;

        totalPinned = meta.data('total') || 0;
        $('#pinnedTotalCount').text(totalPinned);
        $('#pinnedCurrentIndex').text(totalPinned > 0 ? 1 : 0);
        currentPinnedIndex = 1;
        updatePinnedVisibility();
    };

    function updatePinnedVisibility() {
        $('.pinned-item').addClass('hidden opacity-0');
        if (totalPinned > 0) {
            const active = $(`.pinned-item[data-index="${currentPinnedIndex}"]`);
            active.removeClass('hidden').addClass('animate-fade-in opacity-100');
            $('#pinnedCurrentIndex').text(currentPinnedIndex);
        }
        $('#pinnedUp, #pinnedDown').prop('disabled', totalPinned <= 1);
    }

    $('#pinnedUp').on('click', function () {
        currentPinnedIndex = (currentPinnedIndex <= 1) ? totalPinned : currentPinnedIndex - 1;
        updatePinnedVisibility();
    });

    $('#pinnedDown').on('click', function () {
        currentPinnedIndex = (currentPinnedIndex >= totalPinned) ? 1 : currentPinnedIndex + 1;
        updatePinnedVisibility();
    });

    // Initialize on load if present
    initPinnedCarousel();
});

// Better Scrolling & Highlight
window.scrollToNote = async function (noteId) {
    const targetId = `note-card-${noteId}`;

    // 1. Check if note is ALREADY in the DOM
    let validationEl = document.getElementById(targetId);
    if (validationEl) {
        highlightAndScroll(validationEl);
        return;
    }

    // 2. Fetch from server if missing
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'Locating Note...',
            text: 'Loading timeline context...',
            didOpen: () => { Swal.showLoading() },
            allowOutsideClick: false,
            backdrop: true,
            timer: 15000
        });
    }

    try {
        // Prepare params
        const params = new URLSearchParams({
            noteId: noteId,
            pageSize: 20 // Default used in controllers
        });

        // Add current filters to query if available (Global)
        if (window.currentFilters) {
            if (window.currentFilters.search) params.append('searchQuery', window.currentFilters.search);

            if (window.currentFilters.goalIds && window.currentFilters.goalIds.length > 0) {
                window.currentFilters.goalIds.forEach(id => params.append('filterGoalIds', id));
            } else if(window.currentFilters.goalId) {
                 params.append('filterGoalIds', window.currentFilters.goalId);
            }

            if (window.currentFilters.taskIds && window.currentFilters.taskIds.length > 0) {
                window.currentFilters.taskIds.forEach(id => params.append('filterTaskIds', id));
            } else if(window.currentFilters.taskId) {
                 params.append('filterTaskIds', window.currentFilters.taskId);
            }

            if (window.currentFilters.includeGoalRelatedTasks) {
                params.append('includeGoalRelatedTasks', 'true');
            }
        }
        
        // Context Awareness for Detail Pages
        const sentinel = document.getElementById('infinite-scroll-sentinel');
        if (sentinel) {
            const pageSize = sentinel.dataset.pageSize;
            if(pageSize) params.set('pageSize', pageSize);

            const contextGoalId = sentinel.dataset.goalId;
            if (contextGoalId && (!window.currentFilters || !window.currentFilters.goalIds)) {
                params.append('filterGoalIds', contextGoalId);
            }

            const contextTaskId = sentinel.dataset.taskId;
            if (contextTaskId && (!window.currentFilters || !window.currentFilters.taskIds)) {
                params.append('filterTaskIds', contextTaskId);
            }
        }

        // Call the endpoint
        const response = await fetch(`/Notes/GetPageForNote?${params.toString()}`);

        if (!response.ok) {
            if (response.status === 404) throw new Error("Note not found.");
            throw new Error("Server error");
        }

        // Correctly parse JSON now
        const data = await response.json();

        if (data.status && data.html) {
            const container = document.getElementById('journalItemsContainer') || document.getElementById('notes-container'); 

            // Visual separator for jump (optional)
            if (window.infiniteScrollPage && data.pageNumber > window.infiniteScrollPage) {
                const gap = `
                        <div class="w-full text-center my-8 py-4 bg-gray-50/50 border-y border-gray-100">
                             <div class="text-xs text-uppercase text-gray-400 font-bold tracking-widest">
                                <i class="fas fa-history mr-2"></i> JUMPED TO PAGE ${data.pageNumber}
                             </div>
                        </div>`;
                container.insertAdjacentHTML('beforeend', gap);
            }

            // Update global state if applicable
            if(data.pageNumber) window.infiniteScrollPage = data.pageNumber + 1;

            // Inject HTML
            container.insertAdjacentHTML('beforeend', data.html);

            // --- CLOSE LOADING ---
            if (typeof Swal !== 'undefined') Swal.close();

            // --- POLL FOR ELEMENT ---
            const foundElement = await waitForElement(targetId);

            if (foundElement) {
                setTimeout(() => {
                    highlightAndScroll(foundElement);
                    if (window.Prism) window.Prism.highlightAll();
                }, 100);
            } else {
                console.warn(`Element #${targetId} not found.`);
                if (typeof Swal !== 'undefined') Swal.fire('Warning', 'Note loaded but scrolling failed.', 'warning');
            }
        } else {
            if (typeof Swal !== 'undefined') Swal.fire('Error', 'Could not load timeline data.', 'error');
        }
    } catch (err) {
        console.error(err);
        if (typeof Swal !== 'undefined') {
            Swal.close();
            Swal.fire('Note Not Found', 'This note might be filtered out or deleted.', 'info');
        }
    }
};

// --- Helper: Reliable Polling Wait ---
function waitForElement(id) {
    return new Promise((resolve) => {
        let attempts = 0;
        const maxAttempts = 50; // 50 * 50ms = 2.5s total wait

        const interval = setInterval(() => {
            attempts++;
            const element = document.getElementById(id);

            if (element) {
                clearInterval(interval);
                resolve(element);
            } else if (attempts >= maxAttempts) {
                clearInterval(interval);
                resolve(null); // Failed
            }
        }, 50); // Check every 50ms
    });
}

function highlightAndScroll(element) {
    // 1. Smooth Scroll to center of screen
    element.scrollIntoView({ behavior: 'smooth', block: 'center' });

    // 2. Visual Highlight Logic
    element.classList.remove('note-highlight-active');
    void element.offsetWidth; // Force reflow
    element.classList.add('note-highlight-active');

    // 4. Cleanup
    setTimeout(() => {
        element.classList.remove('note-highlight-active');
    }, 3000);
}