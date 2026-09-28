using NUnit.Framework;
using RestSharp;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Net;

namespace AutomatedTesting.Lab3.RestSharp.StepDefinitions
{
    [TestFixture]
    public class ApiTests
    {
        private RestClient _client = null!;
        private string? _token;
        private static int _bookingId;

        [OneTimeSetUp]
        public void Setup()
        {
            var options = new RestClientOptions("https://restful-booker.herokuapp.com")
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            _client = new RestClient(options);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client?.Dispose();
        }

        [Test, Order(1)]
        public void Step1_GetAuthToken_Test()
        {
            var request = new RestRequest("/auth", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new { username = "admin", password = "password123" });

            var response = _client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNull();

            var json = JObject.Parse(response.Content!);
            _token = json["token"]?.ToString();
            _token.Should().NotBeNullOrEmpty();
        }

        [Test, Order(2)]
        public void Step2_CreateBooking_Test()
        {
            var request = new RestRequest("/booking", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");

            var body = new
            {
                firstname = "Ivan",
                lastname = "Vakhula",
                totalprice = 150,
                depositpaid = true,
                bookingdates = new { checkin = "2026-01-01", checkout = "2026-01-10" },
                additionalneeds = "Breakfast"
            };

            request.AddJsonBody(body);

            var response = _client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNull();

            var json = JObject.Parse(response.Content!);
            _bookingId = json["bookingid"] != null ? (int)json["bookingid"]! : 0;
            _bookingId.Should().BeGreaterThan(0);
        }

        [Test, Order(3)]
        public void Step3_GetBookingById_Test()
        {
            EnsureBookingExists();

            var request = new RestRequest($"/booking/{_bookingId}", Method.Get);
            request.AddHeader("Accept", "application/json");

            var response = _client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNull();

            var json = JObject.Parse(response.Content!);
            json["lastname"]?.ToString().Should().Be("Vakhula");
        }

        [Test, Order(4)]
        public void Step4_UpdateBooking_Test()
        {
            EnsureBookingExists();
            EnsureTokenExists();

            var request = new RestRequest($"/booking/{_bookingId}", Method.Put);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Cookie", $"token={_token}");
            request.AddHeader("Authorization", "Basic YWRtaW46cGFzc3dvcmQxMjM=");

            var body = new
            {
                firstname = "Ivan",
                lastname = "UpdatedLastName",
                totalprice = 150,
                depositpaid = true,
                bookingdates = new { checkin = "2026-01-01", checkout = "2026-01-10" },
                additionalneeds = "Breakfast"
            };

            request.AddJsonBody(body);

            var response = _client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNull();

            var json = JObject.Parse(response.Content!);
            json["lastname"]?.ToString().Should().Be("UpdatedLastName");
        }

        [Test, Order(5)]
        public void Step5_DeleteBooking_Test()
        {
            EnsureBookingExists();
            EnsureTokenExists();

            var request = new RestRequest($"/booking/{_bookingId}", Method.Delete);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cookie", $"token={_token}");
            request.AddHeader("Authorization", "Basic YWRtaW46cGFzc3dvcmQxMjM=");

            var response = _client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Test]
        public void Step6_PublicCatFactApi_Test()
        {
            var options = new RestClientOptions("https://catfact.ninja")
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            using var publicClient = new RestClient(options);
            var request = new RestRequest("/fact", Method.Get);

            var response = publicClient.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNull();

            var json = JObject.Parse(response.Content!);
            json["fact"].Should().NotBeNull();
        }

        private void EnsureBookingExists()
        {
            if (_bookingId <= 0)
            {
                Step2_CreateBooking_Test();
            }
        }

        private void EnsureTokenExists()
        {
            if (string.IsNullOrEmpty(_token))
            {
                Step1_GetAuthToken_Test();
            }
        }
    }
}