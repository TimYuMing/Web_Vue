using System.Linq.Expressions;
using Web_Vue.Server.Models;
using Web_Vue.Server.Interfaces;

namespace Web_Vue.Server.Repositories.Base;

/// <summary> BaseGuidRepository (Guid 主鍵) </summary>
/// <typeparam name="T"></typeparam>
public class BaseGuidRepository<T> : IBaseGuidRepository<T> where T : class, IStringEntity
{
    private readonly DbEntityContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary> 建構子 </summary>
    public BaseGuidRepository(DbEntityContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary> 查詢全部 </summary>
    public virtual IQueryable<T> FindAll(bool withDelete = false)
    {
        IQueryable<T> query = _dbSet;

        if (!withDelete && typeof(IUpdateAudit).IsAssignableFrom(typeof(T)))
        {
            var param = Expression.Parameter(typeof(T), "e");
            var left = Expression.Property(param, typeof(T).GetProperty(nameof(IUpdateAudit.IsDelete))!);
            var right = Expression.Constant(false);
            var filter = Expression.Equal(left, right);
            var lambda = (Expression<Func<T, bool>>)Expression.Lambda(filter, param);
            query = query.Where(lambda);
        }

        return query;
    }

    /// <summary> 取得單筆 </summary>
    public virtual async Task<T> GetByIdAsync(Guid id, bool withDelete = false)
    {
        var entity = await _dbSet.FindAsync(id);
        if (!withDelete && entity is IUpdateAudit { IsDelete: true })
        {
            return null;
        }
        return entity;
    }

    /// <summary> 取得單筆 (AsNoTracking) [如果用ID是虛擬的呼叫 會掛掉] </summary>
    public virtual async Task<T> GetByIdAsNoTrackingAsync(Guid id)
    {
        return await FindAll().Where(x => x.ID == id).AsNoTracking().FirstOrDefaultAsync();
    }

    /// <summary> 新增 </summary>
    public virtual async Task<Guid> InsertAsync(T entity)
    {
        if (entity.ID == Guid.Empty)
        {
            entity.ID = Guid.NewGuid();
        }
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity.ID;
    }

    /// <summary> 編輯 </summary>
    public virtual async Task<Guid> UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity.ID;
    }

    /// <summary> 新增 OR 編輯 </summary>
    public virtual async Task<Guid> UpsertAsync(T entity)
    {
        if (entity.ID == Guid.Empty || (await GetByIdAsync(entity.ID, true)) == null)
        {
            await InsertAsync(entity);
        }
        else
        {
            await UpdateAsync(entity);
        }
        return entity.ID;
    }

    /// <summary> 刪除 (自動判斷真刪或假刪) </summary>
    public virtual async Task<Guid> DeleteAsync(T entity)
    {
        if (entity == null) return Guid.Empty;

        if (entity is IUpdateAudit audit)
        {
            audit.IsDelete = true;
        }
        else
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _context.Remove(entity);
        }
        await _context.SaveChangesAsync();
        return entity.ID;
    }
}
