using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class OpenApiFixture : IntegrationTest
    {
        private static readonly string[] Verbs = { "get", "put", "post", "delete", "patch", "head", "options" };

        private readonly HttpClient _httpClient = new HttpClient();
        private JsonElement _document;

        [OneTimeSetUp]
        public void FetchDocument()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, RootUrl + "docs/v3/openapi.json");
            using var response = _httpClient.Send(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            _document = JsonDocument.Parse(text).RootElement.Clone();
        }

        [Test]
        public void every_operation_should_have_a_unique_operation_id()
        {
            var operationIds = GetOperations()
                .Select(o => o.Operation.TryGetProperty("operationId", out var id) ? id.GetString() : null)
                .ToList();

            operationIds.Should().NotContainNulls();
            operationIds.Should().OnlyHaveUniqueItems();
        }

        [TestCase("get", "/api/v3/tag", "getTag")]
        [TestCase("get", "/api/v3/tag/{id}", "getTagById")]
        [TestCase("put", "/api/v3/qualitydefinition/update", "putQualitydefinitionUpdate")]
        [TestCase("get", "/ping", "getPing")]
        [TestCase("get", "/feed/v3/calendar/whisparr.ics", "getFeedV3CalendarWhisparrIcs")]
        public void should_name_operations_after_method_and_path(string verb, string path, string operationId)
        {
            GetOperation(verb, path).GetProperty("operationId").GetString().Should().Be(operationId);
        }

        [TestCase("post", "/api/v3/tag", "201")]
        [TestCase("post", "/api/v3/command", "201")]
        [TestCase("post", "/api/v3/indexer", "201")]
        [TestCase("put", "/api/v3/tag/{id}", "202")]
        [TestCase("put", "/api/v3/indexer/{id}", "202")]
        [TestCase("post", "/api/v3/languageprofile", "202")]
        [TestCase("put", "/api/v3/indexer/bulk", "202")]
        [TestCase("put", "/api/v3/series/editor", "202")]
        [TestCase("put", "/api/v3/episode/monitor", "202")]
        [TestCase("put", "/api/v3/qualitydefinition/update", "202")]
        [TestCase("post", "/api/v3/seasonpass", "202")]
        public void should_document_the_status_code_the_action_returns(string verb, string path, string statusCode)
        {
            var responses = GetOperation(verb, path).GetProperty("responses");

            responses.EnumerateObject().Select(r => r.Name).Should().BeEquivalentTo(new[] { statusCode });
        }

        [TestCase("post", "/api/v3/tag", "TagResource")]
        [TestCase("put", "/api/v3/tag/{id}", "TagResource")]
        public void should_keep_the_response_schema_when_moving_the_status_code(string verb, string path, string schema)
        {
            var response = GetOperation(verb, path).GetProperty("responses").EnumerateObject().Single().Value;

            response.GetProperty("content").GetProperty("application/json").GetProperty("schema").GetProperty("$ref").GetString()
                .Should().Be("#/components/schemas/" + schema);
        }

        [TestCase("get", "/ping")]
        [TestCase("head", "/ping")]
        [TestCase("post", "/login")]
        [TestCase("get", "/logout")]
        public void anonymous_operations_should_not_require_an_api_key(string verb, string path)
        {
            var operation = GetOperation(verb, path);

            operation.TryGetProperty("security", out var security).Should().BeTrue();
            security.GetArrayLength().Should().Be(0);
        }

        [Test]
        public void authenticated_operations_should_use_the_root_security_requirement()
        {
            GetOperation("get", "/api/v3/tag").TryGetProperty("security", out _).Should().BeFalse();
        }

        [Test]
        public void command_resource_should_allow_additional_properties()
        {
            var schema = _document.GetProperty("components").GetProperty("schemas").GetProperty("CommandResource");

            if (schema.TryGetProperty("additionalProperties", out var additionalProperties))
            {
                additionalProperties.ValueKind.Should().NotBe(JsonValueKind.False);
            }
        }

        [Test]
        public void http_uri_should_be_described_as_a_string()
        {
            var wikiUrl = GetSchema("HealthResource").GetProperty("properties").GetProperty("wikiUrl");

            wikiUrl.GetProperty("type").GetString().Should().Be("string");
        }

        [Test]
        public void json_request_bodies_should_be_required()
        {
            foreach (var (path, verb, operation) in GetOperations())
            {
                if (!operation.TryGetProperty("requestBody", out var requestBody) ||
                    !requestBody.GetProperty("content").TryGetProperty("application/json", out _))
                {
                    continue;
                }

                requestBody.TryGetProperty("required", out var required).Should().BeTrue("{0} {1} rejects a request without a body", verb, path);
                required.GetBoolean().Should().BeTrue("{0} {1} rejects a request without a body", verb, path);
            }
        }

        [Test]
        public void every_path_parameter_should_appear_in_its_path_template()
        {
            foreach (var (path, _, operation) in GetOperations())
            {
                if (!operation.TryGetProperty("parameters", out var parameters))
                {
                    continue;
                }

                foreach (var parameter in parameters.EnumerateArray().Where(p => p.GetProperty("in").GetString() == "path"))
                {
                    path.Should().Contain("{" + parameter.GetProperty("name").GetString() + "}", "a path parameter must be in the template");
                }
            }
        }

        [Test]
        public void should_not_document_the_ui()
        {
            var paths = _document.GetProperty("paths").EnumerateObject().Select(p => p.Name).ToList();

            paths.Should().NotContain(new[] { "/", "/{path}", "/content/{path}" });
            paths.Should().Contain("/login");
        }

        [Test]
        public void description_should_name_the_build_it_was_generated_from()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, RootUrl + "api/v3/system/status");
            request.Headers.Add("X-Api-Key", ApiKey);
            using var response = _httpClient.Send(request);

            var status = JsonDocument.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).RootElement;
            var version = status.GetProperty("version").GetString();

            _document.GetProperty("info").GetProperty("description").GetString().Should().Contain(version);
        }

        private JsonElement GetSchema(string name)
        {
            return _document.GetProperty("components").GetProperty("schemas").GetProperty(name);
        }

        private JsonElement GetOperation(string verb, string path)
        {
            _document.GetProperty("paths").TryGetProperty(path, out var item).Should().BeTrue("{0} should be documented", path);
            item.TryGetProperty(verb, out var operation).Should().BeTrue("{0} {1} should be documented", verb, path);

            return operation;
        }

        private List<(string Path, string Verb, JsonElement Operation)> GetOperations()
        {
            var operations = new List<(string, string, JsonElement)>();

            foreach (var path in _document.GetProperty("paths").EnumerateObject())
            {
                foreach (var verb in Verbs)
                {
                    if (path.Value.TryGetProperty(verb, out var operation))
                    {
                        operations.Add((path.Name, verb, operation));
                    }
                }
            }

            return operations;
        }
    }
}
