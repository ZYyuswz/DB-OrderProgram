class APIManager {
  constructor() {
    this.baseURL = 'http://localhost:5002/api';
  }

  // 通用请求方法
  request(url, options = {}) {
    return new Promise((resolve, reject) => {
      const {
        method = 'GET',
        data = {},
        header = {}
      } = options;

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
      console.error('获取订单列表失败:', error);
      throw error;
    }
  }

  // 获取订单详情
  async getOrderDetails(orderId) {
    try {
      const details = await this.request(`/orders/${orderId}/details`);
      return details;
    } catch (error) {
      console.error('获取订单详情失败:', error);
      throw error;
    }
  }



  formatTime(timeString) {
    if (!timeString || typeof timeString !== 'string') {
      return '时间未知';
    }

    try {
      const date = new Date(timeString);
      if (isNaN(date.getTime())) {
        return timeString;
      }

      const now = new Date();
      const diffTime = now - date;
      const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));
      const timeParts = timeString.split(' ');
      const timePart = timeParts.length > 1 ? timeParts[1] : '';
      
      if (diffDays === 0 && timePart) {
        return '今天 ' + (timePart.length >= 5 ? timePart.substring(0, 5) : timePart);
      } else if (diffDays === 1 && timePart) {
        return '昨天 ' + (timePart.length >= 5 ? timePart.substring(0, 5) : timePart);
      } else if (diffDays < 7 && diffDays > 0) {
        return diffDays + '天前';
      } else {
        return timeParts[0] || timeString;
      }
    } catch (error) {
      return timeString;
    }
  }

  // 获取客户积分记录
  async getCustomerPointsRecords(customerId, page = 1, pageSize = 10) {
    try {
      const records = await this.request(`/points/customer/${customerId}/records`, {
        method: 'GET',
        data: { page, pageSize }
      });
      return records;
    } catch (error) {
      console.error('获取积分记录失败:', error);
      throw error;
    }
  }

  // 获取客户积分余额
  async getCustomerPointsBalance(customerId) {
    try {
      const balance = await this.request(`/points/customer/${customerId}/balance`);
      return balance;
    } catch (error) {
      console.error('获取积分余额失败:', error);
      throw error;
    }
  }

  // 获取客户会员信息
  async getCustomerMemberInfo(customerId) {
    try {
      const memberInfo = await this.request(`/member/customer/${customerId}/info`);
      return memberInfo;
    } catch (error) {
      console.error('获取会员信息失败:', error);
      throw error;
    }
  }

  // 获取会员等级规则
  async getMemberLevels() {
    try {
      const levels = await this.request('/member/levels');
      return levels;
    } catch (error) {
      console.error('获取会员等级规则失败:', error);
      throw error;
    }
  }

  // 获取客户消费统计
  async getCustomerConsumptionStats(customerId) {
    try {
      const stats = await this.request(`/member/customer/${customerId}/consumption-stats`);
      return stats;
    } catch (error) {
      console.error('获取消费统计失败:', error);
      throw error;
    }
  }

  // 更新客户累计消费金额
  async updateCustomerTotalConsumption(customerId) {
    try {
      const result = await this.request(`/member/customer/${customerId}/update-consumption`, {
        method: 'POST'
      });
      return result;
    } catch (error) {
      console.error('更新累计消费金额失败:', error);
      throw error;
    }
  }

  // 更新客户会员等级
  async updateCustomerMemberLevel(customerId) {
    try {
      const result = await this.request(`/member/customer/${customerId}/update-level`, {
        method: 'POST'
      });
      return result;
    } catch (error) {
      console.error('更新会员等级失败:', error);
      throw error;
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