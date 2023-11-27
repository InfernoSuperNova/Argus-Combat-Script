
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using VRageMath;

namespace IngameScript.Classes
{
    [Flags]
    public enum SCFlags
    {
        None = 0, //Requires: target pos
        UnifiedTargetAndRefererencePosition = 1 << 1, //Requires: forward reference, target direction, if false aims in a specific direction instead
        UseGunsAsReference = 1 << 2, //Requires: guns
        CalculateLead = 1 << 3, //Requires: projectile velocity, target distance, target velocity
        PositionAroundTarget = 1 << 4, //Requires: target distance, target pos
        OverrideUpDownThrust = 1 << 5, //requires: updown thrust override
        OverrideLeftRightThrust = 1 << 6, //requires: leftright thrust override
        OverrideForwardBackwardThrust = 1 << 7, //requires: forwardbackward thrust override
        OverridePitch = 1 << 8, //requires: pitch override
        OverrideYaw = 1 << 9, //requires: yaw override
        OverrideRoll = 1 << 10, //requires: roll override
        AlignGravity = 1 << 11, //requires: gravity vector
        AvoidCollisions = 1 << 12, //requires: a list of positions to avoid
        ShootingEnabled = 1 << 13, //requires: guns
        UseManeuverThrustForAchievingDistance = 1 << 14, //requires: nothing!
        Bit15 = 1 << 15,
        Bit16 = 1 << 16,
        Bit17 = 1 << 17,
        Bit18 = 1 << 18,
        Bit19 = 1 << 19,
        Bit20 = 1 << 20,
        Bit21 = 1 << 21,
        Bit22 = 1 << 22,
        Bit23 = 1 << 23,
        Bit24 = 1 << 24,
        Bit25 = 1 << 25,
        Bit26 = 1 << 26,
        Bit27 = 1 << 27,
        Bit28 = 1 << 28,
        Bit29 = 1 << 29,
        Bit30 = 1 << 30,
        Bit31 = 1 << 31,
        Bit32 = 1 << 32
    }
    public struct ShipControlInitializationData
    {
        public List<IMyGyro> gyroscopes;
        public List<IMyThrust> thrusters;
        public List<IMyUserControllableGun> guns;
        public IMyTerminalBlock reference;
        public ClampedIntegralPID pitch;
        public ClampedIntegralPID yaw;
        public ClampedIntegralPID upDown;
        public ClampedIntegralPID leftRight;
        public ClampedIntegralPID forwardBackward;
        public double maxAngular;
        public MyGridProgram program;
        public double timeStep;
        public double fireAngleSigma;
    }

    public struct ShipControlUpdateData
    {
        public Vector3D aimPositionTargetPos;
        public Vector3D positionOnlyTargetPos;
        public double requiredDistance;
        public float upDownOverride;
        public float leftRightOverride;
        public float forwardBackwardOverride;
        public float pitchOverride;
        public float yawOverride;
        public float rollOverride;
        public double gravityDirection;
        public List<Vector3D> collidees;


        public Vector3D selfVelocity;
        public Vector3D targetVelocity;
        public float projectileVelocity;
    }


    public class ShipControl
    {
        public MyGridProgram program;
        public Thrusters Thrusters;
        public Gyroscopes Gyroscopes;
        public Guns Guns;
        public IMyTerminalBlock Reference;
        public double TimeStep;
        public double FireAngleSigma;

        public Vector3D PreviousTargetVelocity = Vector3D.Zero; //I really don't want this here
        public ShipControl(ShipControlInitializationData data)
        {
            Thrusters = new Thrusters(data.thrusters, data.reference, data.program, data.upDown, data.leftRight, data.forwardBackward);
            Gyroscopes = new Gyroscopes(data.reference, data.gyroscopes, data.pitch, data.yaw, data.maxAngular);
            Guns = new Guns(data.guns, data.program);
            Reference = data.reference;
            program = data.program;
            TimeStep = data.timeStep;
            FireAngleSigma = data.fireAngleSigma;
        }

