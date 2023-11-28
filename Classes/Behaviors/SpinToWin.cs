using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageRender.Messages;
using IngameScript.Classes.Behaviors;
namespace IngameScript.Classes.Behaviors
{
    public class SpinToWin : Behavior
    {
        public override SCFlags Flags { get; set; } = SCFlags.PositionAroundTarget | SCFlags.UnifiedTargetAndRefererencePosition
                | SCFlags.CalculateLead | SCFlags.OverrideRoll | SCFlags.UseGunsAsReference | SCFlags.ShootingEnabled | SCFlags.OverrideUpDownThrust;

        public SpinToWin(MyGridProgram program)
        {
            UpdateData = new ShipControlUpdateData();
            UpdateData.upDownOverride = 1;
            UpdateData.rollOverride = 0.5f;
            UpdateData.requiredDistance = 1500;
        }
    }
}
