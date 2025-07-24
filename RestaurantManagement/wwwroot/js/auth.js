// 认证和权限管理工具
const Auth = {
    // 检查登录状态
    checkLogin: function() {
        const currentUser = sessionStorage.getItem('currentUser');
        if (!currentUser) {
            window.location.href = 'login.html';
            return false;
        }
        return JSON.parse(currentUser);
    },

    // 获取当前用户信息
    getCurrentUser: function() {
        const currentUser = sessionStorage.getItem('currentUser');
        return currentUser ? JSON.parse(currentUser) : null;
    },

    // 检查用户权限
    hasPermission: function(permission) {
        const user = this.getCurrentUser();
        if (!user) return false;
        
        // 超级管理员拥有所有权限
        if (user.permissions.includes('all')) return true;
        
        return user.permissions.includes(permission);
    },

    // 退出登录
    logout: function() {
        if (confirm('确认要退出登录吗？')) {
            sessionStorage.removeItem('currentUser');
            sessionStorage.removeItem('loginTime');
            window.location.href = 'login.html';
        }
    },

    // 获取用户角色显示文本
    getRoleText: function(user) {
        if (!user) return '';
        return user.role;
    },

    // 获取登录时长
    getLoginDuration: function() {
        const loginTime = sessionStorage.getItem('loginTime');
        if (!loginTime) return '';
        
        const login = new Date(loginTime);
        const now = new Date();
        const diff = now - login;
        
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
        
        if (hours > 0) {
            return `${hours}小时${minutes}分钟`;
        } else {
            return `${minutes}分钟`;
        }
    },

    // 权限控制的按钮显示/隐藏
    controlButtonVisibility: function() {
        const user = this.getCurrentUser();
        if (!user) return;

        // 根据权限控制页面元素显示
        const permissionElements = document.querySelectorAll('[data-permission]');
        permissionElements.forEach(element => {
            const requiredPermission = element.getAttribute('data-permission');
            if (!this.hasPermission(requiredPermission)) {
                element.style.display = 'none';
            }
        });
    },

    // 初始化认证状态
    init: function() {
        // 检查登录状态
        const user = this.checkLogin();
        if (!user) return;

        // 控制权限相关的UI元素
        this.controlButtonVisibility();

        // 添加用户信息到页面
        this.addUserInfoToPage(user);
        
        return user;
    },

    // 添加用户信息到页面
    addUserInfoToPage: function(user) {
        // 查找导航栏
        const navbar = document.querySelector('.navbar');
        if (!navbar) return;

        // 检查是否已经添加了用户信息
        if (navbar.querySelector('.user-info')) return;

        // 创建用户信息区域
        const userInfo = document.createElement('div');
        userInfo.className = 'user-info';
        userInfo.style.cssText = `
            display: flex;
            align-items: center;
            gap: 1rem;
            margin-left: auto;
            color: white;
            font-size: 0.9rem;
        `;

        userInfo.innerHTML = `
            <div class="user-details" style="text-align: right;">
                <div style="font-weight: 500;">${user.username}</div>
                <div style="font-size: 0.8rem; opacity: 0.8;">${user.role}</div>
            </div>
            <div class="user-actions" style="display: flex; gap: 0.5rem;">
                <button onclick="Auth.showUserProfile()" class="btn-user-action" style="
                    background: rgba(255,255,255,0.2);
                    color: white;
                    border: none;
                    padding: 0.5rem;
                    border-radius: 4px;
                    cursor: pointer;
                    font-size: 0.8rem;
                    transition: background-color 0.2s;
                " onmouseover="this.style.backgroundColor='rgba(255,255,255,0.3)'" 
                   onmouseout="this.style.backgroundColor='rgba(255,255,255,0.2)'">
                    👤
                </button>
                <button onclick="Auth.logout()" class="btn-user-action" style="
                    background: rgba(255,255,255,0.2);
                    color: white;
                    border: none;
                    padding: 0.5rem;
                    border-radius: 4px;
                    cursor: pointer;
                    font-size: 0.8rem;
                    transition: background-color 0.2s;
                " onmouseover="this.style.backgroundColor='rgba(220,53,69,0.8)'" 
                   onmouseout="this.style.backgroundColor='rgba(255,255,255,0.2)'">
                    🚪
                </button>
            </div>
        `;

        // 添加到导航栏
        navbar.appendChild(userInfo);
    },

    // 显示用户资料
    showUserProfile: function() {
        const user = this.getCurrentUser();
        if (!user) return;

        const duration = this.getLoginDuration();
        
        alert(`用户资料
用户名: ${user.username}
角色: ${user.role}
登录时间: ${new Date(user.loginTime).toLocaleString('zh-CN')}
在线时长: ${duration}

权限列表:
${user.permissions.includes('all') ? '• 系统所有权限' : user.permissions.map(p => '• ' + this.getPermissionText(p)).join('\n')}`);
    },

    // 获取权限描述文本
    getPermissionText: function(permission) {
        const permissionTexts = {
            'staff': '员工管理',
            'inventory': '库存管理',
            'menu': '菜单管理',
            'tables': '桌台管理',
            'orders': '订单管理',
            'reports': '数据报表',
            'basic-reports': '基础报表',
            'all': '系统所有权限'
        };
        return permissionTexts[permission] || permission;
    }
};

// 页面权限检查
document.addEventListener('DOMContentLoaded', function() {
    // 排除登录页面的权限检查
    if (window.location.pathname.endsWith('login.html')) {
        return;
    }
    
    // 初始化认证
    Auth.init();
});
