using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript.Classes.Behaviors
{
    public class Behavior
    {
        public virtual SCFlags Flags { get; set; } = SCFlags.None;
        public ShipControlUpdateData UpdateData = new ShipControlUpdateData();
    }
}
