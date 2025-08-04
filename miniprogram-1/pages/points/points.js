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
          pointsBalance: balance.pointsBalance || 0
        });
        // 更新本地存储的用户信息
        this.updateUserInfoPoints(balance.pointsBalance || 0);
        return;
      }

      const balance = await API.getCustomerPointsBalance(userInfo.customerId);
      this.setData({
        pointsBalance: balance.pointsBalance || 0
      });
      // 更新本地存储的用户信息
      this.updateUserInfoPoints(balance.pointsBalance || 0);
    } catch (error) {
      console.error('加载积分余额失败:', error);
      wx.showToast({
        title: '获取积分余额失败',
        icon: 'none'
      });
    }
  },

  // 更新本地存储的用户积分信息
  updateUserInfoPoints(points) {
    try {
      const userInfo = wx.getStorageSync('userInfo');
      if (userInfo) {
        const updatedUserInfo = {
          ...userInfo,
          points: points
        };
        wx.setStorageSync('userInfo', updatedUserInfo);
      }
    } catch (error) {
      console.error('更新用户积分信息失败:', error);
    }
  },

  // 加载积分记录数据
  async loadPointsRecords(refresh = false) {
    if (this.data.loading) return;
    
    if (refresh) {
      this.setData({
        page: 1,
        pointsRecords: [],
        hasMore: true
      });
    }

    this.setData({ loading: true });

    try {
      // 调用后端API获取积分记录数据
      const records = await this.fetchPointsRecordsFromAPI();
      
      if (refresh) {
        this.setData({
          pointsRecords: records,
          page: 2
        });
      } else {
        this.setData({
          pointsRecords: [...this.data.pointsRecords, ...records],
          page: this.data.page + 1
        });
      }

      // 判断是否还有更多数据
      if (records.length < this.data.pageSize) {
        this.setData({ hasMore: false });
      }

    } catch (error) {
      console.error('加载积分记录失败:', error);
      wx.showToast({
        title: '加载失败，请重试',
        icon: 'none'
      });
    } finally {
      this.setData({ loading: false });
    }
  },

  // 调用后端API获取积分记录数据
  async fetchPointsRecordsFromAPI() {
    try {
      // 获取当前用户ID（假设存储在userInfo中）
      const userInfo = this.data.userInfo;
      if (!userInfo || !userInfo.customerId) {
        // 如果没有客户ID，使用默认客户ID=1进行测试
        const customerId = 1;
        const records = await API.getCustomerPointsRecords(customerId, this.data.page, this.data.pageSize);
        
        return this.formatPointsRecordsData(records);
      }

      const records = await API.getCustomerPointsRecords(userInfo.customerId, this.data.page, this.data.pageSize);
      return this.formatPointsRecordsData(records);
    } catch (error) {
      console.error('获取积分记录数据失败:', error);
      wx.showToast({
        title: '获取积分记录失败',
        icon: 'none'
      });
      throw error;
    }
  },

  // 格式化积分记录数据
  formatPointsRecordsData(records) {
    return records.map((record, index) => {
      try {
        const formattedRecord = {
          ...record,
          // 确保字段存在且有默认值
          recordId: record.recordId || record.RecordID || index + 1,
          customerId: record.customerId || record.CustomerID || 0,
          orderId: record.orderId || record.OrderID || null,
          pointsChange: record.pointsChange || record.PointsChange || 0,
          recordType: record.recordType || record.RecordType || '未知类型',
          recordTime: record.recordTime || record.RecordTime || '未知时间',
          description: record.description || record.Description || '',
          customerName: record.customerName || record.CustomerName || '未知客户',
          orderAmount: record.orderAmount || record.OrderAmount || 0,
          orderTime: record.orderTime || record.OrderTime || '',
          storeName: record.storeName || record.StoreName || '',
          formattedTime: API.formatTime(record.recordTime || record.RecordTime),
          typeInfo: this.formatRecordType(record.recordType || record.RecordType)
        };
        
        return formattedRecord;
      } catch (error) {
        // 返回安全的默认记录对象
        return {
          recordId: record.recordId || record.RecordID || index + 1,
          customerId: record.customerId || record.CustomerID || 0,
          orderId: record.orderId || record.OrderID || null,
          pointsChange: record.pointsChange || record.PointsChange || 0,
          recordType: record.recordType || record.RecordType || '未知类型',
          recordTime: '时间未知',
          description: record.description || record.Description || '',
          customerName: record.customerName || record.CustomerName || '未知客户',
          orderAmount: record.orderAmount || record.OrderAmount || 0,
          orderTime: record.orderTime || record.OrderTime || '',
          storeName: record.storeName || record.StoreName || '',
          formattedTime: '时间未知',
          typeInfo: { text: '未知类型', class: 'default' }
        };
      }
    });
  },

  // 格式化记录类型
  formatRecordType(type) {
    const typeMap = {
      '消费获得': { text: '消费获得', class: 'earned', icon: '➕' },
      '兑换消费': { text: '积分兑换', class: 'spent', icon: '➖' },
      '过期扣除': { text: '积分过期', class: 'expired', icon: '⚠️' }
    };
    return typeMap[type] || { text: type, class: 'default', icon: '❓' };
  },

  // 下拉刷新
  onPullDownRefresh() {
    this.loadPointsBalance();
    this.loadPointsRecords(true);
    wx.stopPullDownRefresh();
  },

  // 上拉加载更多
  onReachBottom() {
    if (this.data.hasMore && !this.data.loading) {
      this.loadPointsRecords();
    }
  },

  // 跳转到订单详情
  goToOrderDetail(e) {
    const orderId = e.currentTarget.dataset.orderid;
    if (orderId) {
      wx.navigateTo({
        url: `/pages/orders/orders?orderId=${orderId}`
      });
    }
  },

  // 返回个人中心
  goBack() {
    wx.navigateBack();
  },

  // 页面显示时刷新积分余额
  onShow() {
    this.loadPointsBalance();
  },


}); 