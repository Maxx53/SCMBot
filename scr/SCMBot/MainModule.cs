using System;
using Nancy;

namespace SCMBot
{
    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get["/"] = parameters =>
            {
                var form = new GraphFrm();
                form.GetChart();
                return View["index"];
            };
        }
    }

}