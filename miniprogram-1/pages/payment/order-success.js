// pages/payment/order-success.js

  
  Page({
    data: {
      //tableNumber: 'table:1'
      orderId:0,
      orderItems: [], 
      finalPrice: 0,
    },
  
    onLoad: function () {
      wx.setStorageSync('isAddDish', true);
      this.setData({
        orderId : wx.getStorageSync('orderId'),
        finalPrice : wx.getStorageSync('finalPrice')
      });
      const backendApiUrl = 'http://localhost:5002/api/order/detail/'+ this.data.orderId; 
      // 页面加载时，从后端读取订单数据
      try {
        wx.request({
          url: backendApiUrl,
          method: 'GET',
          success: (res) => {           
            if (res.statusCode === 200) {
              console.log(res.data)
              console.log("后端确认菜单为: ",res.data.data.dishes);
              //dishes = res.data.data.dishes;
              //price = res.data.data.finalPrice;
              this.setData({
                orderItems: res.data.data.dishes,
                finalPrice: res.data.data.finalPrice
              });              
            } else {
               wx.showToast({ title: '请稍候', icon: 'none' });
            }
          },          
        });       
      } catch (e) {
          console.error('从数据库读取订单数据失败', e);
        }
      },
    goToHome() {
      wx.setStorageSync('isAddDish',false)
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
  


  