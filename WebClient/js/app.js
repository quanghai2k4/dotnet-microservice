// Microservice Management System - JavaScript Application

// Configuration
const API_BASE_URL = window.location.origin;
const ENDPOINTS = {
    users: '/api/users',
    orders: '/api/orders',
    health: '/health'
};

// Global variables
let users = [];
let orders = [];
let currentSection = 'dashboard';

// Initialize application
document.addEventListener('DOMContentLoaded', function() {
    initializeApp();
});

async function initializeApp() {
    await checkSystemHealth();
    await loadDashboardData();
    showSection('dashboard');
}

// Navigation
function showSection(section) {
    // Hide all sections
    document.querySelectorAll('.content-section').forEach(el => {
        el.style.display = 'none';
    });
    
    // Update nav links
    document.querySelectorAll('.nav-link').forEach(el => {
        el.classList.remove('active');
    });
    
    // Show selected section
    document.getElementById(section).style.display = 'block';
    
    // Update active nav link
    event?.target?.classList?.add('active');
    
    currentSection = section;
    
    // Load section-specific data
    switch(section) {
        case 'users':
            loadUsers();
            break;
        case 'orders':
            loadOrders();
            break;
        case 'dashboard':
            loadDashboardData();
            break;
    }
}

// System Health Check
async function checkSystemHealth() {
    try {
        const response = await fetch(`${API_BASE_URL}${ENDPOINTS.health}`);
        const status = response.ok ? 'Online' : 'Offline';
        document.getElementById('systemStatus').textContent = status;
        
        if (!response.ok) {
            showToast('System health check failed', 'error');
        }
    } catch (error) {
        document.getElementById('systemStatus').textContent = 'Offline';
        console.error('Health check failed:', error);
    }
}

// Dashboard functions
async function loadDashboardData() {
    try {
        const [usersResponse, ordersResponse] = await Promise.all([
            fetch(`${API_BASE_URL}${ENDPOINTS.users}`),
            fetch(`${API_BASE_URL}${ENDPOINTS.orders}`)
        ]);
        
        if (usersResponse.ok && ordersResponse.ok) {
            const usersData = await usersResponse.json();
            const ordersData = await ordersResponse.json();
            
            document.getElementById('totalUsers').textContent = usersData.length;
            document.getElementById('totalOrders').textContent = ordersData.length;
            
            users = usersData;
            orders = ordersData;
        }
    } catch (error) {
        console.error('Error loading dashboard data:', error);
        showToast('Failed to load dashboard data', 'error');
    }
}

// User Management Functions
async function loadUsers() {
    showLoading('usersLoading', true);
    hideTable('usersTable');
    
    try {
        const response = await fetch(`${API_BASE_URL}${ENDPOINTS.users}`);
        if (response.ok) {
            users = await response.json();
            displayUsers();
            showToast(`Loaded ${users.length} users successfully`, 'success');
        } else {
            throw new Error('Failed to load users');
        }
    } catch (error) {
        console.error('Error loading users:', error);
        showToast('Failed to load users', 'error');
    } finally {
        showLoading('usersLoading', false);
    }
}

