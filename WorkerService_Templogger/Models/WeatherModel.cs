using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerService_Templogger.Models
{
    public class WeatherModel
    {
        public Current current { get; set; }
    }

    public class Current
    {
        public float temp { get; set; }
    }

}
