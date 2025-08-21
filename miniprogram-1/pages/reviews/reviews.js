Page({
  data: {
    reviews: [],
    totalReviews: 0,
    averageRating: 0,
    helpfulCount: 0,
    loading: false,
    hasMore: true,
    page: 1,
    pageSize: 10
  },

  onLoad() {
    this.loadReviews();
  },

  onPullDownRefresh() {
    this.loadReviews(true);
    wx.stopPullDownRefresh();
  },

  // 加载评价列表
  async loadReviews(refresh = false) {
    if (this.data.loading) return;

    this.setData({ loading: true });

    try {
      // 模拟API调用 - 实际项目中替换为真实API
      const mockReviews = this.generateMockReviews(refresh ? 1 : this.data.page);
      
      if (refresh) {
        this.setData({
          reviews: mockReviews,
          page: 2,
          hasMore: mockReviews.length === this.data.pageSize
        });
      } else {
        this.setData({
          reviews: [...this.data.reviews, ...mockReviews],
          page: this.data.page + 1,
          hasMore: mockReviews.length === this.data.pageSize
        });
      }

      // 更新统计信息
      this.updateStats();

    } catch (error) {
      console.error('加载评价失败:', error);
      wx.showToast({
        title: '加载失败',
        icon: 'none'
      });
    } finally {
      this.setData({ loading: false });
    }
  },

  // 生成模拟评价数据
  generateMockReviews(page) {
    const stores = ['旗舰店', '分店一', '分店二', '分店三'];
    const comments = [
      '味道很不错，下次还会再来！',
      '服务态度很好，菜品新鲜',
      '价格实惠，性价比高',
      '外卖包装很精致，没有洒漏',
      '配送速度很快，点赞',
      '菜品分量足，味道正宗'
    ];

    const reviews = [];
    const count = page > 3 ? 0 : this.data.pageSize; // 模拟分页结束

    for (let i = 0; i < count; i++) {
      const rating = Math.floor(Math.random() * 2) + 4; // 4-5分
      reviews.push({
        reviewId: Date.now() + i,
        storeName: stores[Math.floor(Math.random() * stores.length)],
        storeAvatar: '/images/store-default.png',
        rating: rating,
        content: comments[Math.floor(Math.random() * comments.length)],
        reviewTime: this.formatTime(Date.now() - Math.random() * 30 * 24 * 60 * 60 * 1000),
        orderId: 'ORD' + (10000 + page * 10 + i),
        orderAmount: (Math.random() * 100 + 20).toFixed(2),
        helpfulCount: Math.floor(Math.random() * 10)
      });
    }

    return reviews;
  },

  // 更新统计信息
  updateStats() {
    const { reviews } = this.data;
    const total = reviews.length;
    const totalRating = reviews.reduce((sum, review) => sum + review.rating, 0);
    const average = total > 0 ? totalRating / total : 0;
    const helpful = reviews.reduce((sum, review) => sum + review.helpfulCount, 0);

    this.setData({
      totalReviews: total,
      averageRating: average,
      helpfulCount: helpful
    });
  },

  // 格式化时间
  formatTime(timestamp) {
    const date = new Date(timestamp);
    const now = new Date();
    const diff = now - date;
    
    if (diff < 60 * 60 * 1000) {
      return Math.floor(diff / (60 * 1000)) + '分钟前';
    } else if (diff < 24 * 60 * 60 * 1000) {
      return Math.floor(diff / (60 * 60 * 1000)) + '小时前';
    } else {
      return Math.floor(diff / (24 * 60 * 60 * 1000)) + '天前';
    }
  },

  // 点赞/取消点赞
  toggleHelpful(e) {
    const index = e.currentTarget.dataset.index;
    const reviews = this.data.reviews;
    const review = reviews[index];
    
    review.helpfulCount += review.isHelpful ? -1 : 1;
    review.isHelpful = !review.isHelpful;

    this.setData({ reviews });
    this.updateStats();

    wx.showToast({
      title: review.isHelpful ? '已点赞' : '已取消',
      icon: 'success'
    });
  },

  // 删除评价
  deleteReview(e) {
    const index = e.currentTarget.dataset.index;
    
    wx.showModal({
      title: '确认删除',
      content: '确定要删除这条评价吗？',
      success: (res) => {
        if (res.confirm) {
          const reviews = this.data.reviews;
          reviews.splice(index, 1);
          
          this.setData({ reviews });
          this.updateStats();

          wx.showToast({
            title: '删除成功',
            icon: 'success'
          });
        }
      }
    });
  },

  // 加载更多
  loadMoreReviews() {
    if (this.data.hasMore && !this.data.loading) {
      this.loadReviews();
    }
  },

  // 跳转到订单页面
  goToOrders() {
    wx.navigateTo({
      url: '/pages/orders/orders'
    });
  }
});