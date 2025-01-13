# IM.Chat

abpvnext  chat module.

## Docker build

```bash
docker build -t iczpnet/chat-auth-server:v0 -f ./host/IczpNet.Chat.AuthServer/Dockerfile .
docker build -t iczpnet/chat-api-host:v0 -f ./host/IczpNet.Chat.HttpApi.Host/Dockerfile .
```

## Startup

```
abp new IczpNet.Chat -t module --no-ui 
```

### Set Startup object: IczpNet.Chat.AuthServer

```
Update-Database
```

### Message

| SenderId | ReceiverId | SessionId |
| -------- | ---------- | --------- |
| C1       | S1.1       | S1-C1     |
| s2       | C2         | S-C2      |
|          |            |           |

## 项目的需求

### 需求

### ss

dasfasdfwftc

- ddaf

## 在线状态

### 在线状态管理 `IServiceStateManager`

1. 群/公众号 解析为 null
2. 人/Anonymous/ShopWaiter/Customer 解析为 [Online | Offline]
3. Shopper（自己或是子账号任何一下在线就解析为 [Online ]），都不在线都解析为【Offline】

## Internal structure 数据结构

### Message 消息表

### ChatObject 聊天对象(参与会话)

#### Persion 人(用户、员工)

#### Customer 人（客户）

#### Room 群

#### Official 公众号

#### ----OfficialGroup 公众号分组（人群定义、临时分组等）

#### Robot 机器人（智能对话）

#### Shopkeeper 电商掌柜(主账号)

#### ShopWaiter 店小二(子账号)

#### Square 广场

## Session

## Room

### CreateRoom

```json
{
  "name": "9999999999999999999999999",
  "code": "string",
  "ownerId": null,
  "type": 0,
  "description": "string",
  "chatObjectIdList": [
    "31F8B124-BEA6-85C4-B4E3-3A07BB313745","329A7C43-9B01-5966-322A-3A07D4A342EA","5B6A6100-CA52-8040-09F2-3A07D4A367ED","972157FA-2449-FEC6-49A1-3A07DE609F58",
  ]
}
```

## 待实现功能

### 以前要求未实现的功能

## Roadmap

### 聊天对象(ChatObject)

- [X] **单用户绑定多个聊天对象**
- [X] 机器人
- [X] 官方号(Official)
- [X] 订阅号(Subscription)
- [X] 掌柜(ShopKeeper)
- [X] 店小二(ShopWaiter)
- [X] 聊天广场(Square)
- [X] 匿名(Anonymous)
- [X] 动态信息添加（添加手机，QQ，职位等）

### 聊天对象扩展(ChatObject)

- [X] 个性签名(Motto)
- [ ] 朋友圈

### 会话功能(Session)

- [X] 单聊
- [X] 群聊
- [X] 官方通知/功能号(Official)
- [X] 聊天广场
- [X] 订阅号/服务号
- [ ] 客服系统（CallCenter）

### 会话验证系统（好友、加群、加聊天广场）

- [X] 好友验证、处理好友验证消息
- [X] 加群验证，群主/管理员处理验证
- [X] 设置验证方式(不需要验证、验证、自动拒绝验证)
- [X] 自动验证

### 会话单元(SessionUnit)

- [X] 我的会话消息
- [X] 会话成员(群内成员，广场成员)
- [X] 共同的好友、共同所在的群/广场
- [X] 群/广场内名称
- [X] 备注好友/群
- [X] 会话置顶
- [X] 会话免打扰功能
- [X] 标记为已读
- [X] 删除会话消息
- [X] 清除会话消息
- [X] 会话角标
- [X] 删除会话（不退群）
- [X] 退出会话（退群）
- [X] 会话开启与停用(官方号、功能号)
- [X] 订阅与取消订阅（订阅号、服务号）
- [X] 设置聊天背景
- [X] 只读会话（通知群、官方功能、公告等）
- [X] 【新消息】角标统计
- [X] 【私有消息】角标统计
- [X] 【特别关注】角标统计
- [X] 【@我】角标统计
- [X] 动态属性备注（添加手机，QQ，职位等）

### 会话管理功能

