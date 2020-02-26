using System;
using System.Windows.Forms;
using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;
using Drake_s_Mod_Menu;
namespace Whistle_Spear {

    public class Spear : Script {
        // ini
        ScriptSettings config;
        Keys activeButton;
        Keys sheathe;
        Keys powerModeUp;
        Keys powerModeDown;
        Keys unstuck;

        // Bool triggers + buttons
        bool modActive = false;
        bool spearActive = false;
        bool keyUp = false;
        bool keyUpController = false;
        bool unsheathed;
        bool finToggle;
        bool superSpeed;
        bool superJump;

        // vars
        Ped[] pedsToTarget;
        Vehicle[] vehiclesToTarget;

        Vector3 comparePositionFront = new Vector3();
        Vector3 comparePositionBack = new Vector3();
        Vector3 comparePositionLeft = new Vector3();
        Vector3 comparePositionRight = new Vector3();
        Vector3 comparePositionTop = new Vector3();
        Vector3 comparePositionCenter = new Vector3();
        Vector3 comparePositionPed = new Vector3();

        Vector3 vehCompPosFront = new Vector3();
        Vector3 vehCompPosBack = new Vector3();
        Vector3 vehCompPosLeft = new Vector3();
        Vector3 vehCompPosRight = new Vector3();
        Vector3 vehCompPosTop = new Vector3();

        float distFront;
        float distBack;
        float distLeft;
        float distRight;
        float distPed;
        float distTop;
        float distCenter;
        float pedTargetDistance;
        float vehTargetDistance;

        int powerIndex;
        int speed;
        int r;
        int a;
        int on;

        Prop spear;
        Prop fin;

        public Spear() {
            this.Tick += onTick;
            this.KeyUp += onKeyUp;
            this.KeyUp += up2;
            this.KeyDown += onKeyDown;
            this.Tick += spearAttackModes;

            config = ScriptSettings.Load("scripts\\Whistle Spear.ini");
            activeButton = config.GetValue("Options", "activeButton", activeButton);
            sheathe = config.GetValue("Options", "sheathe", sheathe);
            powerModeUp = config.GetValue("Options", "powerModeUp", powerModeUp);
            powerModeDown = config.GetValue("Options", "powerModeDown", powerModeDown);
            unstuck = config.GetValue("Options", "unstuck", unstuck);
            finToggle = config.GetValue("Options", "fin", finToggle);
            pedTargetDistance = config.GetValue("Options", "ped target distance", pedTargetDistance);
            vehTargetDistance = config.GetValue("Options", "vehicle target distance", vehTargetDistance);
            superSpeed = config.GetValue("Options", "super speed", superSpeed);
            superJump = config.GetValue("Options", "super jump", superJump);
        }

        // Turn mod on/off
        private void modActivate() {
            if (Mod_Menu.modActivator() == 2) {
                modActive = true;
                on = 1;

                Game.Player.Character.Health = 300;
                Game.Player.Character.MaxHealth = 300;

                spear = World.CreateProp("prop_fnccorgm_02pole", Game.Player.Character.Position + new Vector3(-15, -15, -5), false, false);

                spear.AddBlip();
                spear.CurrentBlip.Color = BlipColor.White;
                spear.CurrentBlip.Scale = 0.7f;
                spear.CurrentBlip.Name = "Spear";

                if (finToggle == true) {
                    fin = World.CreateProp("prop_proxy_hat_01", Game.Player.Character.Position + new Vector3(0, 0, 0), false, false);
                    fin.AttachTo(Game.Player.Character, Game.Player.Character.GetBoneIndex(Bone.SKEL_Head),
                        new Vector3(0.15f, -0.01f, 0.005f),
                        new Vector3(0, 90, -25)
                        );
                }

                UI.ShowSubtitle("~r~Active~w~");
            } else {
                modActive = false;
                on = 0;

                spear.CurrentBlip.Remove();
                spear.Delete();

                if (finToggle == true) {
                    fin.Delete();
                }

                Game.Player.Character.CanRagdoll = true;

                UI.ShowSubtitle("~w~Deactive~w~");
            }
        }

        private void up2(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter && Mod_Menu.modActivator() == 2 && on == 0) {
                modActivate();
            }
            if (e.KeyCode == Keys.Enter && Mod_Menu.modActivator() == 0 && on == 1) {
                modActivate();
            }
        }

