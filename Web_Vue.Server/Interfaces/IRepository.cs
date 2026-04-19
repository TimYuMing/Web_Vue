namespace Web_Vue.Server.Interfaces;

/// <summary> Base Repository Interface (int 主鍵) </summary>
/// <typeparam name="T"></typeparam>
public interface IBaseRepository<T> where T : class
{
    /// <summary> 查詢全部 </summary>
    IQueryable<T> FindAll(bool withDelete = false);

    /// <summary> 取得單筆 </summary>
    Task<T> GetByIdAsync(int id, bool withDelete = false);

    /// <summary> 取得單筆 (AsNoTracking) [如果用ID是虛擬的呼叫 會掛掉] </summary>
    Task<T> GetByIdAsNoTrackingAsync(int id);

    /// <summary> 新增 </summary>
    Task<int> InsertAsync(T entity);

    /// <summary> 編輯 </summary>
    Task<int> UpdateAsync(T entity);

    /// <summary> 新增 OR 編輯 </summary>
    Task<int> UpsertAsync(T entity);

    /// <summary> 刪除 (自動判斷真刪或假刪，Entity 需繼承 IUpdateAudit) </summary>
    Task<int> DeleteAsync(T entity);

    /// <summary> 多筆刪除 (自動判斷真刪或假刪，Entity 需繼承 IUpdateAudit) </summary>
    Task<int> DeleteAsync(IQueryable<T> list);
}

/// <summary> Base Guid Repository Interface (Guid 主鍵) </summary>
/// <typeparam name="T"></typeparam>
public interface IBaseGuidRepository<T> where T : class
{
    /// <summary> 查詢全部 </summary>
    IQueryable<T> FindAll(bool withDelete = false);

    /// <summary> 取得單筆 </summary>
    Task<T> GetByIdAsync(Guid id, bool withDelete = false);

    /// <summary> 取得單筆 (AsNoTracking) [如果用Id是虛擬的呼叫 會掛掉] </summary>
    Task<T> GetByIdAsNoTrackingAsync(Guid id);

    /// <summary> 新增 </summary>
    Task<Guid> InsertAsync(T entity);

    /// <summary> 編輯 </summary>
    Task<Guid> UpdateAsync(T entity);

    /// <summary> 新增 OR 編輯 </summary>
    Task<Guid> UpsertAsync(T entity);

    /// <summary> 刪除 (自動判斷真刪或假刪，Entity 需繼承 IUpdateAudit) </summary>
    Task<Guid> DeleteAsync(T entity);
}

/// <summary> Base Multiple Repository Interface (複合主鍵) </summary>
/// <typeparam name="T"></typeparam>
public interface IBaseMultipleRepository<T> where T : class
{
    /// <summary> 查詢全部 </summary>
    IQueryable<T> FindAll();

    /// <summary> 查詢全部 (透過第一個ID) </summary>
    IQueryable<T> FindAllByAId(int aid);

    /// <summary> 查詢全部第二個ID (透過第一個ID) </summary>
    Task<List<int>> FindAllBIdByAIdAsync(int aid);

    /// <summary> 清除全部 (透過第一個ID) </summary>
    Task ClearByAIdAsync(int aid);

    /// <summary> 查詢全部 (透過第二個ID) </summary>
    IQueryable<T> FindAllByBId(int bid);

    /// <summary> 查詢全部第一個ID (透過第二個ID) </summary>
    Task<List<int>> FindAllAIdByBIdAsync(int bid);

    /// <summary> 清除全部 (透過第二個ID) </summary>
    Task ClearByBIdAsync(int bid);

    /// <summary> 批次新增資料 </summary>
    Task InsertRangeAsync(IEnumerable<T> dataList);

    /// <summary> 刪除 </summary>
    Task DeleteAsync(T entity);
}
