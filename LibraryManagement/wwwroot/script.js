let token = localStorage.getItem('token');
let userRights = [];

document.addEventListener('DOMContentLoaded', function() {
    // Ensure all modals are hidden initially
    document.querySelectorAll('.modal').forEach(modal => {
        modal.classList.add('hidden');
    });

    //// Show JWT token for debugging
    //if (token) {
    //    //console.log('JWT Token:', token);
    //    //console.log('Decoded payload:', JSON.parse(atob(token.split('.')[1])));
    //    // Test API call with token
    //    fetch('/api/auth/me', {
    //        headers: { 'Authorization': `Bearer ${token}` }
    //    })
    //    .then(response => {
    //        console.log('API Response status:', response.status);
    //        if (response.ok) {
    //            return response.json();
    //        } else {
    //            throw new Error(`HTTP ${response.status}`);
    //        }
    //    })
    //    .then(data => {
    //        //console.log('API Response data:', data);
    //        //console.log('userHasRoles[0]:', data.userHasRoles ? data.userHasRoles[0] : 'undefined');
    //        //console.log('role:', data.userHasRoles && data.userHasRoles[0] ? data.userHasRoles[0].role : 'undefined');
    //    })
    //    .catch(error => console.error('API call failed:', error));
    //}

    if (token) {
        // Load user rights from API when page loads with existing token
        loadUserRights().then((success) => {
            if (success) {
                showAdminPanel();
                loadDashboard();
            } else {
                // If loading rights fails, show login
                localStorage.removeItem('token');
                showLogin();
            }
        }).catch((error) => {
            console.error('Failed to load user rights:', error);
            // If loading rights fails, show login
            localStorage.removeItem('token');
            showLogin();
        });
    } else {
        showLogin();
    }
    // Login form
    console.log('Setting up login form event listener');
    document.getElementById('login-form').addEventListener('submit', function(e) {
        console.log('Login form submitted');
        handleLogin(e);
    });

    // Logout
    document.getElementById('logout-btn').addEventListener('click', handleLogout);

    // Navigation
    document.querySelectorAll('.nav-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            switchSection(this.dataset.section);
        });
    });

    // Add buttons
    document.getElementById('add-student-btn').addEventListener('click', () => {
        document.getElementById('student-id').value = '';
        openModal('student-modal', 'Add Student');
    });
    document.getElementById('add-book-btn').addEventListener('click', () => {
        document.getElementById('book-id').value = '';
        openModal('book-modal', 'Add Book');
    });
    document.getElementById('add-user-btn').addEventListener('click', async () => {
        document.getElementById('user-id').value = '';
        document.getElementById('user-password').style.display = 'block';
        document.getElementById('user-password').required = true;
        document.getElementById('user-password').setAttribute('autocomplete', 'new-password');
        await loadRolesForUser();
        openModal('user-modal', 'Add User');
    });
    document.getElementById('add-loan-btn').addEventListener('click', async () => {
        document.getElementById('loan-id').value = '';
        await loadStudentsForLoan();
        await loadBooksForLoan();
        openModal('loan-modal', 'Add Loan');
    });
    document.getElementById('add-role-btn').addEventListener('click', async () => {
        document.getElementById('role-id').value = '';
        await loadRightsForRole();
        openModal('role-modal', 'Add Role');
    });

    // Bulk delete for users
    document.getElementById('select-all-users').addEventListener('change', toggleSelectAllUsers);
    document.getElementById('delete-selected-users-btn').addEventListener('click', deleteSelectedUsers);

    // Bulk delete for roles
    document.getElementById('select-all-roles').addEventListener('change', toggleSelectAllRoles);
    document.getElementById('delete-selected-roles-btn').addEventListener('click', deleteSelectedRoles);

    // Forms
    document.getElementById('student-form').addEventListener('submit', handleStudentSubmit);
    document.getElementById('book-form').addEventListener('submit', handleBookSubmit);
    document.getElementById('user-form').addEventListener('submit', handleUserSubmit);
    document.getElementById('loan-form').addEventListener('submit', handleLoanSubmit);
    document.getElementById('role-form').addEventListener('submit', handleRoleSubmit);

    // Close modals
    document.querySelectorAll('.close').forEach(closeBtn => {
        closeBtn.addEventListener('click', () => closeModal());
    });
});

