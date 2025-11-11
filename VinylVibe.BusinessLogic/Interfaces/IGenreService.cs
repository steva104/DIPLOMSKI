using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.Models;

namespace VinylVibe.BusinessLogic.Interfaces
{
	public interface IGenreService
	{
		IEnumerable<Genre> GetAllGenres();
		Genre? GetGenreById(int id);
		bool CreateGenre(Genre genre, out string errorMessage);
		bool UpdateGenre(Genre genre, out string errorMessage);
		bool DeleteGenre(int id);


	}
}
