// review.js - 订单评价页面
Page({
  data: {
    orderInfo: {}, // 订单信息
    overallRating: 0, // 总体评分
    reviewContent: '', // 评价内容
    canSubmit: false // 是否可以提交
  },

  onLoad(options) {
    // 获取传递的订单信息
    if (options.orderId) {
      this.loadOrderInfo(options.orderId);
    }
    
    // 初始化评分描述
    this.initRatingDescriptions();
  },

  // 加载订单信息
  loadOrderInfo(orderId) {
    // 这里应该从本地存储或API获取订单信息
    // 暂时使用模拟数据
    console.log("传递的订单ID:", orderId, order);
    const orderInfo = {
      orderId: orderId,
      orderTime: '2024-01-20 18:30:00',
      storeName: '旗舰店',
      totalPrice: 168.50
    };
    
    this.setData({ orderInfo });
  },



  // 初始化评分描述
  initRatingDescriptions() {
    this.ratingDescriptions = [
      '非常差',
      '差',
      '一般',
      '好',
      '非常好'
    ];
  },

  // 设置总体评分
  setOverallRating(e) {
    const rating = e.currentTarget.dataset.rating;
    this.setData({ overallRating: rating });
    this.checkCanSubmit();
  },



  // 评价内容输入
  onReviewInput(e) {
    const reviewContent = e.detail.value;
    this.setData({ reviewContent });
    this.checkCanSubmit();
  },



  // 检查是否可以提交
  checkCanSubmit() {
    const canSubmit = this.data.overallRating > 0 && 
                     this.data.reviewContent.trim().length > 0;
    
    this.setData({ canSubmit });
  },

  // 提交评价
  async submitReview() {
    if (!this.data.canSubmit) {
      wx.showToast({
        title: '请完善评价信息',
        icon: 'none'
      });
      return;
    }

    try {
      wx.showLoading({ title: '提交中...' });

      // 收集评价数据
      const reviewData = {
        CustomerID: 1, // 暂时使用默认客户ID，实际应该从用户信息获取
        OrderID: this.data.orderInfo.orderId,
        StoreID: 1, // 暂时使用默认门店ID，实际应该从订单信息获取
        OverallRating: this.data.overallRating,
        Comment: this.data.reviewContent
      };

      console.log('提交评价数据:', reviewData);

      // 调用后端API提交评价
      const response = await this.submitReviewToAPI(reviewData);

      wx.hideLoading();
      
      wx.showToast({
        title: '评价提交成功',
        icon: 'success'
      });

      // 延迟返回上一页
      setTimeout(() => {
        wx.navigateBack();
      }, 1500);

    } catch (error) {
      wx.hideLoading();
      console.error('提交评价失败:', error);
      
      wx.showToast({
        title: '提交失败，请重试',
        icon: 'none'
      });
    }
  },

  // 调用后端API提交评价
  async submitReviewToAPI(reviewData) {
    try {
      console.log('开始发送请求到:', 'http://localhost:5002/api/review');
      console.log('请求数据:', reviewData);
      
      const response = await new Promise((resolve, reject) => {
        wx.request({
          url: 'http://localhost:5002/api/review',
          method: 'POST',
          data: reviewData,
          header: {
            'content-type': 'application/json'
          },
          success: (res) => {
            console.log('请求成功:', res);
            resolve(res);
          },
          fail: (err) => {
            console.log('请求失败:', err);
            reject(new Error(`网络请求失败: ${err.errMsg}`));
          }
        });
      });

      console.log('API响应:', response);

      if (response.statusCode === 200 && response.data && response.data.success) {
        return response.data;
      } else if (response.statusCode === 404) {
        throw new Error('评价API接口不存在，请检查后端服务');
      } else if (response.data && response.data.message) {
        throw new Error(response.data.message);
      } else {
        throw new Error(`提交失败，状态码: ${response.statusCode}`);
      }
    } catch (error) {
      console.error('API调用失败:', error);
      if (error.message.includes('fail')) {
        throw new Error('网络连接失败，请检查后端服务是否启动');
      }
      throw error;
    }
  }
});
