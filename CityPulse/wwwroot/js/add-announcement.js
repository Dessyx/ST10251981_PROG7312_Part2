let currentStep = 1;
const totalSteps = 4;

document.addEventListener('DOMContentLoaded', function() {
    initializeStepNavigation();
    updateFormFields();
    initializeValidation();
    initializeFormSubmit();
    
    // Enable Next button when category is selected
    const categorySelect = document.getElementById('categorySelect');
    if (categorySelect) {
        categorySelect.addEventListener('change', function() {
            const nextBtn = document.querySelector('.form-step[data-step="1"] .next-step');
            if (nextBtn) {
                nextBtn.disabled = this.value === '';
            }
            updateFormFields();
        });
    }
});

function initializeValidation() {
    const titleInput = document.getElementById('titleInput');
    const descriptionInput = document.getElementById('descriptionInput');
    const dateInput = document.getElementById('dateInput');

    if (titleInput) {
        titleInput.addEventListener('input', function() {
            validateField(this, this.value.length >= 5);
        });
    }

    if (descriptionInput) {
        descriptionInput.addEventListener('input', function() {
            validateField(this, this.value.length >= 20);
        });
    }

    if (dateInput) {
        dateInput.addEventListener('change', function() {
            const selectedDate = new Date(this.value);
            const today = new Date();
            validateField(this, selectedDate >= today);
        });
    }
}

function validateField(element, isValid) {
    if (isValid) {
        element.classList.remove('is-invalid');
        element.classList.add('is-valid');
    } else {
        element.classList.remove('is-valid');
        element.classList.add('is-invalid');
    }
}

function initializeFormSubmit() {
    const form = document.getElementById('announcementForm');
    const addAnotherBtn = document.getElementById('addAnotherBtn');

    if (form) {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            // Submit via AJAX
            const formData = new FormData(form);
            
            fetch(form.action, {
                method: 'POST',
                body: formData
            })
            .then(response => {
                if (response.ok || response.redirected) {
                    // Show success modal
                    const successModal = new bootstrap.Modal(document.getElementById('successModal'));
                    successModal.show();
                    
                    // Auto redirect after 3 seconds
                    setTimeout(() => {
                        window.location.href = '/Admin/Dashboard';
                    }, 3000);
                } else {
                    showError('Error creating announcement. Please try again.');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showError('Network error. Please check your connection and try again.');
            });
        });
    }

    if (addAnotherBtn) {
        addAnotherBtn.addEventListener('click', function() {
            window.location.reload();
        });
    }
}

function initializeStepNavigation() {
    const nextButtons = document.querySelectorAll('.next-step');
    const prevButtons = document.querySelectorAll('.prev-step');

    nextButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            if (validateCurrentStep()) {
                if (currentStep < totalSteps) {
                    currentStep++;
                    updateStepDisplay();
                }
            }
        });
    });

    prevButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            if (currentStep > 1) {
                currentStep--;
                updateStepDisplay();
            }
        });
    });
}

function updateStepDisplay() {
    // Hide all form steps
    document.querySelectorAll('.form-step').forEach(step => {
        step.classList.remove('active');
    });

    // Show current step
    const currentFormStep = document.querySelector(`.form-step[data-step="${currentStep}"]`);
    if (currentFormStep) {
        currentFormStep.classList.add('active');
    }

    // Update progress tracker
    document.querySelectorAll('.progress-step').forEach((step, index) => {
        const stepNum = index + 1;
        step.classList.remove('active', 'completed');
        
        if (stepNum < currentStep) {
            step.classList.add('completed');
        } else if (stepNum === currentStep) {
            step.classList.add('active');
        }
    });

    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function validateCurrentStep() {
    if (currentStep === 1) {
        const category = document.getElementById('categorySelect');
        if (!category || category.value === '') {
            category.classList.add('is-invalid');
            showError('Please select a category');
            return false;
        }
        category.classList.remove('is-invalid');
        category.classList.add('is-valid');
    }
    
    if (currentStep === 2) {
        const title = document.getElementById('titleInput');
        const description = document.getElementById('descriptionInput');
        const date = document.getElementById('dateInput');
        
        let isValid = true;
        
        if (!title || title.value.trim().length < 5) {
            validateField(title, false);
            showError('Title must be at least 5 characters');
            isValid = false;
        } else {
            validateField(title, true);
        }
        
        if (!description || description.value.trim().length < 20) {
            validateField(description, false);
            showError('Description must be at least 20 characters');
            isValid = false;
        } else {
            validateField(description, true);
        }
        
        if (!date || !date.value) {
            validateField(date, false);
            showError('Please select a date and time');
            isValid = false;
        } else {
            const selectedDate = new Date(date.value);
            const today = new Date();
            if (selectedDate < today) {
                validateField(date, false);
                showError('Please select a future date');
                isValid = false;
            } else {
                validateField(date, true);
            }
        }
        
        return isValid;
    }
    
    return true;
}

function showError(message) {
    const errorDiv = document.createElement('div');
    errorDiv.className = 'alert alert-danger alert-dismissible fade show';
    errorDiv.style.borderRadius = '15px';
    errorDiv.innerHTML = `
        <i class="bi bi-exclamation-triangle me-2"></i>${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    const currentFormStep = document.querySelector(`.form-step[data-step="${currentStep}"]`);
    const cardBody = currentFormStep.querySelector('.card-body');
    
    // Remove any existing error alerts
    const existingAlert = cardBody.querySelector('.alert-danger');
    if (existingAlert) {
        existingAlert.remove();
    }
    
    cardBody.insertBefore(errorDiv, cardBody.firstChild);
    
    // Auto dismiss after 5 seconds
    setTimeout(() => {
        errorDiv.remove();
    }, 5000);
}

function updateFormFields() {
    const category = document.getElementById('categorySelect').value;
    
    // Hide all dynamic fields first
    const locationField = document.getElementById('locationField');
    const durationField = document.getElementById('durationField');
    const ageGroupField = document.getElementById('ageGroupField');
    const affectedAreasField = document.getElementById('affectedAreasField');
    const contactInfoField = document.getElementById('contactInfoField');
    
    if (locationField) locationField.style.display = 'none';
    if (durationField) durationField.style.display = 'none';
    if (ageGroupField) ageGroupField.style.display = 'none';
    if (affectedAreasField) affectedAreasField.style.display = 'none';
    if (contactInfoField) contactInfoField.style.display = 'none';
    
    // Show relevant fields based on category
    switch(category) {
        case 'Event':
            if (locationField) locationField.style.display = 'block';
            if (durationField) durationField.style.display = 'block';
            if (contactInfoField) contactInfoField.style.display = 'block';
            break;
        case 'Program':
            if (locationField) locationField.style.display = 'block';
            if (durationField) durationField.style.display = 'block';
            if (ageGroupField) ageGroupField.style.display = 'block';
            if (contactInfoField) contactInfoField.style.display = 'block';
            break;
        case 'Notice':
            if (locationField) locationField.style.display = 'block';
            if (durationField) durationField.style.display = 'block';
            if (affectedAreasField) affectedAreasField.style.display = 'block';
            break;
        case 'Announcement':
            if (affectedAreasField) affectedAreasField.style.display = 'block';
            break;
        case 'Emergency':
            if (affectedAreasField) affectedAreasField.style.display = 'block';
            break;
        case 'ServiceUpdate':
            if (locationField) locationField.style.display = 'block';
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
