document.addEventListener('DOMContentLoaded', () => {
    // --- STATE MANAGEMENT ---
    let appState = {
        collections: [],
        activeCollectionId: null,
        activeItems: []
    };

    // --- API HELPER ---
    const getAntiForgeryToken = () => document.querySelector('input[name="__RequestVerificationToken"]').value;

    const api = async (url, method = 'GET', body = null) => {
        const options = {
            method,
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken() // If you use Cookie Auth
                // 'Authorization': 'Bearer ' + token // If you use JWT, add logic to get token
            }
        };
        if (body) options.body = JSON.stringify(body);

        toggleLoading(true);
        try {
            const response = await fetch(url, options);
            toggleLoading(false);
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({}));
                throw new Error(errorData.message || `API Error: ${response.status}`);
            }
            // Return null for 204 No Content
            return response.status === 204 ? null : await response.json();
        } catch (error) {
            toggleLoading(false);
            alert(error.message);
            console.error(error);
            return null;
        }
    };

    // --- DOM ELEMENTS ---
    const collectionsList = document.getElementById('collections-list');
    const welcomeView = document.getElementById('welcome-view');
    const collectionView = document.getElementById('collection-view');
    const collectionTitle = document.getElementById('collection-title');
    const itemsGrid = document.getElementById('items-grid');
    const noItemsMessage = document.getElementById('no-items-message');
    const globalSearchInput = document.getElementById('global-search-input');
    const loadingSpinner = document.getElementById('loading-spinner');

    // Buttons
    const editCollectionBtn = document.getElementById('edit-collection-btn');
    const deleteCollectionBtn = document.getElementById('delete-collection-btn');

    // Modals & Forms
    const collectionModal = document.getElementById('collection-modal');
    const itemModal = document.getElementById('item-modal');
    const collectionForm = document.getElementById('collection-form');
    const itemForm = document.getElementById('item-form');

    // Inputs
    const collectionIdInput = document.getElementById('collection-id-input');
    const collectionNameInput = document.getElementById('collection-name-input');
    const itemIdInput = document.getElementById('item-id-input');
    const itemNameInput = document.getElementById('item-name-input');
    const itemDescriptionInput = document.getElementById('item-description-input');

    // Tagify
    let categoriesTagify, tagsTagify;

    // --- UTILITY ---
    const toggleLoading = (show) => {
        if (show) loadingSpinner.classList.remove('hidden');
        else loadingSpinner.classList.add('hidden');
    };

    const parseTagifyValues = (tagifyInstance) => {
        if (!tagifyInstance || !tagifyInstance.value) return [];
        return tagifyInstance.value.map(tag => tag.value);
    };

    // --- DATA FUNCTIONS ---

    // Fetch all collections
    const fetchCollections = async () => {
        const data = await api('/api/collections');
        if (data) {
            appState.collections = data;
            renderSidebar();
        }
    };

    // Fetch items for a specific collection
    const fetchItems = async (collectionId) => {
        const data = await api(`/api/collections/${collectionId}/items`);
        if (data) {
            appState.activeItems = data;
            renderMainContent();
        }
    };

    // --- RENDER FUNCTIONS ---

    const renderSidebar = () => {
        collectionsList.innerHTML = '';
        if (appState.collections.length === 0) {
            collectionsList.innerHTML = `<p class="px-4 text-sm text-slate-500">No collections yet.</p>`;
            return;
        }

        appState.collections.forEach(collection => {
            const isActive = collection.collectionId === appState.activeCollectionId;
            const link = document.createElement('div');
            link.className = `cursor-pointer px-4 py-2 rounded-md font-medium transition-colors flex justify-between items-center group ${isActive
                ? 'bg-blue-100 text-blue-700'
                : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900'
                }`;

            link.innerHTML = `
                <span class="truncate">${collection.name}</span>
                <span class="text-xs bg-gray-200 text-gray-600 px-2 py-0.5 rounded-full">${collection.itemCount || 0}</span>
            `;

            link.addEventListener('click', () => {
                appState.activeCollectionId = collection.collectionId;
                renderSidebar(); // Re-render to update active class
                fetchItems(collection.collectionId);
            });

            collectionsList.appendChild(link);
        });
    };

    const renderItems = (items) => {
        itemsGrid.innerHTML = '';
        if (!items || items.length === 0) {
            noItemsMessage.classList.remove('hidden');
            return;
        }
        noItemsMessage.classList.add('hidden');

        items.forEach(item => {
            const card = document.createElement('div');
            card.className = 'bg-white rounded-lg shadow-md p-4 flex flex-col gap-3 border hover:shadow-lg transition-shadow relative group';

            // Map category/tag objects to HTML
            const catsHtml = item.categories ? item.categories.map(c => `<span class="bg-indigo-100 text-indigo-800 text-xs font-medium px-2.5 py-0.5 rounded-full">${c.name || c.value || c}</span>`).join('') : '';
            const tagsHtml = item.tags ? item.tags.map(t => `<span class="bg-gray-200 text-gray-800 text-xs font-medium px-2.5 py-0.5 rounded-full">${t.name || t.value || t}</span>`).join('') : '';

            card.innerHTML = `
                <div class="flex justify-between items-start">
                    <h4 class="text-lg font-bold text-slate-800 line-clamp-1" title="${item.name}">${item.name}</h4>
                    <div class="flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                        <button class="edit-item-btn text-slate-400 hover:text-blue-500" data-item-id="${item.itemId}"><svg class="w-5 h-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="m16.862 4.487 1.687-1.688a1.875 1.875 0 1 1 2.652 2.652L6.832 19.82a4.5 4.5 0 0 1-1.897 1.13l-2.685.8.8-2.685a4.5 4.5 0 0 1 1.13-1.897L16.863 4.487Zm0 0L19.5 7.125" /></svg></button>
                        <button class="delete-item-btn text-slate-400 hover:text-red-500" data-item-id="${item.itemId}"><svg class="w-5 h-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.134-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.067-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" /></svg></button>
                    </div>
                </div>
                <p class="text-slate-600 text-sm line-clamp-2">${item.description || ''}</p>
                <div class="mt-auto">
                    ${catsHtml ? `<div class="mb-1 flex flex-wrap gap-1">${catsHtml}</div>` : ''}
                    ${tagsHtml ? `<div class="flex flex-wrap gap-1">${tagsHtml}</div>` : ''}
                </div>
            `;
            itemsGrid.appendChild(card);
        });
    };

    const renderMainContent = () => {
        if (!appState.activeCollectionId) {
            welcomeView.classList.remove('hidden');
            collectionView.classList.add('hidden');
        } else {
            const collection = appState.collections.find(c => c.collectionId === appState.activeCollectionId);
            if (collection) {
                collectionTitle.textContent = collection.name;
                renderItems(appState.activeItems);
                welcomeView.classList.add('hidden');
                collectionView.classList.remove('hidden');
            }
        }
    };

    // --- MODAL & FORM HANDLING ---

    const openCollectionModal = (collection = null) => {
        collectionForm.reset();
        if (collection) {
            document.getElementById('collection-modal-title').textContent = 'Edit Collection';
            collectionIdInput.value = collection.collectionId;
            collectionNameInput.value = collection.name;
        } else {
            document.getElementById('collection-modal-title').textContent = 'Add New Collection';
            collectionIdInput.value = '';
        }
        collectionModal.classList.remove('hidden');
        collectionModal.classList.add('flex');
    };

    const closeCollectionModal = () => {
        collectionModal.classList.add('hidden');
        collectionModal.classList.remove('flex');
    };

    const openItemModal = (item = null) => {
        itemForm.reset();

        // Initialize Tagify
        if (categoriesTagify) categoriesTagify.destroy();
        if (tagsTagify) tagsTagify.destroy();

        const catInput = document.getElementById('item-categories-input');
        const tagInput = document.getElementById('item-tags-input');

        // Important: Tagify modifies the input value. Reset it first.
        catInput.value = '';
        tagInput.value = '';

        categoriesTagify = new Tagify(catInput);
        tagsTagify = new Tagify(tagInput);

        if (item) {
            document.getElementById('item-modal-title').textContent = 'Edit Item';
            itemIdInput.value = item.itemId;
            itemNameInput.value = item.name;
            itemDescriptionInput.value = item.description;

            // Pre-fill Tagify
            if (item.categories) categoriesTagify.addTags(item.categories.map(c => c.name));
            if (item.tags) tagsTagify.addTags(item.tags.map(t => t.name));
        } else {
            document.getElementById('item-modal-title').textContent = 'Add New Item';
            itemIdInput.value = '';
        }
        itemModal.classList.remove('hidden');
        itemModal.classList.add('flex');
    };

    const closeItemModal = () => {
        itemModal.classList.add('hidden');
        itemModal.classList.remove('flex');
    };

    // --- EVENT LISTENERS ---

    // Modals
    document.getElementById('add-collection-btn').addEventListener('click', () => openCollectionModal());
    document.getElementById('add-item-btn').addEventListener('click', () => openItemModal());

    document.getElementById('cancel-collection-modal').addEventListener('click', closeCollectionModal);
    document.getElementById('cancel-item-modal').addEventListener('click', closeItemModal);

    // Edit/Delete Collection (Header buttons)
    editCollectionBtn.addEventListener('click', () => {
        const collection = appState.collections.find(c => c.collectionId === appState.activeCollectionId);
        if (collection) openCollectionModal(collection);
    });

    deleteCollectionBtn.addEventListener('click', async () => {
        if (confirm('Are you sure you want to delete this collection and all its items?')) {
            const success = await api(`/api/collections/${appState.activeCollectionId}`, 'DELETE');
            if (success === null || success) { // 204 returns null
                appState.activeCollectionId = null;
                fetchCollections(); // Refresh list
                welcomeView.classList.remove('hidden');
                collectionView.classList.add('hidden');
            }
        }
    });

    // Collection Form Submit
    collectionForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = collectionIdInput.value;
        const dto = { name: collectionNameInput.value.trim() };

        let result;
        if (id) {
            // Update
            result = await api(`/api/collections/${id}`, 'PUT', dto);
            // PUT usually returns No Content, so we check for success differently or just refresh
            if (result === null) fetchCollections();
        } else {
            // Create
            result = await api('/api/collections', 'POST', dto);
            if (result) {
                fetchCollections(); // Refresh sidebar
                // Optionally switch to new collection
            }
        }
        closeCollectionModal();
        fetchCollections(); // Brute force refresh to be safe
    });

    // Item Form Submit
    itemForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const itemId = itemIdInput.value;
        const collectionId = appState.activeCollectionId;

        // Convert Tagify data to List<string> for API
        const dto = {
            name: itemNameInput.value.trim(),
            description: itemDescriptionInput.value.trim(),
            categories: parseTagifyValues(categoriesTagify),
            tags: parseTagifyValues(tagsTagify)
        };

        if (itemId) {
            // Update
            await api(`/api/collections/${collectionId}/items/${itemId}`, 'PUT', dto);
        } else {
            // Create
            await api(`/api/collections/${collectionId}/items`, 'POST', dto);
        }

        closeItemModal();
        fetchItems(collectionId); // Refresh items
        fetchCollections(); // Refresh sidebar counts
    });

    // Item Grid Clicks (Edit/Delete Item)
    itemsGrid.addEventListener('click', async (e) => {
        const editBtn = e.target.closest('.edit-item-btn');
        const deleteBtn = e.target.closest('.delete-item-btn');

        if (editBtn) {
            const itemId = parseInt(editBtn.dataset.itemId);
            const item = appState.activeItems.find(i => i.itemId === itemId);
            if (item) openItemModal(item);
        }

        if (deleteBtn) {
            const itemId = parseInt(deleteBtn.dataset.itemId);
            if (confirm('Delete this item?')) {
                await api(`/api/collections/${appState.activeCollectionId}/items/${itemId}`, 'DELETE');
                fetchItems(appState.activeCollectionId); // Refresh grid
                fetchCollections(); // Refresh sidebar counts
            }
        }
    });

    // Search (Local Filter for now)
    globalSearchInput.addEventListener('input', (e) => {
        const query = e.target.value.toLowerCase();
        if (!appState.activeItems) return;

        const filtered = appState.activeItems.filter(item =>
            item.name.toLowerCase().includes(query) ||
            (item.tags && item.tags.some(t => t.name.toLowerCase().includes(query)))
        );

        renderItems(filtered);
    });

    // Initial Load
    fetchCollections();
});