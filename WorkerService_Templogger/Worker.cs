using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WorkerService_Templogger.Models;

namespace WorkerService_Templogger
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        // Data retrieved from: https://openweathermap.org/api/one-call-api?gclid=CjwKCAjwnef6BRAgEiwAgv8mQdzdMCyAr4Hu7OXLtjHGxFEmDY3DJcAFrqosvZECCpbt1aGk5S3MHBoCkEcQAvD_BwE
        private readonly string _url = "https://api.openweathermap.org/data/2.5/onecall?lat=33.441792&lon=-94.037689&units=metric&exclude=minutely,hourly,daily&appid=834f817d25f20721907565b84cfb8fbd";
        
        private HttpClient _client;
        private HttpResponseMessage _response;

        private readonly Random _rnd;

        private string _json;
        private WeatherModel _data;

        private float _temp;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _rnd = new Random();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _client = new HttpClient();
            _logger.LogInformation("The service has been started.");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _client.Dispose();
            _logger.LogInformation("The service has been stopped.");
            return base.StopAsync(cancellationToken);
        }

        public void SetRndTemp()
        {
            _temp = _rnd.Next(0,81) - 40; // Ranges between -40 - 40
        }

        public async void SetAPITemp()
        {
            try
            {
                _response = await _client.GetAsync(_url);

                if (_response.IsSuccessStatusCode)
                {
                    try
                    {
                        _json = await _response.Content.ReadAsStringAsync();
                        _data = JsonConvert.DeserializeObject<WeatherModel>(_json);
                        _temp = _data.current.temp;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"Failed. - {ex.Message}");
                    }
                }
                else
                {
                    _logger.LogInformation($"The website is down. Status Code = {_response.StatusCode}"); 
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Failed. The website - {ex.Message}");
            }
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //SetRndTemp(); // Use random generated temp
                SetAPITemp(); // Use API temp

                _logger.LogInformation($"Temperature is {_temp} C at: {DateTimeOffset.Now}");

                if (_temp > 27)
                {
                    _logger.LogInformation($"CAUTION: Temperature exceeding 27 C.");
                }

                await Task.Delay(60*1000, stoppingToken);
            }
        }
    }
}
