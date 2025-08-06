// member.js - 会员特权页面
import API from '../../utils/api.js';

Page({
  data: {
    memberInfo: {
      customerId: 0,
      customerName: '',
      totalConsumption: 0,
      currentLevel: 'bronze',
      currentLevelName: '青铜会员',
      nextLevel: '',
      nextLevelName: '',
      nextLevelThreshold: 0,
      progressToNextLevel: 0,
      vipPoints: 0,
      registerTime: '',
      privileges: [],
      currentLevelInfo: {
        color: '#CD7F32',
        icon: '🥉'
      }
    },
    stats: {
      totalOrders: 0,
      averageOrderAmount: 0,
      monthlyOrders: 0,
      monthlyConsumption: 0,
      favoriteStore: ''
    },
    allLevels: [],
    loading: true,
    userInfo: null,
    memberSince: '',
    remainingAmount: 0
  },

  onLoad() {
    // 页面加载时获取用户信息并加载会员数据
    this.loadUserInfo();
    this.loadMemberData();
  },

  // 获取用户信息
  loadUserInfo() {
    const userInfo = wx.getStorageSync('userInfo');
    const isLogin = wx.getStorageSync('isLogin');
    
    if (!isLogin || !userInfo) {
      wx.showToast({
        title: '请先登录',
        icon: 'none'
      });
      setTimeout(() => {
        wx.navigateBack();
      }, 1500);
      return;
    }
    
    this.setData({
      userInfo: userInfo
    });
  },

  // 加载会员数据
  async loadMemberData() {
    this.setData({ loading: true });

    try {
      // 并行获取会员信息、消费统计和等级规则
      const customerId = this.getCustomerId();
      
      const [memberInfo, stats, levels] = await Promise.all([
        API.getCustomerMemberInfo(customerId),
        API.getCustomerConsumptionStats(customerId),
        API.getMemberLevels()
      ]);

      // 处理会员信息
      const processedMemberInfo = this.processMemberInfo(memberInfo, levels);
      
      // 计算会员时长
      const memberSince = this.calculateMemberSince(memberInfo.registerTime);
      
      // 计算距离下一等级所需金额
      const remainingAmount = memberInfo.nextLevelThreshold - memberInfo.totalConsumption;

      this.setData({
        memberInfo: processedMemberInfo,
        stats: this.processStats(stats),
        allLevels: levels,
        memberSince: memberSince,
        remainingAmount: Math.max(0, remainingAmount.toFixed(2)),
        loading: false
      });

    } catch (error) {
      console.error('加载会员数据失败:', error);
      wx.showToast({
        title: '加载失败，请重试',
        icon: 'none'
      });
      this.setData({ loading: false });
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

  // 处理会员信息
  processMemberInfo(memberInfo, levels) {
    // 找到当前等级的颜色和图标
    const currentLevelData = levels.find(level => level.levelCode === memberInfo.currentLevel);
    
    return {
      ...memberInfo,
      currentLevelInfo: {
        color: currentLevelData?.levelColor || '#CD7F32',
        icon: currentLevelData?.levelIcon || '🥉'
      },
      totalConsumption: parseFloat(memberInfo.totalConsumption).toFixed(2),
      nextLevelThreshold: parseFloat(memberInfo.nextLevelThreshold).toFixed(2),
      progressToNextLevel: parseFloat(memberInfo.progressToNextLevel).toFixed(1)
    };
  },

  // 处理统计数据
  processStats(stats) {
    return {
      ...stats,
      averageOrderAmount: parseFloat(stats.averageOrderAmount || 0).toFixed(2),
      monthlyConsumption: parseFloat(stats.monthlyConsumption || 0).toFixed(2),
      totalConsumption: parseFloat(stats.totalConsumption || 0).toFixed(2)
    };
  },

  // 计算会员时长
  calculateMemberSince(registerTime) {
    try {
      const register = new Date(registerTime);
      const now = new Date();
      const diffTime = Math.abs(now - register);
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
      
      if (diffDays < 30) {
        return `${diffDays}天`;
      } else if (diffDays < 365) {
        const months = Math.floor(diffDays / 30);
        return `${months}个月`;
      } else {
        const years = Math.floor(diffDays / 365);
        const months = Math.floor((diffDays % 365) / 30);
        return months > 0 ? `${years}年${months}个月` : `${years}年`;
      }
    } catch (error) {
      return '未知';
    }
  },

  // 显示等级详情
  showLevelDetail(e) {
    const level = e.currentTarget.dataset.level;
    
    let privilegesList = level.privileges.map(p => `• ${p.privilegeName}: ${p.privilegeDesc}`).join('\n');
    
    wx.showModal({
      title: level.levelName,
      content: `消费门槛: ¥${level.minConsumption}${level.maxConsumption ? ` - ¥${level.maxConsumption}` : '以上'}\n\n会员特权:\n${privilegesList}`,
      showCancel: false,
      confirmText: '我知道了'
    });
  },

  // 下拉刷新
  onPullDownRefresh() {
    this.loadMemberData().then(() => {
      wx.stopPullDownRefresh();
    });
  },

  // 页面显示时刷新数据
  onShow() {
    if (this.data.userInfo) {
      this.loadMemberData();
    }
  },

  // 返回个人中心
  goBack() {
    wx.navigateBack();
  }
});