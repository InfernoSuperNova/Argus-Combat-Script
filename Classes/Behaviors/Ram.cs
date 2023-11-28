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
    public class Ram : Behavior
    {
        public override SCFlags Flags { get; set; } = SCFlags.PositionAroundTarget | SCFlags.UnifiedTargetAndRefererencePosition | SCFlags.UseManeuverThrustForAchievingDistance | SCFlags.RollToCancelVelocity;
        public Ram(MyGridProgram program)
        {
            UpdateData = new ShipControlUpdateData();
            UpdateData.requiredDistance = -100;
            UpdateData.cancelThrustDir = ThrusterDir.Up;
        }
    }
}
