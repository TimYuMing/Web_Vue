using System.Linq.Expressions;
using Web_Vue.Server.Models;
using Web_Vue.Server.Interfaces;

namespace Web_Vue.Server.Repositories.Base;

/// <summary> BaseRepository (int 主鍵) </summary>
/// <typeparam name="T"></typeparam>
public class BaseRepository<T> : IBaseRepository<T> where T : class, IEntity
{
    private readonly DbEntityContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary> 建構子 </summary>
    public BaseRepository(DbEntityContext context)
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
    public virtual async Task<T> GetByIdAsync(int id, bool withDelete = false)
    {
        var entity = await _dbSet.FindAsync(id);
        if (!withDelete && entity is IUpdateAudit { IsDelete: true })
        {
            return null;
        }
        return entity;
    }

    /// <summary> 取得單筆 (AsNoTracking) [如果用ID是虛擬的呼叫 會掛掉] </summary>
    public virtual async Task<T> GetByIdAsNoTrackingAsync(int id)
    {
        return await FindAll().Where(x => x.ID == id).AsNoTracking().FirstOrDefaultAsync();
    }

    /// <summary> 新增 </summary>
    public virtual async Task<int> InsertAsync(T entity)
    {
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity.ID;
    }

    /// <summary> 編輯 </summary>
    public virtual async Task<int> UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity.ID;
    }

    /// <summary> 新增 OR 編輯 </summary>
    public virtual async Task<int> UpsertAsync(T entity)
    {
        if (entity.ID == 0 || (await GetByIdAsync(entity.ID, true)) == null)
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
    public virtual async Task<int> DeleteAsync(T entity)
    {
        if (entity == null) return 0;

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

    /// <summary> 多筆刪除 (自動判斷真刪或假刪) </summary>
    public virtual async Task<int> DeleteAsync(IQueryable<T> list)
    {
        if (typeof(IUpdateAudit).IsAssignableFrom(typeof(T)))
        {
            foreach (var entity in list)
            {
                ((IUpdateAudit)entity).IsDelete = true;
            }
        }
        else
        {
            _context.RemoveRange(list);
        }
        return await _context.SaveChangesAsync();
    }
}
