function formatFullDate(date) {
    // If the date is a string, convert it into a Date object
    if (typeof date === "string") {
        date = new Date(date);
    }

    // Check if the date is an instance of Date and if it's a valid date
    if (!(date instanceof Date) || isNaN(date.getTime())) {
        throw new Error("Invalid Date Format");
    }

    // Define the options for the date format
    const options = {
        weekday: 'long',   // Full weekday name (e.g., Monday)
        year: 'numeric',
        month: 'long',     // Full month name (e.g., December)
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
    };

    // Format the date using 'en-IN' locale (Indian English)
    const formattedDate = new Intl.DateTimeFormat('en-IN', options).format(date);

    return formattedDate;
}

function formatShortDate(date) {
    // If the date is a string, convert it into a Date object
    if (typeof date === "string") {
        date = new Date(date);
    }

    // Check if the date is an instance of Date and if it's a valid date
    if (!(date instanceof Date) || isNaN(date.getTime())) {
        throw new Error("Invalid Date Format");
    }

    // Define the options for the short date format
    const options = {
        year: 'numeric',
        month: 'short',  // Short month name (e.g., Dec)
        day: '2-digit',  // 2-digit day (e.g., 10)
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
    };

    // Format the date using 'en-IN' locale (Indian English)
    const formattedDate = new Intl.DateTimeFormat('en-IN', options).format(date);

    return formattedDate;
}

function returnStatusBadge(status) {
    const parsedStatus = Number.parseInt(status);

    // Base badge class
    const baseClass = 'inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white rounded-full shadow-md transition duration-300 min-w-max';

    // Badge styles by status code
    const statusStyles = {
        0: { label: 'Not Started', bg: 'bg-yellow-400 hover:bg-yellow-500' },
        1: { label: 'Running', bg: 'bg-blue-500 hover:bg-blue-600' },
        2: { label: 'Completed', bg: 'bg-green-500 hover:bg-green-600' },
        3: { label: 'End', bg: 'bg-gray-400 hover:bg-gray-500' },
        4: { label: 'Deleted', bg: 'bg-red-500 hover:bg-red-600' },
    };

    const fallback = {
        label: 'Unknown Type',
        bg: 'bg-gray-900 hover:bg-gray-950',
    };

    const badge = statusStyles[parsedStatus] || fallback;

    return `<span class="${baseClass} ${badge.bg}">${badge.label}</span>`;
}


function returnPriorityBadge(status) {

    // Ensure the status is a valid number (not NaN, not undefined, etc.)
    const data = Number.parseInt(status);

    // Validate if 'status' is a valid number
    if (isNaN(data) || typeof data !== 'number') {
        return '<span class="inline-flex items-center px-3 py-1 text-xs sm:text-sm font-semibold text-white bg-gray-900 rounded-full shadow-md hover:bg-yellow-500 transition duration-300 min-w-max">Unknown Type</span>';
        // Return an error message or default badge if invalid
    }

    switch (data) {
        case 2:
            return '<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-red-500 rounded-full">High</span>';
        case 1:
            return '<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-green-500 rounded-full">Medium</span>';
        case 0:
            return '<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-gray-500 rounded-full">Low</span>';
        default:
            return '<span class="inline-flex items-center px-3 py-1 text-sm font-medium text-white bg-yellow-500 rounded-full">Unknown Type</span>';
    }
}
