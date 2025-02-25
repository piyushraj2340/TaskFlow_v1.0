
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

    const getData = () => data.value;

    const setData = (updateData, reRender) => {
        data.value = updateData;

        console.log("value", data.value);
        console.log("copy", data.copy);
        if (reRender) {
            reRender();
        }
    }

    return { getData, setData };
}

function Modal() {
    // 🔴 Ensure only one instance is created
    if (Modal.instance) {
        return Modal.instance;
    }

    // 🔐 Private variables
    let isModalInit = false;
    let isModalOpen = false;

    // 🔐 Private Selector
    let modalContainer = null;

    // Handel the state...
    const { getData, setData } = useDataState(null); // state data...

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

    //1. Add - Remove Components inside the components we will manage the getData()....
    //2. re-draw - re-render the components....
    //3. handel all the other functionality related to the event will mange by the component itself....

    const config = (parrentSellector) => {
        if (!isModalInit && !modalContainer) {

            if (parrentSellector) {
                parrentSellector.append(modalHtml)
            } else {
                $("body").prepend(modalHtml); // to get access by all the script file
            }

            isModalInit = true;
            modalContainer = $("#Modal");

            // Background close
            if (modalContainer?.length) {
                modalContainer.on('click', function (e) {
                    if (e.target === this || $(e.target).closest("#closeModal").length) {
                        closeModal();
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
    this.init = function (parrentSellector) {
        config(parrentSellector);
    };

    this.isModalInit = function () {
        return isModalInit;
    };

    loadModalContentAsync = async function (ComponentClass, id) {
        try {
            if (!ComponentClass) {
                throw new Error("Missing Model Content!");
            }
            modalContainer.html(modalLoadingHtml);

            const component = new ComponentClass({ getData, setData }); // pass data as props

            const data = await component?.initAsync(id);
            console.log(data);
            if (data) {
                component?.load(modalContainer)
            }
        } catch (error) {
            console.error(error);
        }
    }

    loadModalContent = async function (ComponentClass) {
        try {
            if (!ComponentClass) {
                throw new Error("Missing Model Content!");
            }
            modalContainer.html(modalLoadingHtml);

            const component = new ComponentClass({ getData, setData }); // pass data as props

            const data = await component?.init();
            console.log(data);
            if (data) {
                component?.load(modalContainer);
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

    this.openModal = function (ComponentClass, id) {
        if (isModalInit && modalContainer?.length) {

            modalContainer.removeClass("hidden");
            isModalOpen = true;

            if (!id) {
                loadModalContent(ComponentClass);
            } else {
                loadModalContentAsync(ComponentClass, id);
            }
        }
    }

    closeModal = function () {
        if (isModalInit && modalContainer?.length) {
            if (!getData()?.isModified) {
                isModalOpen = false;
                modalContainer.addClass("hidden");
                modalContainer.empty();
            }
            else {

                const defaultOptions = {
                    title: 'Are you sure?',
                    text: 'Do you want to close the modal? You will lose all the changes!!',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Yes, Close it!',
                    cancelButtonText: 'Cancel',
                    buttonsStyling: false,
                    customClass: {
                        confirmButton: 'bg-red-500 hover:bg-red-600 text-white font-bold py-2 px-4 rounded mr-2',
                        cancelButton: 'bg-gray-300 hover:bg-gray-400 text-black font-bold py-2 px-4 rounded',
                    }
                };

                // Show SweetAlert2 dialog
                Swal.fire(defaultOptions).then((result) => {
                    if (result.isConfirmed) {
                        sModalOpen = false;
                        modalContainer.addClass("hidden");
                        modalContainer.empty();
                    }
                });
            }
        }

        //resetState();
    }

    this.closeModal = closeModal;

    this.isModalOpen = function () {
        return isModalOpen;
    };


    resetModal = function () {
        isModalInit = false;
        isModalOpen = false;

        modalContainer.remove();
        modalContainer = null;
        Modal.instance = null;
    }

    resetData = function () {
        isModalInit = false;
        isModalOpen = false;
    }

    // ✅ Store the instance in the constructor function itself
    Modal.instance = this;
}

const modal = new Modal();
modal.init();


function NotesComponentsTesting() {
    let [getData, setData] = useDataState(null);

    let isEditMode = false;

    this.init = function () {
        setData({
            id: "",
            title: "",
            content: "",
            tags: "",
            isPinned: false,
        });

        isEditMode = false;

        return true;
    }

    this.initAsync = function () {
        return new Promise((resolve, reject) => {
            isEditMode = true;

            $.ajax({
                url: `/Notes/Details/8`,
                method: "POST",
                success: function (response) {
                    setData(response.data);
                    console.log(getData());
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
            <form asp-action="Create" asp-controller="Notes" data-action-note-form="create" data-noteId="${getData()?.id}" method="post" enctype="multipart/form-data" id="noteForm">
                <input type="hidden" name="__RequestVerificationToken" value="@Html.AntiForgeryToken()" />
                <div class="mb-4" id="note-id-field">
                    <label for="noteid" class="mb-1 block font-semibold text-gray-700">Note Id</label>
                    <input id="noteid" name="id" class="w-full rounded-md border border-gray-300 p-2" value="${getData()?.id}" readonly>
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
                    <input type="text" id="tags" name="tags" class="w-full rounded-md border border-gray-300 p-2" onchange="onChangeNotesInput(this)" value="${getData()?.tags}">
                </div>
                <div class="mb-4 flex items-center">
                    <input id="IsPinned" name="IsPinned" type="checkbox" onchange="onChangeNotesInput(this)" ${getData()?.isPinned ? 'checked' : ''}>
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
            if (e.name === "IsPinned") {
                setData({ ...getData(), [e.name]: e.checked });
            } else {
                setData({ ...getData(), [e.name]: e.value });
            }

            console.log(getData())
        }

        $("#noteForm").on('submit', function (e) {
            e.preventDefault();
            try {
                $.ajax({
                    url: isEditMode ? `/Notes/Edit/8` : "/Notes/Create/10/2",
                    method: "POST",
                    data: getData(),
                    success: function (response) {
                        if (response.status) {
                            setTimeout(() => {
                                modal.closeModal();
                            }, 500)
                            showSuccessNotification(response.message || "Notes Saved with Goal!");

                            console.log(response.message || "Notes Saved with Goal!")

                        } else {
                            throw new Error(response.message);
                        }
                    },
                    error: function (xhr, status, error) {
                        throw error;
                    }
                });
            } catch (error) {
                showErrorNotification(error.message || "Notes Saved Failed!");
                console.error("Error:", error);
            }
        })

    }

    this.load = function (selector) {
        reRender(selector);
    }
}