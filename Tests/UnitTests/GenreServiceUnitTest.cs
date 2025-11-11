using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using VinylVibe.BusinessLogic;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;

namespace Tests
{
    public class GenreServiceUnitTest
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IGenreRepository> _mockGenreRepo;
        private GenreService _genreService;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockGenreRepo = new Mock<IGenreRepository>();


            _mockUnitOfWork.Setup(u => u.Genre).Returns(_mockGenreRepo.Object);


            _genreService = new GenreService(_mockUnitOfWork.Object);
        }
        #region GetAllGenresTests
        [Test]
        public void GetAllGenres_WhenGenresExist_ShouldReturnAllGenres()
        {

            var expectedGenres = new List<Genre>
        {
            new Genre { Id = 1, Name = "Rock" },
            new Genre { Id = 2, Name = "Jazz" },
            new Genre { Id = 3, Name = "Pop" }
        };

            _mockGenreRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Genre, bool>>>(),
                It.IsAny<string>()))
                .Returns((Expression<Func<Genre, bool>> filter, string includeProperties) =>
                    expectedGenres.AsQueryable());

            var result = _genreService.GetAllGenres();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result.Select(g => g.Name), Is.EquivalentTo(new[] { "Rock", "Jazz", "Pop" }));

            _mockGenreRepo.Verify(r => r.GetAll(null, null), Times.Once);
        }

        [Test]
        public void GetAllGenres_WhenNoGenresExist_ShouldReturnEmptyList()
        {

            var emptyGenres = new List<Genre>();

            _mockGenreRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Genre, bool>>>(),
                It.IsAny<string>()))
                .Returns((Expression<Func<Genre, bool>> filter, string includeProperties) =>
                    emptyGenres.AsQueryable());

            var result = _genreService.GetAllGenres();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);

            _mockGenreRepo.Verify(r => r.GetAll(null, null), Times.Once);
        }
        #endregion

        #region Get1GenreTest
        [Test]
        public void GetGenreById_WhenGenreExists_ShouldReturnGenre()
        {
            var genreId = 1;
            var expectedGenre = new Genre { Id = genreId, Name = "Rock" };

            _mockGenreRepo.Setup(r => r.Get(
                It.Is<Expression<Func<Genre, bool>>>(expr => expr.Compile()(expectedGenre)),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(expectedGenre);

            var result = _genreService.GetGenreById(genreId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(genreId));
            Assert.That(result.Name, Is.EqualTo("Rock"));

            _mockGenreRepo.Verify(r => r.Get(
                It.Is<Expression<Func<Genre, bool>>>(expr => expr.Compile()(expectedGenre)),
                null,
                false), Times.Once);
        }
        #endregion

        #region CreateGenreTests
        [Test]
        public void CreateGenre_WithValidGenre_ShouldCreateGenreAndReturnTrue()
        {
            var validGenre = new Genre { Name = "Rock" };
            string errorMessage;

            _mockGenreRepo.Setup(r => r.Add(It.IsAny<Genre>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.Save()).Verifiable();

            var result = _genreService.CreateGenre(validGenre, out errorMessage);

            Assert.That(result, Is.True);
            Assert.That(errorMessage, Is.Empty);

            _mockGenreRepo.Verify(r => r.Add(validGenre), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void CreateGenre_WithGenreContainingNumbers_ShouldReturnFalseAndErrorMessage()
        {
            var invalidGenre = new Genre { Name = "Rock123" };
            string errorMessage;

            var result = _genreService.CreateGenre(invalidGenre, out errorMessage);

            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo("The Genre Name can't contain numbers."));

            _mockGenreRepo.Verify(r => r.Add(It.IsAny<Genre>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
        }
        #endregion

        #region UpdateGenreTests
        [Test]
        public void UpdateGenre_WithValidGenre_ShouldUpdateGenreAndReturnTrue()
        {
            var validGenre = new Genre { Id = 1, Name = "Rock" };
            string errorMessage;

            _mockGenreRepo.Setup(r => r.Update(It.IsAny<Genre>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.Save()).Verifiable();

            var result = _genreService.UpdateGenre(validGenre, out errorMessage);

            Assert.That(result, Is.True);
            Assert.That(errorMessage, Is.Empty);

            _mockGenreRepo.Verify(r => r.Update(validGenre), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void UpdateGenre_WithGenreContainingNumbers_ShouldReturnFalseAndErrorMessage()
        {

            var invalidGenre = new Genre { Id = 1, Name = "Rock123" };
            string errorMessage;

            var result = _genreService.UpdateGenre(invalidGenre, out errorMessage);

            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo("The Genre Name can't contain numbers."));

            _mockGenreRepo.Verify(r => r.Update(It.IsAny<Genre>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
        }
        #endregion

        #region DeleteGenreTests
        [Test]
        public void DeleteGenre_WhenGenreExists_ShouldDeleteGenreAndReturnTrue()
        {
            var genreId = 1;
            var existingGenre = new Genre { Id = genreId, Name = "Rock" };

            _mockGenreRepo.Setup(r => r.Get(
                It.Is<Expression<Func<Genre, bool>>>(expr => expr.Compile()(existingGenre)),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(existingGenre);

            _mockGenreRepo.Setup(r => r.Delete(It.IsAny<Genre>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.Save()).Verifiable();

            var result = _genreService.DeleteGenre(genreId);

            Assert.That(result, Is.True);

            _mockGenreRepo.Verify(r => r.Get(
                It.Is<Expression<Func<Genre, bool>>>(expr => expr.Compile()(existingGenre)),
                null,
                false), Times.Once);
            _mockGenreRepo.Verify(r => r.Delete(existingGenre), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void DeleteGenre_WhenGenreDoesNotExist_ShouldReturnFalse()
        {

            var genreId = 999;

            _mockGenreRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<Genre, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns((Genre)null);

            var result = _genreService.DeleteGenre(genreId);

            Assert.That(result, Is.False);

            _mockGenreRepo.Verify(r => r.Delete(It.IsAny<Genre>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
        }
        #endregion
    }
}
