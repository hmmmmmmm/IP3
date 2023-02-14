using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IP3_Group4.Models
{
    public class MainRequests
    {
        public List<Request> requests { get; set; }
    }

    public class Request
    {
        public Img image { get; set; }
        public List<Feature> features { get; set; }
    }

    public class Img
    {
        public string content { get; set; }
    }
    public class Feature
    {
        public string type { get; set; }
    }
}