using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using NzbDrone.Automation.Test.PageModel;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace NzbDrone.Automation.Test
{
    [TestFixture]
    [AutomationTest]
    public abstract class AutomationTest
    {
        private NzbDroneRunner _runner;
        protected ChromeDriver driver;

        public AutomationTest()
        {
            new StartupContext();

            LogManager.Configuration = new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget { Layout = "${level}: ${message} ${exception}" };
            LogManager.Configuration.AddTarget(consoleTarget.GetType().Name, consoleTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, consoleTarget));
        }

        [OneTimeSetUp]
        public void SmokeTestSetup()
        {
            var options = new ChromeOptions();
            options.AddArguments("--headless");
            options.AddArguments("--window-size=1920,1080");
            var service = ChromeDriverService.CreateDefaultService();

            // Timeout as windows automation tests seem to take alot longer to get going
            driver = new ChromeDriver(service, options, TimeSpan.FromMinutes(3));

            // Ensure we start from a desktop-sized viewport so responsive menus render
            driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
            driver.Manage().Window.FullScreen();

            _runner = new NzbDroneRunner(LogManager.GetCurrentClassLogger(), null);
            _runner.KillAll();
            _runner.Start(true);

            driver.Url = "http://localhost:6969";

            var page = new PageBase(driver);
            page.WaitForNoSpinner();

            driver.ExecuteScript("window.Whisparr.NameViews = true;");

            GetPageErrors().Should().BeEmpty();
        }

        protected IEnumerable<string> GetPageErrors()
        {
            return driver.FindElements(By.CssSelector("#errors div"))
                .Select(e => e.Text);
        }

        protected void TakeScreenshot(string name)
        {
            try
            {
                var image = driver.GetScreenshot();
                image.SaveAsFile($"./{name}_test_screenshot.png");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save screenshot {name}, {ex.Message}");
            }
        }

        [OneTimeTearDown]
        public void SmokeTestTearDown()
        {
            _runner.KillAll();
            driver.Quit();
        }

        [TearDown]
        public void AutomationTearDown()
        {
            GetPageErrors().Should().BeEmpty();
        }
    }
}