        private void onTick(object sender, EventArgs e) {
            if (Game.CurrentInputMode == InputMode.GamePad) {
                // Select button
                if (Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && Mod_Menu.modActivator() == 2 && on == 0 && !keyUp) {
                    modActivate();
                    keyUp = true;
                }
                if (Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && Mod_Menu.modActivator() == 0 && on == 1 && !keyUp) {
                    modActivate();
                    keyUp = true;
                }
                if (Game.IsControlJustReleased(2, GTA.Control.ScriptRDown)) {
                    keyUp = false;
                }
            }

            if (modActive == true) {
                pedsToTarget = World.GetNearbyPeds(Game.Player.Character, pedTargetDistance);
                vehiclesToTarget = World.GetNearbyVehicles(Game.Player.Character, vehTargetDistance);

                speed = Function.Call<int>(Hash.GET_ENTITY_SPEED, spear);

                Game.DisableControlThisFrame(2, GTA.Control.Aim);
                Game.DisableControlThisFrame(81, GTA.Control.Cover);
                Game.DisableControlThisFrame(17, GTA.Control.Duck);
                Game.DisableControlThisFrame(2, GTA.Control.LookBehind);
                Game.DisableControlThisFrame(1, GTA.Control.Phone);

                if (superSpeed) {
                    Game.Player.SetRunSpeedMultThisFrame(1.49f);
                }
                if (superJump) {
                    Game.Player.SetSuperJumpThisFrame();
                }
                
                Game.Player.Character.CanRagdoll = false;

                Function.Call(Hash.REMOVE_WEAPON_FROM_PED, Game.Player.Character, 0xFBAB5776);

                UI.DrawTexture("scripts\\Spear Files\\crosshair.png", 0, 0, 100, new Point(615, 320), new Size(55, 55), 0, Color.White);

                // Target method
                try {
                    // Peds
                    foreach (Ped p in pedsToTarget) {
                        if (p != Game.Player.Character) {
                            distPed = p.Position.DistanceTo(GameplayCamera.Position);
                            comparePositionPed = GameplayCamera.Position + GameplayCamera.Direction * distPed;

                            if (powerIndex != 2) {
                                if (spear.IsAttachedTo(p) && !p.IsRagdoll || !p.IsRagdoll && p.MaxHealth == 1) {
                                    p.Kill();
                                }
                            }

                            if (p.IsDead && p.IsOnFire) {
                                Function.Call(Hash.STOP_ENTITY_FIRE, p);
                            }

                            if (!spear.IsAttachedTo(p)) {
                                if (powerIndex == 4) {
                                    Game.DisableControlThisFrame(1, GTA.Control.Attack);

                                    if (p.IsAlive && p.CanFlyThroughWindscreen == false) {
                                        World.DrawMarker(MarkerType.UpsideDownCone, (p.Position + new Vector3(0f, 0f, 1.5f)), new Vector3(0f, 0f, 0f),
                                            new Vector3(0f, 0f, 0f), new Vector3(0.2f, 0.2f, 0.2f), Color.Red, false, false, 0, false, "", "", false);
                                    }

                                    if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                                        p.CanFlyThroughWindscreen = false;

                                    } else if (Game.IsDisabledControlJustPressed(1, GTA.Control.Aim) && p.CanFlyThroughWindscreen == false) {
                                        spear.Detach();
                                        spear.ApplyForce((p.Position - spear.Position) * 5);

                                        if (spear.IsAttachedTo(p) && p.CanFlyThroughWindscreen == false) {
                                            Function.Call(Hash.SET_PED_TO_RAGDOLL, p, 300, 0, 2);
                                            p.ApplyForce(Vector3.WorldDown * 500);
                                            p.MaxHealth = 1;
                                        }
                                    }
                                }

                                if (p.Position.DistanceTo(comparePositionPed) < 0.5 && p.IsAlive && !p.IsSittingInVehicle() && !p.IsInVehicle()) {
                                    World.DrawMarker(MarkerType.UpsideDownCone, (p.Position + new Vector3(0f, 0f, 1.5f)), new Vector3(0f, 0f, 0f),
                                        new Vector3(0f, 0f, 0f), new Vector3(0.2f, 0.2f, 0.2f), Color.White, false, false, 0, false, "", "", false);

                                    if (Game.IsDisabledControlPressed(2, GTA.Control.Aim) && !Game.Player.Character.IsSittingInVehicle()) {
                                        if (unsheathed == true || !spear.IsAttachedTo(Game.Player.Character)) {
                                            if (powerIndex != 4) {
                                                spear.Detach();
                                                spear.ApplyForce((p.Position - spear.Position) * 5);
                                            }

                                            if (spear.IsInRangeOf(p.Position, 5f)) {
                                                spear.AttachTo(p, p.GetBoneIndex(Bone.SKEL_Head),
                                                new Vector3(0, 1f, 0),
                                                new Vector3(90, 0, 0)
                                                );

                                                if (powerIndex == 0) {
                                                    Function.Call(Hash.SET_PED_TO_RAGDOLL, p, 300, 0, 2);
                                                    p.ApplyForce(GameplayCamera.Direction * 15f);
                                                    p.MaxHealth = 1;
                                                }
                                            }

                                            // sounds
                                            if (Game.IsDisabledControlJustPressed(2, GTA.Control.Aim)) {
                                                NAudio.Wave.WaveFileReader attack = new NAudio.Wave.WaveFileReader(@"scripts\Spear Files\attack2.wav");
                                                NAudio.Wave.WaveChannel32 waveAt = new NAudio.Wave.WaveChannel32(attack);
                                                NAudio.Wave.DirectSoundOut output2 = new NAudio.Wave.DirectSoundOut();

                                                output2.Init(waveAt);
                                                output2.Play();
                                                waveAt.Volume = 0.05f;
                                            }

                                            if (powerIndex == 1) {
                                                if (spear.IsAttachedTo(p)) {
                                                    Function.Call(Hash.SET_PED_TO_RAGDOLL, p, 300, 0, 2);
                                                    p.ApplyForce(Vector3.WorldDown * 500);
                                                    p.MaxHealth = 1;
                                                }
                                            }

                                            if (powerIndex == 2) {
                                                if (spear.IsAttachedTo(p)) {
                                                    World.AddExplosion(spear.Position, ExplosionType.Grenade, 10f, 1f);
                                                }
                                            }

                                            Model tmp = new Model("S_M_Y_Fireman_01");
                                            if (powerIndex == 3) {
                                                if (spear.IsAttachedTo(p) && p.IsAlive) {
                                                    Function.Call(Hash.START_ENTITY_FIRE, p);
                                                    if (!p.IsOnFire) {
                                                        p.Kill();
                                                    }
                                                }

                                                if (p.Model.Equals(tmp)) {
                                                    p.Kill();
                                                }
                                            }

                                            World.DrawMarker(MarkerType.UpsideDownCone, (p.Position + new Vector3(0f, 0f, 1.5f)), new Vector3(0f, 0f, 0f),
                                                new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0.2f, 0.2f, 0.2f), Color.Red, false, false, 0, false, "", "", false);
                                        } else {
                                            World.DrawMarker(MarkerType.UpsideDownCone, (p.Position + new Vector3(0f, 0f, 1.5f)), new Vector3(0f, 0f, 0f),
                                                new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0.2f, 0.2f, 0.2f), Color.Blue, false, false, 0, false, "", "", false);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Vehicles
                    foreach (Vehicle v in vehiclesToTarget) {
                        vehCompPosFront = v.GetOffsetInWorldCoords(Vector3.RelativeFront * v.Model.GetDimensions().Y * 0.4f);
                        distFront = vehCompPosFront.DistanceTo(GameplayCamera.Position);
                        comparePositionFront = GameplayCamera.Position + GameplayCamera.Direction * distFront;

                        vehCompPosBack = v.GetOffsetInWorldCoords(Vector3.RelativeBack * v.Model.GetDimensions().Y * 0.4f);
                        distBack = vehCompPosBack.DistanceTo(GameplayCamera.Position);
                        comparePositionBack = GameplayCamera.Position + GameplayCamera.Direction * distBack;

                        vehCompPosLeft = v.GetOffsetInWorldCoords(Vector3.RelativeLeft * v.Model.GetDimensions().X * 0.4f);
                        distLeft = vehCompPosLeft.DistanceTo(GameplayCamera.Position);
                        comparePositionLeft = GameplayCamera.Position + GameplayCamera.Direction * distLeft;

                        vehCompPosRight = v.GetOffsetInWorldCoords(Vector3.RelativeRight * v.Model.GetDimensions().X * 0.4f);
                        distRight = vehCompPosRight.DistanceTo(GameplayCamera.Position);
                        comparePositionRight = GameplayCamera.Position + GameplayCamera.Direction * distRight;

                        vehCompPosTop = v.GetOffsetInWorldCoords(Vector3.RelativeRight * v.Model.GetDimensions().Z * 0.4f);
                        distTop = vehCompPosTop.DistanceTo(GameplayCamera.Position);
                        comparePositionTop = GameplayCamera.Position + GameplayCamera.Direction * distTop;

                        distCenter = v.Position.DistanceTo(GameplayCamera.Position);
                        comparePositionCenter = GameplayCamera.Position + GameplayCamera.Direction * distCenter;

                        if (vehCompPosFront.DistanceTo(comparePositionFront) < 0.5 && v.IsAlive ||
                            vehCompPosBack.DistanceTo(comparePositionBack) < 0.5 && v.IsAlive ||
                            vehCompPosLeft.DistanceTo(comparePositionLeft) < 0.5 && v.IsAlive ||
                            vehCompPosRight.DistanceTo(comparePositionRight) < 0.5 && v.IsAlive ||
                            vehCompPosTop.DistanceTo(comparePositionTop) < 0.5 && v.IsAlive ||
                            v.Position.DistanceTo(comparePositionCenter) < 0.5 && v.IsAlive) {
                            World.DrawMarker(MarkerType.UpsideDownCone, (v.Position + new Vector3(0f, 0f, 2f)), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f),
                            new Vector3(0.4f, 0.4f, 0.4f), Color.White, false, false, 0, false, "", "", false);

                            if (Game.IsDisabledControlPressed(2, GTA.Control.Aim) && !Game.Player.Character.IsSittingInVehicle()) {
                                if (unsheathed == true || !spear.IsAttachedTo(Game.Player.Character)) {
                                    spear.Detach();
                                    spear.ApplyForce((v.Position - spear.Position) * 5);

                                    if (spear.IsInRangeOf(v.Position, 5f)) {
                                        if (powerIndex == 1 || powerIndex == 2) {
                                            spear.AttachTo(v, v.GetBoneIndex("engine"), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                                            v.ApplyForceRelative(Vector3.WorldDown * 50.0f);
                                            // spearCrush
                                            if (powerIndex == 1) {
                                                v.IsInvincible = true;
                                            }
                                        }
                                    }

                                    // sounds (v)
                                    if (Game.IsDisabledControlJustPressed(2, GTA.Control.Aim)) {
                                        NAudio.Wave.WaveFileReader attack = new NAudio.Wave.WaveFileReader(@"scripts\Spear Files\attack2.wav");
                                        NAudio.Wave.WaveChannel32 waveAt = new NAudio.Wave.WaveChannel32(attack);
                                        NAudio.Wave.DirectSoundOut output2 = new NAudio.Wave.DirectSoundOut();

                                        output2.Init(waveAt);
                                        output2.Play();

                                        waveAt.Volume = 0.05f;
                                    }

                                    // spearNormal
                                    if (powerIndex == 0 || powerIndex == 3) {
                                        if (v.Driver.IsAlive || !v.Driver.IsAlive && !v.IsSeatFree(VehicleSeat.Driver)) {
                                            if (spear.IsInRangeOf(v.Position, 5f)) {
                                                spear.AttachTo(v.Driver, v.Driver.GetBoneIndex(Bone.SKEL_Head),
                                                    new Vector3(0, 1f, 0),
                                                    new Vector3(90, 0, 0)
                                                    );
                                                v.SmashWindow(VehicleWindow.FrontLeftWindow);
                                            }
                                        }

                                        if (v.IsSeatFree(VehicleSeat.Driver) && spear.IsInRangeOf(v.Position, 5)) {
                                            if (!v.IsDoorBroken(VehicleDoor.FrontLeftDoor)) {
                                                spear.AttachTo(v, v.GetBoneIndex("handle_dside_f"), new Vector3(0.5f, 0, 0), new Vector3(0, -90, 0));
                                            } else {
                                                spear.AttachTo(v, v.GetBoneIndex("engine"), new Vector3(0.5f, 0, 0), new Vector3(0, -90, 0));
                                            }
                                        }
                                    }

                                    // spearFire
                                    if (powerIndex == 3) {
                                        if (v.Driver.IsAlive || !v.Driver.IsAlive && !v.IsSeatFree(VehicleSeat.Driver)) {
                                            if (spear.IsInRangeOf(v.Position, 5f)) {
                                                spear.AttachTo(v.Driver, v.Driver.GetBoneIndex(Bone.SKEL_Head),
                                                    new Vector3(0, 1f, 0),
                                                    new Vector3(90, 0, 0)
                                                    );
                                                v.SmashWindow(VehicleWindow.FrontLeftWindow);
                                                if (!v.IsTireBurst(0)) {
                                                    v.BurstTire(0);
                                                }
                                                v.Driver.Kill();
                                                Function.Call(Hash.START_ENTITY_FIRE, v.Driver);
                                            }
                                        }

                                        if (v.IsSeatFree(VehicleSeat.Driver) && spear.IsInRangeOf(v.Position, 5)) {
                                            if (!v.IsDoorBroken(VehicleDoor.FrontLeftDoor)) {
                                                spear.AttachTo(v, v.GetBoneIndex("handle_dside_f"), new Vector3(0.5f, 0, 0), new Vector3(0, -90, 0));
                                            } else {
                                                spear.AttachTo(v, v.GetBoneIndex("engine"), new Vector3(0.5f, 0, 0), new Vector3(0, -90, 0));
                                            }
                                        }
                                    }

                                    // spearExplode
                                    if (powerIndex == 2) {
                                        if (spear.IsAttachedTo(v)) {
                                            World.AddOwnedExplosion(Game.Player.Character, spear.Position, ExplosionType.Grenade, 10f, 1f);
                                        }
                                    }

                                    World.DrawMarker(MarkerType.UpsideDownCone, (v.Position + new Vector3(0f, 0f, 2f)), new Vector3(0f, 0f, 0f),
                                    new Vector3(0f, 0f, 0f), new Vector3(0.4f, 0.4f, 0.4f), Color.Red, false, false, 0, false, "", "", false);
                                } else {
                                    World.DrawMarker(MarkerType.UpsideDownCone, (v.Position + new Vector3(0f, 0f, 2f)), new Vector3(0f, 0f, 0f),
                                    new Vector3(0f, 0f, 0f), new Vector3(0.4f, 0.4f, 0.4f), Color.Blue, false, false, 0, false, "", "", false);
                                }
                            } else {
                                v.IsInvincible = false;
                            }
                        }
                    }

                    // recall methods
                    if (Game.IsKeyPressed(sheathe) & !spear.IsAttachedTo(Game.Player.Character)) {
                        spear.ApplyForce((spear.Position - Game.Player.Character.Position) * -5f);
                    }
                    if (Game.IsKeyPressed(unstuck) & !spear.IsAttachedTo(Game.Player.Character) && !keyUp) {
                        spear.ApplyForce(Vector3.WorldUp * 1000f);
                        keyUp = true;
                    }
                    if (!spear.IsAttached() && !Game.IsDisabledControlPressed(2, GTA.Control.Aim)) {
                        if (spear.IsInRangeOf(Game.Player.Character.Position, 1.5f)) {
                            spear.AttachTo(Game.Player.Character, Game.Player.Character.GetBoneIndex(Bone.SKEL_Spine0),
                                new Vector3(-0.8f, -0.13f, 0.5f),
                                new Vector3(0, 115, 0)
                                );
                        }
                    }
                } catch (Exception ugh) { }

                // Slowmo
                if (Game.IsDisabledControlPressed(17, GTA.Control.Duck) && !Game.Player.Character.IsSittingInVehicle()) {
                    Game.TimeScale = 0.4f;
                } else {
                    Game.TimeScale = 1.0f;
                }

                // Player current vehicle check
                if (Game.Player.Character.IsSittingInVehicle()) {
                    spear.Detach();
                    spear.ApplyForce((spear.Position - Game.Player.Character.Position) * -5f);
                    if (!spear.IsAttached() && spear.IsInRangeOf(Game.Player.Character.Position, 2f)) {
                        spear.AttachTo(Game.Player.Character.CurrentVehicle, Game.Player.Character.CurrentVehicle.GetBoneIndex("engine"), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                    }
                }

                // Particle effects
                if (!spear.IsAttachedTo(Game.Player.Character)) {
                    if (Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_rcbarry2")) {
                        Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "scr_rcbarry2");

                        if (spear.IsVisible || !spear.IsVisible) {
                            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "scr_exp_clown_trails", spear,
                                0, 0, 2.33f,
                                0, 0, 0,
                                0.3f, 0, 0, 0);
                        }
                        /* works: 
                        sp_clown_appear_trails /not colorable
                        scr_exp_clown_trails / not color
                        scr_clown_appears / no color
                        scr_exp_clown / no color
                        eject_clown / no color
                        muz_clown / no color                    
                        */
                    } else {
                        Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_rcbarry2");
                    }
                }

                if (speed > 0 || !spear.IsAttached()) {
                    if (Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_carsteal4")) {
                        Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "scr_carsteal4");
                        Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_COLOUR, 255.0f, 0f, 0f);

                        Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "scr_carsteal4_wheel_burnout", fin,
                             0, 0, 0.06,
                             90f, 0, 0,
                             0.05f, 0, 0, 0);
                    } else {
                        Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_carsteal4");
                    }
                }

                // All controller controls (only works in tick)
                // powerIndex
                if (Game.CurrentInputMode == InputMode.GamePad) {
                    if (Game.IsControlJustPressed(2, GTA.Control.ScriptPadRight) && !keyUpController) {
                        keyUpController = true;
                        powerIndex++;
                    }
                    if (Game.IsControlJustReleased(2, GTA.Control.ScriptPadRight)) {
                        keyUpController = false;
                    }
                    if (Game.IsControlJustPressed(2, GTA.Control.ScriptPadLeft) && !keyUpController) {
                        keyUpController = true;
                        powerIndex--;
                    }
                    if (Game.IsControlJustReleased(2, GTA.Control.ScriptPadLeft)) {
                        keyUpController = false;
                    }

                    // Z-Unstuck
                    if (Game.IsControlJustPressed(2, GTA.Control.ScriptPadUp) && !keyUpController) {
                        r = 1;
                        keyUpController = true;
                    }
                    if (Game.IsControlJustReleased(2, GTA.Control.ScriptPadUp)) {
                        r = 0;
                        keyUpController = false;
                    }

                    if (r == 1 && !spear.IsAttachedTo(Game.Player.Character)) {
                        spear.ApplyForce(Vector3.WorldUp * 1000f);
                    }

                    // Recall
                    if (Game.IsControlJustPressed(2, GTA.Control.ScriptRB) && !keyUpController) {
                        a = 1;
                        keyUpController = true;
                    }
                    if (Game.IsControlJustReleased(2, GTA.Control.ScriptRB)) {
                        a = 0;
                        keyUpController = false;
                    }

                    if (a == 1 && !spear.IsAttachedTo(Game.Player.Character)) {
                        spear.ApplyForce((Game.Player.Character.Position - spear.Position) * 5);
                    }

                    // Sheathe / Unsheathe
                    if (Game.IsControlJustPressed(2, GTA.Control.ScriptRB) && !keyUpController) {
                        keyUpController = true;
                    }
                    if (Game.IsControlJustReleased(2, GTA.Control.ScriptRB)) {
                        spearActive = !spearActive;
                        // Sheathe
                        if (!spearActive) {
                            spear.Detach();
                            unsheathed = false;

                            NAudio.Wave.WaveFileReader mp3whis1 = new NAudio.Wave.WaveFileReader(@"scripts\Spear Files\unsheathe.wav");
                            NAudio.Wave.WaveChannel32 waveDown = new NAudio.Wave.WaveChannel32(mp3whis1);
                            NAudio.Wave.DirectSoundOut output = new NAudio.Wave.DirectSoundOut();
                            output.Init(waveDown);
                            output.Play();
                            waveDown.Volume = 0.05f;

                        } else if (spear.IsAttachedTo(Game.Player.Character)) {
                            // Unsheathe
                            spear.AttachTo(Game.Player.Character, Game.Player.Character.GetBoneIndex(Bone.SKEL_Head),
                                new Vector3(0.2f, 1, -0.2f),
                                new Vector3(90, 0, 0)
                                );

                            NAudio.Wave.WaveFileReader recall = new NAudio.Wave.WaveFileReader(@"scripts\Spear Files\return.wav");
                            NAudio.Wave.WaveChannel32 waveRec = new NAudio.Wave.WaveChannel32(recall);
                            NAudio.Wave.DirectSoundOut output3 = new NAudio.Wave.DirectSoundOut();
                            output3.Init(waveRec);
                            output3.Play();
                            waveRec.Volume = 0.05f;

                            unsheathed = true;
                        }

                        keyUpController = false;
                    }
                }
            }
        }

        private void onKeyDown(object sender, KeyEventArgs e) {
            if (modActive == true) {
                // Sheathe / Unsheathe
                if (e.KeyCode == sheathe && !keyUp && !Game.Player.Character.IsSittingInVehicle()) {
                    keyUp = true;

                    if (!spear.IsAttachedTo(Game.Player.Character)) {
                        spearActive = false;
                    } else {
                        spearActive = !spearActive;
                    }

                    // Sheathe
                    if (!spearActive) {
                        spear.Detach();
                        unsheathed = false;

                        NAudio.Wave.WaveFileReader mp3whis1 = new NAudio.Wave.WaveFileReader(@"scripts\Spear Files\unsheathe.wav");
                        NAudio.Wave.WaveChannel32 waveDown = new NAudio.Wave.WaveChannel32(mp3whis1);
                        NAudio.Wave.DirectSoundOut output = new NAudio.Wave.DirectSoundOut();
                        output.Init(waveDown);
                        output.Play();
                        waveDown.Volume = 0.05f;
                    } else if (spear.IsAttachedTo(Game.Player.Character)) {
                        // Unsheathe
                        spear.AttachTo(Game.Player.Character, Game.Player.Character.GetBoneIndex(Bone.SKEL_Head),
                            new Vector3(0.2f, 1, -0.2f),
                            new Vector3(90, 0, 0)
                            );

                        unsheathed = true;

                        NAudio.Wave.WaveFileReader recall = new NAudio.Wave.WaveFileReader(@"scripts\Spear Files\return.wav");
                        NAudio.Wave.WaveChannel32 waveRec = new NAudio.Wave.WaveChannel32(recall);
                        NAudio.Wave.DirectSoundOut output3 = new NAudio.Wave.DirectSoundOut();
                        output3.Init(waveRec);
                        output3.Play();
                        waveRec.Volume = 0.05f;
                    }
                }
            }
        }

        // Anti key spam
        private void onKeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == sheathe) {
                keyUp = false;
            }

            if (e.KeyCode == powerModeUp) {
                keyUp = false;
            }

            if (e.KeyCode == powerModeDown) {
                keyUp = false;
            }

            if (e.KeyCode == unstuck) {
                keyUp = false;
            }
        }

        void spearAttackModes(object sender, EventArgs e) {
            if (modActive) {
                if (powerIndex < -1 || powerIndex > 4) {
                    powerIndex = 0;
                }
                if (powerIndex == -1) {
                    powerIndex = 4;
                }
                if (Game.IsKeyPressed(powerModeUp) && !keyUp) {
                    keyUp = true;
                    powerIndex++;
                }
                if (Game.IsKeyPressed(powerModeDown) && !keyUp) {
                    keyUp = true;
                    powerIndex--;
                }

                // spearNormal
                if (powerIndex == 0) {
                    UI.DrawTexture("scripts\\Spear Files\\spear.png", 0, 0, 100, new Point(220, 650), new Size(200, 10), 2f, Color.White);
                    UI.DrawTexture("scripts\\Spear Files\\normal.png", 0, 0, 100, new Point(415, 640), new Size(23, 25), 2f, Color.White);
                }

                // spearCrush
                if (powerIndex == 1) {
                    UI.DrawTexture("scripts\\Spear Files\\spear.png", 0, 0, 100, new Point(220, 650), new Size(200, 10), 2f, Color.White);
                    UI.DrawTexture("scripts\\Spear Files\\down arrow.png", 0, 0, 100, new Point(415, 635), new Size(25, 25), 2f, Color.White);
                    UI.DrawTexture("scripts\\Spear Files\\broken car.png", 0, 0, 100, new Point(415, 650), new Size(25, 25), 2f, Color.White);

                }

                // spearExplode
                if (powerIndex == 2) {
                    UI.DrawTexture("scripts\\Spear Files\\spear.png", 0, 0, 100, new Point(220, 650), new Size(200, 10), 2f, Color.White);
                    UI.DrawTexture("scripts\\Spear Files\\explode.png", 0, 0, 100, new Point(415, 640), new Size(25, 25), 2f, Color.White);
                }

                // spearFire
                if (powerIndex == 3) {
                    UI.DrawTexture("scripts\\Spear Files\\spear.png", 0, 0, 100, new Point(220, 650), new Size(200, 10), 2f, Color.White);
                    UI.DrawTexture("scripts\\Spear Files\\fire.png", 0, 0, 100, new Point(415, 640), new Size(25, 25), 2f, Color.White);
                }
            }
        }
    }
}