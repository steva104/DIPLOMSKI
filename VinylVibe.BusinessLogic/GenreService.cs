using VinylVibe.BusinessLogic.Interfaces;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;

namespace VinylVibe.BusinessLogic
{
	public class GenreService : IGenreService
	{
		private readonly IUnitOfWork _unitOfWork;

		public GenreService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<Genre> GetAllGenres()
		{
			return _unitOfWork.Genre.GetAll().ToList();
		}

		public Genre? GetGenreById(int id)
		{
			return _unitOfWork.Genre.Get(u => u.Id == id);
		}

		public bool CreateGenre(Genre genre, out string errorMessage)
		{
			errorMessage = string.Empty;
			if (string.IsNullOrEmpty(genre.Name) || genre.Name.Any(char.IsDigit))
			{
				errorMessage = "The Genre Name can't contain numbers.";
				return false;
			}

			_unitOfWork.Genre.Add(genre);
			_unitOfWork.Save();
			return true;
		}

		public bool UpdateGenre(Genre genre, out string errorMessage)
		{
			errorMessage = string.Empty;
			if (string.IsNullOrEmpty(genre.Name) || genre.Name.Any(char.IsDigit))
			{
				errorMessage = "The Genre Name can't contain numbers.";
				return false;
			}

			_unitOfWork.Genre.Update(genre);
			_unitOfWork.Save();
			return true;
		}

		public bool DeleteGenre(int id)
		{
			var genre = _unitOfWork.Genre.Get(u => u.Id == id);
			if (genre == null) return false;

			_unitOfWork.Genre.Delete(genre);
			_unitOfWork.Save();
			return true;
		}
	}
}
