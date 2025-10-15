

function showLoginModal() {
    const modal = new bootstrap.Modal(document.getElementById('loginModal'));
    showLoginForm();
    modal.show();
}

// login form
function showLoginForm() {
    document.getElementById('loginForm').classList.remove('d-none');
    document.getElementById('registerForm').classList.add('d-none');
    document.getElementById('modalTitle').textContent = 'Welcome Back!';
    document.getElementById('loginError').classList.add('d-none');
}

// register form
function showRegisterForm() {
    document.getElementById('loginForm').classList.add('d-none');
    document.getElementById('registerForm').classList.remove('d-none');
    document.getElementById('modalTitle').textContent = 'Create Account';
    document.getElementById('registerError').classList.add('d-none');
}

// Login user
function loginUser() {
    const username = document.getElementById('loginUsername').value.trim();
    const password = document.getElementById('loginPassword').value;

    if (!username || !password) {
        showError('loginError', 'Please enter both username and password');
        return;
    }

    if (username.length < 3) {
        showError('loginError', 'Username must be at least 3 characters');
        return;
    }

    if (password.length < 6) {
        showError('loginError', 'Password must be at least 6 characters');
        return;
    }

    fetch('/User/Login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            usernameOrEmail: username,
            password: password
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            console.log('Login successful, userId:', data.userId);
            
            bootstrap.Modal.getInstance(document.getElementById('loginModal')).hide();
            
            setTimeout(() => {
                if (window.pendingRedirect) {
                    window.location.href = window.pendingRedirect;
                    window.pendingRedirect = null;
                } else {
                    location.reload();
                }
            }, 300);
        } else {
            showError('loginError', data.message || 'Invalid username or password');
        }
    })
    .catch(error => {
        console.error('Login error:', error);
        showError('loginError', 'An error occurred. Please try again.');
    });
}

// Register user
function registerUser() {
    const firstName = document.getElementById('regFirstName').value.trim();
    const lastName = document.getElementById('regLastName').value.trim();
    const username = document.getElementById('regUsername').value.trim();
    const email = document.getElementById('regEmail').value.trim();
    const password = document.getElementById('regPassword').value;
    const confirmPassword = document.getElementById('regConfirmPassword').value;

    document.getElementById('registerError').classList.add('d-none');

    if (!firstName || !lastName || !username || !email || !password || !confirmPassword) {
        showError('registerError', 'Please fill in all required fields');
        return;
    }

    // First name validation
    if (firstName.length < 2) {
        showError('registerError', 'First name must be at least 2 characters');
        return;
    }

    if (!/^[a-zA-Z\s]+$/.test(firstName)) {
        showError('registerError', 'First name can only contain letters');
        return;
    }

    // Last name validation
    if (lastName.length < 2) {
        showError('registerError', 'Last name must be at least 2 characters');
        return;
    }

    if (!/^[a-zA-Z\s]+$/.test(lastName)) {
        showError('registerError', 'Last name can only contain letters');
        return;
    }

    // Username validation
    if (username.length < 3) {
        showError('registerError', 'Username must be at least 3 characters');
        return;
    }

    if (username.length > 50) {
        showError('registerError', 'Username cannot exceed 50 characters');
        return;
    }

    if (!/^[a-zA-Z0-9_]+$/.test(username)) {
        showError('registerError', 'Username can only contain letters, numbers, and underscores');
        return;
    }

    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        showError('registerError', 'Please enter a valid email address');
        return;
    }

    if (email.length > 100) {
        showError('registerError', 'Email cannot exceed 100 characters');
        return;
    }

    // Password validation
    if (password.length < 6) {
        showError('registerError', 'Password must be at least 6 characters');
        return;
    }

    if (password.length > 100) {
        showError('registerError', 'Password cannot exceed 100 characters');
        return;
    }

    // Password strength check
    if (!/[A-Z]/.test(password)) {
        showError('registerError', 'Password must contain at least one uppercase letter');
        return;
    }

    if (!/[a-z]/.test(password)) {
        showError('registerError', 'Password must contain at least one lowercase letter');
        return;
    }

    if (!/[0-9]/.test(password)) {
        showError('registerError', 'Password must contain at least one number');
        return;
    }

    // Confirm password validation
    if (password !== confirmPassword) {
        showError('registerError', 'Passwords do not match');
        return;
    }

    
    fetch('/User/Register', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            firstName: firstName,
            lastName: lastName,
            username: username,
            email: email,
            password: password,
            confirmPassword: confirmPassword
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            console.log('Registration successful! User:', data.username);
            
            showToast(`Account created! Please login as "${data.username}" 🎉`, 'success');
            
            showLoginForm();
            
            document.getElementById('loginUsername').value = data.username || username;
            document.getElementById('loginPassword').value = '';
            document.getElementById('loginPassword').focus();
            
            const loginError = document.getElementById('loginError');
            loginError.className = 'alert alert-success';
            loginError.textContent = '✅ Account created! Please enter your password to login.';
        } else {
            showError('registerError', data.message || 'Registration failed. Username or email may already exist.');
        }
    })
    .catch(error => {
        console.error('Register error:', error);
        showError('registerError', 'An error occurred. Please try again.');
    });
}

// Logout user
function logoutUser() {
    fetch('/User/Logout', {
        method: 'POST'
    })
    .then(() => {
        location.reload();
    })
    .catch(error => {
        console.error('Logout error:', error);
        location.reload();
    });
}


function showError(elementId, message) {
    const errorElement = document.getElementById(elementId);
    errorElement.textContent = message;
    errorElement.classList.remove('d-none');
}

