namespace Web_Vue.Server.Entities.Interfaces;

/// <summary> 具有 int 型別主鍵的實體介面 </summary>
public interface IEntity
{
    /// <summary> 主鍵 </summary>
    int ID { get; set; }
}

/// <summary> 具有 Guid 型別主鍵的實體介面 </summary>
public interface IStringEntity
{
    /// <summary> 主鍵 (Guid) </summary>
    Guid ID { get; set; }
}

/// <summary> 建立稽核介面，記錄資料的建立人員與建立時間 </summary>
public interface ICreateAudit
{
    /// <summary> 建立人員 </summary>
    string? CreateUser { get; set; }

    /// <summary> 建立時間 </summary>
    DateTime CreateTime { get; set; }
}

/// <summary> 更新稽核介面，記錄資料的更新人員、更新時間及軟刪除狀態 </summary>
public interface IUpdateAudit
{
    /// <summary> 更新人員 </summary>
    string? UpdateUser { get; set; }

    /// <summary> 更新時間 </summary>
    DateTime UpdateTime { get; set; }

    /// <summary> 是否已刪除 </summary>
    bool IsDelete { get; set; }
}