// Define loadUserRights function
async function loadUserRights() {
    try {
        const rightsResponse = await fetch('/api/auth/me', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (rightsResponse.ok) {
            const userData = await rightsResponse.json();
            //console.log('User data from API:', userData); // Debug log

            // Extract rights from user data
            userRights = [];
            if (userData.userHasRoles) {
                //console.log('Processing userHasRoles:', userData.userHasRoles);
                userData.userHasRoles.forEach(userRole => {
                    //console.log('Processing userRole:', userRole);
                    if (userRole.role && userRole.role.roleHasRights) {
                        //console.log('Found roleHasRights:', userRole.role.roleHasRights);
                        userRole.role.roleHasRights.forEach(roleRight => {
                            //console.log('Processing roleRight:', roleRight);
                            if (roleRight.name) {
                                userRights.push(roleRight.name);
                                //console.log('Added right:', roleRight.name);
                            } else {
                                //console.log('No name property in roleRight:', roleRight);
                            }
                        });
                    } else {
                        //console.log('No roleHasRights found in userRole:', userRole);
                    }
                });
            } else {
                //console.log('No userHasRoles found in userData');
            }

            //console.log('Final user rights from API:', userRights); // Debug log

            // Apply permissions after loading rights
            applyPermissions();
            return true; // Success
        } else {
            console.error('Failed to get user rights from API');
            userRights = [];
            return false; // Failure
        }
    } catch (error) {
        console.error('Error fetching user rights:', error);
        userRights = [];
        return false; // Failure
    }
}
function showLogin() {
    document.getElementById('login-container').classList.remove('hidden');
    document.getElementById('admin-panel').classList.add('hidden');
}

function showAdminPanel() {
    document.getElementById('login-container').classList.add('hidden');
    document.getElementById('admin-panel').classList.remove('hidden');
    loadCurrentUser();
    // applyPermissions() is now called after loading user rights
}

async function handleLogin(e) {
    e.preventDefault();
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    try {
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ username, password })
        });

        if (response.ok) {
            const data = await response.json();
            token = data.token;
            localStorage.setItem('token', token);
            console.log('Logged in successfully');

            // Load user rights and then show admin panel
            await loadUserRights();
            showAdminPanel();
            loadDashboard();
        } else {
            document.getElementById('login-message').textContent = 'Invalid credentials';
        }
    } catch (error) {
        document.getElementById('login-message').textContent = 'Login failed';
    }
}

function handleLogout() {
    token = null;
    userRights = [];
    localStorage.removeItem('token');
    // Clear all cached data
    clearAllData();
    showLogin();
}

