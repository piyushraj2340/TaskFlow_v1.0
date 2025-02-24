$(document).ready(function () {
    $('#menuButton').click(function () {
        $('#mobileMenu').toggleClass('hidden');
    });
});


//function modalComponent() {


//}

//let headerUI = `
//<div class="flex items-center justify-between bg-indigo-600 px-6 py-4 text-white">
//    <h2 id="note-title" class="text-xl font-semibold tracking-wide">
//        <i class="fa-solid fa-clipboard-list"></i> Create a Progress Record in Goal
//    </h2>
//    <button class="text-2xl text-white hover:text-gray-200" id="closeModal">
//        <i class="fa-solid fa-xmark"></i>
//    </button>
//</div>
//`;

//let bodyUI = `
//<div class="p-6">
//    <p class="mb-4 text-sm text-gray-600">Add Goal Progress with Goal Title</p>
//    <form asp-action="Create" asp-controller="Notes" data-action-note-form="create" data-noteId="${noteId}" method="post" enctype="multipart/form-data" id="noteForm">
//        <input type="hidden" name="__RequestVerificationToken" value="@Html.AntiForgeryToken()" />
//        <div class="mb-4" id="note-id-field">
//            <label for="noteid" class="mb-1 block font-semibold text-gray-700">Note Id</label>
//            <input id="noteid" name="id" class="w-full rounded-md border border-gray-300 p-2" value="${noteId}" readonly>
//        </div>
//        <div class="mb-4">
//            <label for="title" class="mb-1 block font-semibold text-gray-700">Title</label>
//            <input id="title" name="title" class="w-full rounded-md border border-gray-300 p-2" value="${title}" required>
//        </div>
//        <div class="mb-4">
//            <label for="content" class="mb-1 block font-semibold text-gray-700">Content</label>
//            <textarea id="content" name="content" rows="4" class="w-full rounded-md border border-gray-300 p-2" required>${content}</textarea>
//        </div>
//        <div class="mb-4">
//            <label for="tags" class="mb-1 block font-semibold text-gray-700">Tags (Comma separated)</label>
//            <input type="text" id="tags" name="tags" class="w-full rounded-md border border-gray-300 p-2" value="${tags}">
//        </div>
//        <div class="mb-4 flex items-center">
//            <input id="IsPinned" name="IsPinned" type="checkbox" ${isPinned ? 'checked' : ''}>
//            <label for="IsPinned" class="font-semibold text-gray-700">
//                <i class="fa-solid fa-thumbtack"></i> Pin this note
//            </label>
//        </div>
//        <div class="mb-4">
//            <label class="mb-1 block font-semibold text-gray-700">
//                <i class="fa-solid fa-paperclip"></i> Attachments
//            </label>
//            <div class="flex items-center">
//                <label class="flex cursor-pointer items-center space-x-2 rounded-md bg-indigo-600 px-4 py-2 text-white">
//                    <i class="fa-solid fa-upload"></i>
//                    <span>Upload File</span>
//                    <input type="file" id="attachments" name="attachments" class="hidden" value="${attachments}">
//                </label>
//            </div>
//        </div>
//        <div class="mb-4">
//            <label class="mb-1 block font-semibold text-gray-700">
//                <i class="fa-solid fa-palette"></i> Choose Color
//            </label>
//            <div class="flex items-center space-x-3">
//                <label class="cursor-pointer">
//                    <input type="radio" name="theme" value="theme-red" ${theme === 'theme-red' ? 'checked' : ''}>
//                    <div class="h-10 w-10 rounded-full bg-red-500"></div>
//                </label>
//                <label class="cursor-pointer">
//                    <input type="radio" name="theme" value="theme-blue" ${theme === 'theme-blue' ? 'checked' : ''}>
//                    <div class="h-10 w-10 rounded-full bg-blue-500"></div>
//                </label>
//                <label class="cursor-pointer">
//                    <input type="radio" name="theme" value="theme-green" ${theme === 'theme-green' ? 'checked' : ''}>
//                    <div class="h-10 w-10 rounded-full bg-green-500"></div>
//                </label>
//            </div>
//        </div>
//        <div class="flex justify-end rounded-b-lg border-t p-4">
//            <button type="submit" id="saveNotesBtn" class="flex items-center space-x-2 rounded-md bg-indigo-600 px-6 py-2 text-white">
//                <i class="fa-solid fa-save"></i>
//                <span>Save Note</span>
//            </button>
//        </div>
//    </form>
//</div>
//`


