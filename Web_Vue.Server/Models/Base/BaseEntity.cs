using Web_Vue.Server.Interfaces;

namespace Web_Vue.Server.Models.Base;

/// <summary> int 主鍵實體基底類別 (自動遞增) </summary>
public abstract class EntityBase : IEntity
{
    /// <summary> 主鍵 </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Comment("主鍵")]
    public int ID { get; set; }
}

/// <summary> int 主鍵 + 建立稽核實體基底類別 </summary>
public abstract class CreateAuditEntity : EntityBase, ICreateAudit
{
    /// <summary> 建立人員 </summary>
    [BindNever]
    [Comment("建立人員")]
    public string? CreateUser { get; set; }

    /// <summary> 建立時間 </summary>
    [BindNever]
    [Comment("建立時間")]
    public DateTime CreateTime { get; set; }
}

/// <summary> int 主鍵 + 建立稽核 + 更新稽核實體基底類別 (含軟刪除) </summary>
public abstract class AuditEntity : CreateAuditEntity, IUpdateAudit
{
    /// <summary> 更新人員 </summary>
    [BindNever]
    [Comment("更新人員")]
    public string? UpdateUser { get; set; }

    /// <summary> 更新時間 </summary>
    [BindNever]
    [Comment("更新時間")]
    public DateTime UpdateTime { get; set; }

    /// <summary> 是否已刪除 </summary>
    [BindNever]
    [Comment("是否已刪除")]
    public bool IsDelete { get; set; }
}


/// <summary> Guid 主鍵實體基底類別 </summary>
public abstract class StringEntityBase : IStringEntity
{
    /// <summary> 主鍵 </summary>
    [Key]
    [Comment("主鍵")]
    public Guid ID { get; set; }
}

/// <summary> Guid 主鍵 + 建立稽核實體基底類別 </summary>
public abstract class CreateAuditStringEntity : StringEntityBase, ICreateAudit
{
    /// <summary> 建立人員 </summary>
    [BindNever]
    [Comment("建立人員")]
    public string? CreateUser { get; set; }

    /// <summary> 建立時間 </summary>
    [BindNever]
    [Comment("建立時間")]
    public DateTime CreateTime { get; set; }
}

/// <summary> Guid 主鍵 + 建立稽核 + 更新稽核實體基底類別 (含軟刪除) </summary>
public abstract class AuditStringEntity : CreateAuditEntity, IUpdateAudit
{
    /// <summary> 更新人員 </summary>
    [BindNever]
    [Comment("更新人員")]
    public string? UpdateUser { get; set; }

    /// <summary> 更新時間 </summary>
    [BindNever]
    [Comment("更新時間")]
    public DateTime UpdateTime { get; set; }

    /// <summary> 是否已刪除 </summary>
    [BindNever]
    [Comment("是否已刪除")]
    public bool IsDelete { get; set; }
}