function clearAllData() {
    // Clear all table data
    document.querySelectorAll('tbody').forEach(tbody => {
        tbody.innerHTML = '';
    });

    // Clear dashboard stats
    document.getElementById('total-students').textContent = '0';
    document.getElementById('total-books').textContent = '0';
    document.getElementById('active-loans').textContent = '0';

    // Clear current user
    document.getElementById('current-user').textContent = 'Current User: Loading...';

    // Reset to dashboard
    document.querySelectorAll('.section').forEach(section => {
        section.classList.add('hidden');
    });
    document.querySelectorAll('.nav-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    document.getElementById('dashboard').classList.remove('hidden');
    document.querySelector('[data-section="dashboard"]').classList.add('active');
}

function switchSection(sectionName) {
    document.querySelectorAll('.section').forEach(section => {
        section.classList.add('hidden');
    });
    document.querySelectorAll('.nav-btn').forEach(btn => {
        btn.classList.remove('active');
    });

    document.getElementById(sectionName).classList.remove('hidden');
    document.querySelector(`[data-section="${sectionName}"]`).classList.add('active');

    switch (sectionName) {
        case 'students':
            loadStudents();
            break;
        case 'books':
            loadBooks();
            break;
        case 'users':
            loadUsers();
            break;
        case 'loans':
            loadLoans();
            break;
        case 'roles':
            loadRoles();
            break;
    }
}

async function loadDashboard() {
    if (!token) return; // Don't load dashboard if not logged in

    try {
        const [studentsRes, booksRes, loansRes] = await Promise.all([
            fetch('/api/students', { headers: { 'Authorization': `Bearer ${token}` } }),
            fetch('/api/books', { headers: { 'Authorization': `Bearer ${token}` } }),
            fetch('/api/loans', { headers: { 'Authorization': `Bearer ${token}` } })
        ]);

        if (studentsRes.ok) {
            const students = await studentsRes.json();
            document.getElementById('total-students').textContent = students.length;
        }

        if (booksRes.ok) {
            const books = await booksRes.json();
            document.getElementById('total-books').textContent = books.length;
        }

        if (loansRes.ok) {
            const loans = await loansRes.json();
            const activeLoans = loans.filter(loan => !loan.returnDate).length;
            document.getElementById('active-loans').textContent = activeLoans;
        }
    } catch (error) {
        console.error('Failed to load dashboard data', error);
    }
}

async function loadStudents() {
    try {
        const response = await fetch('/api/students', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const students = await response.json();
            const tbody = document.querySelector('#students-table tbody');
            tbody.innerHTML = '';

            students.forEach(student => {
                const row = `
                    <tr>
                        <td>${student.id}</td>
                        <td>${student.firstName}</td>
                        <td>${student.lastName}</td>
                        <td>
                            <button class="edit" data-permission="UpdateStudent" onclick="editStudent(${student.id})">Edit</button>
                            <button class="delete" data-permission="DeleteStudent" onclick="deleteStudent(${student.id})">Delete</button>
                        </td>
                    </tr>
                `;
                tbody.innerHTML += row;
            });
        }
    } catch (error) {
        console.error('Failed to load students', error);
    }
    console.log("permition to apply");
    applyPermissions();
}

async function loadBooks() {
    try {
        const response = await fetch('/api/books', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const books = await response.json();
            const tbody = document.querySelector('#books-table tbody');
            tbody.innerHTML = '';

            books.forEach(book => {
                const row = `
                    <tr>
                        <td>${book.id}</td>
                        <td>${book.title}</td>
                        <td>${book.author}</td>
                        <td>${book.isAvailable ? 'Yes' : 'No'}</td>
                        <td>
                            <button class="edit" data-permission="UpdateBook" onclick="editBook(${book.id})">Edit</button>
                            <button class="delete" data-permission="DeleteBook" onclick="deleteBook(${book.id})">Delete</button>
                        </td>
                    </tr>
                `;
                tbody.innerHTML += row;
            });
        }
    } catch (error) {
        console.error('Failed to load books', error);
    }
    applyPermissions();
}

async function loadUsers() {
    try {
        const response = await fetch('/api/users', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const users = await response.json();
            const tbody = document.querySelector('#users-table tbody');
            tbody.innerHTML = '';

            users.forEach(user => {
                const roles = user.userHasRoles ? user.userHasRoles.map(ur => ur.role.name).join(', ') : '';
                const row = `
                    <tr>
                        <td class="checkbox-column"><input type="checkbox" class="user-checkbox" value="${user.id}"></td>
                        <td>${user.id}</td>
                        <td>${user.displayName}</td>
                        <td>${user.username}</td>
                        <td>${roles}</td>
                        <td>
                            <button class="edit" onclick="editUser(${user.id})">Edit</button>
                            <button class="delete" onclick="deleteUser(${user.id})">Delete</button>
                        </td>
                    </tr>
                `;
                tbody.innerHTML += row;
            });

            // Add event listeners to checkboxes
            document.querySelectorAll('.user-checkbox').forEach(checkbox => {
                checkbox.addEventListener('change', updateDeleteButtonState);
            });
        }
    } catch (error) {
        console.error('Failed to load users', error);
    }
    applyPermissions();
}

async function loadLoans() {
    try {
        const response = await fetch('/api/loans', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const loans = await response.json();
            const tbody = document.querySelector('#loans-table tbody');
            tbody.innerHTML = '';

            loans.forEach(loan => {
                const status = loan.returnDate ? 'Returned' : 'Active';
                const row = `
                    <tr>
                        <td>${loan.id}</td>
                        <td>${loan.student.firstName} ${loan.student.lastName}</td>
                        <td>${loan.book.title}</td>
                        <td>${new Date(loan.borrowDate).toLocaleDateString()}</td>
                        <td>${loan.returnDate ? new Date(loan.returnDate).toLocaleDateString() : '-'}</td>
                        <td>${status}</td>
                        <td>
                            ${!loan.returnDate ? `<button class="edit" data-permission="ReturnBook" onclick="returnBook(${loan.id})">Return</button>` : ''}
                            <button class="delete" data-permission="DeleteLoan" onclick="deleteLoan(${loan.id})">Delete</button>
                        </td>
                    </tr>
                `;
                tbody.innerHTML += row;
            });
        }
    } catch (error) {
        console.error('Failed to load loans', error);
    }
    applyPermissions();
}

function openModal(modalId, title) {
    const modal = document.getElementById(modalId);
    modal.style.display = 'block';
    document.getElementById(`${modalId.replace('-modal', '')}-modal-title`).textContent = title;
}

function closeModal() {
    document.querySelectorAll('.modal').forEach(modal => {
        modal.style.display = 'none';
    });
    document.querySelectorAll('form').forEach(form => form.reset());
}

async function handleStudentSubmit(e) {
    e.preventDefault();
    const id = document.getElementById('student-id').value;
    const firstName = document.getElementById('student-first-name').value;
    const lastName = document.getElementById('student-last-name').value;

    const method = id ? 'PUT' : 'POST';
    const url = id ? `/api/students/${id}` : '/api/students';

    try {
        const response = await fetch(url, {
            method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ firstName, lastName })
        });

        if (response.ok) {
            closeModal();
            loadStudents();
            loadDashboard();
        }
    } catch (error) {
        console.error('Failed to save student', error);
    }
}

