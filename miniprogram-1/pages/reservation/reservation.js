Page({
  data: {
    shops: ["店铺A"],
    shopIndex: 0,

    date: "2025-08-10",
    startDate: "2025-08-10",
    endDate: "2025-12-31",

    timeSlots: ["中午", "晚上"],
    timeIndex: 0,

    durationSlots: [30, 60,90,120,150,180],
    durationIndex: 0,

    // 人数：1人到12人 + 12人以上
    peopleNumbers: Array.from({ length: 12 }, (_, i) => `${i + 1}人`).concat("12人以上"),
    peopleIndex: 0,

    name: "",
    phone: "",
    remark: ""
  },

  onShopChange(e) {
    this.setData({ shopIndex: e.detail.value });
  },

  onTableChange(e) {
    this.setData({ tableIndex: e.detail.value });
  },

  onDateChange(e) {
    this.setData({ date: e.detail.value });
  },

  onTimeChange(e) {
    this.setData({ timeIndex: e.detail.value });
  },

  onDurationChange(e) {
    this.setData({ durationIndex: e.detail.value });
  },

  onPeopleChange(e) {
    this.setData({ peopleIndex: e.detail.value });
  },

  onNameInput(e) {
    this.setData({ name: e.detail.value });
  },

  onPhoneInput(e) {
    this.setData({ phone: e.detail.value });
  },

  onRemarkInput(e) {
    this.setData({ remark: e.detail.value });
  },
  

  onSubmit() {
    userinfo = wx.getStorageSync('customerId');
    console.log(userinfo);
    if (!this.data.name.trim()) {
      wx.showToast({ title: '请输入姓名', icon: 'none' });
      return;
    }
    if (!/^1\d{10}$/.test(this.data.phone)) {
      wx.showToast({ title: '请输入正确手机号', icon: 'none' });
      return;
    }
  
    const mealTimeMap = { "中午": 0, "晚上": 1 };
    const mealTimeValue = mealTimeMap[this.data.timeSlots[this.data.timeIndex]];
    const reservationTime = `${this.data.date}T${mealTimeValue === 0 ? "12:00:00" : "18:00:00"}`;
  
    const reservationData = {
      customerID: 23,
      customerName: this.data.name,
      contactPhone: this.data.phone,
      partySize: parseInt(this.data.peopleNumbers[this.data.peopleIndex]),
      reservationTime,
      expectedDuration: this.data.durationSlots[this.data.durationIndex],
      notes: this.data.remark || "",
      mealTime: mealTimeValue
    };
  
    console.log("提交数据：", reservationData);
    this.postReservation(reservationData);
 
  },

  postReservation(postData){
    const backendApiUrl ='http://localhost:5002/api/TableReservation';  
    wx.request({
      url: backendApiUrl,
      method: 'POST',
      header: {
        'Content-Type': 'application/json'
      },
      data: postData,
      success: (res) => {
        // HTTP状态码200或201通常代表成功
        if (res.statusCode == 200 || res.statusCode == 201) {
          console.log('预约提交，后端返回:', res.data);
                   
          // 订单创建成功后，发起GET请求
            this.getResponse(res.data.data);   
          
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

  getResponse(response){
    
    // 跳转并传递数据
    wx.navigateTo({
      url: `/pages/reservation/reserve-success?data=${encodeURIComponent(JSON.stringify(response))}`
    
    })
  
  }
  
});
