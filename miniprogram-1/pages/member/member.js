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
    memberSince: '',
    loading: false,
    allLevels: [], // 所有会员等级信息
    currentLevelIndex: 0 // 当前等级在所有等级中的索引
  },

  onLoad() {
    this.loadMemberInfo();
    this.loadConsumptionStats();
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

  // 加载会员信息
  async loadMemberInfo() {
    if (this.data.loading) return;
    
    try {
      this.setData({ loading: true });
      
      const customerId = this.getCustomerId();
      console.log('🔄 开始加载会员信息，客户ID:', customerId);
      
      // 获取会员信息
      const memberInfo = await API.getCustomerMemberInfo(customerId);
      console.log('✅ 获取到会员信息:', memberInfo);
      
      // 处理字段名映射，确保兼容PascalCase和camelCase
      const processedMemberInfo = this.processMemberInfo(memberInfo);
      
      // 计算会员时长
      const registerTime = processedMemberInfo.registerTime || processedMemberInfo.RegisterTime;
      const registerDate = new Date(registerTime);
      const now = new Date();
      const diffTime = Math.abs(now - registerDate);
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
      const memberSince = diffDays > 365 ? 
        Math.floor(diffDays / 365) + '年' + Math.floor((diffDays % 365) / 30) + '个月' : 
        Math.floor(diffDays / 30) + '个月' + (diffDays % 30) + '天';

      // 根据会员等级设置颜色和图标
      const levelInfo = this.getLevelInfo(processedMemberInfo.currentLevel);

      this.setData({
        memberInfo: {
          ...processedMemberInfo,
          currentLevelInfo: levelInfo
        },
        memberSince: memberSince,
        loading: false
      });

      // 处理特权数据
      const privileges = this.processPrivileges(
        memberInfo.Privileges || memberInfo.privileges || [], 
        processedMemberInfo.currentLevel
      );
      
      // 更新特权数据
      this.setData({
        'memberInfo.privileges': privileges
      });
      
      console.log('🎁 特权数据已更新:', privileges);

      // 加载所有会员等级信息
      await this.loadAllLevels();
      
    } catch (error) {
      console.error('❌ 加载会员信息失败:', error);
      this.setData({ loading: false });
      wx.showToast({
        title: '加载会员信息失败',
        icon: 'none'
      });
    }
  },

  // 处理会员信息字段映射
  processMemberInfo(memberInfo) {
    console.log('🔍 原始会员信息数据:', memberInfo);
    
    const processed = {
      customerId: memberInfo.CustomerId || memberInfo.customerId || 0,
      customerName: memberInfo.CustomerName || memberInfo.customerName || '',
      totalConsumption: memberInfo.TotalConsumption || memberInfo.totalConsumption || 0,
      currentLevel: memberInfo.CurrentLevel || memberInfo.currentLevel || 'bronze',
      currentLevelName: memberInfo.CurrentLevelName || memberInfo.currentLevelName || '青铜会员',
      nextLevel: memberInfo.NextLevel || memberInfo.nextLevel || '',
      nextLevelName: memberInfo.NextLevelName || memberInfo.nextLevelName || '',
      nextLevelThreshold: memberInfo.NextLevelThreshold || memberInfo.nextLevelThreshold || 0,
      progressToNextLevel: memberInfo.ProgressToNextLevel || memberInfo.progressToNextLevel || 0,
      vipPoints: memberInfo.VipPoints || memberInfo.vipPoints || 0,
      registerTime: memberInfo.RegisterTime || memberInfo.registerTime || new Date(),
      privileges: [] // 先设为空数组，稍后处理
    };
    
    console.log('✅ 处理后的会员信息数据:', processed);
    return processed;
  },

  // 处理特权数据字段映射
  processPrivileges(privileges, currentLevel) {
    if (!Array.isArray(privileges) || privileges.length === 0) {
      console.log('⚠️ 特权数据为空或不是数组，使用默认数据:', privileges);
      
      // 根据当前等级返回默认特权
      return this.getDefaultPrivileges(currentLevel);
    }
    
    return privileges.map(privilege => ({
      privilegeType: privilege.PrivilegeType || privilege.privilegeType || '',
      privilegeName: privilege.PrivilegeName || privilege.privilegeName || '',
      privilegeDesc: privilege.PrivilegeDesc || privilege.privilegeDesc || '',
      privilegeValue: privilege.PrivilegeValue || privilege.privilegeValue || '',
      privilegeIcon: privilege.PrivilegeIcon || privilege.privilegeIcon || '🎁'
    }));
  },

  // 获取默认特权数据
  getDefaultPrivileges(level) {
    const defaultPrivileges = {
      'bronze': [
        {
          privilegeType: 'discount',
          privilegeName: '新人优惠',
          privilegeDesc: '享受9.5折优惠',
          privilegeValue: '95%',
          privilegeIcon: '💰'
        },
        {
          privilegeType: 'points',
          privilegeName: '积分奖励',
          privilegeDesc: '消费1元获得1积分',
          privilegeValue: '1:1',
          privilegeIcon: '⭐'
        }
      ],
      'silver': [
        {
          privilegeType: 'discount',
          privilegeName: '银卡优惠',
          privilegeDesc: '享受9折优惠',
          privilegeValue: '90%',
          privilegeIcon: '💰'
        },
        {
          privilegeType: 'points',
          privilegeName: '积分奖励',
          privilegeDesc: '消费1元获得1.2积分',
          privilegeValue: '1:1.2',
          privilegeIcon: '⭐'
        },
        {
          privilegeType: 'service',
          privilegeName: '优先服务',
          privilegeDesc: '享受优先排队服务',
          privilegeValue: '优先',
          privilegeIcon: '🚀'
        }
      ],
      'gold': [
        {
          privilegeType: 'discount',
          privilegeName: '金卡优惠',
          privilegeDesc: '享受8.5折优惠',
          privilegeValue: '85%',
          privilegeIcon: '💰'
        },
        {
          privilegeType: 'points',
          privilegeName: '积分奖励',
          privilegeDesc: '消费1元获得1.5积分',
          privilegeValue: '1:1.5',
          privilegeIcon: '⭐'
        },
        {
          privilegeType: 'service',
          privilegeName: 'VIP服务',
          privilegeDesc: '享受专属客服服务',
          privilegeValue: '专属',
          privilegeIcon: '👑'
        }
      ]
    };
    
    return defaultPrivileges[level] || defaultPrivileges['bronze'];
  },

  // 加载所有会员等级
  async loadAllLevels() {
    try {
      const levels = await API.getMemberLevels();
      console.log('✅ 获取到会员等级列表:', levels);
      
      // 处理会员等级字段映射
      const processedLevels = levels.map(level => this.processLevel(level));
      
      // 找到当前等级在所有等级中的索引
      const currentLevelIndex = processedLevels.findIndex(level => 
        level.levelCode === this.data.memberInfo.currentLevel
      );

      this.setData({
        allLevels: processedLevels,
        currentLevelIndex: currentLevelIndex
      });
    } catch (error) {
      console.error('❌ 加载会员等级失败:', error);
    }
  },

  // 处理会员等级字段映射
  processLevel(level) {
    return {
      levelCode: level.LevelCode || level.levelCode || '',
      levelName: level.LevelName || level.levelName || '',
      minConsumption: level.MinConsumption || level.minConsumption || 0,
      maxConsumption: level.MaxConsumption || level.maxConsumption || null,
      levelColor: level.LevelColor || level.levelColor || '#CD7F32',
      levelIcon: level.LevelIcon || level.levelIcon || '🥉',
      privileges: level.Privileges || level.privileges || []
    };
  },

  // 加载消费统计
  async loadConsumptionStats() {
    try {
      const customerId = this.getCustomerId();
      console.log('🔄 开始加载消费统计，客户ID:', customerId);
      
      const stats = await API.getCustomerConsumptionStats(customerId);
      console.log('✅ 获取到消费统计:', stats);
      
      // 处理消费统计字段映射
      const processedStats = this.processStats(stats);
      
      this.setData({
        stats: processedStats
      });
    } catch (error) {
      console.error('❌ 加载消费统计失败:', error);
      // 使用默认值，不影响页面显示
    }
  },

  // 处理消费统计字段映射
  processStats(stats) {
    return {
      totalOrders: stats.TotalOrders || stats.totalOrders || 0,
      averageOrderAmount: stats.AverageOrderAmount || stats.averageOrderAmount || 0,
      monthlyOrders: stats.MonthlyOrders || stats.monthlyOrders || 0,
      monthlyConsumption: stats.MonthlyConsumption || stats.monthlyConsumption || 0,
      favoriteStore: stats.FavoriteStore || stats.favoriteStore || ''
    };
  },

  // 根据等级代码获取等级信息
  getLevelInfo(levelCode) {
    const levelInfoMap = {
      'bronze': {
        color: '#CD7F32',
        icon: '🥉'
      },
      'silver': {
        color: '#C0C0C0',
        icon: '🥈'
      },
      'gold': {
        color: '#FFD700',
        icon: '🥇'
      },
      'platinum': {
        color: '#E5E4E2',
        icon: '💎'
      },
      'diamond': {
        color: '#B9F2FF',
        icon: '💍'
      }
    };

    return levelInfoMap[levelCode] || levelInfoMap['bronze'];
  },

  // 下拉刷新
  onPullDownRefresh() {
    this.loadMemberInfo();
    this.loadConsumptionStats();
    wx.stopPullDownRefresh();
  },

  // 页面显示时
  onShow() {
    // 每次显示页面时刷新数据
    this.loadMemberInfo();
    this.loadConsumptionStats();
  },

  // 查看积分记录
  goToPoints() {
    wx.navigateTo({
      url: '/pages/points/points'
    });
  },

  // 升级会员等级
  async upgradeMemberLevel() {
    try {
      const customerId = this.getCustomerId();
      
      wx.showLoading({
        title: '更新中...'
      });

      await API.updateCustomerMemberLevel(customerId);
      
      wx.hideLoading();
      wx.showToast({
        title: '等级更新成功',
        icon: 'success'
      });

      // 重新加载会员信息
      setTimeout(() => {
        this.loadMemberInfo();
      }, 1500);

    } catch (error) {
      wx.hideLoading();
      console.error('升级会员等级失败:', error);
      wx.showToast({
        title: '升级失败',
        icon: 'none'
      });
    }
  }
});