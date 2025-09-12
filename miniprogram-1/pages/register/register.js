// pages/register/register.js
Page({
  data: {
    phone: '',
    password: '',
    confirmPassword: '',
    loading: false
  },

  onLoad() {
    // 页面加载逻辑
  },

  // 输入处理
  onPhoneInput(e) {
    this.setData({ phone: e.detail.value });
  },

  onPasswordInput(e) {
    this.setData({ password: e.detail.value });
  },

  onConfirmPasswordInput(e) {
    this.setData({ confirmPassword: e.detail.value });
  },

  // 注册
  async handleRegister() {
    const { phone, password, confirmPassword } = this.data;

    // 表单验证
    if (!this.validateForm()) {
      return;
    }

    this.setData({ loading: true });

    try {
      const result = await this.requestRegister({
        phone: phone,
        password: password,
        confirmPassword: confirmPassword
      });

      if (result.success) {
        wx.showToast({ 
          title: '注册成功', 
          icon: 'success',
          duration: 2000
        });

        // 延迟跳转
        setTimeout(() => {
          wx.navigateTo({ 
            url: '/pages/login/login?phone=' + phone
          });
        }, 2000);
      } else {
        wx.showToast({ 
          title: result.message || '注册失败', 
          icon: 'none' 
        });
      }
    } catch (error) {
      console.error('注册失败:', error);
      wx.showToast({ 
        title: '网络错误，请重试', 
        icon: 'none' 
      });
    } finally {
      this.setData({ loading: false });
    }
  },

  // 表单验证
  validateForm() {
    const { phone, password, confirmPassword } = this.data;

    if (!phone) {
      wx.showToast({ title: '请输入手机号', icon: 'none' });
      return false;
    }

    if (!/^1[3-9]\d{9}$/.test(phone)) {
      wx.showToast({ title: '请输入正确的手机号', icon: 'none' });
      return false;
    }

    if (!password) {
      wx.showToast({ title: '请输入密码', icon: 'none' });
      return false;
    }

    if (password.length < 6) {
      wx.showToast({ title: '密码至少6位', icon: 'none' });
      return false;
    }

    if (password !== confirmPassword) {
      wx.showToast({ title: '两次密码不一致', icon: 'none' });
      return false;
    }

    return true;
  },

  // API请求：注册
  async requestRegister(data) {
    return new Promise((resolve, reject) => {
      wx.request({
        url: 'http://localhost:5002/api/customer/register',
        method: 'POST',
        data: data,
        header: {
          'content-type': 'application/json'
        },
        success: (res) => {
          if (res.statusCode === 200) {
            resolve(res.data);
          } else {
            reject(new Error(`请求失败: ${res.statusCode}`));
          }
        },
        fail: (err) => {
          reject(new Error('网络连接失败'));
        }
      });
    });
  },

  // 跳转到登录页
  goToLogin() {
    wx.navigateTo({ url: '/pages/login/login' });
  }
});