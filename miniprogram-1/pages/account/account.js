Page({
  data: {
    userInfo: {
      nickname: '未登录',
      avatar: '/images/default-avatar.png',
      phone: '',
      memberLevel: '普通会员',
      points: 0
    },
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
    } else {
      // 未登录，跳转到登录页面
      wx.redirectTo({
        url: '/pages/login/login'
      });
    }
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