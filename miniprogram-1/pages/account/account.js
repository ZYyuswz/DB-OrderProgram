import API from '../../utils/api.js';

Page({
  data: {
    userInfo: {
      nickname: '未登录',
      avatar: '/images/default-avatar.png',
      phone: '',
      memberLevel: '普通会员',
      memberLevelName: '普通会员',
      points: 0,
      totalConsumption: 0
    },
    loadingMember: false,
    menuItems: [
      {
        icon: '👤',
        title: '个人信息',
        desc: '编辑个人资料和设置',
        url: '/pages/profile/profile'
      },
      {
        icon: '⭐',
        title: '我的积分',
        desc: '积分余额和兑换记录',
        url: '/pages/points/points'
      },
      {
        icon: '📋',
        title: '订单记录',
        desc: '查看我的点餐历史',
        url: '/pages/orders/orders'
      },
      {
        icon: '👑',
        title: '会员特权',
        desc: '享受专属优惠和服务',
        url: '/pages/member/member'
      },
      {
        icon: '💬',
        title: '我的评价',
        desc: '对美食的评价和分享',
        url: '/pages/reviews/reviews'
      }
    ]
  },

  onLoad() {
    // 页面加载时获取用户信息
    this.loadUserInfo();
  },

  // 加载用户信息
  loadUserInfo() {
    const userInfo = wx.getStorageSync('userInfo');
    const isLogin = wx.getStorageSync('isLogin');
    
    if (isLogin && userInfo) {
      this.setData({
        userInfo: userInfo
      });
      // 加载最新的会员信息
      this.loadMemberInfo();
    } else {
      // 未登录，跳转到登录页面
      wx.redirectTo({
        url: '/pages/login/login'
      });
    }
  },

  // 加载会员信息
  async loadMemberInfo() {
    if (this.data.loadingMember) return; // 防止重复请求
    
    try {
      this.setData({ loadingMember: true });
      
      const customerId = this.getCustomerId();
      
      // 获取最新的会员信息
      const memberInfo = await API.getCustomerMemberInfo(customerId);
      console.log('🔍 个人中心获取到的会员信息:', memberInfo);
      
      // 处理字段映射，兼容PascalCase和camelCase
      const currentLevelName = memberInfo.CurrentLevelName || memberInfo.currentLevelName || '普通会员';
      const vipPoints = memberInfo.VipPoints || memberInfo.vipPoints || 0;
      const totalConsumption = memberInfo.TotalConsumption || memberInfo.totalConsumption || 0;
      
      console.log('✅ 处理后的字段值:', { currentLevelName, vipPoints, totalConsumption });
      
      // 更新用户信息，包含最新的会员等级
      const updatedUserInfo = {
        ...this.data.userInfo,
        memberLevel: currentLevelName,
        memberLevelName: currentLevelName,
        points: vipPoints,
        totalConsumption: parseFloat(totalConsumption).toFixed(2)
      };
      
      this.setData({
        userInfo: updatedUserInfo,
        loadingMember: false
      });
      
      // 更新本地存储
      wx.setStorageSync('userInfo', updatedUserInfo);
      
    } catch (error) {
      console.error('获取会员信息失败:', error);
      this.setData({ loadingMember: false });
      
      // 显示错误提示（可选）
      if (error.message && !error.message.includes('网络')) {
        wx.showToast({
          title: '获取会员信息失败',
          icon: 'none',
          duration: 2000
        });
      }
    }
  },

  // 获取客户ID
  getCustomerId() {
    const userInfo = this.data.userInfo;
    if (userInfo && userInfo.customerId) {
      return userInfo.customerId;
    }
    // 如果没有客户ID，使用默认客户ID=1进行测试
    return 1;
  },

  // 导航到具体页面
  navigateTo(e) {
    const url = e.currentTarget.dataset.url;
    wx.navigateTo({
      url: url
    });
  },

  // 退出登录
  logout() {
    wx.showModal({
      title: '确认退出',
      content: '确定要退出登录吗？',
      success: (res) => {
        if (res.confirm) {
          // 清除用户信息和登录状态
          wx.removeStorageSync('userInfo');
          wx.removeStorageSync('isLogin');
          wx.removeStorageSync('cart'); // 清除购物车
          
          wx.showToast({
            title: '已退出登录',
            icon: 'success',
            duration: 1500
          });
          
          // 跳转到登录页面
          setTimeout(() => {
            wx.redirectTo({ 
              url: '/pages/login/login' 
            });
          }, 1500);
        }
      }
    });
  },

  onShow() {
    // 页面显示时重新加载用户信息
    this.loadUserInfo();
  },


}); 