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
using OpenQA.Selenium.Remote;

namespace NzbDrone.Automation.Test
{
    [TestFixture]
    [AutomationTest]
    public abstract class AutomationTest
    {
        private NzbDroneRunner _runner;
        protected RemoteWebDriver driver;

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
            var service = ChromeDriverService.CreateDefaultService();

            // Timeout as windows automation tests seem to take alot longer to get going
            driver = new ChromeDriver(service, options, new TimeSpan(0, 3, 0));

            driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);

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
                var image = ((ITakesScreenshot)driver).GetScreenshot();
                image.SaveAsFile($"./{name}_test_screenshot.png", ScreenshotImageFormat.Png);
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
            var pageErrors = GetPageErrors().ToList();

            var status = TestContext.CurrentContext.Result.Outcome.Status;
            var failed = status == NUnit.Framework.Interfaces.TestStatus.Failed;

            if (failed || pageErrors.Any())
            {
                try
                {
                    var name = TestContext.CurrentContext.Test.Name ?? "test";
                    try
                    {
                        var src = driver?.PageSource;
                        if (!string.IsNullOrEmpty(src))
                        {
                            TestContext.Progress.WriteLine($"--- PAGE SOURCE START ({name}) ---\n{src}\n--- PAGE SOURCE END ---");
                        }
                    }
                    catch (Exception ex)
                    {
                        TestContext.Progress.WriteLine("Failed to capture page source: " + ex.Message);
                    }

                    try
                    {
                        // Capture browser console logs when available (helps diagnose CI-only JS errors)
                        try
                        {
                            var logs = driver?.Manage()?.Logs?.GetLog(OpenQA.Selenium.LogType.Browser);
                            if (logs != null && logs.Count > 0)
                            {
                                TestContext.Progress.WriteLine("--- BROWSER CONSOLE LOGS ---");
                                foreach (var entry in logs)
                                {
                                    TestContext.Progress.WriteLine($"[{entry.Timestamp}] {entry.Level}: {entry.Message}");
                                }
                                TestContext.Progress.WriteLine("--- END BROWSER CONSOLE LOGS ---");
                            }
                        }
                        catch (Exception ex)
                        {
                            TestContext.Progress.WriteLine("Failed to capture browser console logs: " + ex.Message);
                        }
                    }
                    catch (Exception) { }

                    try
                    {
                        TakeScreenshot(name + "_teardown");
                    }
                    catch (Exception ex)
                    {
                        TestContext.Progress.WriteLine("Failed to take screenshot: " + ex.Message);
                    }

                    if (pageErrors.Any())
                    {
                        TestContext.Progress.WriteLine("Page reported JS errors:\n" + string.Join("\n", pageErrors));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during teardown diagnostics: " + ex.Message);
                }
            }

            pageErrors.Should().BeEmpty();
        }
    }
}
