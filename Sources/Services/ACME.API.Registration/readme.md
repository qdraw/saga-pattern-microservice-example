
## Used environment variables

```
ConnectionStrings__RabbitMqConnection: rabbit mq
ConnectionStrings__RegistrationDbConnection: Registration db
```

## Run Migration
```bash
cd Sources/Services/ACME.API.Registration
dotnet ef migrations add SeedData
```



To clean database:

```sql
USE master;
GO
ALTER DATABASE AcmeRegistration 
SET SINGLE_USER 
WITH ROLLBACK IMMEDIATE;
GO
DROP DATABASE AcmeRegistration;
```
