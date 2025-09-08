Page({
  data: {
    tableNumber: "",
    area: "",
    reservationID: ""
  },
  onLoad(options) {
    if (options.data) {
      const resData = JSON.parse(decodeURIComponent(options.data));
      this.setData({
        tableNumber: resData.tableNumber,
        area: resData.area,
        reservationID: resData.reservationID
      });
    }
  },
  goHome() {
    wx.reLaunch({
      url: '/pages/index/index' // 首页路径
    });
  }
});