function displayUsers() {
    const tbody = document.getElementById('usersTableBody');
    tbody.innerHTML = '';
    
    users.forEach(user => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${user.id}</td>
            <td>${escapeHtml(user.name)}</td>
            <td>${escapeHtml(user.email)}</td>
            <td>${formatDate(user.createdAt)}</td>
            <td>
                <button class="btn btn-outline-primary btn-action btn-sm" onclick="viewUser(${user.id})">
                    <i class="bi bi-eye"></i> View
                </button>
                <button class="btn btn-outline-danger btn-action btn-sm" onclick="deleteUser(${user.id})">
                    <i class="bi bi-trash"></i> Delete
                </button>
            </td>
        `;
        tbody.appendChild(row);
    });
    
    showTable('usersTable');
}

function resetUserForm() {
    document.getElementById('userForm').reset();
    document.querySelectorAll('.form-control').forEach(el => {
        el.classList.remove('is-valid', 'is-invalid');
    });
}

async function saveUser() {
    const name = document.getElementById('userName').value.trim();
    const email = document.getElementById('userEmail').value.trim();
    
    if (!name || !email) {
        showToast('Please fill in all fields', 'error');
        return;
    }
    
    if (!isValidEmail(email)) {
        showToast('Please enter a valid email address', 'error');
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}${ENDPOINTS.users}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ name, email })
        });
        
        if (response.ok) {
            const newUser = await response.json();
            users.push(newUser);
            displayUsers();
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('userModal'));
            modal.hide();
            
            showToast(`User "${name}" created successfully!`, 'success');
            
            // Update dashboard if visible
            if (currentSection === 'dashboard') {
                loadDashboardData();
            }
        } else {
            const error = await response.text();
            throw new Error(error || 'Failed to create user');
        }
    } catch (error) {
        console.error('Error creating user:', error);
        showToast('Failed to create user', 'error');
    }
}

function viewUser(userId) {
    const user = users.find(u => u.id === userId);
    if (user) {
        alert(`User Details:\n\nID: ${user.id}\nName: ${user.name}\nEmail: ${user.email}\nCreated: ${formatDate(user.createdAt)}`);
    }
}

async function deleteUser(userId) {
    const user = users.find(u => u.id === userId);
    if (!user) return;
    
    if (!confirm(`Are you sure you want to delete user "${user.name}"?`)) {
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}${ENDPOINTS.users}/${userId}`, {
            method: 'DELETE'
        });
        
        if (response.ok) {
            users = users.filter(u => u.id !== userId);
            displayUsers();
            showToast(`User "${user.name}" deleted successfully`, 'success');
            
            // Update dashboard if visible
            if (currentSection === 'dashboard') {
                loadDashboardData();
            }
        } else {
            throw new Error('Failed to delete user');
        }
    } catch (error) {
        console.error('Error deleting user:', error);
        showToast('Failed to delete user', 'error');
    }
}

// Order Management Functions
async function loadOrders() {
    showLoading('ordersLoading', true);
    hideTable('ordersTable');
    
    try {
        // Load orders and users for dropdown
        const [ordersResponse, usersResponse] = await Promise.all([
            fetch(`${API_BASE_URL}${ENDPOINTS.orders}`),
            fetch(`${API_BASE_URL}${ENDPOINTS.users}`)
        ]);
        
        if (ordersResponse.ok) {
            orders = await ordersResponse.json();
            displayOrders();
            showToast(`Loaded ${orders.length} orders successfully`, 'success');
        }
        
        if (usersResponse.ok) {
            users = await usersResponse.json();
            populateUserDropdown();
        }
    } catch (error) {
        console.error('Error loading orders:', error);
        showToast('Failed to load orders', 'error');
    } finally {
        showLoading('ordersLoading', false);
    }
}

function displayOrders() {
    const tbody = document.getElementById('ordersTableBody');
    tbody.innerHTML = '';
    
    orders.forEach(order => {
        const row = document.createElement('tr');
        const statusClass = `status-${order.status.toLowerCase()}`;
        
        row.innerHTML = `
            <td>${order.id}</td>
            <td>${order.userId}</td>
            <td>${escapeHtml(order.productName)}</td>
            <td>${order.quantity}</td>
            <td>$${order.price.toFixed(2)}</td>
            <td><span class="badge ${statusClass}">${order.status}</span></td>
            <td>${formatDate(order.createdAt)}</td>
            <td>
                <button class="btn btn-outline-primary btn-action btn-sm" onclick="viewOrder(${order.id})">
                    <i class="bi bi-eye"></i> View
                </button>
                <button class="btn btn-outline-danger btn-action btn-sm" onclick="deleteOrder(${order.id})">
                    <i class="bi bi-trash"></i> Delete
                </button>
            </td>
        `;
        tbody.appendChild(row);
    });
    
    showTable('ordersTable');
}

function populateUserDropdown() {
    const select = document.getElementById('orderUserId');
    select.innerHTML = '<option value="">Select a user</option>';
    
    users.forEach(user => {
        const option = document.createElement('option');
        option.value = user.id;
        option.textContent = `${user.name} (${user.email})`;
        select.appendChild(option);
    });
}

