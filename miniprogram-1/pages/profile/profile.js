Page({
  data: {
    userInfo: {
      nickname: '',
      phone: '',
      email: '',
      avatar: '',
      memberLevelName: '',
      points: 0
    },
    originalUserInfo: {},
    isFormChanged: false
  },

  onLoad() {
    this.loadUserInfo();
  },

  // 加载用户信息
  loadUserInfo() {
    const userInfo = wx.getStorageSync('userInfo') || {};
    const defaultInfo = {
      nickname: '用户' + Math.floor(Math.random() * 10000),
      phone: '',
      email: '',
      avatar: '/images/default-avatar.png',
      memberLevelName: '普通会员',
      points: 0
    };

    const mergedInfo = { ...defaultInfo, ...userInfo };
    
    this.setData({
      userInfo: mergedInfo,
      originalUserInfo: { ...mergedInfo }
    });
  },

  // 输入处理
  onNicknameInput(e) {
    this.setData({
      'userInfo.nickname': e.detail.value,
      isFormChanged: true
    });
  },

  onPhoneInput(e) {
    this.setData({
      'userInfo.phone': e.detail.value,
      isFormChanged: true
    });
  },

  onEmailInput(e) {
    this.setData({
      'userInfo.email': e.detail.value,
      isFormChanged: true
    });
  },

  // 更换头像
  changeAvatar() {
    wx.chooseImage({
      count: 1,
      sizeType: ['compressed'],
      sourceType: ['album', 'camera'],
      success: (res) => {
        const tempFilePath = res.tempFilePaths[0];
        this.setData({
          'userInfo.avatar': tempFilePath,
          isFormChanged: true
        });
        
        // 这里可以上传图片到服务器
        wx.showToast({
          title: '头像已更新',
          icon: 'success'
        });
      }
    });
  },

  // 保存个人信息
  saveProfile() {
    if (!this.data.isFormChanged) return;

    const { userInfo } = this.data;
    
    // 简单验证
    if (userInfo.phone && !/^1[3-9]\d{9}$/.test(userInfo.phone)) {
      wx.showToast({
        title: '请输入正确的手机号',
        icon: 'none'
      });
      return;
    }

    if (userInfo.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(userInfo.email)) {
      wx.showToast({
        title: '请输入正确的邮箱',
        icon: 'none'
      });
      return;
    }

    // 保存到本地存储
    wx.setStorageSync('userInfo', userInfo);
    
    wx.showToast({
      title: '保存成功',
      icon: 'success'
    });

    this.setData({
      isFormChanged: false,
      originalUserInfo: { ...userInfo }
    });

    // 更新全局数据
    const app = getApp();
    if (app.globalData) {
      app.globalData.userInfo = userInfo;
    }
  },

  // 退出登录
  logout() {
    wx.showModal({
      title: '确认退出',
      content: '确定要退出登录吗？',
      success: (res) => {
        if (res.confirm) {
          wx.removeStorageSync('userInfo');
          wx.removeStorageSync('isLogin');
          
          wx.showToast({
            title: '已退出登录',
            icon: 'success'
          });

          setTimeout(() => {
            wx.reLaunch({
              url: '/pages/login/login'
            });
          }, 1500);
        }
      }
    });
  },

  onShow() {
    this.loadUserInfo();
  }
});