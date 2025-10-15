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
    let searchTimeout;
    searchInput.addEventListener('input', function() {
        filterEvents();
        
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            if (this.value.trim().length > 0) {
                triggerAutoTracking();
            }
        }, 1000); 
    });

      // Category filter
      categoryFilter.addEventListener('change', function() {
        filterEvents();
              
        setTimeout(() => {
            triggerAutoTracking();
        }, 500);
    });

    // Date filters
    dateFrom.addEventListener('change', function() {
        filterEvents();
        setTimeout(() => {
            triggerAutoTracking();
        }, 500);
    });

    dateTo.addEventListener('change', function() {
        filterEvents();
        setTimeout(() => {
            triggerAutoTracking();
        }, 500);
    });

    
    clearFiltersBtn.addEventListener('click', function() {
        searchInput.value = '';
        categoryFilter.value = 'all';
        dateFrom.value = '';
        dateTo.value = '';
        filterChips.forEach(chip => chip.classList.remove('active'));
        filterEvents();
    });

    
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


    gridViewBtn.addEventListener('change', function() {
        if (this.checked) {
            eventsContainer.className = 'row g-4';
            eventsContainer.classList.remove('list-view-mode');
   
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

        resultCount.textContent = visibleCount;

        if (visibleCount === 0) {
            eventsContainer.classList.add('d-none');
            noResults.classList.remove('d-none');
        } else {
            eventsContainer.classList.remove('d-none');
            noResults.classList.add('d-none');
        }
    }
    
   
    function triggerAutoTracking() {
        console.log('🔍 TRIGGER AUTO-TRACKING CALLED');
        
        const isLoggedIn = document.querySelector('[id="userDropdown"]') !== null;
        console.log('   Login status:', isLoggedIn);
        
        if (!isLoggedIn) {
            console.log('⚠️ Not logged in - skipping auto-tracking');
            return;
        }
        
        const searchTerm = searchInput.value.trim();
        const selectedCategory = categoryFilter.value;
        
        console.log('   Search term:', searchTerm || 'none');
        console.log('   Category:', selectedCategory);
        
        
        const allCards = document.querySelectorAll('.event-card');
        console.log('   Total cards on page:', allCards.length);
        
       
        const visibleCards = Array.from(allCards).filter(card => {
            const isVisible = card.style.display !== 'none';
            return isVisible;
        });
        
        console.log('   Visible cards:', visibleCards.length);
        
        const matchedAnnouncements = visibleCards
            .map(card => {
                const button = card.querySelector('.add-to-preferences');
                if (button) {
                    return {
                        id: button.dataset.announcementId,
                        category: button.dataset.category
                    };
                }
                return null;
            })
            .filter(item => item !== null)
            .slice(0, 6); 
        
        console.log('   Announcements to track:', matchedAnnouncements.length);
        matchedAnnouncements.forEach((item, i) => {
            console.log(`      ${i+1}. ${item.category} (ID: ${item.id.substring(0,8)}...)`);
        });
        
        if (matchedAnnouncements.length > 0) {
            console.log('🚀 Starting auto-track...');
            autoTrackFilterPreferences(searchTerm, selectedCategory, matchedAnnouncements);
        } else {
            console.log('❌ No announcements to track!');
        }
    }

   
    document.getElementById('resetSearch')?.addEventListener('click', function() {
        clearFiltersBtn.click();
    });

   
    function trackAnnouncementView(announcementId) {
        fetch('/News/TrackView', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ announcementId: announcementId })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('View tracked for announcement:', announcementId);
            }
        })
        .catch(error => console.error('Error tracking view:', error));
    }


    window.trackAnnouncementView = trackAnnouncementView;

   
    document.querySelectorAll('[data-announcement-id]').forEach(card => {
        card.addEventListener('click', function(e) {
      
            if (e.target.tagName === 'BUTTON' || e.target.closest('button')) {
                return;
            }
            
            const announcementId = this.dataset.announcementId;
            if (announcementId) {
                trackAnnouncementView(announcementId);
                
      
                this.style.transform = 'scale(0.98)';
                setTimeout(() => {
                    this.style.transform = '';
                }, 200);
            }
        });
    });

   
    const clickCounts = {}; 
    
    document.querySelectorAll('.event-card').forEach(card => {
        const cardElement = card.querySelector('.card');
        if (cardElement) {
            cardElement.addEventListener('click', function(e) {
               
                if (e.target.tagName === 'BUTTON' || e.target.closest('button')) {
                    return;
                }
                
                
                const button = this.querySelector('.add-to-preferences');
                if (button) {
                    const announcementId = button.dataset.announcementId;
                    const category = button.dataset.category;
                    
                    if (announcementId) {
                        
                        trackAnnouncementView(announcementId);
                        
                        
                        if (!clickCounts[announcementId]) {
                            clickCounts[announcementId] = 0;
                        }
                        clickCounts[announcementId]++;
                        
                        
                        this.style.transform = 'scale(0.98)';
                        this.style.boxShadow = '0 0 20px rgba(40, 167, 69, 0.5)';
                        
                        setTimeout(() => {
                            this.style.transform = '';
                            this.style.boxShadow = '';
                        }, 300);
                        
                        
                        const count = clickCounts[announcementId];
                        const isLoggedIn = document.querySelector('[id="userDropdown"]') !== null;
                        
                        
                        if (isLoggedIn && count >= 2) {
                            setTimeout(() => {
                                refreshRecommendations();
                            }, 1000);
                        }
                    }
                }
            });
        }
    });

    // Track clicks on trending chips
    document.querySelectorAll('.trending-chip').forEach(chip => {
        chip.addEventListener('click', function(e) {
            const announcementId = this.dataset.announcementId;
            if (announcementId) {
                trackAnnouncementView(announcementId);
                
               
                const badge = this.querySelector('.badge');
                if (badge) {
                    const originalClass = badge.className;
                    badge.className = 'badge bg-success py-2 px-3';
                    badge.innerHTML = '<i class="bi bi-check-circle-fill me-1"></i>Added to Interests!';
                    
                    setTimeout(() => {
                        badge.className = originalClass;
                        const icon = this.dataset.icon || 'bi bi-megaphone-fill';
                        const title = this.dataset.title || 'View';
                        badge.innerHTML = `<i class="${icon} me-1"></i>${title}<i class="bi bi-arrow-right ms-2"></i>`;
                    }, 2000);
                }
            }
        });
    });

  
    console.log('News & Announcements page initialized');
    console.log('Total events loaded:', document.querySelectorAll('.event-card').length);
    console.log('Recommendations loaded:', document.querySelectorAll('[data-announcement-id]').length);
});