async function handleBookSubmit(e) {
    e.preventDefault();
    const id = document.getElementById('book-id').value;
    const title = document.getElementById('book-title').value;
    const author = document.getElementById('book-author').value;

    const method = id ? 'PUT' : 'POST';
    const url = id ? `/api/books/${id}` : '/api/books';

    try {
        const response = await fetch(url, {
            method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ title, author })
        });

        if (response.ok) {
            closeModal();
            loadBooks();
            loadDashboard();
        }
    } catch (error) {
        console.error('Failed to save book', error);
    }
}

async function handleUserSubmit(e) {
    e.preventDefault();
    const id = document.getElementById('user-id').value;
    const displayName = document.getElementById('user-display-name').value;
    const username = document.getElementById('user-username').value;
    const password = id ? undefined : document.getElementById('user-password').value; // Password only for new users
    const roleId = document.getElementById('user-role').value;

    const method = id ? 'PUT' : 'POST';
    const url = id ? `/api/users/${id}` : '/api/users';

    const body = { displayName, username };
    if (password) {
        body.password = password;
    }
    if (roleId) {
        body.roleId = parseInt(roleId);
    }

    try {
        const response = await fetch(url, {
            method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(body)
        });

        if (response.ok) {
            closeModal();
            loadUsers();
        }
    } catch (error) {
        console.error('Failed to save user', error);
    }
}

async function handleLoanSubmit(e) {
    e.preventDefault();
    const id = document.getElementById('loan-id').value;
    const studentId = document.getElementById('loan-student').value;
    const bookId = document.getElementById('loan-book').value;
    const borrowDate = document.getElementById('loan-borrow-date').value;
    const returnDate = document.getElementById('loan-return-date').value;

    const method = id ? 'PUT' : 'POST';
    const url = id ? `/api/loans/${id}` : '/api/loans/borrow';

    try {
        const response = await fetch(url, {
            method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                studentId: parseInt(studentId),
                bookId: parseInt(bookId),
                borrowDate,
                returnDate
            })
        });

        if (response.ok) {
            closeModal();
            loadLoans();
            loadDashboard();
        }
    } catch (error) {
        console.error('Failed to save loan', error);
    }
}

