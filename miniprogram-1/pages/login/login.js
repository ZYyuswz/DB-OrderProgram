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
  async handleLogin() {
    const { username, password } = this.data;

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

    try {
      // 调用真实API进行登录验证
      const loginResult = await this.requestLogin({
        username: username,
        password: password
      });

      if (loginResult.success) {
        // 登录成功，保存用户信息到本地存储
        const userInfo = loginResult.data;
        
        wx.setStorageSync('userInfo', userInfo);
        wx.setStorageSync('isLogin', true);
        wx.setStorageSync('token', loginResult.token || '');
        wx.setStorageSync('customerId', userInfo.customerId || '');

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
          title: loginResult.message || '用户名或密码错误',
          icon: 'none',
          duration: 2000
        });
      }
    } catch (error) {
      console.error('登录失败:', error);
      this.setData({ loading: false });
      
      wx.showToast({
        title: '网络错误，请重试',
        icon: 'none',
        duration: 2000
      });
    }
  },

  // API请求：登录
  async requestLogin(data) {
    return new Promise((resolve, reject) => {
      wx.request({
        url: 'http://localhost:5002/api/customer/login',
        method: 'POST',
        data: data,
        header: {
          'content-type': 'application/json'
        },
        success: (res) => {
          if (res.statusCode === 200) {
            resolve(res.data);
          } else {
            reject(new Error(`请求失败: ${res.statusCode}`));
          }
        },
        fail: (err) => {
          reject(new Error('网络连接失败'));
        }
      });
    });
  },

  // 注册功能
  handleRegister() {
    wx.navigateTo({
      url: '/pages/register/register'
    });
  },

  // 忘记密码
  handleForgotPassword() {
    wx.navigateTo({
      url: '/pages/forgot-password/forgot-password'
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
  },

  onShow() {
    // 页面显示时检查是否有注册成功传递的手机号
    const pages = getCurrentPages();
    const currentPage = pages[pages.length - 1];
    const options = currentPage.options;
    
    if (options.phone) {
      this.setData({
        username: options.phone
      });
    }
  }
});