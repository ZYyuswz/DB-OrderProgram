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
      
      // 为每个订单获取菜品详情
      const ordersWithDetails = await this.loadOrderDetailsForAll(orders);
      
      if (refresh) {
        this.setData({
          orders: ordersWithDetails,
          page: 2
        });
      } else {
        this.setData({
          orders: [...this.data.orders, ...ordersWithDetails],
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
            details: processedDetails
          });
          
        } catch (error) {
          console.error(`获取订单 ${order.orderId || order.OrderID} 详情失败:`, error);
          // 如果获取详情失败，仍然保留订单基本信息
          ordersWithDetails.push({
            ...order,
            details: []
          });
        }
      }
      
      console.log('所有订单详情加载完成:', ordersWithDetails);
      return ordersWithDetails;
      
    } catch (error) {
      console.error('批量加载订单详情失败:', error);
      // 如果批量加载失败，返回原始订单数据
      return orders.map(order => ({ ...order, details: [] }));
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
        console.log('🔍 开始获取客户', customerId, '的订单数据...');
        
        const orders = await API.getCustomerOrders(customerId, this.data.page, this.data.pageSize);
        console.log('📊 后端返回的原始订单数据:', orders);
        console.log('🔍 第一个订单的字段检查:', {
          hasOrderID: orders[0] && 'OrderID' in orders[0],
          hasOrderTime: orders[0] && 'OrderTime' in orders[0],
          hasTotalPrice: orders[0] && 'TotalPrice' in orders[0],
          orderKeys: orders[0] ? Object.keys(orders[0]) : [],
          firstOrder: orders[0]
        });
        
        const formattedOrders = this.formatOrdersData(orders);
        console.log('✨ 格式化后的订单数据:', formattedOrders);
        
        return formattedOrders;
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
        console.log(`🔍 格式化第${index + 1}个订单，原始字段:`, Object.keys(order));
        
        const formattedOrder = {
          ...order,
          // 统一字段名（后端返回大写开头，前端使用小写开头）
          orderId: order.OrderID || order.orderID || order.orderId || 0,
          orderTime: order.OrderTime || order.orderTime || '未知时间',
          totalPrice: order.TotalPrice || order.totalPrice || 0,
          orderStatus: order.OrderStatus || order.orderStatus || '状态未知',
          storeName: order.StoreName || order.storeName || '未知店铺',
          tableNumber: order.TableNumber || order.tableNumber || '',
          customerName: order.CustomerName || order.customerName || '未知客户',
          formattedTime: API.formatTime((order.OrderTime || order.orderTime || '').toString()),
          statusInfo: API.formatOrderStatus(order.OrderStatus || order.orderStatus),
          // 初始化空的详情数组，稍后会通过API获取
          details: [],
          // 调试信息：记录所有可用字段
          __allKeys: Object.keys(order).join(', ')
        };
        
        console.log(`✅ 第${index + 1}个订单格式化完成:`, formattedOrder);
        return formattedOrder;
      } catch (error) {
        console.error(`❌ 格式化第${index + 1}个订单失败:`, error, order);
        // 返回安全的默认订单对象
        return {
          orderId: order.OrderID || order.orderID || order.orderId || index + 1,
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
  async viewOrderDetail(e) {
    const orderId = e.currentTarget.dataset.orderid;
    const order = this.data.orders.find(o => o.orderId === orderId);
    
    if (!order) return;

    // 直接显示订单详情，因为详情已经在加载订单时获取了
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
    
    // 跳转到评价页面，传递订单ID
    wx.navigateTo({
      url: `/pages/review/review?orderId=${orderId}`
    });
  },

  // 跳转到点餐页面
  goToOrder() {
    wx.switchTab({
      url: '/pages/index/index'
    });
  },

  // 阻止事件冒泡
  stopEvent() {
    // 这个方法什么都不做，只是用来阻止事件冒泡
    return;
  }
});