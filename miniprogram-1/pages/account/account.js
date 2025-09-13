import API from '../../utils/api.js';

Page({
  data: {
    userInfo: {
      nickname: '未登录',
      avatar: '/images/default-avatar.png',
      phone: '',
      points: 0,
      totalConsumption: 0
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

  // 获取用户ID
  getCustomerId() {
    // 增加详细的调试信息
    console.group('🔍 getCustomerId() 调试信息');
    
    // 1. 首先获取缓存
    const userInfo = wx.getStorageSync('userInfo');
    console.log('📦 从缓存获取的 userInfo:', userInfo);
    
    // 2. 检查 userInfo 是否存在
    if (userInfo) {
      console.log('✅ userInfo 存在');
      console.log('🔍 userInfo.customerId 的值:', userInfo.customerId);
      console.log('🔍 userInfo.customerId 的类型:', typeof userInfo.customerId);
      
      // 3. 检查 customerId 是否存在
      if (userInfo.customerId) {
        console.log('✅ userInfo.customerId 存在，值为:', userInfo.customerId);
        console.groupEnd();
        return userInfo.customerId;
      } else {
        console.log('❌ userInfo.customerId 不存在或为假值');
      }
    } else {
      console.log('❌ userInfo 不存在或为空');
    }
    
    // 4. 降级处理
    console.log('⚠️ 没有获取到有效的 customerId，使用默认值 1');
    console.groupEnd();
    
    return 1;
  },

    // 加载用户信息
  async loadUserInfo() {
    if (this.data.loading) return;
    

    try {
      this.setData({ loading: true });
      
      const customerId = this.getCustomerId();
      console.log('🔄 个人中心开始加载用户信息，客户ID:', customerId);
      
      // 从数据库获取基本用户信息
      const customerProfile = await API.getCustomerProfile(customerId);
      console.log('✅ 获取到用户基本信息:', customerProfile);
      
      if (customerProfile) {
        // 处理字段映射，兼容PascalCase和camelCase
        const processedUserInfo = {
          customerId: customerProfile.CustomerId || customerProfile.customerId || customerId,
          customerName: customerProfile.CustomerName || customerProfile.customerName || '未设置',
          phone: customerProfile.Phone || customerProfile.phone || '',
          vipLevelName: customerProfile.VipLevelName || customerProfile.vipLevelName || '普通会员',
          points: 0, // 先设为0，由loadMemberInfo更新
          totalConsumption: 0 // 先设为0，由loadMemberInfo更新
        };

        this.setData({
          userInfo: processedUserInfo,
          loading: false
        });

        // 更新本地存储
        const storageUserInfo = wx.getStorageSync('userInfo') || {};
        const updatedStorageInfo = {
          ...storageUserInfo,
          customerId: processedUserInfo.customerId,
          customerName: processedUserInfo.customerName,
          phone: processedUserInfo.phone,
          vipLevelName: processedUserInfo.vipLevelName
        };
        wx.setStorageSync('userInfo', updatedStorageInfo);

        // 加载会员统计信息（积分、消费等）
        this.loadMemberInfo();
      } else {
        // 如果没有找到用户信息，使用默认信息
        this.setData({
          loading: false,
          userInfo: {
            customerId: customerId,
            customerName: '用户未找到',
            phone: '',
            vipLevelName: '普通会员',
            points: 0,
            totalConsumption: 0
          }
        });
      }
      
    } catch (error) {
      console.error('❌ 加载用户信息失败:', error);
      this.setData({ loading: false });
      
      // 显示错误时使用默认信息

      this.setData({
        userInfo: userInfo
      });
      // 加载最新的会员信息
      this.loadMemberInfo();
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
      
      // 更新用户信息，包含最新的会员等级
      const updatedUserInfo = {
        ...this.data.userInfo,
        memberLevel: memberInfo.currentLevelName,
        memberLevelName: memberInfo.currentLevelName,
        points: memberInfo.vipPoints,
        totalConsumption: parseFloat(memberInfo.totalConsumption).toFixed(2)
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