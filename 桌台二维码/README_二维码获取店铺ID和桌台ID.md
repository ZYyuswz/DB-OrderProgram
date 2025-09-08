# 📱 桌台参数获取说明

## 🎯 **概述**

本文档说明如何在微信小程序中获取桌台参数（店铺ID和桌台号），供点餐功能开发使用。

## 🔑 **核心参数**

- **`tableNumber`**: 桌台号（如：A01、B02等）
- **`storeId`**: 店铺ID（如：1、2等）

## 📋 **获取方式**

### **方式1：扫描二维码进入**

当用户扫描桌台二维码进入小程序时，参数会自动传递到页面：

```javascript
// 在页面的 onLoad 方法中获取参数
onLoad(options) {
  console.log('页面参数:', options);
  
  // 获取桌台参数
  const tableNumber = options.tableNumber;
  const storeId = options.storeId;
  
  // 设置到页面数据中
  this.setData({
    tableNumber: tableNumber,
    storeId: storeId
  });
}
```

### **方式2：从页面数据中获取**

```javascript
// 在页面方法中获取当前桌台信息
const { tableNumber, storeId } = this.data;

console.log('当前桌台号:', tableNumber);
console.log('当前店铺ID:', storeId);
```

## 🚀 **使用示例**

### **1. 获取菜单**

```javascript
// 调用后端API获取菜单
wx.request({
  url: 'http://localhost:5002/api/menu',
  data: {
    storeId: this.data.storeId,
    tableNumber: this.data.tableNumber
  },
  success: (res) => {
    console.log('菜单数据:', res.data);
  }
});
```

### **2. 提交订单**

```javascript
// 提交订单时包含桌台信息
wx.request({
  url: 'http://localhost:5002/api/orders',
  method: 'POST',
  data: {
    storeId: this.data.storeId,
    tableNumber: this.data.tableNumber,
    items: [
      // 菜品列表...
    ],
    totalPrice: 99.00
  },
  success: (res) => {
    console.log('订单提交成功:', res.data);
  }
});
```

### **3. 查询桌台状态**

```javascript
// 查询当前桌台状态
wx.request({
  url: 'http://localhost:5002/api/table/status',
  data: {
    storeId: this.data.storeId,
    tableNumber: this.data.tableNumber
  },
  success: (res) => {
    console.log('桌台状态:', res.data);
  }
});
```

## 📱 **页面显示**

在 `index.js` 中，桌台信息会自动显示：

```javascript
// 检查二维码参数
checkQRCodeParams(options) {
  // 解析参数逻辑...
  
  if (tableNumber && storeId) {
    this.setData({
      tableNumber: tableNumber,
      storeId: storeId,
      isFromQRCode: true
    });
  }
}
```

在 `index.wxml` 中，会显示桌台信息卡片：

```xml
<view wx:if="{{isFromQRCode}}" class="table-info-card">
  <view class="info-row">
    <text class="label">店铺ID:</text>
    <text class="value">{{storeId}}</text>
  </view>
  <view class="info-row">
    <text class="label">桌台号:</text>
    <text class="value">{{tableNumber}}</text>
  </view>
</view>
```

## ⚠️ **注意事项**

1. **参数验证**：使用前请检查参数是否存在
2. **数据类型**：`storeId` 通常是数字，`tableNumber` 通常是字符串
3. **错误处理**：添加适当的错误处理逻辑

## 🔧 **调试技巧**

在微信开发者工具的控制台中，可以看到详细的参数解析过程：

```
页面参数: {tableNumber: "A01&storeId=1"}
解码后的tableNumber: A01&storeId=1
标准化后的字符串: A01&storeId=1
解析后的参数: {tableNumber: "A01", storeId: 1}
✅ 从二维码进入小程序
桌台号: A01
店铺ID: 1
```

## 📞 **技术支持**

如有问题，请检查：
1. 二维码是否正确生成
2. 参数是否正确传递
3. 页面是否正确解析参数

---

**简单总结**：扫描二维码后，在页面的 `this.data.tableNumber` 和 `this.data.storeId` 中就能获取到桌台参数了！🎉
