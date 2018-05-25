using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StrategyCricket.Models
{
    public class GameViewModel
    {
        public List<int> Team_1_BatsmenIds { get; set; }
        public List<int> Team_2_BatsmenIds { get; set; }
        public List<int> Team_1_BowlerIds { get; set; }
        public List<int> Team_2_BowlerIds { get; set; }

    }
}