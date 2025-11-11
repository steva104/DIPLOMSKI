using Microsoft.Data.SqlClient;
using Microsoft.Playwright;
using static System.Net.Mime.MediaTypeNames;

namespace TestEndToEnd
{
    public class Tests
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _page;

        [SetUp]
        public async Task Setup()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
            });

            _context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                RecordVideoDir = "videos/",
                RecordVideoSize = new RecordVideoSize { Width = 1280, Height = 720 } 
            });

            _page = await _context.NewPageAsync(); 
        }

        [TearDown]
        public async Task Teardown()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }
        private static string RegisteredEmail;
        private static string RegisteredPassword;
        private static string TestUserEmail;
        private const string TestUserPassword = "Newuser123!";

        #region CustomerJourneyTests
        [Test, Order(1)]
        public async Task UserJourney_BuysProductAsync()
        {
            RegisteredEmail = $"stevadj_{Guid.NewGuid().ToString("N").Substring(0, 8)}@gmail.com";
            RegisteredPassword = "Steva104!";

            await _page.GotoAsync("https://localhost:7104/");

            await _page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
            await _page.GetByLabel("Email").FillAsync(RegisteredEmail);
            await _page.GetByLabel("Password", new() { Exact = true }).FillAsync(RegisteredPassword);
            await _page.GetByLabel("Confirm Password").FillAsync(RegisteredPassword);
            await _page.GetByLabel("Name").FillAsync("Stevan Djurdjic");
            await _page.GetByLabel("Street Address").FillAsync("Bulevar 103");
            await _page.GetByLabel("City").FillAsync("Belgrade");
            await _page.GetByLabel("Country").FillAsync("Serbia");
            await _page.GetByLabel("Postal Code").FillAsync("11000");
            await _page.GetByLabel("Phone Number").FillAsync("4343234234");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

            await _page.WaitForSelectorAsync(".card", new PageWaitForSelectorOptions { Timeout = 5000 });

            var firstProductCard = _page.Locator(".card").First;
            var detailsButton = firstProductCard.Locator("a.btn-primary", new LocatorLocatorOptions { HasTextString = "Details" });
            await detailsButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await detailsButton.ClickAsync();

            var addToCartButton = _page.GetByRole(AriaRole.Button, new() { Name = "Add to Cart" });
            await addToCartButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await addToCartButton.ClickAsync();

            await _page.GetByRole(AriaRole.Link, new() { Name = " (1)" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Proceed to Checkout" }).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Place Order" }).ClickAsync();

            await _page.WaitForSelectorAsync("text=Order placed successfully!!!");
            var successMessage = await _page.InnerTextAsync("h1.text-primary");
            Assert.That(successMessage, Is.EqualTo("Order placed successfully!!!"));
        }
        [Test, Order(2)]
        public async Task RegisterWithExistingEmailShowsErrorAsync()
        {
            string existingEmail = RegisteredEmail;
            string password = RegisteredPassword;

            await _page.GotoAsync("https://localhost:7104/");

            
            await _page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
            await _page.GetByLabel("Email").FillAsync(existingEmail);
            await _page.GetByLabel("Password", new() { Exact = true }).FillAsync(password);
            await _page.GetByLabel("Confirm Password").FillAsync(password);
            await _page.GetByLabel("Name").FillAsync("Stevan Djurdjic");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();


            await _page.WaitForSelectorAsync(".text-danger", new PageWaitForSelectorOptions { Timeout = 5000 });
            var errorMessage = await _page.InnerTextAsync(".text-danger");

            Assert.That(errorMessage, Does.Contain($"Username '{existingEmail}' is already taken."));
        }
        [Test, Order(3)]
        public async Task AddMultipleProductsToCartAsync()
        {
            await _page.GotoAsync("https://localhost:7104/");

            await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await _page.GetByLabel("Email").FillAsync(RegisteredEmail);
            await _page.GetByLabel("Password", new() { Exact = true }).FillAsync(RegisteredPassword);
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

            await _page.WaitForSelectorAsync(".card");

            var productCards = _page.Locator(".card");
            int count = await productCards.CountAsync();

            for (int i = 0; i < 2 && i < count; i++)
            {
                var detailsButton = productCards.Nth(i).Locator("a.btn-primary", new LocatorLocatorOptions { HasTextString = "Details" });
                await detailsButton.ClickAsync();

                var addToCartButton = _page.GetByRole(AriaRole.Button, new() { Name = "Add to Cart" });
                await addToCartButton.ClickAsync();

                await _page.GetByRole(AriaRole.Link, new() { Name = "Home" }).ClickAsync();
                await _page.WaitForSelectorAsync(".card");
            }

            await _page.GetByRole(AriaRole.Link, new() { Name = " (2)" }).ClickAsync();

            var cartItems = _page.Locator("div.row.mb-4");
            int itemsCount = await cartItems.CountAsync();
            Assert.That(itemsCount, Is.EqualTo(2));
        }
        #endregion

        #region ProductManipulationTests
        [Test, Order(7)]
        public async Task CreateProductAsync()
        {
            await _page.GotoAsync("https://localhost:7104/");
            await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("stevadj2@gmail.com");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Steva104!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Admin Options" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Product" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = " Add Product" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Title" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Title" }).FillAsync("Test12345");
            await _page.Locator("iframe[title=\"Rich Text Area\"]").ContentFrame.GetByRole(AriaRole.Paragraph).ClickAsync();
            await _page.Locator("iframe[title=\"Rich Text Area\"]").ContentFrame.GetByLabel("Rich Text Area").FillAsync("Test product 123");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "UPC" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "UPC" }).FillAsync("22222");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Artist" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Artist" }).FillAsync("Test Artist");
            await _page.GetByRole(AriaRole.Spinbutton, new() { Name = "Year" }).ClickAsync();
            await _page.GetByRole(AriaRole.Spinbutton, new() { Name = "Year" }).FillAsync("2025");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "List Price" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "List Price" }).FillAsync("200");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Price for 1-" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Price for 1-" }).FillAsync("180");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Price for 5+" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Price for 5+" }).FillAsync("160");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Price for 10+" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Price for 10+" }).FillAsync("140");
            await _page.GetByLabel("GenreId").SelectOptionAsync(new[] { "4" });
            await _page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "NED.png" });
            await _page.GetByRole(AriaRole.Button, new() { Name = "Create Product" }).ClickAsync();

            await _page.WaitForSelectorAsync(".toast-message");

            var toastText = await _page.InnerTextAsync(".toast-message");

            Assert.That(toastText, Does.Contain("Product saved successfully!"));


        }
        [Test]
        public async Task DeleteProductUnSuccessfullyAsync()
        {
            await _page.GotoAsync("https://localhost:7104/");
            await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("stevadj2@gmail.com");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Steva104!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Admin Options" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Product" }).ClickAsync();
            await _page.WaitForSelectorAsync($"#tblProduct td:has-text(\"Test\")");
            var row = _page.Locator("#tblProduct tr", new() { HasText = "Thriller" }).First;
            await row.Locator("a:has-text(\"Delete\")").ClickAsync();
            await _page.Locator(".swal2-confirm").ClickAsync();
            await _page.WaitForSelectorAsync(".toast-message");
            var toastText = await _page.InnerTextAsync(".toast-message");
            Assert.That(toastText, Does.Contain("There was an error while deleting the product."));

        }

        [Test]
        public async Task DeleteProductSuccessfullyAsync()
        {
            await _page.GotoAsync("https://localhost:7104/");
            await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("stevadj2@gmail.com");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Steva104!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Admin Options" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Product" }).ClickAsync();
            await _page.WaitForSelectorAsync($"#tblProduct td:has-text(\"Test\")");
            var row = _page.Locator("#tblProduct tr", new() { HasText = "Test12345" }).First;
            await row.Locator("a:has-text(\"Delete\")").ClickAsync();
            await _page.Locator(".swal2-confirm").ClickAsync();
            await _page.WaitForSelectorAsync(".toast-message");
            var toastText = await _page.InnerTextAsync(".toast-message");
            Assert.That(toastText, Does.Contain("Successfully deleted the product."));

        }
        #endregion

        #region UserManipulationTests
        [Test, Order(4)]
        public async Task RegisterNewUserAsync()
        {

            TestUserEmail = $"newusertest_{Guid.NewGuid().ToString("N").Substring(0, 8)}@gmail.com";

            await _page.GotoAsync("https://localhost:7104/");
            await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("stevadj2@gmail.com");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Steva104!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Admin Options" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Create New User" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync(TestUserEmail);
          
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password", Exact = true }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password", Exact = true }).FillAsync(TestUserPassword);
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Confirm Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Confirm Password" }).FillAsync(TestUserPassword);
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Name" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Name" }).FillAsync("New User Test");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Street Address" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Street Address" }).FillAsync("Test");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("Test");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Country" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Country" }).FillAsync("Test");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Postal Code" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Postal Code" }).FillAsync("1101001");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Phone Number" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Phone Number" }).FillAsync("3131413");
            await _page.Locator("#Input_Role").SelectOptionAsync(new[] { "Admin" });
            await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

            await _page.WaitForSelectorAsync(".toast-message");

            var toastText = await _page.InnerTextAsync(".toast-message");

            Assert.That(toastText, Does.Contain("User created successfully"));

        }

        [Test, Order(5)]
        public async Task UpdateUserRoleAsync()
        {
            await _page.GotoAsync("https://localhost:7104/");
            await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("stevadj2@gmail.com");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Steva104!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Admin Options" }).ClickAsync();
         
            await _page.GetByRole(AriaRole.Link, new() { Name = "Users" }).ClickAsync();
            await _page.GetByRole(AriaRole.Row, new() { Name = $"New User Test {TestUserEmail}" }).GetByRole(AriaRole.Link).ClickAsync();
            await _page.Locator("#User_Role").SelectOptionAsync(new[] { "Company" });
            await _page.Locator("#User_CompanyId").SelectOptionAsync(new[] { "7" });
            await _page.GetByRole(AriaRole.Button, new() { Name = "Update Role" }).ClickAsync();

            var row = _page.GetByRole(AriaRole.Row, new() { Name = $"New User Test {TestUserEmail}" });
            Assert.That(await row.InnerTextAsync(), Does.Contain("Company"));

        }
        [Test, Order(6)]
        public async Task LockUnlockUserAsync()
        {
            await _page.GotoAsync("https://localhost:7104/");
            await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("stevadj2@gmail.com");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Steva104!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Admin Options" }).ClickAsync();

            await _page.GetByRole(AriaRole.Link, new() { Name = "Users" }).ClickAsync();
            await _page.GetByRole(AriaRole.Row, new() { Name = $"New User Test {TestUserEmail}" }).Locator("a").First.ClickAsync();

            await _page.WaitForSelectorAsync(".toast-message");

            var toastText = await _page.InnerTextAsync(".toast-message");

            Assert.That(toastText, Does.Contain("Successfully locked/unlocked"));
        }
        #endregion

        #region OrderManipulationTests
        [Test]
        public async Task CancelOrderAsync()
        {
            await _page.GotoAsync("https://localhost:7104/");
            await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("stevadj2@gmail.com");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Steva104!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

            await _page.GetByRole(AriaRole.Link, new() { Name = "Orders" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Approved" }).ClickAsync();
            var firstRow = _page.Locator("#tblOrder tbody tr").First;
            await firstRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await firstRow.Locator("a").First.ClickAsync();
            await _page.Locator("button:has-text('Cancel Order')").ClickAsync();
            await _page.WaitForSelectorAsync(".toast-message");

            var toastText = await _page.InnerTextAsync(".toast-message");

            Assert.That(toastText, Does.Contain("Order cancelled successfully!"));
        }
        [Test]
        public async Task ProcessOrderAsync()
        {
            await _page.GotoAsync("https://localhost:7104/");
            await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("stevadj2@gmail.com");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Steva104!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

            await _page.GetByRole(AriaRole.Link, new() { Name = "Orders" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Approved" }).ClickAsync();
            var firstRow = _page.Locator("#tblOrder tbody tr").First;
            await firstRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await firstRow.Locator("a").First.ClickAsync();
            await _page.Locator("button:has-text('Start Processing')").ClickAsync();
            await _page.WaitForSelectorAsync(".toast-message");

            var toastText = await _page.InnerTextAsync(".toast-message");

            Assert.That(toastText, Does.Contain("Order updated successfully!"));
        }
        [Test]
        public async Task ShipOrderAsync()
        {
            await _page.GotoAsync("https://localhost:7104/");
            await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("stevadj2@gmail.com");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Steva104!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

            await _page.GetByRole(AriaRole.Link, new() { Name = "Orders" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "In proccess" }).ClickAsync();
            var firstRow = _page.Locator("#tblOrder tbody tr").First;
            await firstRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await firstRow.Locator("a").First.ClickAsync();

            await _page.Locator("#carrier").ClickAsync();
            await _page.Locator("#carrier").FillAsync("1010010");
            await _page.Locator("input[name=\"OrderHeader.TrackingNumber\"]").ClickAsync();
            await _page.Locator("input[name=\"OrderHeader.TrackingNumber\"]").FillAsync("1010100101");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Ship Order" }).ClickAsync();

            await _page.WaitForSelectorAsync(".toast-message");

            var toastText = await _page.InnerTextAsync(".toast-message");

            Assert.That(toastText, Does.Contain("Order shipped successfully!"));
        }
        [Test]
        public async Task ShipOrder_MissingTrackingNumberAsync()
        {
            await _page.GotoAsync("https://localhost:7104/");
            await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("stevadj2@gmail.com");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Steva104!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

            await _page.GetByRole(AriaRole.Link, new() { Name = "Orders" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "In proccess" }).ClickAsync();
            var firstRow = _page.Locator("#tblOrder tbody tr").First;
            await firstRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await firstRow.Locator("a").First.ClickAsync();

            await _page.Locator("#carrier").ClickAsync();
            await _page.Locator("#carrier").FillAsync("1010010");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Ship Order" }).ClickAsync();

            await _page.WaitForSelectorAsync(".swal2-popup", new PageWaitForSelectorOptions { Timeout = 5000 });

            var alertText = await _page.InnerTextAsync(".swal2-html-container");

            Assert.That(alertText, Does.Contain("Please enter the tracking number!"));
        }
        #endregion

    }
}