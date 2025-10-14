document.addEventListener('DOMContentLoaded', function() {
    
    // Get all filter elements
    const searchInput = document.getElementById('searchInput');
    const categoryFilter = document.getElementById('categoryFilter');
    const dateFrom = document.getElementById('dateFrom');
    const dateTo = document.getElementById('dateTo');
    const clearFiltersBtn = document.getElementById('clearFilters');
    const filterChips = document.querySelectorAll('.filter-chip');
    const eventsContainer = document.getElementById('eventsContainer');
    const resultCount = document.getElementById('resultCount');
    const noResults = document.getElementById('noResults');
    const gridViewBtn = document.getElementById('gridView');
    const listViewBtn = document.getElementById('listView');

    // Search functionality
    searchInput.addEventListener('input', function() {
        filterEvents();
    });

    // Category filter
    categoryFilter.addEventListener('change', function() {
        filterEvents();
    });

    // Date filters
    dateFrom.addEventListener('change', function() {
        filterEvents();
    });

    dateTo.addEventListener('change', function() {
        filterEvents();
    });

    // Clear filters button
    clearFiltersBtn.addEventListener('click', function() {
        searchInput.value = '';
        categoryFilter.value = 'all';
        dateFrom.value = '';
        dateTo.value = '';
        filterChips.forEach(chip => chip.classList.remove('active'));
        filterEvents();
    });

    // Quick filter chips
    filterChips.forEach(chip => {
        chip.addEventListener('click', function() {
            filterChips.forEach(c => c.classList.remove('active'));
            this.classList.add('active');
            
            const filter = this.dataset.filter;
            const today = new Date();
            
            switch(filter) {
                case 'today':
                    dateFrom.value = today.toISOString().split('T')[0];
                    dateTo.value = today.toISOString().split('T')[0];
                    break;
                case 'this-week':
                    const startOfWeek = new Date(today);
                    startOfWeek.setDate(today.getDate() - today.getDay());
                    const endOfWeek = new Date(startOfWeek);
                    endOfWeek.setDate(startOfWeek.getDate() + 6);
                    dateFrom.value = startOfWeek.toISOString().split('T')[0];
                    dateTo.value = endOfWeek.toISOString().split('T')[0];
                    break;
                case 'this-month':
                    const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
                    const endOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0);
                    dateFrom.value = startOfMonth.toISOString().split('T')[0];
                    dateTo.value = endOfMonth.toISOString().split('T')[0];
                    break;
                case 'upcoming':
                    dateFrom.value = today.toISOString().split('T')[0];
                    dateTo.value = '';
                    break;
            }
            
            filterEvents();
        });
    });

    // View toggle
    gridViewBtn.addEventListener('change', function() {
        if (this.checked) {
            eventsContainer.className = 'row g-4';
            eventsContainer.classList.remove('list-view-mode');
            // Reset all cards to grid layout
            const eventCards = document.querySelectorAll('.event-card');
            eventCards.forEach(card => {
                const isEvent = card.querySelector('.card-header')?.textContent.includes('Event');
                if (isEvent) {
                    card.className = 'col-12 col-md-6 col-lg-4 event-card';
                } else {
                    card.className = 'col-12 col-md-6 col-lg-4 event-card';
                }
            });
        }
    });

    listViewBtn.addEventListener('change', function() {
        if (this.checked) {
            eventsContainer.className = 'row g-2 list-view-mode';
            // Change all cards to full width list layout
            const eventCards = document.querySelectorAll('.event-card');
            eventCards.forEach(card => {
                card.className = 'col-12 event-card';
            });
        }
    });

    // Main filter function
    function filterEvents() {
        const searchTerm = searchInput.value.toLowerCase();
        const selectedCategory = categoryFilter.value;
        const fromDate = dateFrom.value;
        const toDate = dateTo.value;
        
        const eventCards = document.querySelectorAll('.event-card');
        let visibleCount = 0;

        eventCards.forEach(card => {
            const cardCategory = card.dataset.category;
            const cardDate = card.dataset.date;
            const cardText = card.textContent.toLowerCase();
            
            let showCard = true;
            
            // Search filter
            if (searchTerm && !cardText.includes(searchTerm)) {
                showCard = false;
            }
            
            // Category filter
            if (selectedCategory !== 'all' && cardCategory !== selectedCategory.toLowerCase()) {
                showCard = false;
            }
            
            // Date filter
            if (fromDate && cardDate < fromDate) {
                showCard = false;
            }
            if (toDate && cardDate > toDate) {
                showCard = false;
            }
            
            if (showCard) {
                card.style.display = 'block';
                visibleCount++;
            } else {
                card.style.display = 'none';
            }
        });

        // Update result count
        resultCount.textContent = visibleCount;

        // Show/hide no results message
        if (visibleCount === 0) {
            eventsContainer.classList.add('d-none');
            noResults.classList.remove('d-none');
        } else {
            eventsContainer.classList.remove('d-none');
            noResults.classList.add('d-none');
        }
    }

    // Reset search button
    document.getElementById('resetSearch')?.addEventListener('click', function() {
        clearFiltersBtn.click();
    });

    // Initialize
    console.log('News & Announcements page initialized');
    console.log('Total events loaded:', document.querySelectorAll('.event-card').length);
});

