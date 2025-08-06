// pages/order/order.js
// pages/order/order.js
Page({
  data: {
    tableNumber: 'table:00', // 将来可以从其他地方获取
    orderItems: [], // 订单商品列表
    totalPrice: 0   // 总价
  },

  onLoad: function (options) {
    // 页面加载时，从本地缓存读取订单数据
    try {
      const items = wx.getStorageSync('order_items');
      const price = wx.getStorageSync('order_total_price');
      if (items && price) {
        this.setData({
          orderItems: items,
          totalPrice: price
        });
      }
    } catch (e) {
      console.error('读取订单数据失败', e);
    }
  },
  
  // 将来用于调用微信支付
  handlePayment: function() {
    wx.showToast({
      title: '正在开发中...',
      icon: 'none'
    });
    // 在这里可以编写调用微信支付API的逻辑
  }
})