        public void Update(SCFlags flags, ShipControlUpdateData data)
        {
            bool detachedPosition = false;
            Vector3D AimingReferencePos = Reference.GetPosition();
            Vector3D AimingDirection = Vector3D.Zero;
            if ((flags & SCFlags.UnifiedTargetAndRefererencePosition) != 0)
            {
                AimingDirection = (data.aimPositionTargetPos - AimingReferencePos).Normalized();
            }
            else
            {
                detachedPosition = true;
            }

            if ((flags & SCFlags.UseGunsAsReference) != 0)
            {
                AimingReferencePos = Guns.AreAvailable() ? Guns.GetAimingReferencePos() : AimingReferencePos;
            }
            if ((flags & SCFlags.CalculateLead) != 0)
            {
                //Lead calculation code
                Vector3D leadAim = TargetLeading.GetAimDirection(AimingReferencePos, data.selfVelocity, data.aimPositionTargetPos, data.targetVelocity, ref PreviousTargetVelocity, TimeStep, data.projectileVelocity);
                AimingDirection = leadAim.X != double.NaN ? leadAim : AimingDirection;
            }
            if ((flags & SCFlags.PositionAroundTarget) != 0)
            {

                Vector3D currentDistanceVector = data.aimPositionTargetPos - AimingReferencePos;
                if (detachedPosition)
                {
                    currentDistanceVector = data.positionOnlyTargetPos - AimingReferencePos;
                }

                Vector3D _TargetDirection = currentDistanceVector.Normalized();
                float currentDistance = (float)currentDistanceVector.Length();
                if ((flags & SCFlags.UseManeuverThrustForAchievingDistance) != 0)
                {
                    float error = (float)(currentDistance - data.requiredDistance);
                    float thrustErrorUp = Vector3.Dot(Reference.WorldMatrix.Up, _TargetDirection) * error;
                    float thrustErrorLeft = Vector3.Dot(Reference.WorldMatrix.Left, _TargetDirection) * error;
                    float thrustErrorForward = Vector3.Dot(Reference.WorldMatrix.Forward, _TargetDirection) * error;

                    Thrusters.CorrectError(thrustErrorUp, thrustErrorLeft, thrustErrorForward);

                }
                else
                {
                    float dot = Vector3.Dot(Reference.WorldMatrix.Forward, _TargetDirection);
                    float error = (float)(currentDistance - data.requiredDistance) * dot;
                    error = float.IsNaN(error) ? 0 : error;
                    Thrusters.CorrectErrorAxis(error, ThrusterAxis.ForwardBackward);
                }
            }



            if ((flags & SCFlags.ShootingEnabled) != 0)
            {
                if (Vector3D.Dot(Reference.WorldMatrix.Forward, AimingDirection.Normalized()) > FireAngleSigma)
                {
                    program.Echo("Firing!");
                    Guns.Fire();
                }
                else
                {
                    program.Echo("Holding Fire!");
                    Guns.Cancel();
                }
            }

            //OVERRIDES
            if ((flags & SCFlags.OverrideUpDownThrust) != 0)
            {
                Thrusters.SetThrustInAxis(data.upDownOverride, ThrusterAxis.UpDown);
            }
            if ((flags & SCFlags.OverrideLeftRightThrust) != 0)
            {
                Thrusters.SetThrustInAxis(data.leftRightOverride, ThrusterAxis.LeftRight);
            }
            if ((flags & SCFlags.OverrideForwardBackwardThrust) != 0)
            {
                Thrusters.SetThrustInAxis(data.forwardBackwardOverride, ThrusterAxis.ForwardBackward);
            }


            if (AimingDirection != Vector3D.Zero)
            {

                Gyroscopes.FaceShipTowards(AimingDirection.Normalized(), 0);
            }


            if ((flags & SCFlags.OverridePitch) != 0)
            {
                Gyroscopes.ApplyGyroPitch(data.pitchOverride);
            }
            if ((flags & SCFlags.OverrideYaw) != 0)
            {
                Gyroscopes.ApplyGyroYaw(data.yawOverride);
            }
            if ((flags & SCFlags.OverrideRoll) != 0)
            {
                Gyroscopes.ApplyGyroRoll(data.rollOverride);
            }


        }
    }
}
