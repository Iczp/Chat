# IczpNet.Chat 文档说明

1. 数据结构
2. [在线状态](./online.md)



推送

```json
{
  appUserId: "360cfedb-e92d-3331-1fad-3a086371e0e4",
  scopes: [
    { chatObjectId: 13, sessionUnitId: "c9c166f8-6c5b-9e26-94fd-3a118ae7014f" },
    { chatObjectId: 14, sessionUnitId: "fb885b6b-9f6d-18a7-c877-3a118ae70142" },
    { chatObjectId: 5862, sessionUnitId: "08dbc5ed-24a1-4493-dc7d-3a118ae70150" }
  ],
  command: "Chat",
  payload: {
    content: { text: "55", id: "62d463c9-1533-4f3a-d496-3a1877287e8d" },
    id: 7276159,
    senderName: "江芳宜",
    messageType: 0,
    reminderType: null,
    isPrivate: false,
    isRollbacked: false,
    isRemindAll: false,
    rollbackTime: null,
    creationTime: "2025-03-05T14:26:08.6605257+08:00",
    senderSessionUnit: {
      id: "fb885b6b-9f6d-18a7-c877-3a118ae70142",
      ownerId: 14,
      ownerObjectType: 1,
      displayName: null,
      memberName: null,
      owner: {
        chatObjectTypeId: null,
        code: "101661",
        description: "我是江芳宜",
        gender: 0,
        thumbnail: "/file?id=5f06565b-6342-c5ec-1978-3a110b70d7ca",
        portrait: "/file?id=4ab08a5d-1c9a-a959-409b-3a110b70d7ca",
        appUserId: "360cfedb-e92d-3331-1fad-3a086371e0e4",
        isPublic: false,
        isEnabled: true,
        isDefault: false,
        objectType: 1,
        serviceStatus: 1,
        id: 14,
        parentId: null,
        name: "江芳宜",
        depth: 0,
        childrenCount: 0,
        fullPath: "14",
        fullPathName: "江芳宜"
      },
      tagList: []
    }
  }
};
```

```json
{
  appUserId: "360cfedb-e92d-3331-1fad-3a086371e0e4",
  scopes: [
    { chatObjectId: 13, sessionUnitId: "c9c166f8-6c5b-9e26-94fd-3a118ae7014f" },
    { chatObjectId: 14, sessionUnitId: "fb885b6b-9f6d-18a7-c877-3a118ae70142" },
    { chatObjectId: 5862, sessionUnitId: "08dbc5ed-24a1-4493-dc7d-3a118ae70150" }
  ],
  command: "IncrementCompleted",
  payload: { messageId: 7276159 }
};

```

