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

            // Runtime HTTP diagnostics: fetch index.html and referenced assets to ensure the server is serving the frontend
            try
            {
                using var http = new System.Net.Http.HttpClient();
                http.Timeout = TimeSpan.FromSeconds(5);

                var baseUrl = "http://localhost:6969";

                // Retry a few times to allow the server to come up and serve the site root '/'
                string index = null;
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        var resp = http.GetAsync(baseUrl + "/").GetAwaiter().GetResult();
                        TestContext.Progress.WriteLine($"HTTP GET / -> {(int)resp.StatusCode} {resp.ReasonPhrase}");
                        if (resp.IsSuccessStatusCode)
                        {
                            index = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            var head = index.Length > 1000 ? index.Substring(0, 1000) : index;
                            TestContext.Progress.WriteLine("--- / (root) HTML head ---\n" + head + "\n--- end root HTML head ---");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        TestContext.Progress.WriteLine($"HTTP diagnostic attempt {i + 1} failed: {ex.Message}");
                    }

                    System.Threading.Thread.Sleep(500);
                }

                if (!string.IsNullOrEmpty(index))
                {
                    // extract script src and link href references
                    try
                    {
                        var scriptUrls = new System.Text.RegularExpressions.Regex("<script[^>]+src=\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                            .Matches(index)
                            .Cast<System.Text.RegularExpressions.Match>()
                            .Select(m => m.Groups[1].Value)
                            .Distinct()
                            .ToList();

                        foreach (var script in scriptUrls)
                        {
                            var url = script.StartsWith("/") ? baseUrl + script : baseUrl + "/" + script.TrimStart('.', '/');
                            try
                            {
                                var r = http.GetAsync(url).GetAwaiter().GetResult();
                                TestContext.Progress.WriteLine($"HTTP GET {url} -> {(int)r.StatusCode} {r.ReasonPhrase}");
                            }
                            catch (Exception ex)
                            {
                                TestContext.Progress.WriteLine($"Failed to GET {url}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TestContext.Progress.WriteLine("Failed to parse index.html for scripts: " + ex.Message);
                    }
                }
                else
                {
                    TestContext.Progress.WriteLine("index.html was not served by the application during diagnostics");
                }
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine("HTTP diagnostics failed: " + ex.Message);
            }

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
                    catch (Exception)
                    {
                        // Intentionally ignore errors capturing browser logs
                    }

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
