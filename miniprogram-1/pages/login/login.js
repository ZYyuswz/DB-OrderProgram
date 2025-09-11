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

  goToRegister() {
    wx.navigateTo({
      url: '/pages/register/register'
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