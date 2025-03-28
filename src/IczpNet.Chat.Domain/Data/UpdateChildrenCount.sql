

--[Chat_SessionPermissionGroup]
UPDATE [dbo].[Chat_SessionPermissionGroup] 
  SET [dbo].[Chat_SessionPermissionGroup].ChildrenCount = (
  SELECT COUNT(1) FROM [dbo].[Chat_SessionPermissionGroup] WHERE ParentId=x.Id
  )
  FROM [dbo].[Chat_SessionPermissionGroup] x


--[Chat_SessionOrganization]
UPDATE [dbo].[Chat_SessionOrganization] 
  SET [dbo].[Chat_SessionOrganization].ChildrenCount = (
  SELECT COUNT(1) FROM [dbo].[Chat_SessionOrganization] WHERE ParentId=x.Id
  )
  FROM [dbo].[Chat_SessionOrganization] x

--[Chat_ChatObjectCategory]
UPDATE [dbo].[Chat_ChatObjectCategory] 
  SET [dbo].[Chat_ChatObjectCategory].ChildrenCount = (
  SELECT COUNT(1) FROM [dbo].[Chat_ChatObjectCategory] WHERE ParentId=x.Id
  )
  FROM [dbo].[Chat_ChatObjectCategory] x


--[Chat_ChatObject]
UPDATE [dbo].[Chat_ChatObject] 
  SET [dbo].[Chat_ChatObject].ChildrenCount = (
  SELECT COUNT(1) FROM [dbo].[Chat_ChatObject] WHERE ParentId=x.Id
  )
  FROM [dbo].[Chat_ChatObject] x