//const noteModalTemplate = `
//<div id="noteModal" class="fixed inset-0 z-20 flex hidden w-full items-center justify-center bg-gray-800 bg-opacity-50">
//    <div class="custom-scrollbar relative max-h-[90vh] w-full max-w-xl overflow-auto rounded-lg bg-white shadow-xl sm:w-4/5 lg:w-1/2 xl:w-1/3">
//        ${headerUI}
//        ${bodyUI}
//        ${footerUI}
//    </div>
//</div>`;

//function ModalComponent({ isModalOpen }) {


//    return `
//    <div id="Modal" class="fixed inset-0 z-20 flex hidden w-full items-center justify-center bg-gray-800 bg-opacity-50">
//        <div class="custom-scrollbar relative max-h-[90vh] w-full max-w-xl overflow-auto rounded-lg bg-white shadow-xl sm:w-4/5 lg:w-1/2 xl:w-1/3">
//            ${headerUI}
//            ${bodyUI}
//            ${footerUI}
//        </div>
//    </div>`;
//}

const noContentHtml = `
    <div class="inset-0 z-50 flex items-center justify-center bg-gray-800 bg-opacity-50">
        <div class="flex w-full flex-col items-center justify-center space-y-4 bg-white">
            <div class="flex flex-col items-center space-y-3">
                <i class="fa-solid fa-box-open text-5xl text-gray-400"></i>
                <h2 class="text-xl font-semibold text-gray-700">No Content Found</h2>
                <p class="text-sm text-gray-500">It looks like there's nothing to display here.</p>
            </div>
        </div>
    </div>
`;

function useDataState(initialData) {
    let data = {
        value: initialData
    };

    function getData() {
        return data.value; // Always return the latest value
    }

    function setData(updateData, reRender) {
        data.value = updateData;
        console.log(data);

        if (reRender) {
            reRender();
        }
    }

    return [getData, setData]
}