function addToPreferences(button, announcementId, category) {
   
    if (button.disabled) return;
    button.disabled = true;
    
    console.log('📤 Sending to server:', { announcementId, category });
    
   
    fetch('/News/TrackView', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        body: JSON.stringify({ announcementId: announcementId })
    })
    .then(response => {
        console.log('📥 Response status:', response.status);
        if (!response.ok) {
            throw new Error('Network response was not ok: ' + response.status);
        }
        return response.json();
    })
    .then(data => {
        if (data.success) {
            console.log('✅ Added to preferences:', announcementId, category, 'IsLoggedIn:', data.isLoggedIn);
            
           
            const originalHTML = button.innerHTML;
            const originalClasses = button.className;
            
           
            button.className = 'btn btn-success w-100';
            if (originalClasses.includes('btn-sm')) {
                button.className += ' btn-sm';
            }
            button.innerHTML = '<i class="bi bi-check-circle-fill me-1"></i>Added!';
            
           
            if (data.isLoggedIn) {
                showToast(`${category} added to your interests! 💝 (Saved to your account)`, 'success');
                
               
                setTimeout(() => {
                    refreshRecommendations();
                }, 1000);
            } else {
                showToast(`${category} tracked! Login to save permanently. 💝`, 'info');
            }
            
           
            setTimeout(() => {
                button.className = originalClasses;
                button.innerHTML = originalHTML;
                button.disabled = false;
            }, 1500);
        } else {
            button.disabled = false;
            showToast('Error: ' + (data.message || 'Could not track preference'), 'danger');
        }
    })
    .catch(error => {
        console.error('Error adding to preferences:', error);
        button.disabled = false;
        showToast('Error: ' + error.message + '. Check console for details.', 'danger');
    });
}


