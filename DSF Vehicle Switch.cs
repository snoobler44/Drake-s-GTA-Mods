using System;
using System.Windows.Forms;
using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;
using Drake_s_Mod_Menu;

namespace DSF_Vehicle_Switch {
    public class DSF : Script {
        ScriptSettings config;
        Keys Switch;

        bool modActive = false;
        bool keyup = false;
        bool keyUpController = false;
        bool cameraToggle = false;
        bool effectToggle;

        int on;

        Ped Player = Game.Player.Character;
        Camera main = new Camera(0);

        Vehicle[] vehicles;


        public DSF() {
            this.Tick += cameraMovement;
            this.Tick += vehicleDetection;
            this.Tick += controllerSupport;
            this.KeyUp += keyUp;
            this.KeyUp += modActivateKey;

            config = ScriptSettings.Load("scripts\\DSF Vehicle Switch.ini");
            Switch = config.GetValue("Options", "switch", Switch);
            effectToggle = config.GetValue("Options", "effect", effectToggle);
        }

        private void modActivateKey(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter && Mod_Menu.modActivator() == 3 && on == 0) {
                modActivate();
            }
            if (e.KeyCode == Keys.Enter && Mod_Menu.modActivator() == 0 && on == 1) {
                modActivate();
            }
        }

        private void modActivate() {
            if (Mod_Menu.modActivator() == 3) {
                modActive = true;
                on = 1;
                UI.ShowSubtitle("~y~Active~w~");

                if (modActive) {
                    main = World.CreateCamera(Game.Player.Character.Position + new Vector3(5, 5, 5), new Vector3(0, 0, 0), 80f);
                } else {
                    Function.Call(Hash.RENDER_SCRIPT_CAMS, 0, 1, 500, false, false);
                    Function.Call(Hash._STOP_SCREEN_EFFECT, "RaceTurbo");
                    main.Destroy();
                    Game.TimeScale = 1f;
                }
            } else {
                modActive = false;
                on = 0;
                Function.Call(Hash.RENDER_SCRIPT_CAMS, 0, 1, 600, 1, 0);
                Function.Call(Hash._STOP_SCREEN_EFFECT, "RaceTurbo");
                UI.ShowSubtitle("~w~Deactive~w~");
            }
        }

