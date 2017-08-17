using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.CorrespondenceReview
{
    public class Put
    {
        public string Body { get; set; }
        public bool? IsRecommended { get; set; }
    }
}
