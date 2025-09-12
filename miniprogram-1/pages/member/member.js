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
      const customerId = this.getCustomerId();
      
      const [memberInfo, stats, levels] = await Promise.all([
        API.getCustomerMemberInfo(customerId),
        API.getCustomerConsumptionStats(customerId),
        API.getMemberLevels()
      ]);

      // 处理会员信息
      const processedMemberInfo = this.formatMemberInfo(memberInfo, levels);
      
      // 计算会员时长
      const memberSince = this.calculateMemberSince(processedMemberInfo.registerTime);
      
      // 计算距离下一等级所需金额
      const remainingAmount = processedMemberInfo.nextLevelThreshold - processedMemberInfo.totalConsumption;

      this.setData({
        memberInfo: processedMemberInfo,
        stats: this.processStats(stats),
        allLevels: levels,
        memberSince: memberSince,
        remainingAmount: Math.max(0, remainingAmount.toFixed(2)),
        loading: false
      });

      // 处理特权数据
      const privileges = this.processPrivileges(processedMemberInfo.currentLevel);
      this.setData({
        'memberInfo.privileges': privileges
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
    return 1; // 默认ID用于测试
  },

  // 格式化会员信息（只负责数据格式，不做请求）
  formatMemberInfo(memberInfo, levels) {
    const currentLevelData = levels.find(level => level.levelCode === memberInfo.currentLevel);

    const processed = {
      customerId: memberInfo.CustomerId || memberInfo.customerId || 0,
      customerName: memberInfo.CustomerName || memberInfo.customerName || '',
      totalConsumption: parseFloat(memberInfo.TotalConsumption || memberInfo.totalConsumption || 0).toFixed(2),
      currentLevel: memberInfo.CurrentLevel || memberInfo.currentLevel || 'bronze',
      currentLevelName: memberInfo.CurrentLevelName || memberInfo.currentLevelName || '青铜会员',
      nextLevel: memberInfo.NextLevel || memberInfo.nextLevel || '',
      nextLevelName: memberInfo.NextLevelName || memberInfo.nextLevelName || '',
      nextLevelThreshold: parseFloat(memberInfo.NextLevelThreshold || memberInfo.nextLevelThreshold || 0).toFixed(2),
      progressToNextLevel: parseFloat(memberInfo.ProgressToNextLevel || memberInfo.progressToNextLevel || 0).toFixed(1),
      vipPoints: memberInfo.VipPoints || memberInfo.vipPoints || 0,
      registerTime: memberInfo.RegisterTime || memberInfo.registerTime || new Date(),
      privileges: [],
      currentLevelInfo: {
        color: currentLevelData?.levelColor || '#CD7F32',
        icon: currentLevelData?.levelIcon || '🥉'
      }
    };
    
    return processed;
  },

  // 处理特权数据：只展示折扣
  processPrivileges(currentLevel) {
    const discount = this.getDiscountByLevel(currentLevel);
    return [
      {
        privilegeType: 'discount',
        privilegeName: '会员折扣',
        privilegeDesc: '根据您的会员等级享受专属折扣',
        privilegeValue: discount.fold + '折',
        privilegeIcon: '💰'
      }
    ];
  },

  // 根据等级获取折扣
  getDiscountByLevel(level) {
    const map = {
      bronze: { fold: 9.9, percent: 99 },
      silver: { fold: 9.5, percent: 95 },
      gold: { fold: 9.0, percent: 90 },
      platinum: { fold: 8.5, percent: 85 },
      diamond: { fold: 8.0, percent: 80 }
    };
    const key = (level || 'bronze').toString().toLowerCase();
    return map[key] || map['bronze'];
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
