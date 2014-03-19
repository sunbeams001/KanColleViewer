using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper.Models
{
    public class ShipCriticalConditionEventArgs
    {
        public Ship Ship { get; private set; }

        public string FleetName { get; private set; }

        public ShipCriticalConditionEventArgs(Ship ship)
        {
            this.Ship = ship;
            //this.FleetName = fleetname;
        }
    }
}
