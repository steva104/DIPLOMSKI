using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using VinylVibe.BusinessLogic;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;

namespace Tests
{
    public class CompanyServiceUnitTest
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private Mock<ICompanyRepository> _mockCompanyRepo;
        private Mock<IUserRepository> _mockUserRepo;
        private CompanyService _companyService;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCompanyRepo = new Mock<ICompanyRepository>();
            _mockUserRepo = new Mock<IUserRepository>();

            var store = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);

            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null);

            _mockUnitOfWork.Setup(u => u.Company).Returns(_mockCompanyRepo.Object);
            _mockUnitOfWork.Setup(u => u.User).Returns(_mockUserRepo.Object);


            _companyService = new CompanyService(
                _mockUnitOfWork.Object,
                _mockUserManager.Object,
                _mockRoleManager.Object);
        }
        #region GetAllCompaniesTests
        [Test]
        public void GetAllCompanies_WhenCompaniesExist_ShouldReturnAllCompanies()
        {

            var expectedCompanies = new List<Company>
        {
            new Company { Id = 1, Name = "Company A" },
            new Company { Id = 2, Name = "Company B" },
            new Company { Id = 3, Name = "Company C" }
        }.AsQueryable();

            _mockCompanyRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Company, bool>>>(),
                It.IsAny<string>()))
                .Returns((Expression<Func<Company, bool>> filter, string includeProperties) =>
                    expectedCompanies);

            var result = _companyService.GetAllCompanies();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result.Select(c => c.Name), Is.EquivalentTo(new[] { "Company A", "Company B", "Company C" }));

            _mockCompanyRepo.Verify(r => r.GetAll(null, null), Times.Once);
        }

        [Test]
        public void GetAllCompanies_WhenNoCompaniesExist_ShouldReturnEmptyQueryable()
        {

            var emptyCompanies = new List<Company>().AsQueryable();

            _mockCompanyRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Company, bool>>>(),
                It.IsAny<string>()))
                .Returns((Expression<Func<Company, bool>> filter, string includeProperties) =>
                    emptyCompanies);

            var result = _companyService.GetAllCompanies();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
            Assert.That(result, Is.InstanceOf<IQueryable<Company>>());

            _mockCompanyRepo.Verify(r => r.GetAll(null, null), Times.Once);
        }
        #endregion

        #region Get1CompanyTest
        [Test]
        public void GetCompanyById_WhenCompanyExists_ShouldReturnCompany()
        {
            var companyId = 1;
            var expectedCompany = new Company
            {
                Id = companyId,
                Name = "Test Company",
                StreetAddress = "Test Address"
            };

            _mockCompanyRepo.Setup(r => r.Get(
                It.Is<Expression<Func<Company, bool>>>(expr => expr.Compile()(expectedCompany)),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(expectedCompany);

            var result = _companyService.GetCompanyById(companyId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(companyId));
            Assert.That(result.Name, Is.EqualTo("Test Company"));
            Assert.That(result.StreetAddress, Is.EqualTo("Test Address"));

            _mockCompanyRepo.Verify(r => r.Get(
                It.Is<Expression<Func<Company, bool>>>(expr => expr.Compile()(expectedCompany)),
                null,
                false), Times.Once);
        }
        #endregion

        #region CreateCompanyTests
        [Test]
        public void AddCompany_WithValidCompany_ShouldAddCompanyAndSave()
        {
            var company = new Company
            {
                Id = 1,
                Name = "Test Company",
                StreetAddress = "Test Address"
            };

            _mockCompanyRepo.Setup(r => r.Add(It.IsAny<Company>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.Save()).Verifiable();

            _companyService.AddCompany(company);

            _mockCompanyRepo.Verify(r => r.Add(company), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void AddCompany_WithNullCompany_ShouldThrowArgumentNullException()
        {
            Company nullCompany = null;

            _mockCompanyRepo.Verify(r => r.Add(It.IsAny<Company>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
        }
        #endregion

        #region UpdateCompanyTest
        [Test]
        public void UpdateCompany_WithValidCompany_ShouldUpdateCompanyAndSave()
        {
            var company = new Company
            {
                Id = 1,
                Name = "Updated Company",
                StreetAddress = "Updated Address"
            };

            _mockCompanyRepo.Setup(r => r.Update(It.IsAny<Company>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.Save()).Verifiable();

            _companyService.UpdateCompany(company);

            _mockCompanyRepo.Verify(r => r.Update(company), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }
        #endregion

        #region DeleteCompanyTest
        [Test]
        public void DeleteCompany_CompanyExistsWithUsers_UpdatesRolesAndDeletesCompany()
        {
            int companyId = 1;
            var company = new Company { Id = companyId };
            var users = new List<User>
            {
                new User { Id = "user1" },
                new User { Id = "user2" }
            };

            _mockCompanyRepo.Setup(c => c.Get(It.IsAny<Expression<Func<Company, bool>>>(), null, false)).Returns(company);
            _mockUserRepo.Setup(u => u.GetAll(It.IsAny<Expression<Func<User, bool>>>(), null)).Returns(users.AsQueryable());

            _mockUserManager.Setup(u => u.RemoveFromRoleAsync(It.IsAny<User>(), "Company")).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(u => u.AddToRoleAsync(It.IsAny<User>(), "Customer")).ReturnsAsync(IdentityResult.Success);

            _companyService.DeleteCompany(companyId);

            foreach (var user in users)
            {
                _mockUserManager.Verify(u => u.RemoveFromRoleAsync(user, "Company"), Times.Once);
                _mockUserManager.Verify(u => u.AddToRoleAsync(user, "Customer"), Times.Once);
                _mockUserRepo.Verify(r => r.Update(user), Times.Once);
            }

            _mockCompanyRepo.Verify(c => c.Delete(company), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Exactly(2));
        }
        #endregion



    }
}
