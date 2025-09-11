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

        // 登录成功后跳转
        setTimeout(() => {
          // 优先跳回登录前保存的页面（含扫码参数）
          try {
            const pending = wx.getStorageSync('pendingRedirect');
            if (pending && pending.page) {
              const page = pending.page;
              const options = pending.options || {};

              // 仅支持回到 tabBar 页用 switchTab，其余用 reLaunch
              const tabBarPages = [
                '/pages/index/index',
                '/pages/goods/list',
                '/pages/reservation/reservation',
                '/pages/account/account'
              ];

              // 组装带参数的 url（用于非 tabBar 页面）
              const query = Object.keys(options).map(k => `${k}=${encodeURIComponent(options[k])}`).join('&');
              const urlWithQuery = query ? `${page}?${query}` : page;

              if (tabBarPages.includes(page)) {
                // 不清理 pendingRedirect，留给落地页读取并清理
                wx.switchTab({ url: page });
              } else {
                wx.reLaunch({ url: urlWithQuery });
                try { wx.removeStorageSync('pendingRedirect'); } catch (e2) {}
              }
              return;
            }
          } catch (e) {}

          // 默认回到首页
          wx.switchTab({ url: '/pages/index/index' });
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

  onLoad(options) {
    // 始终停留在登录页；如果是通过二维码进入，则缓存参数供登录后回跳使用
    try {
      let qrOptions = options || {};

      // 兼容从应用入口和页面入口获取参数
      if ((!qrOptions || Object.keys(qrOptions).length === 0) && wx.getEnterOptionsSync) {
        const enter = wx.getEnterOptionsSync();
        if (enter) {
          qrOptions = { ...(enter.query || {}), scene: enter.scene };
        }
      }
      if ((!qrOptions || Object.keys(qrOptions).length === 0) && wx.getLaunchOptionsSync) {
        const launch = wx.getLaunchOptionsSync();
        if (launch) {
          qrOptions = { ...(launch.query || {}), scene: launch.scene };
        }
      }

      const hasQrParams = qrOptions && (qrOptions.tableNumber || qrOptions.storeId || qrOptions.scene || qrOptions.q);
      if (hasQrParams) {
        wx.setStorageSync('pendingRedirect', {
          page: '/pages/index/index',
          options: qrOptions
        });
      }
    } catch (e) {}
  }
}); 