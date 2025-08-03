// api.js - API工具类
class APIManager {
  constructor() {
    // 后端API基础URL - 根据实际部署情况修改
    // this.baseURL = 'http://192.168.2.21:5000/api'; // 开发环境（使用本机IP）
    this.baseURL = 'http://localhost:5000/api'; // 本地环境（需要开发者工具允许localhost）
    // this.baseURL = 'https://your-api-domain.com/api'; // 生产环境
  }

  // 通用请求方法
  request(url, options = {}) {
    return new Promise((resolve, reject) => {
      const {
        method = 'GET',
        data = {},
        header = {}
      } = options;

      // 获取用户token（如果需要的话）
      const userInfo = wx.getStorageSync('userInfo');
      if (userInfo && userInfo.token) {
        header.Authorization = `Bearer ${userInfo.token}`;
      }

      wx.request({
        url: this.baseURL + url,
        method: method,
        data: data,
        header: {
          'Content-Type': 'application/json',
          ...header
        },
        success: (res) => {
          if (res.statusCode === 200) {
            resolve(res.data);
          } else {
            reject(new Error(`请求失败: ${res.statusCode}`));
          }
        },
        fail: (error) => {
          reject(new Error('网络连接失败，请检查网络设置'));
        }
      });
    });
  }

  // 获取客户订单列表
  async getCustomerOrders(customerId, page = 1, pageSize = 10) {
    try {
      const orders = await this.request(`/orders/customer/${customerId}`, {
        method: 'GET',
        data: { page, pageSize }
      });
      return orders;
    } catch (error) {
      // 如果API调用失败，返回模拟数据
      return this.getMockOrders();
    }
  }

  // 获取订单详情
  async getOrderDetails(orderId) {
    try {
      const details = await this.request(`/orders/${orderId}/details`);
      return details;
    } catch (error) {
      return this.getMockOrderDetails(orderId);
    }
  }

  // 模拟订单数据（用于开发和测试）
  getMockOrders() {
    return [
      {
        orderId: 1,
        orderTime: '2024-01-20 18:30:00',
        totalPrice: 168.50,
        orderStatus: '已完成',
        storeName: '旗舰店[模拟数据]',
        tableNumber: 'A-05',
        customerName: '默认会员[模拟]',
        details: [
          { dishName: '宫保鸡丁', quantity: 1, unitPrice: 58.50, subtotal: 58.50, specialRequests: '微辣' },
          { dishName: '蚂蚁上树', quantity: 2, unitPrice: 45.00, subtotal: 90.00, specialRequests: '' },
          { dishName: '白米饭', quantity: 2, unitPrice: 8.00, subtotal: 16.00, specialRequests: '' }
        ]
      },
      {
        orderId: 2,
        orderTime: '2024-01-19 12:15:00',
        totalPrice: 89.80,
        orderStatus: '已完成',
        storeName: '旗舰店[模拟数据]',
        tableNumber: 'B-12',
        customerName: '默认会员[模拟]',
        details: [
          { dishName: '红烧肉', quantity: 1, unitPrice: 68.00, subtotal: 68.00, specialRequests: '少油' },
          { dishName: '紫菜蛋花汤', quantity: 1, unitPrice: 18.00, subtotal: 18.00, specialRequests: '' }
        ]
      },
      {
        orderId: 3,
        orderTime: '2024-01-18 19:20:00',
        totalPrice: 245.60,
        orderStatus: '制作中',
        storeName: '旗舰店[模拟数据]',
        tableNumber: 'C-08',
        customerName: '默认会员[模拟]',
        details: [
          { dishName: '水煮鱼', quantity: 1, unitPrice: 128.00, subtotal: 128.00, specialRequests: '不要香菜' },
          { dishName: '麻婆豆腐', quantity: 1, unitPrice: 38.00, subtotal: 38.00, specialRequests: '中辣' },
          { dishName: '酸辣土豆丝', quantity: 1, unitPrice: 28.00, subtotal: 28.00, specialRequests: '' },
          { dishName: '白米饭', quantity: 3, unitPrice: 8.00, subtotal: 24.00, specialRequests: '' }
        ]
      }
    ];
  }

  // 模拟订单详情数据
  getMockOrderDetails(orderId) {
    const orders = this.getMockOrders();
    const order = orders.find(o => o.orderId === orderId);
    return order ? order.details : [];
  }

  // 格式化时间
  formatTime(timeString) {
    // 安全检查
    if (!timeString || typeof timeString !== 'string') {
      return '时间未知';
    }

    try {
      const date = new Date(timeString);
      
      // 检查日期是否有效
      if (isNaN(date.getTime())) {
        return timeString; // 返回原始字符串
      }

      const now = new Date();
      const diffTime = now - date;
      const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));
      
      // 安全处理时间字符串分割
      const timeParts = timeString.split(' ');
      const timePart = timeParts.length > 1 ? timeParts[1] : '';
      
      if (diffDays === 0 && timePart) {
        return '今天 ' + (timePart.length >= 5 ? timePart.substring(0, 5) : timePart);
      } else if (diffDays === 1 && timePart) {
        return '昨天 ' + (timePart.length >= 5 ? timePart.substring(0, 5) : timePart);
      } else if (diffDays < 7 && diffDays > 0) {
        return diffDays + '天前';
      } else {
        return timeParts[0] || timeString; // 返回日期部分或原始字符串
      }
    } catch (error) {
      return timeString; // 发生错误时返回原始字符串
    }
  }

  // 格式化订单状态
  formatOrderStatus(status) {
    const statusMap = {
      '待处理': { text: '待处理', class: 'pending' },
      '制作中': { text: '制作中', class: 'processing' },
      '已完成': { text: '已完成', class: 'completed' },
      '已结账': { text: '已结账', class: 'completed' }
    };
    return statusMap[status] || { text: status, class: 'default' };
  }
}

// 导出API管理实例
export default new APIManager();