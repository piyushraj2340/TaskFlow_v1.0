$(document).ready(function () {
    // --- Badge Helper Functions (Provided by you) ---
    function returnStatusBadge(status) {
        const parsedStatus = Number.parseInt(status);
        const baseClass = 'inline-flex items-center px-2 py-1 text-xs font-semibold text-white rounded-full min-w-max';
        const statusStyles = {
            0: { label: 'Not Started', bg: 'bg-yellow-400' },
            1: { label: 'Running', bg: 'bg-blue-500' },
            2: { label: 'Completed', bg: 'bg-green-500' },
            3: { label: 'End', bg: 'bg-gray-400' },
            4: { label: 'Deleted', bg: 'bg-red-500' },
        };
        const fallback = { label: 'Unknown', bg: 'bg-gray-900' };
        const badge = statusStyles[parsedStatus] || fallback;
        return `<span title="Status" class="${baseClass} ${badge.bg}">${badge.label}</span>`;
    }

    function returnPriorityBadge(priority) {
        const data = Number.parseInt(priority);
        if (isNaN(data)) {
            return ''; // Return nothing if priority is not a valid number
        }
        const baseClass = 'inline-flex items-center px-2 py-1 text-xs font-semibold text-white rounded-full min-w-max';
        switch (data) {
            case 2:
                return `<span title="Priority" class="${baseClass} bg-red-500">High</span>`;
            case 1:
                return `<span title="Priority" class="${baseClass} bg-green-500">Medium</span>`;
            case 0:
                return `<span title="Priority" class="${baseClass} bg-gray-500">Low</span>`;
            default:
                return ''; // Return nothing for unknown types
        }
    }

    // --- Original Menu and Dropdown Logic (Unchanged) ---
    $('#hamburgerBtn').on('click', function (event) {
        event.stopPropagation();
        $('#mobileMenu').toggleClass('hidden');
    });
    // ... (rest of your unchanged menu/dropdown code) ...
    $('#desktopFilterBtn').on('click', function (event) {
        event.stopPropagation();
        $('#desktopFilterMenu').toggleClass('hidden');
    });
    $('#mobileFilterBtn').on('click', function (event) {
        event.stopPropagation();
        $('#mobileFilterMenu').toggleClass('hidden');
    });
    function setupFilterMenu(menuSelector, labelSelector, inputSelector) {
        $(menuSelector).on('click', 'a', function (e) {
            e.preventDefault();
            const filterValue = $(this).data('filter');
            $(labelSelector).text(filterValue);
            $(inputSelector).val(filterValue);
            $(menuSelector).addClass('hidden');
        });
    }
    setupFilterMenu('#desktopFilterMenu', '#desktopFilterLabel', '#desktopFilterInput');
    setupFilterMenu('#mobileFilterMenu', '#mobileFilterLabel', '#mobileFilterInput');
    $(document).on('click', function () {
        $('#desktopFilterMenu').addClass('hidden');
        $('#mobileFilterMenu').addClass('hidden');
    });


    // --- Global Search Handler ---
    const $searchInput = $('input[name="query"]');
    const $filterInput = $('input[name="filter"]');
    const $resultsBox = $('#globalSearchResults');
    let lastSearchTerm = '';
    let lastFilter = 'All';
    let debounceTimer = null;

    // Helper: Build result HTML (★★★ MODIFIED SECTION ★★★)
    function buildResultHtml(results) {
        if (!results || results.length === 0) {
            return '<div class="p-4 text-gray-500">No results found.</div>';
        }
        return results.map(r => {
            // Check for priority/status and generate badges. Check for null because 0 is a valid value.
            const priorityBadge = r.priority != null ? returnPriorityBadge(r.priority) : '';
            const statusBadge = r.status != null ? returnStatusBadge(r.status) : '';
            const timeStampHtml = r.timeStamp ? `<time datetime="${r.timeStamp}">Created: ${new Date(r.timeStamp).toLocaleDateString()}</time>` : '';
            const pinnedIcon = r.isPinned ? '📌' : '';

            return `
                <a href="${r.url}" class="block px-4 py-3 border-b last:border-b-0 hover:bg-indigo-50 transition">
                    <div class="flex justify-between items-start">
                        <div class="font-semibold text-indigo-700">
                            ${r.title} <span class="text-sm">${pinnedIcon}</span>
                        </div>
                        <div class="text-xs font-bold text-gray-400 capitalize">${r.type}</div>
                    </div>
                    <div class="text-sm text-gray-600 mt-1">${r.snippet || ''}</div>
                    <div class="mt-2 flex items-center flex-wrap gap-2 text-xs text-slate-500">
                        ${priorityBadge}
                        ${statusBadge}
                        ${timeStampHtml}
                    </div>
                </a>
            `;
        }).join('');
    }

    // --- Rest of the search logic (Unchanged) ---
    function showResults(results) {
        $resultsBox.html(buildResultHtml(results)).removeClass('hidden');
    }
    function hideResults() {
        $resultsBox.addClass('hidden').empty();
    }
    function positionResultsBox() {
        const nav = $('nav.bg-indigo-600');
        if (nav.length) {
            $resultsBox.css({
                top: nav.offset().top + nav.outerHeight(),
                left: nav.offset().left,
                width: nav.outerWidth()
            });
        }
    }
    function performSearch(term, filter) {
        if (!term || term.length < 2) {
            hideResults();
            return;
        }
        lastSearchTerm = term;
        lastFilter = filter;
        let apiType = filter.toLowerCase().replace('/journal', '');
        if (apiType === 'all') apiType = '';

        $.get('/Search/Query', { q: term, pageNumber: 1, pageSize: 10 }, function (resp) {
            if (resp && resp.status && Array.isArray(resp.data)) {
                let filtered = resp.data;
                if (apiType) {
                    filtered = filtered.filter(r => r.type === apiType);
                }
                showResults(filtered);
                positionResultsBox();
            } else {
                hideResults();
            }
        });
    }

    $searchInput.on('input', function () {
        const term = $(this).val();
        const filter = $filterInput.val();
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => performSearch(term, filter), 250);
    });

    $('input[name="filter"]').on('change', function () {
        if (lastSearchTerm) performSearch(lastSearchTerm, $(this).val());
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#globalSearchResults, input[name="query"], #desktopFilterBtn, #mobileFilterBtn').length) {
            hideResults();
        }
    });

    $(document).on('keydown', function (e) {
        if (e.key === 'Escape') hideResults();
    });

    $resultsBox.on('click', 'a', function () {
        hideResults();
    });

    $(window).on('resize', positionResultsBox);

    $('form[action="/Search/Query"]').on('submit', function (e) {
        e.preventDefault();
        const term = $(this).find('input[name="query"]').val();
        const filter = $(this).find('input[name="filter"]').val();
        performSearch(term, filter);
    });
});