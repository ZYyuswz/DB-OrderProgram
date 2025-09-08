// reviews.js - 我的评价页面
import API from '../../utils/api.js';

Page({
  data: {
    reviews: [], // 评价列表
    totalReviews: 0, // 总评价数
    averageRating: 0, // 平均评分
    helpfulCount: 0, // 获赞数
    loading: false, // 加载状态
    hasMore: true, // 是否还有更多数据
    page: 1, // 当前页码
    pageSize: 10, // 每页条数
    userInfo: null // 用户信息
  },

  onLoad() {
    // 页面加载时获取用户信息并加载评价数据
    this.loadUserInfo();
    this.loadReviews(true);
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

  // 加载评价数据
  async loadReviews(refresh = false) {
    if (this.data.loading) return;
    
    try {
      this.setData({ loading: true });
      
      const page = refresh ? 1 : this.data.page;
      
      // 获取当前用户ID
      const userInfo = this.data.userInfo;
      const customerId = userInfo?.customerId || 1;
      
      console.log('🔄 获取评价数据，客户ID:', customerId, '页码:', page);
      
      // 调用后端API获取评价数据
      const response = await this.fetchReviewsFromAPI(customerId, page, this.data.pageSize);
      
      if (refresh) {
        // 刷新时替换所有数据
        this.setData({
          reviews: response.reviews || [],
          page: 1,
          hasMore: (response.reviews || []).length >= this.data.pageSize,
          loading: false
        });
      } else {
        // 加载更多时追加数据
        this.setData({
          reviews: [...this.data.reviews, ...(response.reviews || [])],
          page: page + 1,
          hasMore: (response.reviews || []).length >= this.data.pageSize,
          loading: false
        });
      }
      
      // 更新统计信息
      this.updateStats(response.stats);
      
    } catch (error) {
      console.error('❌ 获取评价数据失败:', error);
      this.setData({ loading: false });
      
      if (error.message && !error.message.includes('网络')) {
        wx.showToast({
          title: '获取评价数据失败',
          icon: 'none'
        });
      }
    }
  },

  // 调用后端API获取评价数据
  async fetchReviewsFromAPI(customerId, page, pageSize) {
    try {
      console.log('📡 调用评价API，客户ID:', customerId);
      
      // 调用后端API获取评价列表
      const reviewsResponse = await new Promise((resolve, reject) => {
        wx.request({
          url: `http://localhost:5002/api/review/customer/${customerId}`,
          method: 'GET',
          success: (res) => {
            if (res.statusCode === 200 && res.data.success) {
              resolve(res.data);
            } else {
              reject(new Error(res.data.message || '获取评价失败'));
            }
          },
          fail: (err) => {
            reject(new Error('网络请求失败: ' + err.errMsg));
          }
        });
      });

      console.log('✅ 评价API响应:', reviewsResponse);

      // 处理评价数据
      const processedReviews = this.processReviewsData(reviewsResponse.data || []);
      
      // 计算统计信息
      const stats = this.calculateStats(processedReviews);
      
      return {
        reviews: processedReviews,
        stats: stats
      };
      
    } catch (error) {
      console.error('❌ 调用评价API失败:', error);
      
      // 如果API调用失败，使用模拟数据（仅用于测试）
      if (error.message.includes('网络') || error.message.includes('连接')) {
        console.log('⚠️ 使用模拟数据进行测试');
        return this.getMockReviewsData();
      }
      
      throw error;
    }
  },

  // 处理评价数据
  processReviewsData(reviews) {
    return reviews.map(review => ({
      reviewId: review.reviewID || review.ReviewID || 0,
      customerId: review.customerID || review.CustomerID || 0,
      orderId: review.orderID || review.OrderID || 0,
      storeId: review.storeID || review.StoreID || 0,
      rating: review.overallRating || review.OverallRating || 0,
      content: review.comment || review.Comment || '暂无评价内容',
      reviewTime: review.reviewTime || review.ReviewTime || new Date(),
      status: review.status || review.Status || '待审核',
      storeName: review.storeName || review.StoreName || '未知门店',
      customerName: review.customerName || review.CustomerName || '匿名用户',
      orderTime: review.orderTime || review.OrderTime || '',
      // 格式化显示字段
      formattedTime: this.formatTime(review.reviewTime || review.ReviewTime || new Date()),
      formattedRating: (review.overallRating || review.OverallRating || 0) + '分',
      // 模拟数据（后续可以从API获取真实数据）
      helpfulCount: Math.floor(Math.random() * 20), // 模拟获赞数
      orderAmount: (Math.random() * 200 + 30).toFixed(2) // 模拟订单金额
    }));
  },

  // 计算统计信息
  calculateStats(reviews) {
    const total = reviews.length;
    const totalRating = reviews.reduce((sum, review) => sum + (review.rating || 0), 0);
    const average = total > 0 ? (totalRating / total).toFixed(1) : 0;
    const helpful = reviews.reduce((sum, review) => sum + (review.helpfulCount || 0), 0);

    return {
      totalReviews: total,
      averageRating: parseFloat(average),
      helpfulCount: helpful
    };
  },

  // 更新统计信息
  updateStats(stats) {
    if (stats) {
      this.setData({
        totalReviews: stats.totalReviews || 0,
        averageRating: stats.averageRating || 0,
        helpfulCount: stats.helpfulCount || 0
      });
    }
  },

  // 格式化时间
  formatTime(timeString) {
    if (!timeString) return '时间未知';
    
    try {
      const date = new Date(timeString);
      if (isNaN(date.getTime())) {
        return timeString;
      }

      const now = new Date();
      const diffTime = now - date;
      const diffMinutes = Math.floor(diffTime / (1000 * 60));
      const diffHours = Math.floor(diffTime / (1000 * 60 * 60));
      const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));

      if (diffMinutes < 60) {
        return `${diffMinutes}分钟前`;
      } else if (diffHours < 24) {
        return `${diffHours}小时前`;
      } else if (diffDays < 7) {
        return `${diffDays}天前`;
      } else {
        return date.toISOString().split('T')[0]; // 返回日期部分
      }
    } catch (error) {
      return timeString;
    }
  },

  // 点赞/取消点赞
  async toggleHelpful(e) {
    const index = e.currentTarget.dataset.index;
    const reviewId = this.data.reviews[index].reviewId;
    
    try {
      // 调用后端API更新点赞状态
      // 这里需要后端提供相应的API接口
      console.log('👍 点赞评价:', reviewId);
      
      // 暂时使用前端模拟
      const reviews = this.data.reviews.map((item, i) => {
        if (i === index) {
          const newHelpfulCount = item.isHelpful ? item.helpfulCount - 1 : item.helpfulCount + 1;
          return {
            ...item,
            helpfulCount: newHelpfulCount,
            isHelpful: !item.isHelpful
          };
        }
        return item;
      });
      
      this.setData({ reviews });
      this.updateStats(this.calculateStats(reviews));
      
      wx.showToast({
        title: this.data.reviews[index].isHelpful ? '已取消点赞' : '已点赞',
        icon: 'success'
      });
      
    } catch (error) {
      console.error('点赞操作失败:', error);
    }
  },

  // 删除评价
  async deleteReview(e) {
    const index = e.currentTarget.dataset.index;
    const reviewId = this.data.reviews[index].reviewId;
    
    wx.showModal({
      title: '确认删除',
      content: '确定要删除这条评价吗？',
      success: async (res) => {
        if (res.confirm) {
          try {
            // 调用后端API删除评价
            // 这里需要后端提供相应的API接口
            console.log('🗑️ 删除评价:', reviewId);
            
            // 暂时使用前端删除
            const reviews = [...this.data.reviews];
            reviews.splice(index, 1);
            
            this.setData({ reviews });
            this.updateStats(this.calculateStats(reviews));
            
            wx.showToast({
              title: '删除成功',
              icon: 'success'
            });
            
          } catch (error) {
            console.error('删除评价失败:', error);
            wx.showToast({
              title: '删除失败',
              icon: 'none'
            });
          }
        }
      }
    });
  },

  // 下拉刷新
  onPullDownRefresh() {
    this.loadReviews(true);
    wx.stopPullDownRefresh();
  },

  // 上拉加载更多
  onReachBottom() {
    if (this.data.hasMore && !this.data.loading) {
      this.loadReviews();
    }
  },

  // 页面显示时刷新数据
  onShow() {
    if (this.data.userInfo) {
      this.loadReviews(true);
    }
  },

  // 跳转到订单页面
  goToOrders() {
    wx.navigateTo({
      url: '/pages/orders/orders'
    });
  },

  // 查看订单详情
  viewOrderDetail(e) {
    const orderId = e.currentTarget.dataset.orderid;
    if (orderId) {
      wx.navigateTo({
        url: `/pages/orders/orders?orderId=${orderId}`
      });
    }
  },

  // 模拟数据（仅用于测试，当API不可用时）
  getMockReviewsData() {
    console.log('📋 使用模拟评价数据');
    
    const mockReviews = [
      {
        reviewID: 1,
        customerID: 1,
        orderID: 1001,
        storeID: 1,
        overallRating: 5,
        comment: "味道很不错，服务态度很好！",
        reviewTime: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
        status: "已审核",
        storeName: "旗舰店",
        customerName: "测试用户",
        orderTime: "2024-01-20 18:30:00"
      },
      {
        reviewID: 2,
        customerID: 1,
        orderID: 1002,
        storeID: 2,
        overallRating: 4,
        comment: "配送速度很快，包装完好",
        reviewTime: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(),
        status: "已审核",
        storeName: "分店一",
        customerName: "测试用户",
        orderTime: "2024-01-18 12:15:00"
      }
    ];
    
    const processedReviews = this.processReviewsData(mockReviews);
    const stats = this.calculateStats(processedReviews);
    
    return {
      reviews: processedReviews,
      stats: stats
    };
  }
});