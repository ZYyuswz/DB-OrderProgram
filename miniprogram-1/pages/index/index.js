// index.js
Page({
  data: {
    tableNumber: null,        // 桌台号
    storeId: null,            // 店铺ID
    isFromQRCode: false,      // 是否从二维码进入
    storeInfo: null,          // 店铺信息
    tableInfo: null           // 桌台信息
  },

  onLoad(options) {
    // 页面加载逻辑
    console.log('点餐页面加载 - 此页面预留给其他组开发');
    wx.setStorageSync('isAddDish',false)
    // 检查是否从二维码进入
    this.checkQRCodeParams(options);
  },

  onShow() {
    // 页面显示逻辑
  },

  /**
   * 检查二维码参数 - 供点餐开发组参考
   */
  checkQRCodeParams(options) {
    console.log('页面参数:', options);
    
    // 处理可能的URL编码问题
    let tableNumber = null;
    let storeId = null;
    
    // 检查是否有tableNumber参数
    if (options.tableNumber) {
      let decodedTableNumber = decodeURIComponent(options.tableNumber);
      console.log('解码后的tableNumber:', decodedTableNumber);
      
      // 检查是否包含storeId信息（处理各种分隔符）
      if (decodedTableNumber.includes('&storeId=') || 
          decodedTableNumber.includes('\\u0026storeId=') ||
          decodedTableNumber.includes('\u0026storeId=')) {
        
        // 统一替换各种分隔符为标准分隔符
        let normalizedString = decodedTableNumber
          .replace('\\u0026', '&')
          .replace('\u0026', '&');
        
        console.log('标准化后的字符串:', normalizedString);
        
        const parts = normalizedString.split('&storeId=');
        if (parts.length === 2) {
          tableNumber = parts[0];
          storeId = parseInt(parts[1]);
          console.log('解析后的参数:', { tableNumber, storeId });
        }
      } else {
        tableNumber = decodedTableNumber;
        storeId = options.storeId || 1;
        console.log('直接获取的参数:', { tableNumber, storeId });
      }
    }
    
    // 检查是否成功解析到参数
    if (tableNumber && storeId) {
      // 从二维码进入
      this.setData({
        tableNumber: tableNumber,
        storeId: storeId,
        isFromQRCode: true
      });
      
      console.log('✅ 从二维码进入小程序');
      console.log('桌台号:', tableNumber);
      console.log('店铺ID:', storeId);
      
      wx.setStorageSync('tableId', tableNumber)
      wx.setStorageSync('storeId', storeId)

      // 显示桌台信息
      this.showTableInfo();
      
    } else {
      console.log('普通进入小程序，无桌台信息');
    }
  },

  /**
   * 显示桌台信息 - 供点餐开发组参考
   */
  showTableInfo() {
    const { tableNumber, storeId } = this.data;
    
    // 显示桌台信息提示
    wx.showToast({
      title: `欢迎来到桌台${tableNumber}`,
      icon: 'success',
      duration: 2000
    });
    
    // 加载桌台信息
    this.loadStoreAndTableInfo();
  },

  /**
   * 加载桌台信息 - 供点餐开发组参考
   * 这里展示了如何获取和使用 tableNumber 和 storeId
   */
  loadStoreAndTableInfo() {
    const { tableNumber, storeId } = this.data;
    
    console.log(`📱 点餐开发组参考：`);
    console.log(`店铺ID: ${storeId}`);
    console.log(`桌台号: ${tableNumber}`);
    console.log(`这两个参数可以用于：`);
    console.log(`1. 调用后端API获取菜单`);
    console.log(`2. 提交订单时标识桌台`);
    console.log(`3. 查询桌台状态`);
    
    // 简化：只保留核心信息
    setTimeout(() => {
      this.setData({
        storeInfo: {
          storeId: storeId,
          // 其他店铺信息可以通过API获取
        },
        tableInfo: {
          tableNumber: tableNumber,
          // 其他桌台信息可以通过API获取
        }
      });
    }, 500);
  }
});