function NotesComponents() {
    let [getData, setData] = useDataState(null);

    this.init = function () {
        setData({
            Id: "",
            title: "",
            content: "",
            tags: "",
            isPinned: "",
        });

        return true;
    }

    this.initAsync = function () {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: `/Notes/Details/8`,
                method: "POST",
                success: function (response) {
                    setData(response.data);
                    console.log(getData().value);
                    return resolve(response.data);
                },
                error: function (xhr, status, error) {
                    console.error("Error:", error);
                    return reject(error);
                }
            });
        });
    };

    const reRender = function (parrentElement) {
        const contentWithData = `
            <p class="mb-4 text-sm text-gray-600">${getData()?.title}</p>
            <form asp-action="Create" asp-controller="Notes" data-action-note-form="create" data-noteId="${getData()?.noteId}" method="post" enctype="multipart/form-data" id="noteForm">
                <input type="hidden" name="__RequestVerificationToken" value="@Html.AntiForgeryToken()" />
                <div class="mb-4" id="note-id-field">
                    <label for="noteid" class="mb-1 block font-semibold text-gray-700">Note Id</label>
                    <input id="noteid" name="id" class="w-full rounded-md border border-gray-300 p-2" value="${getData()?.Id}" readonly>
                </div>
                <div class="mb-4">
                    <label for="title" class="mb-1 block font-semibold text-gray-700">Title</label>
                    <input id="title" name="title" class="w-full rounded-md border border-gray-300 p-2" onchange="onChangeNotesInput(this)" value="${getData()?.title}" required>
                </div>
                <div class="mb-4">
                    <label for="content" class="mb-1 block font-semibold text-gray-700">Content</label>
                    <textarea id="content" name="content" rows="4" class="w-full rounded-md border border-gray-300 p-2" onchange="onChangeNotesInput(this)" required>${getData()?.content}</textarea>
                </div>
                <div class="mb-4">
                    <label for="tags" class="mb-1 block font-semibold text-gray-700">Tags (Comma separated)</label>
                    <input type="text" id="tags" name="tags" class="w-full rounded-md border border-gray-300 p-2" value="${getData()?.tags}">
                </div>
                <div class="mb-4 flex items-center">
                    <input id="IsPinned" name="IsPinned" type="checkbox" ${getData()?.isPinned ? 'checked' : ''}>
                    <label for="IsPinned" class="font-semibold text-gray-700">
                        <i class="fa-solid fa-thumbtack"></i> Pin this note
                    </label>
                </div>
                <div class="mb-4">
                    <label class="mb-1 block font-semibold text-gray-700">
                        <i class="fa-solid fa-paperclip"></i> Attachments
                    </label>
                    <div class="flex items-center">
                        <label class="flex cursor-pointer items-center space-x-2 rounded-md bg-indigo-600 px-4 py-2 text-white">
                            <i class="fa-solid fa-upload"></i>
                            <span>Upload File</span>
                            <input type="file" id="attachments" name="attachments" class="hidden" value="${getData()?.attachments}">
                        </label>
                    </div>
                </div>
                <div class="mb-4">
                    <label class="mb-1 block font-semibold text-gray-700">
                        <i class="fa-solid fa-palette"></i> Choose Color
                    </label>
                    <div class="flex items-center space-x-3">
                        <label class="cursor-pointer">
                            <input type="radio" name="theme" value="theme-red" ${getData()?.theme === 'theme-red' ? 'checked' : ''}>
                            <div class="h-10 w-10 rounded-full bg-red-500"></div>
                        </label>
                        <label class="cursor-pointer">
                            <input type="radio" name="theme" value="theme-blue" ${getData()?.theme === 'theme-blue' ? 'checked' : ''}>
                            <div class="h-10 w-10 rounded-full bg-blue-500"></div>
                        </label>
                        <label class="cursor-pointer">
                            <input type="radio" name="theme" value="theme-green" ${getData()?.theme === 'theme-green' ? 'checked' : ''}>
                            <div class="h-10 w-10 rounded-full bg-green-500"></div>
                        </label>
                    </div>
                </div>
                <div class="flex justify-end rounded-b-lg border-t p-4">
                    <button type="submit" id="saveNotesBtn" class="flex items-center space-x-2 rounded-md bg-indigo-600 px-6 py-2 text-white">
                        <i class="fa-solid fa-save"></i>
                        <span>Save Note</span>
                    </button>
                </div>
            </form>
        `;

        const content = getData() ? contentWithData : noContentHtml;
        parrentElement.html(`
            <div class="custom-scrollbar relative h-full sm:max-h-[90vh] w-full max-w-xl overflow-auto rounded-lg bg-white shadow-xl sm:w-4/5 lg:w-1/2 xl:w-1/3">
                <div class="flex items-center justify-between bg-indigo-600 px-6 py-4 text-white">
                    <h2 id="note-title" class="text-xl font-semibold tracking-wide">
                        <i class="fa-solid fa-clipboard-list"></i> Create a Progress Record in Goal
                    </h2>
                    <button class="text-2xl text-white hover:text-gray-200" id="closeModal">
                        <i class="fa-solid fa-xmark"></i>
                    </button>
                </div>
                <div class="p-6">
                    ${content}
                </div>
            </div>
        `);

        window.onChangeNotesInput = function (e) {
            setData({ ...getData(), [e.name]: e.value });
            console.log(getData());
        }

        $("#noteForm").on('submit', function (e) {
            e.preventDefault();
            $.ajax({
                url: `/Notes/Edit/8`,
                method: "POST",
                data: getData(),
                success: function (response) {
                    showSuccessNotification(response.message || "Notes Saved with Goal!");

                    setTimeout(() => {
                        $("#noteModal").fadeOut();
                    }, 500)

                    console.log(response.message || "Notes Saved with Goal!")
                },
                error: function (xhr, status, error) {
                    showErrorNotification(error.message || "Notes Saved Failed!");
                    console.error("Error:", error);
                }
            });
        })

    }

    this.load = function (selector) {
        reRender(selector);
    }
}

