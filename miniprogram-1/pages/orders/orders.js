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

  // 为所有订单加载菜品详情
  async loadOrderDetailsForAll(orders) {
    try {
      const ordersWithDetails = [];
      
      for (const order of orders) {
        try {
          // 获取订单详情
          const details = await API.getOrderDetails(order.orderId || order.OrderID);
          
          // 检查订单是否已评价
          const reviewStatus = await API.checkOrderReviewStatus(order.orderId || order.OrderID);
          
          // 处理详情数据，确保字段名正确，并过滤掉"辣度选择"菜品
          const processedDetails = (details || [])
            .filter(detail => {
              const dishName = detail.DishName || detail.dishName || '';
              // 过滤掉"辣度选择"相关的菜品
              const excludeKeywords = ['辣度选择', '辣度', '选择', 'SpicyLevel', 'spicyLevel'];
              return !excludeKeywords.some(keyword => dishName.includes(keyword));
            })
            .map(detail => ({
              dishName: detail.DishName || detail.dishName || '未知菜品',
              unitPrice: detail.UnitPrice || detail.unitPrice || 0,
              quantity: detail.Quantity || detail.quantity || 0,
              subtotal: detail.Subtotal || detail.subtotal || 0,
              specialRequests: detail.SpecialRequests || detail.specialRequests || ''
            }));
          
          // 合并订单信息和详情
          ordersWithDetails.push({
            ...order,
            details: processedDetails,
            hasReview: !!reviewStatus, // 是否已评价
            reviewData: reviewStatus // 评价数据
          });
          
        } catch (error) {
          // 现在404不会抛出错误，所以这里只处理真正的错误
          console.error(`获取订单 ${order.orderId || order.OrderID} 详情失败:`, error);
          // 如果获取详情失败，仍然保留订单基本信息
          ordersWithDetails.push({
            ...order,
            details: [],
            hasReview: false,
            reviewData: null
          });
        }
      }
      
      console.log('所有订单详情加载完成:', ordersWithDetails);
      return ordersWithDetails;
      
    } catch (error) {
      console.error('批量加载订单详情失败:', error);
      // 如果批量加载失败，返回原始订单数据
      return orders.map(order => ({ 
        ...order, 
        details: [],
        hasReview: false,
        reviewData: null
      }));
    }
  },

  async fetchOrdersFromAPI() {
    try {
      const userInfo = this.data.userInfo;
      let customerId;
      if (!userInfo || !userInfo.customerId) {
        customerId = 1; // 默认
        console.warn('⚠️ 未找到用户 customerId，使用默认 1');
      } else {
        customerId = userInfo.customerId;
      }
      
      console.log(`🚀 调用 API: getCustomerOrders(${customerId}, ${this.data.page}, ${this.data.pageSize})`);
      const orders = await API.getCustomerOrders(customerId, this.data.page, this.data.pageSize);
      
      console.log('📦 API 响应状态:', orders.length > 0 ? '有数据' : '空数组');
      console.log('📦 完整响应:', orders); // 打印整个响应
      
      const formattedOrders = this.formatOrdersData(orders);
      return formattedOrders;
    } catch (error) {
      console.error('💥 API 错误详情:', error.response || error); // 打印更多错误
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
// 跳转到评价页面
goToReview(e) {
    const orderId = e.currentTarget.dataset.orderid;
    const order = this.data.orders.find(o => o.orderId === orderId);
    
    if (!order) {
      wx.showToast({
        title: '订单信息获取失败',
        icon: 'none'
      });
      return;
    }
    
    // 检查订单是否已评价
    if (order.hasReview) {
      wx.showToast({
        title: '该订单已评价',
        icon: 'none'
      });
      return;
    }
    
    // 跳转到评价页面，传递订单ID
    wx.navigateTo({
      url: `/pages/review/review?orderId=${orderId}`
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