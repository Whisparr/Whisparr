using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace NzbDrone.Automation.Test.PageModel
{
    public class PageBase
    {
        private readonly RemoteWebDriver _driver;

        public PageBase(RemoteWebDriver driver)
        {
            _driver = driver;
            driver.Manage().Window.Maximize();
        }

        public IWebElement FindByClass(string className, int timeout = 30)
        {
            return Find(By.ClassName(className), timeout);
        }

        public IWebElement Find(By by, int timeout = 30)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));
            try
            {
                return wait.Until(d => d.FindElement(by));
            }
            catch (WebDriverTimeoutException ex)
            {
                try
                {
                    Console.WriteLine($"--- DIAGNOSTIC: Timeout finding element by {by} after {timeout} seconds --- {ex.Message}");

                    // Dump a small diagnostic snippet to help triage CI DOM differences
                    var pageSource = _driver.PageSource ?? string.Empty;
                    Console.WriteLine("--- DIAGNOSTIC: PageSource START ---");
                    Console.WriteLine(pageSource.Length > 20000 ? pageSource.Substring(0, 20000) : pageSource);
                    Console.WriteLine("--- DIAGNOSTIC: PageSource END ---");

                    var links = _driver.FindElements(By.CssSelector("a"));
                    Console.WriteLine($"Found {links.Count} <a> elements (showing up to 20):");
                    for (var i = 0; i < Math.Min(20, links.Count); i++)
                    {
                        Console.WriteLine($"[{i}] '{links[i].Text}'");
                    }
                }
                catch (Exception)
                {
                    // Ignore any exceptions during diagnostics
                }

                throw;
            }
        }

        public void WaitForNoSpinner(int timeout = 30)
        {
            // give the spinner some time to show up.
            Thread.Sleep(200);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));
            wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(By.ClassName("followingBalls"));
                    return !element.Displayed;
                }
                catch (StaleElementReferenceException)
                {
                    return true;
                }
                catch (NoSuchElementException)
                {
                    return true;
                }
            });
        }

        public IWebElement MovieNavIcon => Find(By.LinkText("Movies"));

        public IWebElement CalendarNavIcon => Find(By.LinkText("Calendar"));

        public IWebElement ActivityNavIcon => Find(By.LinkText("Activity"));

        public IWebElement WantedNavIcon => Find(By.LinkText("Wanted"));

        public IWebElement SettingNavIcon => Find(By.LinkText("Settings"));

        public IWebElement SystemNavIcon => Find(By.PartialLinkText("System"));
    }
}
