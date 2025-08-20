// index.js
Page({
  data: {
    // 此页面数据预留给其他组进行开发
  },

  onLoad() {
    // 页面加载逻辑
    console.log('点餐页面加载 - 此页面预留给其他组开发');
    wx.setStorageSync('isAddDish', false);
  },

  onShow() {
    // 页面显示逻辑
  }
});
