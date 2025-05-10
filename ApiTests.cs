#Ensek API TestCase
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ENSEK_Test
{
    public class Order
    {
        public int Id { get; set; }
        public string Fuel { get; set; }
        public int Quantity { get; set; }
        public DateTime Time { get; set; }
    }

//NUnut framework test casess
    [TestFixture]
    public class ApiTests
    {
        private readonly string _baseUrl = "https://ensekapicandidatetest.azurewebsites.net";
        private RestClient _client;
        private List<string> _fuels = new List<string> { "gas", "electricity" };
        private List<Order> _createdOrders = new List<Order>();

//Setting up the base method
        [SetUp]
        public void Setup()
        {
            _client = new RestClient(_baseUrl);
        }

//Testcases
        [Test, Order(1)]
        public void ResetTestData()
        {
            var request = new RestRequest("/reset", Method.Post);
            var response = _client.Execute(request);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test, Order(2)]
        public void BuyFuel()
        {
            foreach (var fuel in _fuels)
            {
                var request = new RestRequest($"/buy/{fuel}/1", Method.Post);
                var response = _client.Execute(request);
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            }
        }

        [Test, Order(3)]
        public void VerifyOrdersExist()
        {
            var request = new RestRequest("/orders", Method.Get);
            var response = _client.Execute(request);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var orders = JsonConvert.DeserializeObject<List<Order>>(response.Content);

            foreach (var fuel in _fuels)
            {
                orders.Should().Contain(o => o.Fuel.Equals(fuel, StringComparison.OrdinalIgnoreCase) && o.Quantity == 1);
            }

            _createdOrders = orders;
        }

        [Test, Order(4)]
        public void ValidatePastOrders()
        {
            var pastOrders = _createdOrders.Where(o => o.Time.Date < DateTime.UtcNow.Date);
            Console.WriteLine($"No. of orders before today: {pastOrders.Count()}");
        }


        [Test, Order(5)]
        public void InvalidQuantityTest()
        {
            var request = new RestRequest("/buy/gas/-5", Method.Post);
            var response = _client.Execute(request);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
