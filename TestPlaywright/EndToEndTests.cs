using Microsoft.Playwright;

namespace TestPlaywright
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class Tests : PageTest
    {
        [Test]
        public async Task UserJourney_BuysProductAsync() 
        {
            // Idi na lokalni URL
            await Page.GotoAsync("https://localhost:7104/");

            // Registracija
            await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
            await Page.GetByLabel("Email").FillAsync("stevadj@gmail.com");
            await Page.GetByLabel("Password", new() { Exact = true }).FillAsync("Steva104!");
            await Page.GetByLabel("Confirm Password").FillAsync("Steva104!");
            await Page.GetByLabel("Name").FillAsync("Stevan Djurdjic");
            await Page.GetByLabel("Street Address").FillAsync("Bulevar 103");
            await Page.GetByLabel("City").FillAsync("Belgrade");
            await Page.GetByLabel("Country").FillAsync("Serbia");
            await Page.GetByLabel("Postal Code").FillAsync("11000");
            await Page.GetByLabel("Phone Number").FillAsync("4343234234");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

            // Sačekaj da se proizvodi učitaju
            await Page.WaitForSelectorAsync(".card", new PageWaitForSelectorOptions { Timeout = 5000 });

            // Klik na Details dugme prvog proizvoda
            var firstProductCard = Page.Locator(".card").First;
            var detailsButton = firstProductCard.Locator("a.btn-primary", new LocatorLocatorOptions { HasTextString = "Details" });
            await detailsButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await detailsButton.ClickAsync();

            // Dodaj u korpu i checkout
            var addToCartButton = Page.GetByRole(AriaRole.Button, new() { Name = "Add to Cart" });
            await addToCartButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await addToCartButton.ClickAsync();

            await Page.GetByRole(AriaRole.Link, new() { Name = " (1)" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Proceed to Checkout" }).ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Place Order" }).ClickAsync();

            // Assert
            await Page.WaitForSelectorAsync("text=Order placed successfully!!!");
            var successMessage = await Page.InnerTextAsync("h1.text-primary");
            Assert.That(successMessage, Is.EqualTo("Order placed successfully!!!"));
        }
    }
}
