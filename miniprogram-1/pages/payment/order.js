// pages/order/order.js
Page({
  data: {
    tableId: 1, // 这里可以动态获取，例如从扫码参数中
    tableNumber : '',
    orderItems: [], 
    totalPrice: 0,
    remark: '',
    
    // 用于存储需要提交到后端的数据
    storeId: 1, // 假设店铺ID为1，实际应从全局或缓存获取
    customerId: 1, // 假设顾客ID为1，实际应在用户登录后获取
  },

  onLoad: function (options) {
    // 页面加载时，从本地缓存读取订单数据
    try {
      const items = wx.getStorageSync('order_items');
      const price = wx.getStorageSync('order_total_price');
      this.data.tableId = parseInt(wx.getStorageSync('tableId')) || 1;
      this.data.storeId = wx.getStorageSync('storeId') || 1;
      //必须要setdata，否则无法渲染上去
      this.setData({tableNumber : wx.getStorageSync('tableNumber')});
      //测试时不用，正式运行时取消掉
      // this.data.customerId = wx.getStorageSync('userInfo').phone || 1;
      console.log(this.data.tableId,this.data.storeId);
      console.log(items,price);
      if (items && price) {
        this.setData({
          orderItems: items,
          totalPrice: price
        });
      }
      
      // 示例：如果通过扫码进入，可以从 options 中获取桌号
      // if(options.tableId) {
      //   this.setData({ tableNumber: `table:${options.tableId}` });
      // }

    } catch (e) {
      console.error('读取订单数据失败', e);
    }
  },
  onInputChange(e) {
    this.setData({
      remark: e.detail.value
    });
  },
  /**
   * 主要改动函数：处理下单和支付流程
   */
  handleOrder: function() {
    // 0. 显示加载动画，防止用户重复点击
    wx.showLoading({
      title: '正在提交订单...',
      mask: true // 防止穿透点击
    });

    // 1. 准备POST请求的数据
    const postData = this.preparePostData();
    if (!postData) {
      wx.hideLoading();
      return; // 数据准备失败则中止
    }

    // 2. 发起POST请求，提交订单
    this.postOrder(postData);
  },
  
  /**
   * 准备要POST到后端的数据
   * @returns {object|null} 组装好的数据对象，或在失败时返回null
   */
  preparePostData: function() {
    const tableId = this.data.tableId;
    if (isNaN(tableId)) {
      wx.showToast({ title: '桌号信息错误', icon: 'none' });
      return null;
    }

    // 格式化订单详情
    const orderDetails = this.data.orderItems.map(item => {
      return {
        dishId: item.dishId,
        quantity: item.quantity,
        unitPrice: item.Price,
        specialRequests: item.dishRemark 
      };
      
    });

    // 组装成最终的请求体
    const data = {
      order: {
        tableId: tableId,
        customerId: this.data.customerId,
        storeId: this.data.storeId,
        orderTime: new Date().toISOString() // 生成ISO 8601格式的时间字符串
      },
      orderDetails: orderDetails // 这里属性名是 orderDetails
    };
    const isAddDish = wx.getStorageSync('isAddDish')||false;
    return isAddDish?orderDetails:data;
  },
  
  /**
   * 发起POST请求的函数
   * @param {object} postData - 要发送的数据
   */
  postOrder: function(postData) {
    const that = this; // 保存this指向
    const isAddDish = wx.getStorageSync('isAddDish')||false;
    const backendApiUrl = "http://localhost:5002/api/order"+ ( isAddDish ? ("/"+this.data.customerId ): "");    
    const method = isAddDish?'PUT':'POST';
    console.log(isAddDish,backendApiUrl,this.data.customerId,method);
    wx.request({
      url: backendApiUrl,
      method: method,
      header: {
        'Content-Type': 'application/json'
        // 'Authorization': 'Bearer ' + wx.getStorageSync('token') // 如果需要登录凭证
      },
      data: postData,
      success: (res) => {
        // HTTP状态码200或201代表成功
        if (res.statusCode === 200 || res.statusCode === 201) {
          console.log('订单提交成功，后端返回:', res.data);
          this.putStatus();         
            // 订单创建成功后，再根据需求发起GET请求获取总价
            if(isAddDish){wx.redirectTo({ url: '/pages/payment/order-success'});return;}
          that.getTotalPriceFromServer(res.data.data);         
        } else {
          // 其他状态码，表示有错误
          wx.hideLoading();
          console.error('订单提交失败:', res);
          wx.showToast({
            title: '下单失败: ' + (res.data.message || '请稍后再试'),
            icon: 'none'
          });
        }
      },
      fail: (err) => {
        // 请求本身失败，例如网络问题
        wx.hideLoading();
        console.error('请求失败:', err);
        wx.showToast({
          title: '网络错误，请检查网络连接',
          icon: 'none'
        });
      }
    });
  },
  
  putStatus:function () {
    wx.request({
      url: 'http://localhost:5002/api/cache/status/'+ this.data.tableId,
      method: 'PUT',   
        success: (res) => {        
          if (res.statusCode === 200) {
            console.log("更新缓存状态成功");
          }
        }
    })
  },

  deleteCache:function () {
    wx.request({
      url: 'http://localhost:5002/api/cache/table/'+ this.data.tableId,
      method: 'DELETE',   
        success: (res) => {        
          if (res.statusCode === 200) {
            console.log("删除缓存成功");
          }
        }
    })
  },

  /**
   * 发起GET请求，从后端获取并确认总价
   * @param {number} orderId - 订单ID
   */
  getTotalPriceFromServer: function(orderId) {
    const backendApiUrl = 'http://localhost:5002/api/order/'+ orderId; 
    wx.setStorageSync('orderId', orderId);
    wx.request({
      url: backendApiUrl,
      method: 'GET',
      success: (res) => {
        wx.hideLoading(); // 在这里隐藏加载提示
        if (res.statusCode === 200) {
          console.log(res.data)
          console.log(`后端确认总价为: ${res.data.data.finalPrice}`);
          wx.showToast({
            title: '下单成功！',
            icon: 'success',
            duration: 2000
          });
          // 这里可以进行页面跳转，例如跳转到支付页面或订单详情页
          wx.redirectTo({ url: '/pages/payment/order-success'})//?orderId=' + orderId });
          
        } else {
           wx.showToast({ title: '无法确认总价', icon: 'none' });
        }
      },
      fail: (err) => {
        wx.hideLoading();
        console.error('获取总价失败:', err);
        wx.showToast({ title: '网络错误', icon: 'none' });
      }
    });
  }
})