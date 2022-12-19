

To clean database:

```sql
USE master;
GO
ALTER DATABASE AcmeNotifications 
SET SINGLE_USER 
WITH ROLLBACK IMMEDIATE;
GO
DROP DATABASE AcmeNotifications;
```



## Run Migration
```bash
cd Sources/Services/ACME.API.Notifications
dotnet ef migrations add StartupData
```
