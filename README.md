# IM.Chat

abpvnext v6.0 chat module.



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

### 



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

- [x] **单用户绑定多个聊天对象**
- [x] 机器人
- [x] 官方号(Official)
- [x] 订阅号(Subscription)
- [x] 掌柜(ShopKeeper)
- [x] 店小二(ShopWaiter)
- [x] 聊天广场(Square)

### 聊天对象扩展(ChatObject)

- [x] 个性签名(Motto)
- [ ] 朋友圈



### 会话功能(Session)

- [x] 单聊
- [x] 群聊
- [x] 官方通知/功能号(Official)
- [x] 聊天广场
- [x] 订阅号/服务号
- [ ] 客服系统（CallCenter）



### 会话验证系统（好友、加群、加聊天广场）

- [x] 好友验证、处理好友验证消息
- [x] 加群验证，群主/管理员处理验证
- [ ] 设置验证方式(不需要验证、验证、自动拒绝验证)
- [ ] 自动验证



### 会话单元(SessionUnit)

- [x] 我的会话消息
- [x] 会话成员(群内成员，广场成员)
- [x] 共同的好友、共同所在的群/广场
- [x] 群/广场内名称
- [x] 备注好友/群
- [x] 会话置顶
- [x] 会话免打扰功能
- [x] 标记为已读
- [x] 删除会话消息
- [x] 清除会话消息
- [x] 会话角标
- [x] 删除会话（不退群）
- [x] 退出会话（退群）
- [x] 会话开启与停用(官方号、功能号)
- [x] 订阅与取消订阅（订阅号、服务号）
- [x] 设置聊天背景
- [ ] 只读会话（通知群、官方功能、公告等）



### 会话管理功能

  - [x] 会话生成器(SessionGenerator)
  - [ ] 会话盒子(SessionBox)

  - [x] 创建群聊
  - [x] 创建聊天广场
  - [x] 邀请成员加群
  - [ ] 二维码扫码加入群聊
  - [ ] 二维码扫码加入聊天广场
  - [x] 角色管理、角色权限分配

  - [x] 组织架构

  - [x] 角色管理、角色权限分配

  - [x] 权限管理

  - [x] 权限分组

  - [ ] 权限起用与禁用

  - [ ] 会话标签（SessionTag）-- 

  - [ ] 会话菜单功能

    



### 消息模板功能

  - [x] 系统消息(Command)
  - [x] 文本消息
  - [x] 图片消息
  - [x] 语音消息
  - [x] 视频消息
  - [x] 文件消息
  - [ ] 文章消息
  - [x] HTML消息
  - [x] 链接消息
  - [x] 名片消息
  - [x] HTML消息[H5、markdown]
  - [x] 位置消息（Location）
  - [ ] 公告消息（群公告、广场公告）
  - [ ] 红包消息



### 消息扩展功能

  - [x] 转发消息
  - [x] 群发消息
  - [x] 引用消息
  - [x] 消息提醒器
  - [ ] 消息收藏夹
  - [ ] 消息已读记录器(ReadedRecorder)
  - [ ] 消息打开记录器(OpenedRecorder)



### 客服系统（CallCenter）
  - [x] 子账号管理
  - [ ] 设置服务状态(挂起、接收、忙录等)
  - [ ] 会话转接功能
  - [ ] 消息分流功能
  - [ ] 客户发起会话(自动加入会话)



### 智能对话功能(机器人)

- [x] 机器人账号
- [x] 机器人主动发通知
- [ ] ChatGPT



### WebHook

- [ ] 权限验证（APIKey）

- [ ] ApiKey管理功能

- [ ] Api日志

  

### 扩展功能

  - [ ] 实现共享位置

  - [ ] 扫码登录

  - [ ] 文件服务器（文件预览）

    