        private void cameraMovement(object sender, EventArgs e) {
            if (modActive) {
                if (Game.IsKeyPressed(Switch) && !keyup ||
                    Game.IsControlPressed(2, GTA.Control.ScriptLS) && !keyUpController) {
                    cameraToggle = !cameraToggle;

                    if (cameraToggle) {
                        main.Position = Player.Position + new Vector3(0, 0, 15);
                        if (effectToggle == true) {
                            Function.Call(Hash._START_SCREEN_EFFECT, "RaceTurbo", 10000, true);
                        }
                    }

                    keyup = true;
                    keyUpController = true;
                }

                if (cameraToggle) {
                    UI.DrawTexture("scripts\\DSF Files\\crosshair.png", 0, 0, 100, new Point(495, 260), new Size(295, 200), 0f, Color.Orange);

                    Game.DisableControlThisFrame(2, GTA.Control.MoveUpDown);
                    Game.DisableControlThisFrame(2, GTA.Control.MoveDownOnly);
                    Game.DisableControlThisFrame(2, GTA.Control.MoveLeftRight);
                    Game.DisableControlThisFrame(2, GTA.Control.Cover);
                    Game.DisableControlThisFrame(2, GTA.Control.Duck);
                    Game.DisableControlThisFrame(2, GTA.Control.Attack);
                    Game.DisableControlThisFrame(2, GTA.Control.Aim);

                    Game.TimeScale = 0.05f;
                    Function.Call(Hash.RENDER_SCRIPT_CAMS, 1, 1, 600, false, false);

                    Function.Call(Hash._DISABLE_FIRST_PERSON_CAM_THIS_FRAME);
                    Function.Call(Hash._DISABLE_VEHICLE_FIRST_PERSON_CAM_THIS_FRAME);

                    World.RenderingCamera.Rotation = GameplayCamera.Rotation;

                    // left = -1
                    // right = 1
                    // up = -1
                    // down = 1

                    Vector3 yAxis = World.RenderingCamera.Direction / 1.2f;
                    Vector3 xAxis = new Vector3(-World.RenderingCamera.Direction.Y, World.RenderingCamera.Direction.X, 0) / 1.2f;
                    Vector3 zAxis = new Vector3(0, 0, 0.5f);

                    // cam movement
                    if (Game.IsKeyPressed(Keys.W) && !Game.IsKeyPressed(Keys.S) ||
                        Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) < 0) {
                        World.RenderingCamera.Position += yAxis;
                    }
                    if (Game.IsKeyPressed(Keys.S) && !Game.IsKeyPressed(Keys.W) ||
                        Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) > 0) {
                        World.RenderingCamera.Position -= yAxis;
                    }
                    if (Game.IsKeyPressed(Keys.A) && !Game.IsKeyPressed(Keys.D) ||
                        Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) < 0) {
                        World.RenderingCamera.Position += xAxis;
                    }
                    if (Game.IsKeyPressed(Keys.D) && !Game.IsKeyPressed(Keys.A) ||
                        Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) > 0) {
                        World.RenderingCamera.Position -= xAxis;
                    }

                    if (Game.IsKeyPressed(Keys.Space) && !Game.IsKeyPressed(Keys.ControlKey) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRUp)) {
                        World.RenderingCamera.Position += zAxis;
                    }
                    if (Game.IsKeyPressed(Keys.ControlKey) && !Game.IsKeyPressed(Keys.Space) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRDown)) {
                        World.RenderingCamera.Position -= zAxis;
                    }

                    if (Game.IsKeyPressed(Keys.W) && Game.IsKeyPressed(Keys.A) ||
                        Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) < 0 && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) < 0) {
                        World.RenderingCamera.Position += (yAxis + xAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.W) && Game.IsKeyPressed(Keys.D) ||
                        Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) < 0 && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) > 0) {
                        World.RenderingCamera.Position += (yAxis - xAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.S) && Game.IsKeyPressed(Keys.A) ||
                        Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) > 0 && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) < 0) {
                        World.RenderingCamera.Position -= (yAxis - xAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.S) && Game.IsKeyPressed(Keys.D) ||
                        Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) > 0 && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) > 0) {
                        World.RenderingCamera.Position -= (yAxis + xAxis) / 1.2f;
                    }

                    if (Game.IsKeyPressed(Keys.Space) && Game.IsKeyPressed(Keys.W) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRUp) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) < 0) {
                        World.RenderingCamera.Position += (zAxis + yAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.Space) && Game.IsKeyPressed(Keys.S) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRUp) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) > 0) {
                        World.RenderingCamera.Position += (zAxis - yAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.Space) && Game.IsKeyPressed(Keys.D) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRUp) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) > 0) {
                        World.RenderingCamera.Position += (zAxis - xAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.Space) && Game.IsKeyPressed(Keys.A) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRUp) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) < 0) {
                        World.RenderingCamera.Position += (zAxis + xAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.ControlKey) && Game.IsKeyPressed(Keys.W) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRDown) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) < 0) {
                        World.RenderingCamera.Position -= (zAxis - yAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.ControlKey) && Game.IsKeyPressed(Keys.S) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRDown) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) > 0) {
                        World.RenderingCamera.Position -= (zAxis + yAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.ControlKey) && Game.IsKeyPressed(Keys.D) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRDown) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) > 0) {
                        World.RenderingCamera.Position -= (zAxis + xAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.ControlKey) && Game.IsKeyPressed(Keys.A) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRDown) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) < 0) {
                        World.RenderingCamera.Position -= (zAxis - xAxis) / 1.2f;
                    }

                    if (Game.IsKeyPressed(Keys.Space) && Game.IsKeyPressed(Keys.W) && Game.IsKeyPressed(Keys.A) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRUp) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) < 0 && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) < 0) {
                        World.RenderingCamera.Position += (xAxis + yAxis + zAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.Space) && Game.IsKeyPressed(Keys.W) && Game.IsKeyPressed(Keys.D) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRUp) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) < 0 && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) > 0) {
                        World.RenderingCamera.Position += (-xAxis + yAxis + zAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.Space) && Game.IsKeyPressed(Keys.S) && Game.IsKeyPressed(Keys.A) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRUp) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) > 0 && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) < 0) {
                        World.RenderingCamera.Position += (xAxis - yAxis + zAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.Space) && Game.IsKeyPressed(Keys.S) && Game.IsKeyPressed(Keys.D) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRUp) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) > 0 && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) > 0) {
                        World.RenderingCamera.Position += (-xAxis - yAxis + zAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.ControlKey) && Game.IsKeyPressed(Keys.W) && Game.IsKeyPressed(Keys.A) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRDown) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) < 0 && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) < 0) {
                        World.RenderingCamera.Position -= (-xAxis - yAxis + zAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.ControlKey) && Game.IsKeyPressed(Keys.W) && Game.IsKeyPressed(Keys.D) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRDown) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) < 0 && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) > 0) {
                        World.RenderingCamera.Position -= (xAxis - yAxis + zAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.ControlKey) && Game.IsKeyPressed(Keys.S) && Game.IsKeyPressed(Keys.A) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRDown) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) > 0 && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) < 0) {
                        World.RenderingCamera.Position -= (-xAxis + yAxis + zAxis) / 1.2f;
                    }
                    if (Game.IsKeyPressed(Keys.ControlKey) && Game.IsKeyPressed(Keys.S) && Game.IsKeyPressed(Keys.D) ||
                        Game.IsControlPressed(2, GTA.Control.ScriptRDown) && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisY) > 0 && Game.GetControlNormal(2, GTA.Control.ScriptLeftAxisX) > 0) {
                        World.RenderingCamera.Position -= (xAxis + yAxis + zAxis) / 1.2f;
                    }

                } else {
                    Game.TimeScale = 1f;

                    Function.Call(Hash.RENDER_SCRIPT_CAMS, 0, 1, 600, 1, 0);
                    Function.Call(Hash._STOP_SCREEN_EFFECT, "RaceTurbo");
                }
            }
        }

        private void vehicleDetection(object sender, EventArgs e) {
            if (modActive) {
                try {
                    if (cameraToggle) {
                        Game.DisableControlThisFrame(1, GTA.Control.Attack);
                        vehicles = World.GetNearbyVehicles(World.RenderingCamera.Position, 80f);
                        RaycastResult ray = World.RaycastCapsule(World.RenderingCamera.Position + new Vector3(0, 0, 1.5f), World.RenderingCamera.Direction, 100f, 1.0f, IntersectOptions.Mission_Entities);

                        foreach (Vehicle v in vehicles) {
                            UIText vehicleName = new UIText(v.FriendlyName + ", " + v.ClassType.ToString(), new Point(110, 540), 0.63f, Color.White, GTA.Font.HouseScript, true);
                            if (ray.HitEntity == v) {
                                vehicleName.Draw();

                                if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                                    v.Driver.Delete();
                                    Player.SetIntoVehicle(v, VehicleSeat.Driver);
                                    Player.CurrentVehicle.MaxHealth = 901;
                                    cameraToggle = false;
                                }

                                World.DrawMarker(MarkerType.UpsideDownCone, (v.Position + new Vector3(0f, 0f, v.Model.GetDimensions().Z + 1)), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f),
                                    new Vector3(0.4f, 0.4f, 0.4f), Color.Orange, false, false, 0, false, "", "", false);
                            }
                        }
                    }
                    foreach (Vehicle s in vehicles) {
                        if (!Player.IsInVehicle() || Game.IsEnabledControlJustPressed(1, GTA.Control.VehicleExit)) {
                            s.MaxHealth = 900;
                        }

                        if (s.IsSeatFree(VehicleSeat.Driver) && s.MaxHealth == 901) {
                            s.MaxHealth = 900;
                            s.CreateRandomPedOnSeat(VehicleSeat.Driver);
                            if (s.Driver != Player) {
                                s.Driver.Task.ReactAndFlee(Player);
                            }
                        }
                    }
                } catch {
                    Exception why;
                }
            }
        }

        private void keyUp(object sender, KeyEventArgs e) {
            if (modActive) {
                if (e.KeyCode == Switch) {
                    keyup = false;
                }
            }
        }

        private void controllerSupport(object sender, EventArgs e) {
            // Activate / Deactivate
            if (Game.IsControlPressed(2, GTA.Control.ScriptRDown) && Mod_Menu.modActivator() == 3 && on == 0 && !keyUpController) {
                modActivate();
                keyUpController = true;
            }
            if (Game.IsControlPressed(2, GTA.Control.ScriptRDown) && Mod_Menu.modActivator() == 0 && on == 1 && !keyUpController) {
                modActivate();
                keyUpController = true;
            }



            if (Game.CurrentInputMode == InputMode.GamePad) {
                if (Game.IsControlJustReleased(2, GTA.Control.ScriptLS) || Game.IsControlJustReleased(2, GTA.Control.ScriptRDown) || Game.IsControlJustReleased(2, GTA.Control.ScriptRightAxisX)) {
                    keyUpController = false;
                }
            }
        }
    }
}
