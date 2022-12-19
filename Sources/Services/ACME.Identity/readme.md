
To clean database:

```sql
USE master;
GO
ALTER DATABASE AcmeIdentity 
SET SINGLE_USER 
WITH ROLLBACK IMMEDIATE;
GO
DROP DATABASE AcmeIdentity;
```

# Default accounts:

## Backoffice
```
admin@qdraw.nl
Test123!@
```

## Contact
```
contact@qdraw.nl
Test123!@
```

## Run Migration
```bash
cd Sources/Services/ACME.Identity
dotnet ef migrations add StartupData
```
