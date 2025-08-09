// pages/payment/order-success.js

  
  Page({
    goToHome() {
      wx.reLaunch({
        url: '/pages/index/index' // 修改为你的首页路径
      });
    },
  
    goToMenu() {
      wx.reLaunch({
        url: '/pages/goods/list' // 修改为你的点餐页路径
      });
    }
  });
  


  