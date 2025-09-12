// login.js
Page({
  data: {
    username: '',
    password: '',
    loading: false
  },

  // 用户名输入
  onUsernameInput(e) {
    this.setData({
      username: e.detail.value
    });
  },

  // 密码输入
  onPasswordInput(e) {
    this.setData({
      password: e.detail.value
    });
  },

  // 登录功能
  handleLogin() {
    const { username, password } = this.data;
     // 先清空所有旧缓存
    wx.removeStorageSync('userInfo');
    wx.removeStorageSync('token');
    wx.removeStorageSync('customerId');
    wx.removeStorageSync('isLogin');

    // 表单验证
    if (!username.trim()) {
      wx.showToast({
        title: '请输入用户名',
        icon: 'none'
      });
      return;
    }

    if (!password.trim()) {
      wx.showToast({
        title: '请输入密码',
        icon: 'none'
      });
      return;
    }

    // 显示加载状态
    this.setData({ loading: true });

<<<<<<< Updated upstream
    // 模拟登录验证（纯前端，无后端验证）
    setTimeout(() => {
      // 验证演示账户
      const validAccounts = {
        'admin': '123456',
        'user': '123456',
        'test': '123456'
      };

      if (validAccounts[username] && validAccounts[username] === password) {
        // 登录成功，保存用户信息到本地存储
        const userInfo = {
          username: username,
          nickname: username === 'admin' ? '系统管理员' : 
                   username === 'user' ? '普通用户' : 
                   username === 'test' ? '测试用户' : '用户昵称',
          avatar: '/images/default-avatar.png',
          phone: '138****8888',
          memberLevel: username === 'admin' ? '钻石会员' : '黄金会员',
          points: username === 'admin' ? 5000 : 1520,
          loginTime: new Date().getTime()
        };

        wx.setStorageSync('userInfo', userInfo);
        wx.setStorageSync('isLogin', true);

=======
    try {
        const loginResult = await this.requestLogin({
          username: username,
          password: password
        });
    
        console.log('🔍 完整的登录返回结果:', loginResult);
        console.log('🔍 loginResult.data 结构:', loginResult.data);
        console.log('🔍 loginResult.token:', loginResult.token);
    
        if (loginResult.success) {
            // 正确的数据提取方式
            const userInfo = loginResult.data.userInfo; // 从 data.userInfo 获取
            const customerId = userInfo.customerId;     // 从 userInfo.customerId 获取
            const token = loginResult.token || '';      // token 为 undefined
            // 保存到缓存
            wx.setStorageSync('userInfo', userInfo);
            wx.setStorageSync('isLogin', true);
            wx.setStorageSync('token', token);          // 这里会存储空字符串
            wx.setStorageSync('customerId', customerId);
          
            console.log('✅ 用户信息已更新到缓存:', {
              userInfo: userInfo,
              token: token,
              customerId: customerId
            });
>>>>>>> Stashed changes
        this.setData({ loading: false });

        wx.showToast({
          title: '登录成功',
          icon: 'success',
          duration: 1500
        });

        // 登录成功后跳转到主界面
        setTimeout(() => {
          wx.switchTab({
            url: '/pages/index/index'
          });
        }, 1500);
      } else {
        // 登录失败 
        this.setData({ loading: false });
        
        wx.showToast({
          title: '用户名或密码错误',
          icon: 'none',
          duration: 2000
        });
      }

    }, 1000); // 模拟网络请求延迟
  },

  // 注册功能（暂时只是提示）
  handleRegister() {
    wx.showToast({
      title: '注册功能待开发',
      icon: 'none'
    });
  },

  // 忘记密码
  handleForgotPassword() {
    wx.showToast({
      title: '找回密码功能待开发',
      icon: 'none'
    });
  },

  onLoad() {
    // 检查是否已经登录
    const isLogin = wx.getStorageSync('isLogin');
    if (isLogin) {
      // 如果已登录，直接跳转到主界面
      wx.switchTab({
        url: '/pages/index/index'
      });
    }
  }
}); 