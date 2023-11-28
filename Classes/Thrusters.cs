
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using VRageMath;

namespace IngameScript.Classes
{
    public enum ThrusterDir
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Backward

    }

    public enum ThrusterAxis
    {
        UpDown,
        LeftRight,
        ForwardBackward
    }

    public class Thrusters
    {
        float thrustDeadzone = 0.1f;

        public List<IMyThrust> UpThrust;
        public List<IMyThrust> DownThrust;
        public List<IMyThrust> LeftThrust;
        public List<IMyThrust> RightThrust;
        public List<IMyThrust> ForwardThrust;
        public List<IMyThrust> BackwardThrust;


        double[] ThrustMul = { 0, 0, 0, 0, 0, 0 };

        public MyGridProgram program;

        public ClampedIntegralPID UpDownPID;
        public ClampedIntegralPID LeftRightPID;
        public ClampedIntegralPID ForwardBackwardPID;
        public Thrusters(List<IMyThrust> AllThrust, IMyTerminalBlock ForwardReference, MyGridProgram program, ClampedIntegralPID UpDownPID, ClampedIntegralPID LeftRightPID, ClampedIntegralPID ForwardBackwardPID)
        {
            UpThrust = new List<IMyThrust>();
            DownThrust = new List<IMyThrust>();
            LeftThrust = new List<IMyThrust>();
            RightThrust = new List<IMyThrust>();
            ForwardThrust = new List<IMyThrust>();
            BackwardThrust = new List<IMyThrust>();




            double TotalUpThrust = 0;
            double TotalDownThrust = 0;
            double TotalLeftThrust = 0;
            double TotalRightThrust = 0;
            double TotalForwardThrust = 0;
            double TotalBackwardThrust = 0;
            double TotalThrust = 0;

            

            //1080000
            //7200000
            foreach (var thruster in AllThrust)
            {
                //compare the thruster direction to the controller direction

                if (thruster.WorldMatrix.Forward == -ForwardReference.WorldMatrix.Forward)
                {
                    ForwardThrust.Add(thruster);
                    TotalForwardThrust += thruster.MaxThrust;
                }
                else if (thruster.WorldMatrix.Forward == ForwardReference.WorldMatrix.Forward)
                {
                    BackwardThrust.Add(thruster);
                    TotalBackwardThrust += thruster.MaxThrust;
                }
                else if (thruster.WorldMatrix.Forward == -ForwardReference.WorldMatrix.Left)
                {
                    LeftThrust.Add(thruster);
                    TotalLeftThrust += thruster.MaxThrust;
                }
                else if (thruster.WorldMatrix.Forward == ForwardReference.WorldMatrix.Left)
                {
                    RightThrust.Add(thruster);
                    TotalRightThrust += thruster.MaxThrust;
                }
                else if (thruster.WorldMatrix.Forward == -ForwardReference.WorldMatrix.Up)
                {
                    UpThrust.Add(thruster);
                    TotalUpThrust += thruster.MaxThrust;
                }
                else if (thruster.WorldMatrix.Forward == ForwardReference.WorldMatrix.Up)
                {
                    DownThrust.Add(thruster);
                    TotalDownThrust += thruster.MaxThrust;
                }
                //there's probably a better solution lol


            }

            TotalThrust = TotalForwardThrust + TotalBackwardThrust + TotalLeftThrust + TotalRightThrust + TotalUpThrust + TotalDownThrust;

            ThrustMul[0] = 1 - TotalUpThrust / TotalThrust;
            ThrustMul[1] = 1 - TotalDownThrust / TotalThrust;
            ThrustMul[2] = 1 - TotalLeftThrust / TotalThrust;
            ThrustMul[3] = 1 - TotalRightThrust / TotalThrust;
            ThrustMul[4] = 1 - TotalLeftThrust / TotalThrust;
            ThrustMul[5] = 1 - TotalRightThrust / TotalThrust;

            this.program = program;

            this.UpDownPID = UpDownPID;
            this.LeftRightPID = LeftRightPID;
            this.ForwardBackwardPID = ForwardBackwardPID;
        }
        private List<IMyThrust> GetThrusterDir(ThrusterDir dir)
        {
            if (dir == ThrusterDir.Up)
            {
                return UpThrust;
            }
            else if (dir == ThrusterDir.Down)
            {
                return DownThrust;
            }
            else if (dir == ThrusterDir.Left)
            {
                return LeftThrust;
            }
            else if (dir == ThrusterDir.Right)
            {
                return RightThrust;
            }
            else if (dir == ThrusterDir.Forward)
            {
                return ForwardThrust;
            }
            else if (dir == ThrusterDir.Backward)
            {
                return BackwardThrust;
            }
            return null;
        }

        private ThrusterDir GetThrusterDirFromAxis(ThrusterAxis axis, int sign)
        {
            switch (axis)
            {
                case ThrusterAxis.UpDown:
                    if (sign > 0)
                    {
                        return ThrusterDir.Up;
                    }
                    else
                    {
                        return ThrusterDir.Down;
                    }
                case ThrusterAxis.LeftRight:
                    if (sign > 0)
                    {
                        return ThrusterDir.Left;
                    }
                    else
                    {
                        return ThrusterDir.Right;
                    }
                case ThrusterAxis.ForwardBackward:
                    if (sign > 0)
                    {
                        return ThrusterDir.Forward;
                    }
                    else
                    {
                        return ThrusterDir.Backward;
                    }
            }
            return ThrusterDir.Up;
        }

        public void SetThrustInDirection(float Thrust, ThrusterDir Dir)
        {
            var actualList = GetThrusterDir(Dir);
            IMyThrust[] thrusters = new IMyThrust[actualList.Count];
            actualList.CopyTo(thrusters);
            foreach (var thruster in thrusters)
            {
                if (thruster == null)
                {
                    actualList.Remove(thruster);
                    continue;
                }
                thruster.ThrustOverridePercentage = Thrust;
            }
        }
        
        public void SetThrustInAxis(float Thrust, ThrusterAxis Axis)
        {
            var sign = Math.Sign(Thrust);
            ThrusterDir MainThrust = GetThrusterDirFromAxis(Axis, sign);
            ThrusterDir OppositeThrust = GetThrusterDirFromAxis(Axis, -sign);
            SetThrustInDirection(Math.Abs(Thrust) - thrustDeadzone, MainThrust);
            SetThrustInDirection(0, OppositeThrust);
        }

        //Up, Down, Left, Right, Reference, Backward
        public void SetThrustInDirections(float[] Thrust)
        {
            for (int i = 0; i < Thrust.Length; i++) {
                SetThrustInDirection(Thrust[i], (ThrusterDir)i);
            }
        }
        //UpDown, LeftRight, ForwardBackward
        public void SetThrustInAxes(float[] Thrust)
        {
            for (int i = 0; i < Thrust.Length; i++)
            {
                SetThrustInAxis(Thrust[i], (ThrusterAxis)i);
            }
        }




        public void CorrectError(float thrustErrorUp, float thrustErrorLeft, float thrustErrorForward)
        {;
            //Axes with assymmetric thrust should be weighted differently to allow for unified PID values
            thrustErrorUp = (float)(Math.Sign(thrustErrorUp) == 1 ? thrustErrorUp * ThrustMul[0] : thrustErrorUp * ThrustMul[1]);
            thrustErrorLeft = (float)(Math.Sign(thrustErrorLeft) == 1 ? thrustErrorLeft * ThrustMul[2] : thrustErrorLeft * ThrustMul[3]);
            thrustErrorForward = (float)(Math.Sign(thrustErrorForward) == 1 ? thrustErrorForward * ThrustMul[4] : thrustErrorForward * ThrustMul[5]);
            float[] thrusterOverrides = { 0, 0, 0 };
            UpDownPID.Control(thrustErrorUp);
            LeftRightPID.Control(thrustErrorLeft);
            ForwardBackwardPID.Control(thrustErrorForward);
            thrusterOverrides[0] = (float)UpDownPID.Value;
            thrusterOverrides[1] = (float)LeftRightPID.Value;
            thrusterOverrides[2] = (float)ForwardBackwardPID.Value;
            SetThrustInAxes(thrusterOverrides);
        }

        public void CorrectErrorAxis(float thrustError, ThrusterAxis axis)
        {
            thrustError = (float)(Math.Sign(thrustError) == 1 ? thrustError * ThrustMul[(int)axis * 2] : thrustError * ThrustMul[(int)axis * 2 + 1]);
            ForwardBackwardPID.Control(thrustError); //Probably need to make structs for each thruster direction so that I don't have to arbitrarily choose a PID here
            SetThrustInAxis((float)ForwardBackwardPID.Value, axis);
        }

        public void DisableThrustOverrides()
        {
            foreach (var thruster in UpThrust)
            {
                thruster.ThrustOverride = 0;
            }
            foreach (var thruster in DownThrust)
            {
                thruster.ThrustOverride = 0;
            }
            foreach (var thruster in LeftThrust)
            {
                thruster.ThrustOverride = 0;
            }
            foreach (var thruster in RightThrust)
            {
                thruster.ThrustOverride = 0;
            }
            foreach (var thruster in ForwardThrust)
            {
                thruster.ThrustOverride = 0;
            }
            foreach (var thruster in BackwardThrust)
            {
                thruster.ThrustOverride = 0;
            }
    }
    
}
