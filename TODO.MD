### 事项

- [ ] 推送添加消息内容,字段: message



### 邀请成员加群接口 /api/chat/room/invite

添加id   sessionUnitId

```
  // sessionUnitId
    id?:string;
```



登录添加 deviceType

```ts

type DeviceType 'pc' | 'phone' | 'web' | 'html5plus' | 'tablet' | 'pad' | 'webview'
```









### 在线连接池以及设备信息

- [x] 服务器异常关闭时，在线用户边界处理
- [x] 多设备、踢出设备, 上线/下线通知
- [x] ListSet 分布式索引（快速定位用户是否在线，以及在线设备[pc,phone,tablet,pad,web]）
- [x] 好友管理（上线/下线通知）



### 扫码登录 qr-login

- [ ] 扩展 OpenIddict 登录方式（qr-login）
- [ ] SignalR连接
- [ ] 扫码登录

### App版本管理

- [ ] 增加预发布
- [ ] 增加 Electron PC版App 的版本管理