- [X] 会话生成器(SessionGenerator)
- [ ] 会话盒子(SessionBox)
- [X] 创建聊天广场
- [ ] 二维码扫码加入群聊
- [ ] 二维码扫码加入聊天广场
- [X] 角色管理、角色权限分配
- [X] 组织架构
- [X] 角色管理、角色权限分配
- [X] 会话内权限（组织、角色、加人，踢人等）
- [X] 权限分组
- [ ] 权限启用与禁用
- [ ] 会话标签（SessionTag）--
- [X] 会话菜单功能
- [ ] “拍一拍”
- [X] @所有人、@XXX
- [ ] 禁言（管理员、群主）
- [X] 是否允许设置“免打扰”

### 群聊功能

- [X] 创建群聊
- [X] 邀请成员入群
- [X] 邀请成员加群
- [X] 群主权限（拥有所有权限）
- [X] 群角色功能（可配置多角色）
- [X] 群成员权限（可单一配置成员权限）
- [X] [权限]更新群名称并通知群成员
- [X] [权限]更新头头像并通知群成员
- [X] 转让群主
- [X] 共同所在的群
- [X] 加群验证（验证消息由【机器人/群助手】发送到【群主或管理员】）
- [X] 管理员/群主处理验证
- [X] 添加/取消【特别关注】（关注群成员，有新消息时标注为【特别关注】）
- [X] 入群方式设置
- [ ] 二维码扫码加入群聊
- [ ] 面对面加群
- [ ] 邀请码加群
- [X] 群内组织架构
- [ ] 群内公告
- [ ] 群机器人提醒功能
- [ ] 设置是否可以转发群里消息
- [ ] 设置新成员“默认角色”
- [X] 设置新成员“历史消息查看范围”

### 聊天广场

- [X] 创建聊天广场
- [X] 广场成员（分组）
- [ ] 特别关注（关注群成员，有新消息时标注为【特别关注】）
- [ ] 设置广场进入方式（公有、私有）
- [ ] 广场公告
- [ ] 机器人提醒功能

### 单聊天功能

- [ ] 设置“能过账号找到我”
- [ ] 设置“通过手机号找到我”
- [X] 设置“好友验证方式”
- [ ] 设置是否群内加好友

### 官方号功能号(Official)

- [X] 启用与停用功能
- [X] 设置是否可以停用
- [X] 设置是否为只读（通知功能 ，成员不能发消息，只能接收消息）

### 订阅号功能

- [X] 订阅与取消订阅（同时发会话内私有通知）
- [ ] 设置是否可以停用
- [X] 设置是否为只读（通知功能 ，成员不能发消息，只能接收消息）

#### 掌柜与店小二

- [X] 创建子账号
- [ ] 子账号管理（开启、禁用、删除）
- [X] 消息同步
- [ ] 消息转接
- [ ] 账号状态设置（挂起、忙录）

### 消息模板功能

- [X] 系统消息(Command)
- [X] 文本消息
- [X] 图片消息
- [X] 语音消息
- [X] 视频消息
- [X] 文件消息
- [ ] 文章消息
- [X] HTML消息
- [X] 链接消息
- [X] 名片消息
- [X] HTML消息[H5、markdown]
- [X] 位置消息（Location）
- [X] 历史聊天记录
- [ ] 公告消息（群公告、广场公告）
- [X] 红包消息（未实现支付功能，只能积分形式收发红包）

### 消息扩展功能

- [X] 转发消息
- [X] 群发消息
- [X] 引用消息
- [X] 消息提醒器
- [X] 消息收藏夹
- [X] 消息已读记录器(ReadedRecorder)
- [X] 消息打开记录器(OpenedRecorder)
- [ ] **敏感词过滤\审核**
- [ ] Elasticsearch（elastic.co）

### 客服系统（CallCenter）

- [X] 子账号管理
- [ ] 设置服务状态(挂起、接收、忙录等)
- [ ] 会话转接功能
- [ ] 消息分流功能
- [ ] 客户发起会话(自动加入会话)

### 智能对话功能(机器人)

- [X] 机器人账号
- [X] 机器人主动发通知
- [ ] ChatGPT
- [ ] Google Gemini

### 开发者功能

- [X] 开发者设置（Token、EncodingAesKey、PostUrl）
- [X] 开启与关闭功能
- [X] 验证及验签（signature）
- [X] Http请求开发者服务日志
- [X] 后台作业调用开发者及重试功能
- [ ] 验证开发者主机（HOST）
- [ ] 开发者Demo
- [ ] 开发者SDK

### WebHook

- [ ] 权限验证（APIKey）
- [ ] ApiKey管理功能
- [ ] Api日志

### 扩展功能

- [ ] 实现共享位置
- [ ] 扫码登录
- [ ] 文件服务器（文件预览）
