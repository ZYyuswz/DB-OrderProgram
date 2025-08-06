// list.js - 修复版测试文件
Page({
  data: {
    listType: 1,
    name: '',
    orderBy: '',
    page: 1,
    categories: [], // 分类数组
    activeCategory: null, // 当前激活分类
    goods: [],
    show_seller_number: '1',
    skuCurGoods: null,
    
    cartItems: [], // 购物车商品
    totalPrice: 0, // 总价格
    totalQuantity: 0, // 总数量
    showCartPopup: false, // 是否显示购物车弹窗
    minOrderPrice: 20, // 最低起送价
    $t: {
      common: {
        searchPlaceholder: "搜索菜品",
        empty: "暂无菜品"
      },     
    }
  },

  onLoad: function (options) {
    // 设置测试数据
    this.setData({
      categories: this.getTestCategories(),
      activeCategory: 1 // 默认激活第一个分类
    });
    // 初始化购物车
    this.updateCartSummary();
  },
  
  // 获取测试菜品数据
    // 获取测试分类数据
    getTestCategories: function() {
      return [
        {
          id: 1,
          name: "前菜",
          description: "开胃冷盘，精致小食",
          goods: [
            {
              dishId: 2,
              dishName: "拍黄瓜",
              pic: "/images/dish/2.jpg",
              characteristic: "清爽开胃，蒜香浓郁",
              Price: 12,
              numberSells: 89,
              stores: 30
            }
          ]
        },
        {
          id: 2,
          name: "主菜",
          description: "招牌热菜，美味佳肴",
          goods: [
            {
              dishId: 1,
              dishName: "宫保鸡丁",
              pic: "/images/dish/1.jpg",
              characteristic: "经典川菜，微辣，鸡肉鲜嫩",
              Price: 28,
              numberSells: 128,
              stores: 20
            }
          ]
        },
        {
          id: 3,
          name: "饮料",
          description: "清凉饮品，解渴佳品",
          goods: [
            {
              dishId: 3,
              dishName: "可乐",
              pic: "/images/dish/3.jpg",
              characteristic: "冰镇可乐，畅爽解渴",
              Price: 6,
              numberSells: 256,
              stores: 100
            }
          ]
        }
      ];
    },
  
// 增加商品数量
increaseQuantity: function(e) {
  const dishId = e.currentTarget.dataset.id;
  this.addToCart(dishId);
},

// 减少商品数量
decreaseQuantity: function(e) {
  const dishId = e.currentTarget.dataset.id;
  this.removeFromCart(dishId);
},

// 添加商品到购物车
addToCart: function(dishId) {
  const categories = this.data.categories;
  let cartItems = this.data.cartItems;
  let itemFound = false;
  
  // 在分类数据中查找商品
  for (const category of categories) {
    for (const good of category.goods) {
      if (good.dishId == dishId) {
        // 更新商品数量
        if (!good.quantity) good.quantity = 0;
        good.quantity += 1;
        
        // 更新购物车
        const existingItem = cartItems.find(item => item.dishId == dishId);
        if (existingItem) {
          existingItem.quantity += 1;
        } else {
          cartItems.push({
            dishId: good.dishId,
            dishName: good.dishName,
            Price: good.Price,
            quantity: 1
          });
        }
        
        itemFound = true;
        break;
      }
    }
    if (itemFound) break;
  }
  
  // 更新数据
  this.setData({
    categories: categories,
    cartItems: cartItems
  }, () => {
    this.updateCartSummary();
  });
},

// 从购物车移除商品
removeFromCart: function(dishId) {
  const categories = this.data.categories;
  let cartItems = this.data.cartItems;
  
  // 在分类数据中查找商品
  for (const category of categories) {
    for (const good of category.goods) {
      if (good.dishId == dishId && good.quantity > 0) {
        // 更新商品数量
        good.quantity -= 1;
        
        // 更新购物车
        const itemIndex = cartItems.findIndex(item => item.dishId == dishId);
        if (itemIndex !== -1) {
          if (cartItems[itemIndex].quantity > 1) {
            cartItems[itemIndex].quantity -= 1;
          } else {
            cartItems.splice(itemIndex, 1);
          }
        }
        
        break;
      }
    }
  }
  
  // 更新数据
  this.setData({
    categories: categories,
    cartItems: cartItems
  }, () => {
    this.updateCartSummary();
  });
},

// 更新购物车摘要
updateCartSummary: function() {
  let totalPrice = 0;
  let totalQuantity = 0;
  
  // 计算总价和总数量
  for (const item of this.data.cartItems) {
    totalPrice += item.Price * item.quantity;
    totalQuantity += item.quantity;
  }
  
  this.setData({
    totalPrice: totalPrice.toFixed(2),
    totalQuantity: totalQuantity
  });
},

// 切换购物车弹窗
toggleCartPopup: function() {
  this.setData({
    showCartPopup: !this.data.showCartPopup
  });
},

// 清空购物车
clearCart: function() {
  const categories = this.data.categories;
  
  // 重置所有商品数量
  for (const category of categories) {
    for (const good of category.goods) {
      good.quantity = 0;
    }
  }
  
  this.setData({
    categories: categories,
    cartItems: [],
    showCartPopup: false
  }, () => {
    this.updateCartSummary();
  });
},

// 结算
checkout: function() {
  // 检查购物车是否为空
  if (this.data.cartItems.length === 0) {
    wx.showToast({
      title: '购物车是空的',
      icon: 'none'
    });
    return; // 阻止跳转
  }

  // 1. 将购物车数据和总价存入本地缓存
  try {
    wx.setStorageSync('order_items', this.data.cartItems);
    wx.setStorageSync('order_total_price', this.data.totalPrice);
  } catch (e) {
    console.error('存储订单数据失败', e);
  }

  // 2. 跳转到新的订单确认页面
  wx.navigateTo({
    url: '/pages/payment/order' // 新页面的路径
  });
},
  // 搜索功能 - 测试用
  search: function() {
    wx.showLoading({ title: '搜索中...' });
    setTimeout(() => {
      const keyword = this.data.name.toLowerCase();
      const filteredGoods = this.getTestGoods().filter(function(item) {
        return item.dishName.toLowerCase().includes(keyword);
      });
      this.setData({ goods: filteredGoods });
      wx.hideLoading();
    }, 500);
  },
  switchCategory: function(e) {
    const categoryId = e.currentTarget.dataset.id;
    this.setData({
      activeCategory: categoryId
    });
  },
  
  /**
   * 监听菜品列表滚动
   */
  onGoodsScroll: function(e) {
    // 获取滚动位置
    const scrollTop = e.detail.scrollTop;
    
    // 计算当前应该激活的分类
    // 这里需要实现根据滚动位置确定当前显示的分类
    // 可以使用IntersectionObserver或计算各分类位置
    
    // 示例伪代码：
    // const categories = this.data.categories;
    // for (let i = 0; i < categories.length; i++) {
    //   const category = categories[i];
    //   const top = this.getCategoryPosition(category.id);
    //   if (scrollTop >= top && scrollTop < top + categoryHeight) {
    //     this.setData({ activeCategory: category.id });
    //     break;
    //   }
    // }
  },
  
  /**
   * 获取分类位置（需要配合选择器）
   */
  getCategoryPosition: function(categoryId) {
    return new Promise((resolve) => {
      wx.createSelectorQuery()
        .select('#category' + categoryId)
        .boundingClientRect(rect => {
          resolve(rect.top);
        })
        .exec();
    });
  },

  // 输入处理
  bindinput: function(e) {
    this.setData({ name: e.detail.value });
  },

  // 确认搜索
  bindconfirm: function(e) {
    this.setData({ name: e.detail.value, page: 1 });
    this.search();
  },

  // 筛选排序
  filter: function(e) {
    const orderBy = e.currentTarget.dataset.val;
    this.setData({ orderBy: orderBy, page: 1 });
    
    // 简单排序逻辑
    let sortedGoods = this.data.goods.slice();
    if (orderBy === 'priceUp') {
      sortedGoods.sort(function(a, b) {
        return a.minPrice - b.minPrice;
      });
    } else if (orderBy === 'ordersDown') {
      sortedGoods.sort(function(a, b) {
        return b.numberSells - a.numberSells;
      });
    }
    
    this.setData({ goods: sortedGoods });
  },

  // 加入购物车
  addShopCar: function(e) {
    const dishId = e.currentTarget.dataset.id;
    const dish = this.data.goods.find(function(item) {
      return item.dishId == dishId;
    });
    
    if (!dish) return;
    
    // 50%概率显示SKU弹窗
    if (Math.random() > 0.5) {
      this.showSkuForDish(dish);
    } else {
      wx.showToast({
        title: dish.dishName + ' ' + this.data.$t.goodsDetail.addCartSuccess,
        icon: 'success'
      });
    }
  },
  
  // 显示菜品SKU选择
  showSkuForDish: function(dish) {
    const skuData = {
      basicInfo: {
        id: dish.dishId,
        name: dish.dishName,
        storesBuy: 1,
        stores: dish.stores
      },
      properties: [
        {
          id: 1,
          name: "规格",
          childsCurGoods: [
            { id: 101, propertyId: 1, name: "小份", active: true },
            { id: 102, propertyId: 1, name: "中份", active: false },
            { id: 103, propertyId: 1, name: "大份", active: false }
          ]
        }
      ]
    };
    
    this.setData({ skuCurGoods: skuData });
  },
  
  // 关闭SKU弹窗
  closeSku: function() {
    this.setData({ skuCurGoods: null });
  },
  
  // 选择SKU属性
  skuSelect: function(e) {
    const pid = e.currentTarget.dataset.pid;
    const id = e.currentTarget.dataset.id;
    const skuCurGoods = JSON.parse(JSON.stringify(this.data.skuCurGoods));
    
    // 更新选中状态
    const property = skuCurGoods.properties.find(function(p) {
      return p.id == pid;
    });
    property.childsCurGoods.forEach(function(item) {
      item.active = item.id == id;
    });
    
    this.setData({ skuCurGoods: skuCurGoods });
  },
  
  // 增加购买数量
  storesJia: function() {
    const skuCurGoods = JSON.parse(JSON.stringify(this.data.skuCurGoods));
    if (skuCurGoods.basicInfo.storesBuy < skuCurGoods.basicInfo.stores) {
      skuCurGoods.basicInfo.storesBuy++;
      this.setData({ skuCurGoods: skuCurGoods });
    }
  },
  
  // 减少购买数量
  storesJian: function() {
    const skuCurGoods = JSON.parse(JSON.stringify(this.data.skuCurGoods));
    if (skuCurGoods.basicInfo.storesBuy > 1) {
      skuCurGoods.basicInfo.storesBuy--;
      this.setData({ skuCurGoods: skuCurGoods });
    }
  },
  
  // 添加带SKU的商品到购物车
  addCarSku: function() {
    const dish = this.data.skuCurGoods.basicInfo;
    wx.showToast({
      title: dish.name + ' ' + this.data.$t.goodsDetail.addCartSuccess,
      icon: 'success'
    });
    this.closeSku();
  }
});