function refreshRecommendations() {
    console.log('🔄 Refreshing recommendations...');
    
    fetch('/News/GetRecommendations?count=6')
        .then(response => response.json())
        .then(data => {
            console.log('📊 User Preferences:', data.preferences);
            console.log('👤 User ID:', data.userId);
            console.log('📋 Recommendations:', data.recommendations);
            
            if (data.recommendations && data.recommendations.length > 0) {
                console.log('✅ Got', data.recommendations.length, 'recommendations');
                
           
                if (data.preferences && Object.keys(data.preferences).length > 0) {
                    console.log('📈 Your top interests:');
                    Object.entries(data.preferences)
                        .sort((a, b) => b[1] - a[1])
                        .forEach(([category, count]) => {
                            console.log(`  - ${category}: ${count} interactions`);
                        });
                }
                
             
                const recommendationsContainer = document.getElementById('recommendationsContainer');
                
                if (recommendationsContainer) {
                
                    recommendationsContainer.innerHTML = '';
                    
                   
                    data.recommendations.slice(0, 3).forEach(rec => {
                        const col = document.createElement('div');
                        col.className = 'col-12 col-md-6 col-lg-4';
                               col.innerHTML = `
                                   <div class="card h-100 shadow-sm hover-lift border-0" style="cursor: pointer;" data-announcement-id="${rec.id}">
                                       <div class="card-body">
                                           <span class="badge ${getCategoryBadgeClass(rec.category)} mb-2">
                                               <i class="${getCategoryIcon(rec.category)} me-1"></i>${rec.category}
                                           </span>
                                           <h6 class="fw-bold mb-2">${rec.title}</h6>
                                           <p class="text-muted small mb-2">
                                               <i class="bi bi-calendar3 me-1"></i>${formatDate(rec.date)}
                                           </p>
                                           <p class="card-text small">${truncateText(rec.description, 100)}</p>
                                       </div>
                                   </div>
                               `;
                        recommendationsContainer.appendChild(col);
                    });
                    
                   
                    const recommendationsCard = document.getElementById('recommendationsCard');
                    if (recommendationsCard) {
                        recommendationsCard.style.boxShadow = '0 0 30px rgba(40, 167, 69, 0.6)';
                        recommendationsCard.style.transition = 'box-shadow 0.5s ease';
                        
                       
                        recommendationsCard.scrollIntoView({ behavior: 'smooth', block: 'start' });
                        
                        setTimeout(() => {
                            recommendationsCard.style.boxShadow = '';
                        }, 2500);
                    }
                    
                    console.log('✅ RECOMMENDATIONS UPDATED! Scroll up to see them.');
                } else {
                    console.log('⚠️ Recommendations container not found on page');
                }
            } else {
                console.log('⚠️ No recommendations returned');
            }
        })
        .catch(error => {
            console.error('❌ Error refreshing recommendations:', error);
        });
}


function getCategoryBadgeClass(category) {
    const map = {
        'Announcement': 'bg-primary',
        'Event': 'bg-info',
        'ServiceUpdate': 'bg-success',
        'Notice': 'bg-notice-orange',
        'Program': 'bg-secondary',
        'Emergency': 'bg-danger'
    };
    return map[category] || 'bg-secondary';
}

function getCategoryIcon(category) {
    const map = {
        'Announcement': 'bi bi-megaphone-fill',
        'Event': 'bi bi-people-fill',
        'ServiceUpdate': 'bi bi-check-circle-fill',
        'Notice': 'bi bi-exclamation-triangle-fill',
        'Program': 'bi bi-star-fill',
        'Emergency': 'bi bi-exclamation-octagon-fill'
    };
    return map[category] || 'bi bi-info-circle-fill';
}

function getCategoryButtonClass(category) {
    const map = {
        'Announcement': 'btn-primary',
        'Event': 'btn-info text-white',
        'ServiceUpdate': 'btn-success',
        'Notice': 'btn-notice-orange',
        'Program': 'btn-secondary text-white',
        'Emergency': 'btn-danger'
    };
    return map[category] || 'btn-primary';
}

function formatDate(dateStr) {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

function truncateText(text, maxLength) {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
}


function autoTrackFilterPreferences(searchTerm, selectedCategory, matchedAnnouncements) {
    const isLoggedIn = document.querySelector('[id="userDropdown"]') !== null;
    if (!isLoggedIn) {
        console.log('⚠️ Not logged in - skipping auto-track');
        return;
    }
    
    const itemsToTrack = matchedAnnouncements.slice(0, 6);
    if (itemsToTrack.length === 0) {
        console.log('⚠️ No items to track');
        return;
    }
    
    console.log(`🎯 AUTO-TRACKING ${itemsToTrack.length} items...`);
    if (searchTerm) console.log(`   🔍 Search: "${searchTerm}"`);
    if (selectedCategory !== 'all') console.log(`   📂 Category: ${selectedCategory}`);
    
    let trackedCount = 0;
    const totalItems = itemsToTrack.length;
    
   
    itemsToTrack.forEach((item, index) => {
        setTimeout(() => {
            fetch('/News/TrackView', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ announcementId: item.id })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    trackedCount++;
                    console.log(`   ✅ Tracked ${trackedCount}/${totalItems}: ${item.category}`);
                    
                   
                    if (trackedCount === totalItems) {
                        console.log(`🔄 All ${totalItems} items tracked! Refreshing recommendations...`);
                        setTimeout(() => {
                            refreshRecommendations();
                        }, 300);
                    }
                }
            })
            .catch(error => {
                console.error(`❌ Track error:`, error);
            });
        }, index * 150);
    });
}
