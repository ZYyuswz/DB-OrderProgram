// orders.js - 订单记录页面
import API from '../../utils/api.js';

Page({
  data: {
    orders: [], // 订单列表
    loading: false, // 加载状态
    hasMore: true, // 是否还有更多数据
    page: 1, // 当前页码
    pageSize: 10, // 每页条数
    userInfo: null // 用户信息
  },

  onLoad() {
    // 页面加载时获取用户信息并加载订单
    this.loadUserInfo();
    this.loadOrders();
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

  // 加载订单数据
  async loadOrders(refresh = false) {
    if (this.data.loading) return;
    
    if (refresh) {
      this.setData({
        page: 1,
        orders: [],
        hasMore: true
      });
    }

    this.setData({ loading: true });

    try {
      // 调用后端API获取订单数据
      const orders = await this.fetchOrdersFromAPI();
      
      if (refresh) {
        this.setData({
          orders: orders,
          page: 2
        });
      } else {
        this.setData({
          orders: [...this.data.orders, ...orders],
          page: this.data.page + 1
        });
      }

      // 判断是否还有更多数据
      if (orders.length < this.data.pageSize) {
        this.setData({ hasMore: false });
      }

    } catch (error) {
      console.error('加载订单失败:', error);
      wx.showToast({
        title: '加载失败，请重试',
        icon: 'none'
      });
    } finally {
      this.setData({ loading: false });
    }
  },

  // 调用后端API获取订单数据
  async fetchOrdersFromAPI() {
    try {
      // 获取当前用户ID（假设存储在userInfo中）
      const userInfo = this.data.userInfo;
      if (!userInfo || !userInfo.customerId) {
        // 如果没有客户ID，使用默认客户ID=1进行测试
        const customerId = 1;
        const orders = await API.getCustomerOrders(customerId, this.data.page, this.data.pageSize);
        
        return this.formatOrdersData(orders);
      }

      const orders = await API.getCustomerOrders(userInfo.customerId, this.data.page, this.data.pageSize);
      return this.formatOrdersData(orders);
    } catch (error) {
      console.error('获取订单数据失败:', error);
      wx.showToast({
        title: '获取订单数据失败',
        icon: 'none'
      });
      throw error;
    }
  },

  // 格式化订单数据
  formatOrdersData(orders) {
    return orders.map((order, index) => {
      try {
        const formattedOrder = {
          ...order,
          // 确保字段存在且有默认值
          orderId: order.orderId || order.OrderID || 0,
          orderTime: order.orderTime || order.OrderTime || '未知时间',
          totalPrice: order.totalPrice || order.TotalPrice || 0,
          orderStatus: order.orderStatus || order.OrderStatus || '状态未知',
          storeName: order.storeName || order.StoreName || '未知店铺',
          tableNumber: order.tableNumber || order.TableNumber || '',
          customerName: order.customerName || order.CustomerName || '未知客户',
          formattedTime: API.formatTime(order.orderTime || order.OrderTime),
          statusInfo: API.formatOrderStatus(order.orderStatus || order.OrderStatus)
        };
        
        return formattedOrder;
      } catch (error) {
        // 返回安全的默认订单对象
        return {
          orderId: order.orderId || order.OrderID || index + 1,
          orderTime: '时间未知',
          totalPrice: 0,
          orderStatus: '状态未知',
          storeName: '未知店铺',
          tableNumber: '',
          customerName: '未知客户',
          formattedTime: '时间未知',
          statusInfo: { text: '状态未知', class: 'default' },
          details: []
        };
      }
    });
  },

  // 查看订单详情
  viewOrderDetail(e) {
    const orderId = e.currentTarget.dataset.orderid;
    const order = this.data.orders.find(o => o.orderId === orderId);
    
    if (!order) return;

    // 显示订单详情弹窗
    this.setData({
      selectedOrder: order,
      showDetail: true
    });
  },

  // 关闭详情弹窗
  closeDetail() {
    this.setData({
      showDetail: false,
      selectedOrder: null
    });
  },

  // 下拉刷新
  onPullDownRefresh() {
    this.loadOrders(true);
    wx.stopPullDownRefresh();
  },

  // 上拉加载更多
  onReachBottom() {
    if (this.data.hasMore && !this.data.loading) {
      this.loadOrders();
    }
  },

  // 重新点餐（跳转到点餐页面）
  reorder(e) {
    const orderId = e.currentTarget.dataset.orderid;
    wx.showToast({
      title: '正在准备重新点餐',
      icon: 'loading'
    });
    
    setTimeout(() => {
      wx.switchTab({
        url: '/pages/index/index'
      });
    }, 1000);
  },

  // 跳转到点餐页面
  goToOrder() {
    wx.switchTab({
      url: '/pages/index/index'
    });
  }
});