function resetOrderForm() {
    document.getElementById('orderForm').reset();
    document.querySelectorAll('.form-control').forEach(el => {
        el.classList.remove('is-valid', 'is-invalid');
    });
    populateUserDropdown();
}

async function saveOrder() {
    const userId = document.getElementById('orderUserId').value;
    const productName = document.getElementById('orderProduct').value.trim();
    const quantity = parseInt(document.getElementById('orderQuantity').value);
    const price = parseFloat(document.getElementById('orderPrice').value);
    
    if (!userId || !productName || !quantity || !price) {
        showToast('Please fill in all fields', 'error');
        return;
    }
    
    if (quantity < 1) {
        showToast('Quantity must be at least 1', 'error');
        return;
    }
    
    if (price < 0) {
        showToast('Price must be greater than or equal to 0', 'error');
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}${ENDPOINTS.orders}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                userId: parseInt(userId),
                productName,
                quantity,
                price
            })
        });
        
        if (response.ok) {
            const newOrder = await response.json();
            orders.push(newOrder);
            displayOrders();
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('orderModal'));
            modal.hide();
            
            showToast(`Order for "${productName}" created successfully!`, 'success');
            
            // Update dashboard if visible
            if (currentSection === 'dashboard') {
                loadDashboardData();
            }
        } else {
            const error = await response.text();
            throw new Error(error || 'Failed to create order');
        }
    } catch (error) {
        console.error('Error creating order:', error);
        showToast('Failed to create order', 'error');
    }
}

function viewOrder(orderId) {
    const order = orders.find(o => o.id === orderId);
    if (order) {
        const user = users.find(u => u.id === order.userId);
        const userName = user ? user.name : 'Unknown User';
        
        alert(`Order Details:\n\nID: ${order.id}\nUser: ${userName} (ID: ${order.userId})\nProduct: ${order.productName}\nQuantity: ${order.quantity}\nPrice: $${order.price.toFixed(2)}\nStatus: ${order.status}\nCreated: ${formatDate(order.createdAt)}`);
    }
}

async function deleteOrder(orderId) {
    const order = orders.find(o => o.id === orderId);
    if (!order) return;
    
    if (!confirm(`Are you sure you want to delete order for "${order.productName}"?`)) {
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}${ENDPOINTS.orders}/${orderId}`, {
            method: 'DELETE'
        });
        
        if (response.ok) {
            orders = orders.filter(o => o.id !== orderId);
            displayOrders();
            showToast(`Order for "${order.productName}" deleted successfully`, 'success');
            
            // Update dashboard if visible
            if (currentSection === 'dashboard') {
                loadDashboardData();
            }
        } else {
            throw new Error('Failed to delete order');
        }
    } catch (error) {
        console.error('Error deleting order:', error);
        showToast('Failed to delete order', 'error');
    }
}

// Utility Functions
function showLoading(elementId, show) {
    const element = document.getElementById(elementId);
    if (element) {
        element.style.display = show ? 'block' : 'none';
    }
}

function showTable(tableId) {
    const table = document.getElementById(tableId);
    if (table) {
        table.style.display = 'table';
    }
}

function hideTable(tableId) {
    const table = document.getElementById(tableId);
    if (table) {
        table.style.display = 'none';
    }
}

function showToast(message, type = 'success') {
    const toastId = type === 'success' ? 'successToast' : 'errorToast';
    const messageId = type === 'success' ? 'successMessage' : 'errorMessage';
    
    document.getElementById(messageId).textContent = message;
    
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement);
    toast.show();
}

function formatDate(dateString) {
    if (!dateString) return 'N/A';
    
    try {
        const date = new Date(dateString);
        return date.toLocaleString('vi-VN', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    } catch (error) {
        return 'Invalid Date';
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Auto-refresh functionality
setInterval(() => {
    if (currentSection === 'dashboard') {
        checkSystemHealth();
        loadDashboardData();
    }
}, 30000); // Refresh every 30 seconds