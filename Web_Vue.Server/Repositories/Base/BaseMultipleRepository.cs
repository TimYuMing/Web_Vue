using Web_Vue.Server.Models;
using Web_Vue.Server.Interfaces;

namespace Web_Vue.Server.Repositories.Base;

/// <summary> BaseMultipleRepository (複合主鍵) </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseMultipleRepository<T> : IBaseMultipleRepository<T> where T : class
{
    /// <summary> Context </summary>
    protected readonly DbEntityContext Context;
    private readonly DbSet<T> _dbSet;

    /// <summary> 建構子 </summary>
    protected BaseMultipleRepository(DbEntityContext context)
    {
        Context = context;
        _dbSet = Context.Set<T>();
    }

    /// <summary> 查詢全部 </summary>
    public virtual IQueryable<T> FindAll()
    {
        return _dbSet;
    }

    /// <summary> 查詢全部 (透過第一個ID) </summary>
    public abstract IQueryable<T> FindAllByAId(int aid);

    /// <summary> 查詢全部第二個ID (透過第一個ID) </summary>
    public abstract Task<List<int>> FindAllBIdByAIdAsync(int aid);

    /// <summary> 查詢全部 (透過第二個ID) </summary>
    public abstract IQueryable<T> FindAllByBId(int bid);

    /// <summary> 查詢全部第一個ID (透過第二個ID) </summary>
    public abstract Task<List<int>> FindAllAIdByBIdAsync(int bid);

    /// <summary> 清除全部 (透過第一個ID) </summary>
    public abstract Task ClearByAIdAsync(int aid);

    /// <summary> 清除全部 (透過第二個ID) </summary>
    public abstract Task ClearByBIdAsync(int bid);

    /// <summary> 批次新增資料 </summary>
    public abstract Task InsertRangeAsync(IEnumerable<T> dataList);

    /// <summary> 刪除 </summary>
    public virtual async Task DeleteAsync(T entity)
    {
        if (entity != null)
        {
            if (Context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            Context.Remove(entity);
            await Context.SaveChangesAsync();
        }
    }
}