function Modal() {
    // 🔴 Ensure only one instance is created
    if (Modal.instance) {
        return Modal.instance;
    }

    // 🔐 Private variables
    let isModalInit = false;
    let isModalOpen = false;

    // 🔐 Private HTML template
    const modalHtml =
        `<div id="Modal" class="fixed inset-0 z-20 flex hidden w-full max-h-[100vh] items-center justify-center bg-gray-800 bg-opacity-50">
    </div>`;

    const modalLoadingHtml = `
        <div class="flex h-full w-full flex-col items-center justify-center space-y-4 bg-white">
            <div class="flex items-center space-x-3">
                <div class="h-10 w-10 animate-spin rounded-full border-4 border-indigo-500 border-t-transparent"></div>
                <h2 class="text-xl font-semibold text-gray-700">Loading getData()...</h2>
            </div>
            <p class="text-sm text-gray-500">Please wait while we fetch the latest updates.</p>
        </div>
    `;

    // 🔐 Private Selector
    let modalContainer = null;

    //1. Add - Remove Components inside the components we will manage the getData()....
    //2. re-draw - re-render the components....
    //3. handel all the other functionality related to the event will mange by the component itself....

    const config = () => {
        if (!isModalInit && !modalContainer) {
            $("body").prepend(modalHtml);
            isModalInit = true;

            modalContainer = $("#Modal");

            // Background close
            if (modalContainer?.length) {
                modalContainer.on('click', function (e) {
                    if (e.target === this || $(e.target).closest("#closeModal").length) {
                        toggleModal();
                    }
                });
            }

            console.log("Modal initialized, With Config!");
        }
        else {
            console.log("Modal is already initialized.");
        }
    }

    // 🌍 Public methods
    this.init = function () {
        config();
    };

    this.isModalInit = function () {
        return isModalInit;
    };

    loadModalContentAsync = async function (components) {
        try {
            modalContainer.html(modalLoadingHtml);
            const data = await components?.init();
            console.log(data);
            if (data) {
                components?.load(modalContainer)
            }
        } catch (error) {
            console.error(error);
        }
    }

    const toggleModal = function () {
        if (isModalInit && modalContainer?.length) {
            modalContainer.toggleClass("hidden");

            isModalOpen = !isModalOpen;

            console.log(`Modal is now ${isModalOpen ? "open" : "closed"}`);
        } else {
            console.warn("Modal is not initialized! Call modal.init() first.");
        }
    };

    this.toggleModal = toggleModal;

    this.openModal = function (component) {
        if (isModalInit && modalContainer?.length) {
            modalContainer.removeClass("hidden");
            isModalOpen = true;
            const newComponent = new NotesComponents();
            loadModalContentAsync(newComponent);
        }
    }

    this.closeModel = function () {
        if (isModalInit && modalContainer?.length) {
            isModalOpen = false;
            modalContainer.addClass("hidden");
            modalContainer.empty();
        }
    }

    this.isModalOpen = function () {
        return isModalOpen;
    };

    // ✅ Store the instance in the constructor function itself
    Modal.instance = this;
}

const modal = new Modal();
modal.init();

