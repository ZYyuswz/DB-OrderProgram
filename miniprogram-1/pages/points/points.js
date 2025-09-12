// points.js - 积分记录页面
import API from '../../utils/api.js';

Page({
  data: {
    pointsRecords: [], // 积分记录列表
    loading: false, // 加载状态
    hasMore: true, // 是否还有更多数据
    page: 1, // 当前页码
    pageSize: 10, // 每页条数
    userInfo: null, // 用户信息
    pointsBalance: 0 // 积分余额
  },

  onLoad() {
    // 页面加载时获取用户信息并加载积分记录
    this.loadUserInfo();
    this.loadPointsBalance();
    this.loadPointsRecords();
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

  // 加载积分余额
  async loadPointsBalance() {
    try {
      // 获取当前用户ID（假设存储在userInfo中）
      const userInfo = this.data.userInfo;
      if (!userInfo || !userInfo.customerId) {
        // 如果没有客户ID，使用默认客户ID=1进行测试
        const customerId = 1;
        const balance = await API.getCustomerPointsBalance(customerId);
        this.setData({
          pointsBalance: balance.pointsBalance || balance.PointsBalance || balance.currentPoints || balance.CurrentPoints || 0
        });
      } else {
        const balance = await API.getCustomerPointsBalance(userInfo.customerId);
        this.setData({
          pointsBalance: balance.pointsBalance || balance.PointsBalance || balance.currentPoints || balance.CurrentPoints || 0
        });
      }
    } catch (error) {
      console.error('获取积分余额失败:', error);
      // 使用默认值
      this.setData({
        pointsBalance: 0
      });
    }
  },

  // 加载积分记录
  async loadPointsRecords(refresh = false) {
    if (this.data.loading) return;
    
    try {
      this.setData({ loading: true });
      
      const page = refresh ? 1 : this.data.page;
      
      // 获取当前用户ID
      const userInfo = this.data.userInfo;
      const customerId = userInfo?.customerId || 1; // 如果没有客户ID，使用默认客户ID=1进行测试
      
      console.log('🔄 获取积分记录，客户ID:', customerId, '页码:', page);
      
      const response = await API.getCustomerPointsRecords(customerId, page, this.data.pageSize);
      console.log('✅ 积分记录响应:', response);
      
      // 处理响应数据
      const records = response.records || response || [];
      const processedRecords = records.map(record => this.processRecord(record));
      
      if (refresh) {
        // 刷新时替换所有数据
        this.setData({
          pointsRecords: processedRecords,
          page: 1,
          hasMore: records.length >= this.data.pageSize,
          loading: false
        });
      } else {
        // 加载更多时追加数据
        this.setData({
          pointsRecords: [...this.data.pointsRecords, ...processedRecords],
          page: page + 1,
          hasMore: records.length >= this.data.pageSize,
          loading: false
        });
      }
      
    } catch (error) {
      console.error('❌ 获取积分记录失败:', error);
      this.setData({ loading: false });
      
      if (error.message && !error.message.includes('网络')) {
        wx.showToast({
          title: '获取积分记录失败',
          icon: 'none'
        });
      }
    }
  },

  // 处理单条积分记录
  processRecord(record) {
    return {
      ...record,
      // 格式化时间
      formattedTime: API.formatTime(record.changeTime || record.ChangeTime || record.recordTime || record.RecordTime || ''),
      // 格式化积分变化
      formattedPoints: (record.pointsChange || record.PointsChange || 0) > 0 
        ? `+${record.pointsChange || record.PointsChange}` 
        : `${record.pointsChange || record.PointsChange}`,
      // 确定积分变化类型
      changeType: (record.pointsChange || record.PointsChange || 0) > 0 ? 'earn' : 'spend',
      // 处理描述信息
      description: record.description || record.Description || '积分变动',
      // 处理原因
      reason: record.reason || record.Reason || record.changeReason || record.ChangeReason || '系统操作'
    };
  },

  // 下拉刷新
  onPullDownRefresh() {
    this.loadPointsBalance();
    this.loadPointsRecords(true).then(() => {
      wx.stopPullDownRefresh();
    });
  },

  // 上拉加载更多
  onReachBottom() {
    if (this.data.hasMore && !this.data.loading) {
      this.loadPointsRecords();
    }
  },

  // 页面显示时刷新数据
  onShow() {
    if (this.data.userInfo) {
      this.loadPointsBalance();
      this.loadPointsRecords(true);
    }
  },

  // 返回上一页
  goBack() {
    wx.navigateBack();
  }
});