// Edit functions
async function editStudent(id) {
    try {
        const response = await fetch(`/api/students/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const student = await response.json();
            document.getElementById('student-id').value = student.id;
            document.getElementById('student-first-name').value = student.firstName;
            document.getElementById('student-last-name').value = student.lastName;
            openModal('student-modal', 'Edit Student');
        }
    } catch (error) {
        console.error('Failed to load student', error);
    }
}

async function editBook(id) {
    try {
        const response = await fetch(`/api/books/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const book = await response.json();
            document.getElementById('book-id').value = book.id;
            document.getElementById('book-title').value = book.title;
            document.getElementById('book-author').value = book.author;
            openModal('book-modal', 'Edit Book');
        } else {
            console.error('Failed to load book:', response.status, response.statusText);
        }
    } catch (error) {
        console.error('Failed to load book', error);
    }
}

async function editUser(id) {
    try {
        const response = await fetch(`/api/users/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const user = await response.json();
            document.getElementById('user-id').value = user.id;
            document.getElementById('user-display-name').value = user.displayName;
            document.getElementById('user-username').value = user.username;
            // Show password field for editing (leave empty to keep current password)
            document.getElementById('user-password').style.display = 'block';
            document.getElementById('user-password').required = false;
            document.getElementById('user-password').setAttribute('autocomplete', 'current-password');
            document.getElementById('user-password').value = ''; // Empty for security
            await loadRolesForUser();
            // Set role if available
            if (user.userHasRoles && user.userHasRoles.length > 0) {
                document.getElementById('user-role').value = user.userHasRoles[0].roleId;
            }
            openModal('user-modal', 'Edit User');
        }
    } catch (error) {
        console.error('Failed to load user', error);
    }
}

// Delete functions
async function deleteStudent(id) {
    if (confirm('Are you sure you want to delete this student?')) {
        try {
            const response = await fetch(`/api/students/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (response.ok) {
                loadStudents();
                loadDashboard();
            }
        } catch (error) {
            console.error('Failed to delete student', error);
        }
    }
}

