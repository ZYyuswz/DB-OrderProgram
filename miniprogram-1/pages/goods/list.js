// list.js 

Page({
  data: {
    categories: [], // 分类数组
    activeCategory: null, // 当前激活分类
    
    cartItems: [], // 购物车商品
    totalPrice: 0, // 总价格
    totalQuantity: 0, // 总数量

    showDishPopup: false,
    selectedDish: {},
    dishRemark: '',
    selectedIceOption: '',
    selectedSpicyOption: '',
    iceOptions: [
      { label: '去冰', value: '去冰' },
      { label: '少冰', value: '少冰' },
      { label: '正常', value: '正常' },
    ],
    spicyOptions: [
      { label: '不辣', value: '不辣' },
      { label: '微辣', value: '微辣' },
      { label: '中辣', value: '中辣' },
      { label: '重辣', value: '重辣' },
    ],
    $t: {
      common: {
        searchPlaceholder: "搜索菜品",
        empty: "暂无菜品"
      },     
    }
  },
  

    
  onLoad: function (options) {
    wx.request({
      url: 'http://localhost:5000/api/dish',
      method: 'GET',
      success: (res) => {        
        if (res.statusCode === 200) {
          console.log(`接收成功`);
          const dishes = res.data.data;
          this.setData({
            categories: this.getTransformedDishData(dishes),
            activeCategory: 1 // 默认激活第一个分类
          });
        }
      }})   
    // 设置测试数据
    
    // 初始化购物车
    this.updateCartSummary();    
       
  },
  getTransformedDishData: function (rawData) {
    const categoryMap = {
      1: { name: "凉菜", description: "开胃冷盘，精致小食" },
      2: { name: "热菜", description: "招牌热菜，美味佳肴" },
      3: { name: "汤", description: "清炖浓煮，荤素皆宜" },
      4: { name: "主食", description: "米面杂粮，煮炒蒸烤" },
      5: { name: "甜点", description: "香甜可口，冷热皆备" },
      6: { name: "饮料", description: "清凉饮品，解渴佳品" },
      7:{name:"忌口", description:"香菜和辣度选择"}
    };
  
    const categoryResult = {};
  
    rawData.forEach(dish => {
      const cid = dish.categoryId;
      if (!categoryMap[cid]) return; // 忽略无效分类
  
      if (!categoryResult[cid]) {
        categoryResult[cid] = {
          id: cid,
          name: categoryMap[cid].name,
          description: categoryMap[cid].description,
          goods: []
        };
      }
  
      // 组装商品字段
      categoryResult[cid].goods.push({
        dishId: dish.dishId,
        dishName: dish.dishName,
        pic: "/images/dish/ID-"+ dish.dishId +".png",
        characteristic: dish.description,
        Price: dish.price, 
        categoryId:dish.categoryId, 
        estimatedTime:dish.estimatedTime,
        dishRemark:''     
      });
    });
  
    // 返回数组形式
    return Object.values(categoryResult);
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


// 打开弹窗
openDishPopup(e) {
  const dish = e.currentTarget.dataset.dish;
  this.setData({
    selectedDish: dish,
    showDishPopup: true,
    dishRemark: '',
    selectedIceOption: ''
  });
},

// 关闭弹窗
closeDishPopup() {
  this.setData({
    showDishPopup: false,
    selectedDish: {},
    dishRemark: '',
    selectedIceOption: ''
  });
  
},

// 输入备注
onRemarkInput(e) {
  this.setData({ dishRemark: e.detail.value });
},

// 选择冰量
onIceOptionChange(e) {
  this.setData({ selectedIceOption: e.detail.value });
},
onSpicyOptionChange(e) {
  this.setData({ selectedSpicyOption: e.detail.value });
},

// 确认选择
confirmDishPopup() {
  let remark = this.data.dishRemark;
  let dishId = this.data.selectedDish.dishId;
  
  // 如果是饮料，合并冰量选项到备注
  if (this.data.selectedDish.categoryId === 6 && this.data.selectedIceOption || this.data.selectedDish.dishName === "辣度选择" && this.data.selectedSpicyOption) {
    console.log(this.data.selectedIceOption,this.data.selectedSpicyOption);
    remark = `${this.data.selectedIceOption}${this.data.selectedSpicyOption}${remark}`;
  }

  // 保存到数据库或购物车项

  //TODO：将remark添加到对应dishId的dishRemark区
  for (const category of this.data.categories) {
    for (const good of category.goods) {
      if (good.dishId == dishId) {
        good.dishRemark = remark;
        console.log("下单前的商品数据",good);
      }
    }
  }
  this.addToCart(dishId);
  this.closeDishPopup();
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
            quantity: 1,
            dishRemark:good.dishRemark
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
    console.log(his.data.cartItems);
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
  }
});