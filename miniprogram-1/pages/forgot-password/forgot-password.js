// pages/forgot-password/forgot-password.js
Page({
  data: {
    username: '',
    phone: '',
    newPassword: '',
    confirmPassword: '',
    loading: false
  },

  onLoad() {
    // 页面加载逻辑
  },

  // 输入处理
  onUsernameInput(e) {
    this.setData({ username: e.detail.value });
  },

  onPhoneInput(e) {
    this.setData({ phone: e.detail.value });
  },

  onNewPasswordInput(e) {
    this.setData({ newPassword: e.detail.value });
  },

  onConfirmPasswordInput(e) {
    this.setData({ confirmPassword: e.detail.value });
  },

  // 重置密码
  async handleResetPassword() {
    const { username, phone, newPassword, confirmPassword } = this.data;

    // 表单验证
    if (!this.validateForm()) {
      return;
    }

    this.setData({ loading: true });

    try {
      const result = await this.requestResetPassword({
        username: username,
        phone: phone,
        newPassword: newPassword,
        confirmPassword: confirmPassword
      });

      if (result.success) {
        wx.showToast({ 
          title: '密码重置成功', 
          icon: 'success',
          duration: 2000
        });

        // 延迟返回登录页
        setTimeout(() => {
          wx.navigateTo({ 
            url: '/pages/login/login?username=' + username
          });
        }, 2000);
      } else {
        wx.showToast({ 
          title: result.message || '重置密码失败', 
          icon: 'none' 
        });
      }
    } catch (error) {
      console.error('重置密码失败:', error);
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
    const { username, phone, newPassword, confirmPassword } = this.data;

    if (!username) {
      wx.showToast({ title: '请输入用户名', icon: 'none' });
      return false;
    }

    if (!phone) {
      wx.showToast({ title: '请输入手机号', icon: 'none' });
      return false;
    }

    if (!/^1[3-9]\d{9}$/.test(phone)) {
      wx.showToast({ title: '请输入正确的手机号', icon: 'none' });
      return false;
    }

    if (!newPassword) {
      wx.showToast({ title: '请输入新密码', icon: 'none' });
      return false;
    }

    if (newPassword.length < 6) {
      wx.showToast({ title: '密码至少6位', icon: 'none' });
      return false;
    }

    if (newPassword !== confirmPassword) {
      wx.showToast({ title: '两次密码不一致', icon: 'none' });
      return false;
    }

    return true;
  },

  // API请求：重置密码
  async requestResetPassword(data) {
    return new Promise((resolve, reject) => {
      wx.request({
        url: 'http://100.80.24.218:5002/api/customer/reset-password',
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

  // 返回登录页
  goToLogin() {
    wx.navigateTo({ url: '/pages/login/login' });
  }
});