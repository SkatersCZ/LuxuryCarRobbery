using System;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.Math;
using GTA.UI;
using Screen = GTA.UI.Screen;
using System.Drawing;
using System.Collections.Specialized;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Net.Mime;

namespace LuxuryCarRobbery
{
    public class Main : Script
    {
        Blip CarDeliver;
        Vehicle TargetCar;
        Ped Target;
        MissionCase MissionProgress;
        int lastHelpTime = 0;
        int helpInterval = 5000;
        Blip MissionStart;
        enum MissionCase { Freemode, HitStart, HitEnd, StealCarStart, StealCarEnd, StealCarComplete }
        public Main()
        {
            Tick += onTick;
            KeyDown += onKeyDown;
        }
        private void onTick(object sender, EventArgs e)
        {
            switch (MissionProgress)
            {
                case MissionCase.Freemode:
                    {
                        if (MissionStart == null)
                        {
                            MissionStart = World.CreateBlip(new Vector3(982.0953f, -1535.729f, 30.69516f));
                        }
                        if (MissionStart != null)
                        {
                            MissionStart.Sprite = (BlipSprite)207;
                            MissionStart.Color = BlipColor.Yellow;
                            MissionStart.Name = "Luxury Car Robbery";
                        }
                        if (Game.Player.Character.Position.DistanceTo(MissionStart.Position) < 30)
                        {
                            World.DrawMarker(MarkerType.VerticalCylinder, new Vector3(982.0953f, -1535.729f, 30.69516f - 1f), Vector3.Zero, Vector3.Zero, new Vector3(2, 2, 1), Color.Aqua);
                        }
                        if (Game.Player.Character.Position.DistanceTo(MissionStart.Position) < 1.5f)
                        {
                            if (Game.GameTime > lastHelpTime + helpInterval)
                            {
                                lastHelpTime = Game.GameTime;
                                DisplayHelpText("Press ~INPUT_CONTEXT~ to start the Luxury Car Robbery");
                            }
                            if (Game.IsControlJustPressed(GTA.Control.Context))
                            {
                                MissionProgress = MissionCase.HitStart;
                            }
                        }
                    }
                    break;

                case MissionCase.HitStart:
                    {
                        MissionStart.Delete();
                        if (Target == null)
                        {
                            Target = World.CreatePed(PedHash.Business03AMY, new Vector3(-398.1156f, 142.9092f, 65.50865f - 1f), 177.3397f);
                        }
                        if (Target != null)
                        {
                            if (TargetCar == null)
                            {
                                TargetCar = World.CreateVehicle(VehicleHash.ItaliGTB, new Vector3(-407.2278f, 133.9522f, 65.17496f - 1f), 88.88081f);
                            }
                            Target.AddBlip();
                            Target.AttachedBlip.Color = BlipColor.Red;
                            Screen.ShowSubtitle("Kill the ~r~Target");
                            MissionProgress = MissionCase.HitEnd;
                        }
                        if (TargetCar.Health == 0)
                        {
                            Screen.ShowSubtitle("~r~Mission Failed");
                            Target.AttachedBlip.Delete();
                            TargetCar.AttachedBlip.Delete();
                            MissionProgress = MissionCase.Freemode;
                        }
                        if (Game.Player.Character.IsDead)
                        {
                            Screen.ShowSubtitle("~r~Mission Failed");
                            Target.AttachedBlip.Delete();
                            TargetCar.AttachedBlip.Delete();
                            MissionProgress = MissionCase.Freemode;
                        }
                    }
                    break;

                case MissionCase.HitEnd:
                    {
                        if (Target.IsFleeing)
                        {
                            Game.Player.WantedLevel = 3;
                        }
                        if (Target.IsDead)
                        {
                            Target.MarkAsNoLongerNeeded();
                            Target.AttachedBlip.Delete();
                            Game.Player.WantedLevel = 3;
                            MissionProgress = MissionCase.StealCarStart;
                        }
                        if (TargetCar.Health == 0)
                        {
                            Screen.ShowSubtitle("~r~Mission Failed");
                            Target.AttachedBlip.Delete();
                            TargetCar.AttachedBlip.Delete();
                            MissionProgress = MissionCase.Freemode;
                        }
                        if (Game.Player.Character.IsDead)
                        {
                            Screen.ShowSubtitle("~r~Mission Failed");
                            Target.AttachedBlip.Delete();
                            TargetCar.AttachedBlip.Delete();
                            MissionProgress = MissionCase.Freemode;
                        }
                    }
                    break;

                case MissionCase.StealCarStart:
                    {
                        if (TargetCar != null)
                        {
                            TargetCar.AddBlip();
                            TargetCar.AttachedBlip.Color = BlipColor.Green;
                            MissionProgress = MissionCase.StealCarEnd;
                        }
                    }
                    break;

                case MissionCase.StealCarEnd:
                    {
                        Screen.ShowSubtitle("Steal the ~g~Car");
                        if (Game.Player.Character.IsInVehicle(TargetCar))
                        {
                            MissionProgress = MissionCase.StealCarComplete;
                        }
                        if (TargetCar.Health == 0)
                        {
                            Screen.ShowSubtitle("~r~Mission Failed");
                            Target.AttachedBlip.Delete();
                            TargetCar.AttachedBlip.Delete();
                            MissionProgress = MissionCase.Freemode;
                        }
                        if (Game.Player.Character.IsDead)
                        {
                            Screen.ShowSubtitle("~r~Mission Failed");
                            Target.AttachedBlip.Delete();
                            TargetCar.AttachedBlip.Delete();
                            MissionProgress = MissionCase.Freemode;
                        }
                    }
                    break;
                case MissionCase.StealCarComplete:
                    {
                        if (CarDeliver == null)
                        {
                            CarDeliver = World.CreateBlip(new Vector3(974.5074f, -1533.99f, 30.67001f));
                        }
                        if (CarDeliver != null)
                        {
                            CarDeliver.ShowRoute = true;
                            Screen.ShowSubtitle("Deliver the ~g~Car ~w~to the location");
                        }
                        if (TargetCar.Position.DistanceTo(CarDeliver.Position) < 1.5f)
                        {
                            CarDeliver.Delete();
                            TargetCar.AttachedBlip.Delete();
                            Game.Player.Character.Task.LeaveVehicle();
                            TargetCar.LockStatus = VehicleLockStatus.CannotEnter;
                            Screen.ShowSubtitle("~g~Mission Successful");
                            TargetCar.MarkAsNoLongerNeeded();
                            Game.Player.Money += 500000;
                            MissionProgress = MissionCase.Freemode;
                        }
                        if (TargetCar.Health == 0)
                        {
                            Screen.ShowSubtitle("~r~Mission Failed");
                            Target.AttachedBlip.Delete();
                            TargetCar.AttachedBlip.Delete();
                            MissionProgress = MissionCase.Freemode;
                        }
                        if (Game.Player.Character.IsDead)
                        {
                            Screen.ShowSubtitle("~r~Mission Failed");
                            Target.AttachedBlip.Delete();
                            TargetCar.AttachedBlip.Delete();
                            MissionProgress = MissionCase.Freemode;
                        }
                    }
                    break;
            }
        }
        private void onKeyDown(object sender, KeyEventArgs e)
        {

        }

        public static void DisplayHelpText(string text)
        {
            InputArgument[] arguments = new InputArgument[] { "STRING" };
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, arguments);
            InputArgument[] argumentArray2 = new InputArgument[] { text };
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, argumentArray2);
            InputArgument[] argumentArray3 = new InputArgument[] { 0, 0, 1, -1, };
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_HELP, argumentArray3);
        }
    }
}