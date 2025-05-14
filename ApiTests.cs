using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace ENSEK_Test
{
    //class Fuel with attributes ID, Name
    public class Fuel
    {
        [JsonProperty("energy_id")]
        public int EnergyID { get; set; }

        [JsonProperty("price_per_unit")]
        public double PricePerunit { get; set; }

        [JsonProperty("quantity_of_units")]
        public int QuanityOfUnits { get; set; }

        [JsonProperty("unit_type")]
        public string  UnitType{ get; set; }
    }

    //class Order with attributes ID, FuelId, Quantity and Created
    public class Order
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("fuel")]
        public string FuelId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("time")]
        public DateTime Created { get; set; }
    }

    //Test Entry point
    [TestFixture]
    public class EnsekApiTestSuite
    {
        private RestClient _client;
        private const string BaseUrl = "https://qacandidatetest.ensek.io";
        private List<Order> _orders;
        private static Dictionary<string, Fuel> _purchasedFuel;
        private string _bearerToken;
        private const string Username = "test";
        private const string Password = "testing";

        //Setting up the rest client
        //Setting up the rest client
        [SetUp]
        public void Setup()
        {
            _client = new RestClient(BaseUrl);
            LoginAndGetToken();
        }

        // Login method to get the authentication token       
        private void LoginAndGetToken()
        {
            var loginRequest = new RestRequest("/ENSEK/login", Method.Post);
            loginRequest.AddJsonBody(new { username = Username, password = Password });
            var loginResponse = _client.Execute(loginRequest);
            loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            // Parse token from response           
            dynamic loginResponseContent = JsonConvert.DeserializeObject<dynamic>(loginResponse.Content); 
            _bearerToken = loginResponseContent.access_token;
            // Ensure token is not null or empty           
            _bearerToken.Should().NotBeNullOrEmpty();
        }

        //CreateRequest method, pass resource and method
        private RestRequest CreateRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method);
            request.AddHeader("Authorization", $"Bearer {_bearerToken}");
            return request;
        }

        //Test Cases
        [Test, Order(1)]
        public void ResetTestData()
        {
            var request = CreateRequest("/ENSEK/reset", Method.Post);
            var response = _client.Execute(request);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            Console.Write("Reset Completed");
        }

        [Test, Order(2)]
        public void BuyEachFuel()
        {
            var fuelRequest = CreateRequest("/ENSEK/energy", Method.Get); 
            var fuelResponse = _client.Execute(fuelRequest); 
            fuelResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _purchasedFuel = JsonConvert.DeserializeObject<Dictionary<string, Fuel>>(fuelResponse.Content);
            foreach (var enegeryName in _purchasedFuel)
            {
                Console.Write("Enerygy Name " + enegeryName);
            }
            _purchasedFuel.Should().NotBeEmpty();
            foreach (var fuel in _purchasedFuel)
            {
                var val = fuel.Value.EnergyID;
                var buyRequest = CreateRequest("/ENSEK/buy/"+fuel.Value.EnergyID+"/1", Method.Put);
                var buyResponse = _client.Execute(buyRequest); 
                buyResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                Console.Write(buyResponse.Content);
            }
        }

        [Test, Order(3)]
        public void VerifyOrdersExist()
        {
            var request = CreateRequest("/ENSEK/orders", Method.Get); 
            var response = _client.Execute(request); 
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _orders = JsonConvert.DeserializeObject<List<Order>>(response.Content);
            Console.Write(response.Content);

            var fuelRequest = CreateRequest("/ENSEK/energy", Method.Get);
            var fuelResponse = _client.Execute(fuelRequest);
            fuelResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            _purchasedFuel = JsonConvert.DeserializeObject<Dictionary<string, Fuel>>(fuelResponse.Content);

            foreach (var fuel in _purchasedFuel) 
            {
                for (int i = 0; i < _orders.Count; i++)
                {
                    try
                    {
                        if (_orders[i].FuelId == fuel.Key && _orders[i].Quantity == 1)
                        {
                            Console.Write("Order Found \n");
                            Console.Write("Order ID " + _orders[i].Id + "\n");
                        }
                    }
                    catch (AssertionException e)
                    {
                    }
                }

            }
        }

        [Test, Order(4)]
        public void ValidateOrdersBeforeToday()
        {
            if (_orders == null || !_orders.Any()) 
                VerifyOrdersExist();
            var pastOrders = _orders.Where(o => DateTime.SpecifyKind(o.Created, DateTimeKind.Utc).Date < DateTime.UtcNow.Date);
            Console.WriteLine($"Orders created before today: {pastOrders.Count()}");
        }

        [Test, Order(5)]
        public void InvalidFuelIdShouldFail()
        {
            var request = CreateRequest("/ENSEK/buy/9999/1", Method.Put); 
            var response = _client.Execute(request); 
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            Console.Write(response.Content);

        }
    }
}