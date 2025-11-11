using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VinylVibe.DataAccess.Data;
using VinylVibe.DataAccess.Repository.IRepository;

namespace VinylVibe.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> DbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.DbSet=_db.Set<T>();
            _db.Products.Include(x => x.Genre);
        }

        public void Add(T entity)
        {
           DbSet.Add(entity);
        }

        public void Delete(T entity)
        {
           DbSet.Remove(entity);
        }

        public void DeleteMultiple(IEnumerable<T> entity)
        {
            DbSet.RemoveRange(entity);
        }

        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked=false)
        {
            IQueryable<T> query;
            if (tracked)
            {
                 query = DbSet;
               
            }
            else
            {
                query = DbSet.AsNoTracking();
            }

            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }

        //public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties=null)
        //{
        //    IQueryable<T> query = DbSet;
        //    if (filter != null)
        //    {
        //        query = query.Where(filter);
        //    }
        //    if (!string.IsNullOrEmpty(includeProperties))
        //    {
        //        foreach(var includeProp in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            query = query.Include(includeProp);
        //        }
        //    }
        //    return query.ToList();  
        //}
        public IQueryable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> query = DbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query;
        }
    }
}
