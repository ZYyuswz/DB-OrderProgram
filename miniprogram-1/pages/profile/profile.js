import API from '../../utils/api.js';

Page({
  data: {
    userInfo: {
      customerId: 0,
      customerName: '',
      phone: '',
      email: '',
      vipLevelName: '普通会员'
    },
    originalUserInfo: {},
    isFormChanged: false,
    loading: false
  },

  onLoad() {
    this.loadUserInfo();
  },

  // 获取用户ID
  getCustomerId() {
    const userInfo = wx.getStorageSync('userInfo');
    if (userInfo && userInfo.customerId) {
      return userInfo.customerId;
    }
    // 如果没有客户ID，使用默认客户ID=1进行测试
    return 1;
  },

  // 加载用户信息
  async loadUserInfo() {
    if (this.data.loading) return;
    
    try {
      this.setData({ loading: true });
      
      const customerId = this.getCustomerId();
      console.log('🔄 开始加载用户信息，客户ID:', customerId);
      
      // 从数据库获取用户信息
      const customerInfo = await API.getCustomerProfile(customerId);
      console.log('✅ 获取到用户信息:', customerInfo);
      
      if (customerInfo) {
        const processedInfo = {
          customerId: customerInfo.CustomerId || customerInfo.customerId || customerId,
          customerName: customerInfo.CustomerName || customerInfo.customerName || '',
          phone: customerInfo.Phone || customerInfo.phone || '',
          email: customerInfo.Email || customerInfo.email || '',
          vipLevelName: customerInfo.VipLevelName || customerInfo.vipLevelName || '普通会员'
        };
        
        this.setData({
          userInfo: processedInfo,
          originalUserInfo: { ...processedInfo },
          loading: false
        });
      } else {
        // 如果没有找到用户信息，使用默认信息
        const defaultInfo = {
          customerId: customerId,
          customerName: '未设置',
          phone: '',
          email: '',
          vipLevelName: '普通会员'
        };
        
        this.setData({
          userInfo: defaultInfo,
          originalUserInfo: { ...defaultInfo },
          loading: false
        });
      }
      
    } catch (error) {
      console.error('❌ 加载用户信息失败:', error);
      this.setData({ loading: false });
      wx.showToast({
        title: '加载用户信息失败',
        icon: 'none'
      });
    }
  },

  // 输入处理
  onCustomerNameInput(e) {
    this.setData({
      'userInfo.customerName': e.detail.value,
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

  // 保存个人信息
  async saveProfile() {
    if (!this.data.isFormChanged) return;

    const { userInfo } = this.data;
    
    // 简单验证
    if (!userInfo.customerName.trim()) {
      wx.showToast({
        title: '请输入昵称',
        icon: 'none'
      });
      return;
    }

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

    try {
      wx.showLoading({
        title: '保存中...'
      });

      // 准备更新数据 - 确保字段名与后端一致
      const updateInfo = {
        CustomerName: userInfo.customerName,
        Phone: userInfo.phone || null,
        Email: userInfo.email || null
      };

      console.log('准备保存的用户信息:', userInfo);
      console.log('发送到后端的更新数据:', updateInfo);

      // 调用API更新用户信息
      const response = await API.updateCustomerProfile(userInfo.customerId, updateInfo);

      if (response && response.success) {
        // 更新本地存储
        const storageUserInfo = wx.getStorageSync('userInfo') || {};
        const updatedStorageInfo = {
          ...storageUserInfo,
          customerName: userInfo.customerName,
          phone: userInfo.phone,
          email: userInfo.email
        };
        wx.setStorageSync('userInfo', updatedStorageInfo);

        wx.hideLoading();
        wx.showToast({
          title: '保存成功',
          icon: 'success',
          duration: 1500
        });

        this.setData({
          isFormChanged: false,
          originalUserInfo: { ...userInfo }
        });

        // 更新全局数据
        const app = getApp();
        if (app.globalData) {
          app.globalData.userInfo = updatedStorageInfo;
        }
      } else {
        throw new Error(response?.message || '保存失败');
      }

    } catch (error) {
      wx.hideLoading();
      console.error('保存用户信息失败:', error);
      wx.showToast({
        title: error.message || '保存失败，请重试',
        icon: 'none',
        duration: 2000
      });
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