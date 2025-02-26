
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

        if (reRender) {
            reRender();
        }
    }

    return [ getData, setData ];
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
    const [ getData, setData ] = useDataState(null); // state data...

    // 🔐 Private HTML template
    const modalHtml =
        `<div id="Modal" class="fixed inset-0 z-20 flex hidden w-full max-h-full items-center justify-center bg-gray-800 bg-opacity-50">
    </div>`;

    const modalLoadingHtml = `
        <div class="flex h-full w-full flex-col items-center justify-center space-y-4 bg-white">
            <div class="flex items-center space-x-3">
                <div class="h-10 w-10 animate-spin rounded-full border-4 border-indigo-500 border-t-transparent"></div>
                <h2 class="text-xl font-semibold text-gray-700">Loading Model Data...</h2>
            </div>
            <p class="text-sm text-gray-500">Please wait while we fetch the latest updates.</p>
        </div>
    `;

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
        }
        else {
            console.error("Modal is already initialized.");
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
            if (data) {
                component?.load(modalContainer)
            } else {
                showErrorNotification("Falied To Goal Data Model!");
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

            if (data) {
                component?.load(modalContainer);
            }
        } catch (error) {
            console.error(error);
        }
    }

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
    }

    this.closeModal = function () {
        if (isModalInit && modalContainer?.length) {
            isModalOpen = false;
            modalContainer.addClass("hidden");
            modalContainer.empty
        }
    };

    this.isModalOpen = function () {
        return isModalOpen;
    };

    // ✅ Store the instance in the constructor function itself
    Modal.instance = this;
}

const modal = new Modal();
modal.init();