# 🔐 WeChat配置说明

## 🎯 **概述**

本文档说明如何配置项目中的WeChat参数，包括AppID、AppSecret和AccessToken。

## ⚠️ **重要提醒**

- **不要将真实的AppSecret和AccessToken提交到代码仓库**
- **这些是敏感信息，泄露可能导致安全问题**
- **建议使用环境变量或配置文件来管理这些参数**

## 🔧 **配置文件位置**

需要配置的文件：`ConsoleApp1/appsettings.json`

## 📋 **配置项说明**

### **1. AppID (小程序ID)**

```json
"AppId": "YOUR_APP_ID_HERE"
```

**获取方式**：
1. 登录微信公众平台：https://mp.weixin.qq.com/
2. 进入"开发" → "开发管理" → "开发设置"
3. 在"开发者ID"部分找到"AppID(小程序ID)"

**示例**：
```json
"AppId": "wx1234567890abcdef"
```

### **2. AppSecret (小程序密钥)**

```json
"AppSecret": "YOUR_APP_SECRET_HERE"
```

**获取方式**：
1. 在同一个"开发设置"页面
2. 找到"AppSecret(小程序密钥)"
3. 点击"重置"按钮生成新的密钥

**示例**：
```json
"AppSecret": "abcdef1234567890abcdef1234567890"
```

### **3. AccessToken (访问令牌)**

```json
"AccessToken": "YOUR_ACCESS_TOKEN_HERE"
```

**获取方式**：
1. 使用AppID和AppSecret调用微信API
2. 访问：`https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=YOUR_APP_ID&secret=YOUR_APP_SECRET`

**API调用示例**：
```bash
curl "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=wx1234567890abcdef&secret=abcdef1234567890abcdef1234567890"
```

**返回结果**：
```json
{
    "access_token": "95_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "expires_in": 7200
}
```

## 🚀 **配置步骤**

### **步骤1：获取WeChat信息**

1. 登录微信公众平台
2. 获取AppID和AppSecret
3. 使用API获取AccessToken

### **步骤2：更新配置文件**

编辑 `ConsoleApp1/appsettings.json`：

```json
{
  "WeChat": {
    "AppId": "你的实际AppID",
    "AppSecret": "你的实际AppSecret",
    "AccessToken": "你的实际AccessToken"
  }
}
```

### **步骤3：验证配置**

1. 启动后端服务：`dotnet run`
2. 访问二维码测试页面：`http://localhost:5002/qrcode-test.html`
3. 尝试生成二维码，确认配置正确

## 🔍 **常见问题**

### **问题1：AccessToken过期**

AccessToken有效期为2小时，过期后需要重新获取。

**解决方案**：
1. 重新调用获取AccessToken的API
2. 更新配置文件中的AccessToken值

### **问题2：AppSecret错误**

**解决方案**：
1. 检查AppSecret是否正确复制
2. 如果怀疑泄露，可以在微信公众平台重置AppSecret

### **问题3：权限不足**

**解决方案**：
1. 确认小程序已发布到对应版本
2. 检查开发者权限设置
3. 验证AppID是否正确

## 📱 **小程序项目配置**

同时需要更新小程序项目的配置：

### **更新 project.config.json**

```json
{
  "appid": "你的实际AppID"
}
```

### **更新 app.js (如果需要)**

```javascript
App({
  globalData: {
    appId: '你的实际AppID'
  }
})
```

## 🛡️ **安全建议**

1. **环境变量**：考虑使用环境变量存储敏感信息
2. **配置文件**：不要将包含真实信息的配置文件提交到版本控制
3. **定期更新**：定期更新AppSecret和AccessToken
4. **权限控制**：限制对配置文件的访问权限

## 📋 **配置检查清单**

- [ ] AppID已正确配置
- [ ] AppSecret已正确配置
- [ ] AccessToken已获取并配置
- [ ] 小程序项目配置已更新
- [ ] 后端服务能正常启动
- [ ] 二维码生成功能正常

## 🎉 **配置完成标志**

当以下条件都满足时，说明配置完成：

1. ✅ 配置文件中的占位符已替换为真实值
2. ✅ 后端服务正常启动
3. ✅ 二维码生成成功
4. ✅ 小程序能正常扫码启动
5. ✅ 桌台参数正确传递

---

**总结**：完成这些配置后，你就可以正常使用二维码生成和扫码功能了！🚀
