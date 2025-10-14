function updateFormFields() {
    const category = document.getElementById('categorySelect').value;
    
    // Hide all dynamic fields first
    document.getElementById('locationField').style.display = 'none';
    document.getElementById('durationField').style.display = 'none';
    document.getElementById('ageGroupField').style.display = 'none';
    document.getElementById('affectedAreasField').style.display = 'none';
    document.getElementById('contactInfoField').style.display = 'none';
    
    // Show relevant fields based on category
    switch(category) {
        case 'Event':
            document.getElementById('locationField').style.display = 'block';
            document.getElementById('durationField').style.display = 'block';
            document.getElementById('contactInfoField').style.display = 'block';
            break;
        case 'Program':
            document.getElementById('locationField').style.display = 'block';
            document.getElementById('durationField').style.display = 'block';
            document.getElementById('ageGroupField').style.display = 'block';
            document.getElementById('contactInfoField').style.display = 'block';
            break;
        case 'Notice':
            document.getElementById('locationField').style.display = 'block';
            document.getElementById('durationField').style.display = 'block';
            document.getElementById('affectedAreasField').style.display = 'block';
            break;
        case 'Announcement':
            document.getElementById('affectedAreasField').style.display = 'block';
            break;
        case 'Emergency':
            document.getElementById('affectedAreasField').style.display = 'block';
            break;
        case 'ServiceUpdate':
            document.getElementById('locationField').style.display = 'block';
            break;
    }
    
    // Update placeholder text based on category
    updatePlaceholders(category);
}

function updatePlaceholders(category) {
    const locationInput = document.querySelector('input[name="Location"]');
    const durationInput = document.querySelector('input[name="Duration"]');
    const ageGroupInput = document.querySelector('input[name="AgeGroup"]');
    const affectedAreasInput = document.querySelector('input[name="AffectedAreas"]');
    const contactInfoInput = document.querySelector('input[name="ContactInfo"]');
    
    switch(category) {
        case 'Event':
            if (locationInput) locationInput.placeholder = 'e.g., City Central Park, Community Center';
            if (durationInput) durationInput.placeholder = 'e.g., 8:00 AM - 2:00 PM, All Day';
            if (contactInfoInput) contactInfoInput.placeholder = 'e.g., events@citypulse.gov.za, (021) 123-4567';
            break;
        case 'Program':
            if (locationInput) locationInput.placeholder = 'e.g., Municipal Building, Online';
            if (durationInput) durationInput.placeholder = 'e.g., 6 weeks, Deadline: Oct 31, 2025';
            if (ageGroupInput) ageGroupInput.placeholder = 'e.g., 18-35 years, All ages';
            if (contactInfoInput) contactInfoInput.placeholder = 'e.g., programs@citypulse.gov.za, (021) 123-4567';
            break;
        case 'Notice':
            if (locationInput) locationInput.placeholder = 'e.g., Main Street, Citywide';
            if (durationInput) durationInput.placeholder = 'e.g., 6 hours, 2 days';
            if (affectedAreasInput) affectedAreasInput.placeholder = 'e.g., Hilltop Heights, Sunset Park';
            break;
        case 'Announcement':
            if (affectedAreasInput) affectedAreasInput.placeholder = 'e.g., Central Business District, Riverside, Green Valley';
            break;
        case 'Emergency':
            if (affectedAreasInput) affectedAreasInput.placeholder = 'e.g., All areas, Specific neighborhoods';
            break;
        case 'ServiceUpdate':
            if (locationInput) locationInput.placeholder = 'e.g., Main Street, Citywide';
            break;
    }
}

// Initialize form on page load
document.addEventListener('DOMContentLoaded', function() {
    updateFormFields();
});