async function deleteBook(id) {
    if (confirm('Are you sure you want to delete this book?')) {
        try {
            const response = await fetch(`/api/books/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (response.ok) {
                loadBooks();
                loadDashboard();
            }
        } catch (error) {
            console.error('Failed to delete book', error);
        }
    }
}

async function deleteUser(id) {
    if (confirm('Are you sure you want to delete this user?')) {
        try {
            const response = await fetch(`/api/users/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (response.ok) {
                loadUsers();
            }
        } catch (error) {
            console.error('Failed to delete user', error);
        }
    }
}

async function deleteLoan(id) {
    if (confirm('Are you sure you want to delete this loan?')) {
        try {
            const response = await fetch(`/api/loans/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (response.ok) {
                loadLoans();
                loadDashboard();
            }
        } catch (error) {
            console.error('Failed to delete loan', error);
        }
    }
}

async function returnBook(loanId) {
    try {
        const response = await fetch(`/api/loans/return/${loanId}`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            loadLoans();
            loadDashboard();
        }
    } catch (error) {
        console.error('Failed to return book', error);
    }
}

async function loadCurrentUser() {
    try {
        // Decode JWT token to get user info (simple decode, not secure for production)
        const payload = JSON.parse(atob(token.split('.')[1]));
        const username = payload.sub || payload.username || 'Unknown User';

        // Try to get full user info from API
        const response = await fetch('/api/auth/me', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const user = await response.json();
            document.getElementById('current-user').textContent = `Current User: ${user.displayName || user.username}`;
        } else {
            document.getElementById('current-user').textContent = `Current User: ${username}`;
        }
    } catch (error) {
        console.error('Failed to load current user', error);
        document.getElementById('current-user').textContent = 'Current User: Unknown';
    }
}

async function handleRoleSubmit(e) {
    e.preventDefault();
    const id = document.getElementById('role-id').value;
    const name = document.getElementById('role-name').value;
    const selectedRights = Array.from(document.querySelectorAll('.right-checkbox:checked')).map(cb => parseInt(cb.value));

    const method = id ? 'PUT' : 'POST';
    const url = id ? `/api/roles/${id}` : '/api/roles';

    try {
        const response = await fetch(url, {
            method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ name, rightIds: selectedRights })
        });

        if (response.ok) {
            closeModal();
            loadRoles();
        }
    } catch (error) {
        console.error('Failed to save role', error);
    }
}

async function editRole(id) {
    try {
        const response = await fetch(`/api/roles/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const role = await response.json();
            document.getElementById('role-id').value = role.id;
            document.getElementById('role-name').value = role.name;

            // Load rights and check assigned ones
            await loadRightsForRole();
            if (role.roleHasRights) {
                role.roleHasRights.forEach(rhr => {
                    const checkbox = document.querySelector(`.right-checkbox[value="${rhr.rightId}"]`);
                    if (checkbox) checkbox.checked = true;
                });
            }

            openModal('role-modal', 'Edit Role');
        }
    } catch (error) {
        console.error('Failed to load role', error);
    }
}

async function deleteRole(id) {
    if (confirm('Are you sure you want to delete this role?')) {
        try {
            const response = await fetch(`/api/roles/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (response.ok) {
                loadRoles();
            }
        } catch (error) {
            console.error('Failed to delete role', error);
        }
    }
}

async function loadStudentsForLoan() {
    try {
        const response = await fetch('/api/students', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const students = await response.json();
            const select = document.getElementById('loan-student');
            select.innerHTML = '<option value="">Select Student</option>';

            students.forEach(student => {
                const option = document.createElement('option');
                option.value = student.id;
                option.textContent = `${student.firstName} ${student.lastName}`;
                select.appendChild(option);
            });
        }
    } catch (error) {
        console.error('Failed to load students for loan', error);
    }
}

async function loadBooksForLoan() {
    try {
        const response = await fetch('/api/books', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const books = await response.json();
            const select = document.getElementById('loan-book');
            select.innerHTML = '<option value="">Select Book</option>';

            books.forEach(book => {
                const option = document.createElement('option');
                option.value = book.id;
                option.textContent = `${book.title} by ${book.author}`;
                select.appendChild(option);
            });
        }
    } catch (error) {
        console.error('Failed to load books for loan', error);
    }
}

async function loadRoles() {
    try {
        const response = await fetch('/api/roles', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const roles = await response.json();
            const tbody = document.querySelector('#roles-table tbody');
            tbody.innerHTML = '';

            roles.forEach(role => {
                const rights = role.roleHasRights ? role.roleHasRights.map(rr => rr.right.name).join(', ') : '';
                const row = `
                    <tr>
                        <td class="checkbox-column"><input type="checkbox" class="role-checkbox" value="${role.id}"></td>
                        <td>${role.id}</td>
                        <td>${role.name}</td>
                        <td>${rights}</td>
                        <td>
                            <button class="edit" data-permission="UpdateRole" onclick="editRole(${role.id})">Edit</button>
                            <button class="delete" data-permission="DeleteRole" onclick="deleteRole(${role.id})">Delete</button>
                        </td>
                    </tr>
                `;
                tbody.innerHTML += row;
            });

            // Add event listeners to checkboxes
            document.querySelectorAll('.role-checkbox').forEach(checkbox => {
                checkbox.addEventListener('change', updateDeleteRolesButtonState);
            });
        }
    } catch (error) {
        console.error('Failed to load roles', error);
    }
    applyPermissions();
}

async function loadRolesForUser() {
    try {
        const response = await fetch('/api/roles', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const roles = await response.json();
            const select = document.getElementById('user-role');
            select.innerHTML = '<option value="">Select Role</option>';

            roles.forEach(role => {
                const option = document.createElement('option');
                option.value = role.id;
                option.textContent = role.name;
                select.appendChild(option);
            });
        }
    } catch (error) {
        console.error('Failed to load roles for user', error);
    }
}

async function loadRightsForRole() {
    try {
        const response = await fetch('/api/rights', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const rights = await response.json();
            const rightsList = document.getElementById('rights-list');
            rightsList.innerHTML = '';

            rights.forEach(right => {
                const checkboxDiv = document.createElement('div');
                checkboxDiv.innerHTML = `
                    <label>
                        <input type="checkbox" value="${right.id}" class="right-checkbox">
                        ${right.name}
                    </label>
                `;
                rightsList.appendChild(checkboxDiv);
            });
        }
    } catch (error) {
        console.error('Failed to load rights for role', error);
    }
}

function toggleSelectAllUsers() {
    const selectAllCheckbox = document.getElementById('select-all-users');
    const userCheckboxes = document.querySelectorAll('.user-checkbox');

    userCheckboxes.forEach(checkbox => {
        checkbox.checked = selectAllCheckbox.checked;
    });

    updateDeleteButtonState();
}

function updateDeleteButtonState() {
    const checkedBoxes = document.querySelectorAll('.user-checkbox:checked');
    const deleteButton = document.getElementById('delete-selected-users-btn');

    if (checkedBoxes.length > 0) {
        deleteButton.disabled = false;
        deleteButton.textContent = `Delete Selected (${checkedBoxes.length})`;
    } else {
        deleteButton.disabled = true;
        deleteButton.textContent = 'Delete Selected';
    }
}

async function deleteSelectedUsers() {
    const checkedBoxes = document.querySelectorAll('.user-checkbox:checked');
    const userIds = Array.from(checkedBoxes).map(cb => parseInt(cb.value));

    if (userIds.length === 0) {
        alert('No users selected');
        return;
    }

    const confirmMessage = `Are you sure you want to delete ${userIds.length} user(s)?`;
    if (!confirm(confirmMessage)) {
        return;
    }

    try {
        const deletePromises = userIds.map(id =>
            fetch(`/api/users/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            })
        );

        const results = await Promise.all(deletePromises);
        const successCount = results.filter(r => r.ok).length;

        if (successCount === userIds.length) {
            alert(`Successfully deleted ${successCount} user(s)`);
        } else {
            alert(`Deleted ${successCount} out of ${userIds.length} user(s)`);
        }

        loadUsers();
    } catch (error) {
        console.error('Failed to delete users', error);
        alert('Failed to delete users');
    }
}

function toggleSelectAllRoles() {
    const selectAllCheckbox = document.getElementById('select-all-roles');
    const roleCheckboxes = document.querySelectorAll('.role-checkbox');

    roleCheckboxes.forEach(checkbox => {
        checkbox.checked = selectAllCheckbox.checked;
    });

    updateDeleteRolesButtonState();
}

function updateDeleteRolesButtonState() {
    const checkedBoxes = document.querySelectorAll('.role-checkbox:checked');
    const deleteButton = document.getElementById('delete-selected-roles-btn');

    if (checkedBoxes.length > 0) {
        deleteButton.disabled = false;
        deleteButton.textContent = `Delete Selected (${checkedBoxes.length})`;
    } else {
        deleteButton.disabled = true;
        deleteButton.textContent = 'Delete Selected';
    }
}

async function deleteSelectedRoles() {
    const checkedBoxes = document.querySelectorAll('.role-checkbox:checked');
    const roleIds = Array.from(checkedBoxes).map(cb => parseInt(cb.value));

    if (roleIds.length === 0) {
        alert('No roles selected');
        return;
    }

    const confirmMessage = `Are you sure you want to delete ${roleIds.length} role(s)?`;
    if (!confirm(confirmMessage)) {
        return;
    }

    try {
        const deletePromises = roleIds.map(id =>
            fetch(`/api/roles/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            })
        );

        const results = await Promise.all(deletePromises);
        const successCount = results.filter(r => r.ok).length;

        if (successCount === roleIds.length) {
            alert(`Successfully deleted ${successCount} role(s)`);
        } else {
            alert(`Deleted ${successCount} out of ${roleIds.length} role(s)`);
        }

        loadRoles();
    } catch (error) {
        console.error('Failed to delete roles', error);
        alert('Failed to delete roles');
    }
}

function hasPermission(rightName) {
    const hasRight = userRights.includes(rightName);
    console.log(`Checking permission for ${rightName}: ${hasRight}`);
    return hasRight;
}

function applyPermissions() {
    // Apply permissions to all elements with data-permission attribute
    document.querySelectorAll('[data-permission]').forEach(element => {
        const requiredPermission = element.getAttribute('data-permission');
        const permissions = requiredPermission.split(',').map(p => p.trim());
        const hasAnyPermission = permissions.some(permission => hasPermission(permission));

        if (!hasAnyPermission) {
            element.style.display = 'none';
        } else {
            element.style.display = element.tagName === 'BUTTON' ? 'inline-block' : '';
        }
    });
}