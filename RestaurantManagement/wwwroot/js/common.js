// 通用工具函数和全局变量

// API基础URL
const API_BASE = '/api';

// 通用的API请求函数
async function apiRequest(url, options = {}) {
    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    const config = {
        ...defaultOptions,
        ...options,
        headers: {
            ...defaultOptions.headers,
            ...options.headers,
        },
    };

    try {
        const response = await fetch(API_BASE + url, config);
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        // 检查响应是否有内容
        const contentType = response.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            return await response.json();
        } else {
            return null; // 对于成功的空响应
        }
    } catch (error) {
        console.error('API请求失败:', error);
        throw error;
    }
}

// GET请求
async function apiGet(url) {
    return await apiRequest(url, { method: 'GET' });
}

// POST请求
async function apiPost(url, data) {
    return await apiRequest(url, {
        method: 'POST',
        body: JSON.stringify(data),
    });
}

// PUT请求
async function apiPut(url, data) {
    return await apiRequest(url, {
        method: 'PUT',
        body: JSON.stringify(data),
    });
}

// DELETE请求
async function apiDelete(url) {
    return await apiRequest(url, { method: 'DELETE' });
}

// 显示通知消息
function showNotification(message, type = 'info') {
    // 移除现有的通知
    const existingNotification = document.querySelector('.notification');
    if (existingNotification) {
        existingNotification.remove();
    }

    // 创建通知元素
    const notification = document.createElement('div');
    notification.className = `notification alert alert-${type}`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 1001;
        min-width: 300px;
        padding: 1rem;
        border-radius: 4px;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        animation: slideIn 0.3s ease-out;
    `;

    // 设置通知类型样式
    switch (type) {
        case 'success':
            notification.style.backgroundColor = '#d4edda';
            notification.style.color = '#155724';
            notification.style.borderColor = '#c3e6cb';
            break;
        case 'error':
            notification.style.backgroundColor = '#f8d7da';
            notification.style.color = '#721c24';
            notification.style.borderColor = '#f5c6cb';
            break;
        case 'warning':
            notification.style.backgroundColor = '#fff3cd';
            notification.style.color = '#856404';
            notification.style.borderColor = '#ffeaa7';
            break;
        default:
            notification.style.backgroundColor = '#d1ecf1';
            notification.style.color = '#0c5460';
            notification.style.borderColor = '#bee5eb';
    }

    notification.textContent = message;

    // 添加关闭按钮
    const closeBtn = document.createElement('span');
    closeBtn.innerHTML = '&times;';
    closeBtn.style.cssText = `
        float: right;
        font-size: 20px;
        font-weight: bold;
        cursor: pointer;
        margin-left: 10px;
    `;
    closeBtn.onclick = () => notification.remove();
    notification.appendChild(closeBtn);

    // 添加到页面
    document.body.appendChild(notification);

    // 3秒后自动移除
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 3000);
}

// 添加滑入动画
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
`;
document.head.appendChild(style);

// 确认对话框
function showConfirm(message, callback) {
    const result = confirm(message);
    if (result && callback) {
        callback();
    }
    return result;
}

// 格式化日期时间
function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
    });
}

// 格式化日期
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    });
}

// 格式化货币
function formatCurrency(amount) {
    return '¥' + parseFloat(amount).toFixed(2);
}

// 获取状态显示文本和样式类
function getStatusInfo(status, type = 'table') {
    const statusMap = {
        table: {
            '空闲': { text: '空闲', class: 'status-available' },
            '占用': { text: '占用', class: 'status-occupied' },
            '预订': { text: '预订', class: 'status-reserved' },
            '清洁中': { text: '清洁中', class: 'status-cleaning' }
        },
        order: {
            '进行中': { text: '进行中', class: 'status-occupied' },
            '已结账': { text: '已结账', class: 'status-available' },
            '已取消': { text: '已取消', class: 'status-cleaning' }
        },
        staff: {
            '在职': { text: '在职', class: 'status-available' },
            '离职': { text: '离职', class: 'status-cleaning' }
        },
        attendance: {
            '正常': { text: '正常', class: 'status-available' },
            '迟到': { text: '迟到', class: 'status-occupied' },
            '早退': { text: '早退', class: 'status-occupied' },
            '缺勤': { text: '缺勤', class: 'status-cleaning' }
        }
    };

    return statusMap[type]?.[status] || { text: status, class: 'status-secondary' };
}

// 表单验证
function validateForm(formElement) {
    const inputs = formElement.querySelectorAll('input[required], select[required], textarea[required]');
    let isValid = true;

    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.style.borderColor = '#e74c3c';
            isValid = false;
        } else {
            input.style.borderColor = '#ddd';
        }
    });

    return isValid;
}

// 重置表单
function resetForm(formElement) {
    formElement.reset();
    const inputs = formElement.querySelectorAll('input, select, textarea');
    inputs.forEach(input => {
        input.style.borderColor = '#ddd';
    });
}

// 模态框管理
function showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'block';
        // 添加点击背景关闭功能
        modal.onclick = function(event) {
            if (event.target === modal) {
                hideModal(modalId);
            }
        };
    }
}

function hideModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
    }
}

// 页面加载时的通用初始化
document.addEventListener('DOMContentLoaded', function() {
    // 设置当前导航项为活跃状态
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.nav-links a');
    
    navLinks.forEach(link => {
        link.classList.remove('active');
        if (link.getAttribute('href') === currentPath.split('/').pop() || 
            (currentPath === '/' && link.getAttribute('href') === 'index.html')) {
            link.classList.add('active');
        }
    });

    // 为所有关闭按钮添加事件监听器
    document.querySelectorAll('.close').forEach(closeBtn => {
        closeBtn.onclick = function() {
            const modal = this.closest('.modal');
            if (modal) {
                modal.style.display = 'none';
            }
        };
    });

    // ESC键关闭模态框
    document.addEventListener('keydown', function(event) {
        if (event.key === 'Escape') {
            const openModals = document.querySelectorAll('.modal[style*="block"]');
            openModals.forEach(modal => {
                modal.style.display = 'none';
            });
        }
    });
});

// 导出给全局使用
window.API = {
    get: apiGet,
    post: apiPost,
    put: apiPut,
    delete: apiDelete
};

window.Utils = {
    showNotification,
    showConfirm,
    formatDateTime,
    formatDate,
    formatCurrency,
    getStatusInfo,
    validateForm,
    resetForm,
    showModal,
    hideModal
};
