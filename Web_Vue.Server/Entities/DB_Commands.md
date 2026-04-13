# EF Core + PostgreSQL 資料庫操作指令

> 所有指令均在 **`Web_Vue.Server/`** 目錄下執行（即 `.csproj` 所在資料夾）。

---

## 專案 DbContext 說明

| 檔案 | 說明 |
|---|---|
| `Entities/DbEntityContext.cs` | `partial` — 存放 DbSet 與 OnModelCreating 邏輯 |
| `Entities/DbContextSetting.cs` | `partial` — 存放建構子、SaveChanges、稽核欄位自動填入邏輯 |

DbContext 類別名稱為 `DbEntityContext`，位於命名空間 `Web_Vue.Server.Entities`。

---

## 安裝 dotnet-ef 全域工具

```bash
dotnet tool install --global dotnet-ef
```

> 若已安裝但版本過舊，可更新：
> ```bash
> dotnet tool update --global dotnet-ef
> ```

---

## 連線字串設定

於 `appsettings.Development.json`（本機開發）或 `appsettings.json`（正式）調整：

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=web_vue;Username=postgres;Password=yourpassword"
}
```

---

## Migration 指令

> 本專案只有一個 DbContext（`DbEntityContext`），EF Core 會自動偵測，**不需要** `--context` 參數。  
> `--output-dir Entities/Migrations` 只需在**首次**建立時指定，之後 EF Core 會從既有 Migration 自動偵測輸出目錄。

### 初始化（首次建立，產生初始 Migration）

```bash
dotnet ef migrations add InitialCreate --output-dir Entities/Migrations
```

### 新增 Migration（產生異動腳本）

```bash
dotnet ef migrations add <MigrationName>
```

範例：

```bash
dotnet ef migrations add AddUserTable
```

### 套用 Migration（更新資料庫）

```bash
dotnet ef database update
```

> 套用至指定版本：
> ```bash
> dotnet ef database update <MigrationName>
> ```

### 移除最後一個尚未套用的 Migration

```bash
dotnet ef migrations remove
```

### 查看 Migration 清單

```bash
dotnet ef migrations list
```

### 還原所有 Migration（清空所有資料表結構）

```bash
dotnet ef database update 0
```

---

## 產生 SQL 腳本（不直接執行，可用於 DBA 審核）

### 產生全部 Migration 的 SQL

```bash
dotnet ef migrations script
```

### 產生從指定 Migration 開始的 SQL

```bash
dotnet ef migrations script <FromMigration> <ToMigration>
```

範例（產生部署到正式環境的腳本）：

```bash
dotnet ef migrations script 0 HEAD -o deploy.sql
```

---

## 其他常用指令

### 顯示目前 DbContext 資訊

```bash
dotnet ef dbcontext info
```

### 反向工程（從現有資料庫產生 Entity）

```bash
dotnet ef dbcontext scaffold "連線字串" Npgsql.EntityFrameworkCore.PostgreSQL -o Entities/Generated
```

---

## 軟刪除說明

本專案使用 **軟刪除** 機制（`IsDelete` 欄位），所有繼承 `AuditEntity` 的實體，
查詢時均自動套用 `WHERE "IsDelete" = false` 全域篩選（定義於 `DbEntityContext.cs` 的 `OnModelCreating`）。

若需要查詢已刪除的資料，請使用：

```csharp
dbContext.Users.IgnoreQueryFilters().Where(...);
```

---

## 稽核欄位說明

| 欄位         | 自動填入時機      | 說明                          |
|--------------|------------------|-------------------------------|
| CreateUser   | 新增 (Added)     | 當前登入使用者帳號（`UserInfoModel.Account`）|
| CreateTime   | 新增 (Added)     | UTC+8 台灣時間                 |
| UpdateUser   | 新增 / 修改      | 當前登入使用者帳號              |
| UpdateTime   | 新增 / 修改      | UTC+8 台灣時間                 |
| IsDelete     | 手動設為 true    | 軟刪除旗標，預設 false         |

> **注意**：修改資料時，`CreateUser` 與 `CreateTime` 不會被覆蓋。

---

## 基底實體繼承關係

```
EntityBase  (int PK)
    └── CreateAuditEntity   (+ CreateUser / CreateTime)
            └── AuditEntity        (+ UpdateUser / UpdateTime / IsDelete)

StringEntityBase  (Guid PK)
    └── CreateAuditStringEntity   (+ CreateUser / CreateTime)
            └── AuditStringEntity        (+ UpdateUser / UpdateTime / IsDelete)
```

使用時繼承對應的基底類別即可，例如：

```csharp
public class Product : AuditEntity
{
    public string Name { get; set; } = string.Empty;
}
```

---

## JWT 登入流程

1. `POST /api/auth/login` — 傳入 `{ account, password }`，取得 `token`
2. 後續請求在 Authorization Header 帶上：`Bearer <token>`
3. `GET /api/auth/me` — 回傳完整 `UserInfoModel`（包含帳號、姓名、角色清單等）

Swagger UI 可於右上角「Authorize」按鈕輸入 Token。
