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



## SessionUnit 会话单元

1. chatMember 可以不用了,取消群成员、订阅号成员，替代方案 sessionUnit ，同时用于服务号、聊天广场、店铺掌柜、店小二

2. 店铺消息分流

3. chatObject加children 子账号，启用shopKeeper,ShopWaiter

4. channels改为person-person
  person-room
  room-person
  person - shopkeeper

5. 

  sessionTag，role

  history last time=null

  KillSession  删除消息会话,不退群, killTime

  
  clearMessage 清空消息，不退群，clearTime 或MessageAutoId

  
  removeSession 删除消息会话,不退群

  
  KillSession  退出群，IsKilled，killTime

  
  KillSession  退出群，不删除会话，用于查看历史IsKilled，killTime，hisoryLastTime

  
  deleteMessages 用于删除文件助手消息

  
    channel要入库 chatFuctions：sendSound，vite,remind@everyone

  
  session ,全员禁言 is read only

  
  provider公众号菜单

  
  会话类型sessionType 与OwnerId

  
  removeSession 删除消息会话,不退群 removetime

### Session

## 待实现功能

### 以前要求未实现的功能
