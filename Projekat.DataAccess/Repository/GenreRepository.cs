using VinylVibe.DataAccess.Data;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace VinylVibe.DataAccess.Repository
{
    public class GenreRepository : Repository<Genre>, IGenreRepository
    {

        private ApplicationDbContext _db;
        public GenreRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

       

        public void Update(Genre genre)
        {
           _db.Genres.Update(genre);
        }
    }
}