//toast notification
function showToast(message, type = 'success') {
    // Check if toast container exists
    let toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toastContainer';
        toastContainer.style.position = 'fixed';
        toastContainer.style.top = '80px';
        toastContainer.style.right = '20px';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }
    

    let icon = 'check-circle-fill';
    if (type === 'warning') icon = 'exclamation-triangle-fill';
    else if (type === 'danger') icon = 'x-circle-fill';
    else if (type === 'info') icon = 'info-circle-fill';
    

    const toast = document.createElement('div');
    toast.className = `alert alert-${type} alert-dismissible fade show shadow-lg`;
    toast.style.minWidth = '300px';
    toast.style.borderRadius = '10px';
    toast.innerHTML = `
        <i class="bi bi-${icon} me-2"></i>
        ${message}
        <button type="button" class="btn-close" onclick="this.parentElement.remove()"></button>
    `;
    
    toastContainer.appendChild(toast);
    

    setTimeout(() => {
        toast.remove();
    }, 4000);
}

// Check if user is logged in
function isUserLoggedIn() {
    return document.querySelector('[id="userDropdown"]') !== null;
}


function requireLogin(callback) {
    if (!isUserLoggedIn()) {
        showLoginModal();
     
        window.pendingAction = callback;
        return false;
    }
    return true;
}


function checkLoginAndRedirect(url) {
    if (isUserLoggedIn()) {
   
        window.location.href = url;
    } else {

        window.pendingRedirect = url;
        showLoginModal();
    }
}


function setupValidation() {

    const loginUsername = document.getElementById('loginUsername');
    if (loginUsername) {
        loginUsername.addEventListener('input', function() {
            if (this.value.length > 0 && this.value.length < 3) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
            } else if (this.value.length >= 3) {
                this.classList.add('is-valid');
                this.classList.remove('is-invalid');
            } else {
                this.classList.remove('is-valid', 'is-invalid');
            }
        });
    }

    // Password validation (login)
    const loginPassword = document.getElementById('loginPassword');
    if (loginPassword) {
        loginPassword.addEventListener('input', function() {
            if (this.value.length > 0 && this.value.length < 6) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
            } else if (this.value.length >= 6) {
                this.classList.add('is-valid');
                this.classList.remove('is-invalid');
            } else {
                this.classList.remove('is-valid', 'is-invalid');
            }
        });
    }

    // Register form validation
    const regFirstName = document.getElementById('regFirstName');
    if (regFirstName) {
        regFirstName.addEventListener('input', function() {
            if (this.value.length >= 2 && /^[a-zA-Z\s]+$/.test(this.value)) {
                this.classList.add('is-valid');
                this.classList.remove('is-invalid');
            } else if (this.value.length > 0) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
            } else {
                this.classList.remove('is-valid', 'is-invalid');
            }
        });
    }

    const regLastName = document.getElementById('regLastName');
    if (regLastName) {
        regLastName.addEventListener('input', function() {
            if (this.value.length >= 2 && /^[a-zA-Z\s]+$/.test(this.value)) {
                this.classList.add('is-valid');
                this.classList.remove('is-invalid');
            } else if (this.value.length > 0) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
            } else {
                this.classList.remove('is-valid', 'is-invalid');
            }
        });
    }

    const regUsername = document.getElementById('regUsername');
    if (regUsername) {
        regUsername.addEventListener('input', function() {
            if (this.value.length >= 3 && this.value.length <= 50 && /^[a-zA-Z0-9_]+$/.test(this.value)) {
                this.classList.add('is-valid');
                this.classList.remove('is-invalid');
            } else if (this.value.length > 0) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
            } else {
                this.classList.remove('is-valid', 'is-invalid');
            }
        });
    }

    const regEmail = document.getElementById('regEmail');
    if (regEmail) {
        regEmail.addEventListener('input', function() {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (emailRegex.test(this.value)) {
                this.classList.add('is-valid');
                this.classList.remove('is-invalid');
            } else if (this.value.length > 0) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
            } else {
                this.classList.remove('is-valid', 'is-invalid');
            }
        });
    }

    const regPassword = document.getElementById('regPassword');
    if (regPassword) {
        regPassword.addEventListener('input', function() {
            const hasUpper = /[A-Z]/.test(this.value);
            const hasLower = /[a-z]/.test(this.value);
            const hasNumber = /[0-9]/.test(this.value);
            const isLongEnough = this.value.length >= 6;
            
            if (hasUpper && hasLower && hasNumber && isLongEnough) {
                this.classList.add('is-valid');
                this.classList.remove('is-invalid');
            } else if (this.value.length > 0) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
            } else {
                this.classList.remove('is-valid', 'is-invalid');
            }
        });
    }

    const regConfirmPassword = document.getElementById('regConfirmPassword');
    if (regConfirmPassword) {
        regConfirmPassword.addEventListener('input', function() {
            const password = document.getElementById('regPassword').value;
            if (this.value === password && this.value.length > 0) {
                this.classList.add('is-valid');
                this.classList.remove('is-invalid');
            } else if (this.value.length > 0) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
            } else {
                this.classList.remove('is-valid', 'is-invalid');
            }
        });
    }
}


document.addEventListener('DOMContentLoaded', function() {
    setupValidation();

    const loginPassword = document.getElementById('loginPassword');
    if (loginPassword) {
        loginPassword.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                loginUser();
            }
        });
    }

    const regConfirmPassword = document.getElementById('regConfirmPassword');
    if (regConfirmPassword) {
        regConfirmPassword.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                registerUser();
            }
        });
    }
});
