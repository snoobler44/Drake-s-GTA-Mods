using System;
using System.Windows.Forms;
using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;
using Drake_s_Mod_Menu;

namespace Cicada_Dagger {
    public class Cicada : Script {
        // ini
        ScriptSettings config;
        Keys activeButton;
        Keys recall;
        Keys powerModeDown;
        Keys powerModeUp;
        Keys passive;

        // setup
        Prop dagger;
        Prop maskCircle;
        Prop maskMiddle;
        Prop maskLeft;
        Prop maskRight;

        int powerMode;

        bool modActive = false;
        bool keyUp = false;
        bool attached = false;
        bool passiveOn = false;
        bool mask;
        bool superSpeed;
        bool superJump;

        Ped[] pedsToTarget;
        Vehicle[] vehicles;
        Ped[] nearbyPeds;
        Vehicle[] nearbyVehs;
        Entity[] nearbyStuff;
        Vehicle vehiclesToTarget;

        Vector3 comparePositionFront;
        Vector3 comparePositionBack;
        Vector3 comparePositionLeft;
        Vector3 comparePositionRight;
        Vector3 comparePositionCenter;
        Vector3 comparePositionPed;

        Vector3 vehCompPosFront;
        Vector3 vehCompPosBack;
        Vector3 vehCompPosLeft;
        Vector3 vehCompPosRight;

        float distFront;
        float distBack;
        float distLeft;
        float distRight;
        float distCenter;
        float distPed;

        string model;

        int i;
        int r;
        int on;

        public Cicada() {
            this.Tick += tick;
            this.KeyUp += up;
            this.KeyUp += up2;

            config = ScriptSettings.Load("scripts\\Cicada Dagger.ini");
            activeButton = config.GetValue("Options", "activeButton", activeButton);
            recall = config.GetValue("Options", "recall", recall);
            powerModeDown = config.GetValue("Options", "powerModeDown", powerModeDown);
            powerModeUp = config.GetValue("Options", "powerModeUp", powerModeUp);
            passive = config.GetValue("Options", "passive", passive);
            model = config.GetValue("Options", "model", model);
            mask = config.GetValue("Options", "mask", mask);
            superSpeed = config.GetValue("Options", "super speed", superSpeed);
            superJump = config.GetValue("Options", "super jump", superJump);
        }

        // Mod Active
        private void modActivate() {
            try {
                if (Mod_Menu.modActivator() == 1) {
                    modActive = true;
                    on = 1;

                    Game.Player.Character.Health = 300;
                    Game.Player.Character.MaxHealth = 300;

                    dagger = World.CreateProp(model, Game.Player.Character.Position, false, false);
                    dagger.AddBlip();
                    dagger.CurrentBlip.Color = BlipColor.Yellow;
                    dagger.CurrentBlip.Scale = 0.7f;
                    dagger.CurrentBlip.Name = "Dagger";

                    if (mask) {
                        // mask setup
                        // maskCircle
                        maskCircle = World.CreateProp("w_ar_specialcarbine_boxmag", Game.Player.Character.Position, false, false);
                        maskCircle.AttachTo(Game.Player.Character, Game.Player.Character.GetBoneIndex(Bone.SKEL_Head),
                            new Vector3(-0.0078f, 0.0622f, -0.0001f),
                            new Vector3(93f, 167.38f, -105.7735f)
                        );

                        // maskMiddle
                        maskMiddle = World.CreateProp("w_ar_specialcarbine_mag1", Game.Player.Character.Position, false, false);
                        maskMiddle.AttachTo(Game.Player.Character, Game.Player.Character.GetBoneIndex(Bone.SKEL_Head),
                            new Vector3(0.0175f, 0.1272f, -0.0851f),
                            new Vector3(-1.6061f, -179.1025f, 20.5556f)
                        );

                        // maskLeft
                        maskLeft = World.CreateProp("w_pi_vintage_pistol_mag1", Game.Player.Character.Position, false, false);
                        maskLeft.AttachTo(Game.Player.Character, Game.Player.Character.GetBoneIndex(Bone.SKEL_Head),
                            new Vector3(0.0316f, 0.0427f, -0.0667f),
                            new Vector3(-78.9061f, 9.7856f, -119.2810f)
                        );

                        // maskRight
                        maskRight = World.CreateProp("w_pi_vintage_pistol_mag1", Game.Player.Character.Position, false, false);
                        maskRight.AttachTo(Game.Player.Character, Game.Player.Character.GetBoneIndex(Bone.SKEL_Head),
                            new Vector3(0.0316f, 0.0427f, 0.0703f),
                            new Vector3(-96.9061f, 5.7856f, -128.2810f)
                        );
                    }

                    UI.ShowSubtitle("~o~Active~w~");
                } else {
                    modActive = false;
                    on = 0;

                    dagger.Delete();
                    dagger.CurrentBlip.Remove();

                    if (mask) {
                        maskCircle.Delete();
                        maskMiddle.Delete();
                        maskLeft.Delete();
                        maskRight.Delete();
                    }

                    Game.Player.Character.CanRagdoll = true;

                    UI.ShowSubtitle("~w~Deactive~w~");
                }
            } catch { Exception why; }
        }

        private void up2(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter && Mod_Menu.modActivator() == 1 && on == 0) {
                modActivate();
            }
            if (e.KeyCode == Keys.Enter && Mod_Menu.modActivator() == 0 && on == 1) {
                modActivate();
            }
        }

        private void tick(object sender, EventArgs e) {
            if (Game.CurrentInputMode == InputMode.GamePad) {
                // Select button
                if (Game.IsControlPressed(2, GTA.Control.ScriptRDown) && Mod_Menu.modActivator() == 1 && on == 0 && !keyUp) {
                    modActivate();
                    keyUp = true;
                }
                if (Game.IsControlPressed(2, GTA.Control.ScriptRDown) && Mod_Menu.modActivator() == 0 && on == 1 && !keyUp) {
                    modActivate();
                    keyUp = true;
                }
                if (Game.IsControlJustReleased(2, GTA.Control.ScriptRDown)) {
                    keyUp = false;
                }
            }

            if (modActive) {
                Game.DisableControlThisFrame(2, GTA.Control.Aim);
                Game.DisableControlThisFrame(81, GTA.Control.Cover);
                Game.DisableControlThisFrame(2, GTA.Control.LookBehind);
                Game.DisableControlThisFrame(2, GTA.Control.Phone);

                if (superSpeed) {
                    Game.Player.SetRunSpeedMultThisFrame(1.49f);
                }
                if (superJump) {
                    Game.Player.SetSuperJumpThisFrame();
                }

                Game.Player.Character.CanRagdoll = false;
                Function.Call(Hash.REMOVE_WEAPON_FROM_PED, Game.Player.Character, 0xFBAB5776);

                pedsToTarget = World.GetNearbyPeds(Game.Player.Character, 45f);
                vehicles = World.GetNearbyVehicles(Game.Player.Character.Position, 30f);
                vehiclesToTarget = World.GetClosestVehicle(Game.Player.Character.Position, 10f);
                nearbyPeds = World.GetNearbyPeds(dagger.Position, 20f);
                nearbyVehs = World.GetNearbyVehicles(dagger.Position, 20f);
                nearbyStuff = World.GetNearbyEntities(dagger.Position, 20f);

                UI.DrawTexture("scripts\\Dagger Files\\crosshair.png", 0, 0, 100, new Point(550, 255), new Size(185, 185), 2f, Color.Orange);
                if (attached == true) {
                    UI.DrawTexture("scripts\\Dagger Files\\hooked.png", 0, 0, 100, new Point(200, 600), new Size(130, 100), 2f, Color.White);
                }

                try {
                    // Peds
                    foreach (Ped p in pedsToTarget) {
                        dagger.SetNoCollision(p, true);
                        p.SetNoCollision(dagger, true);

                        if (Game.IsControlJustPressed(2, GTA.Control.Phone) && !keyUp) {
                            i = 1;
                            keyUp = true;
                        }
                        if (Game.IsControlJustReleased(2, GTA.Control.Phone)) {
                            i = 0;
                            keyUp = false;
                        }

                        // attach checks
                        if (p != Game.Player.Character) {
                            if (dagger.IsInRangeOf(p.Position, 1.5f) && !dagger.IsAttachedTo(Game.Player.Character) && p.IsAlive) {
                                attached = true;
                            }
                            if (dagger.IsInRangeOf(p.Position, 1.5f) && !dagger.IsAttachedTo(Game.Player.Character) && !p.IsAlive) {
                                attached = false;
                            }
                            if (dagger.IsInRangeOf(p.Position, 1.5f) && Game.IsKeyPressed(Keys.Z) || i == 1) {
                                attached = false;
                            }
                            if (attached == true && Game.IsKeyPressed(Keys.Z) || i == 1) {
                                attached = false;
                            }
                        }

                        // reset ped flag
                        if (dagger.IsAttached() && Game.IsKeyPressed(Keys.Z) || i == 1) {
                            p.CanFlyThroughWindscreen = true;
                        }

                        // target ped + ped flag
                        if (p != Game.Player.Character) {
                            distPed = p.Position.DistanceTo(GameplayCamera.Position);
                            comparePositionPed = GameplayCamera.Position + GameplayCamera.Direction * distPed;

                            if (attached == false) {
                                if (p.Position.DistanceTo(comparePositionPed) < 0.5 && p.IsAlive && !p.IsSittingInVehicle()) {
                                    World.DrawMarker(MarkerType.UpsideDownCone, (p.Position + new Vector3(0f, 0f, 1.5f)), new Vector3(0f, 0f, 0f),
                                        new Vector3(0f, 0f, 0f), new Vector3(0.2f, 0.2f, 0.2f), Color.White, false, false, 0, false, "", "", false);

                                    // throw anim
                                    if (Game.IsDisabledControlPressed(2, GTA.Control.Aim) && dagger.IsAttachedTo(Game.Player.Character) && !Game.Player.Character.IsGettingUp && !Game.Player.Character.IsInVehicle()) {
                                        Game.Player.Character.Task.PlayAnimation("weapons@projectile@grenade_str",
                                            "throw_m_fb_forward", 8.0f, -5.0f, 600, AnimationFlags.None, 0f);
                                    }

                                    // throw dagger
                                    if (Game.IsDisabledControlPressed(2, GTA.Control.Aim) && !Game.Player.Character.IsSittingInVehicle() && !Game.Player.Character.IsGettingUp) {
                                        dagger.Detach();
                                        dagger.ApplyForce((p.Position - dagger.Position) * 3f);

                                        p.CanFlyThroughWindscreen = false;

                                        World.DrawMarker(MarkerType.UpsideDownCone, (p.Position + new Vector3(0f, 0f, 1.5f)), new Vector3(0f, 0f, 0f),
                                            new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0.2f, 0.2f, 0.2f), Color.DarkOrange, false, false, 0, false, "", "", false);
                                    }
                                }
                            }
                        }

                        // collisions
                        if (attached == true) {
                            p.SetNoCollision(Game.Player.Character, true);
                        }
                        if (Game.IsKeyPressed(Keys.Z) || Game.IsDisabledControlPressed(1, GTA.Control.Attack)) {
                            p.SetNoCollision(Game.Player.Character, false);
                        }

                        // attach ped
                        if (dagger.IsInRangeOf(p.Position, 1.5f) && !Game.IsKeyPressed(Keys.Z) && p.IsAlive && attached == true && p.CanFlyThroughWindscreen == false) {
                            Function.Call(Hash.SET_PED_TO_RAGDOLL, p, 1000, 1000, 2);
                            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY, p, dagger, p.GetBoneIndex(Bone.RB_Neck_1), 0,
                                0f, 0f, 0.30f,
                                0f, 0f, 0f,
                                0, 0, 0,
                                0f, false, 0, false, false, 2);
                        }

                        // detach
                        if (Game.IsKeyPressed(Keys.Z) || i == 1 && dagger.IsInRangeOf(p.Position, 1.4f) && attached == false || p.IsDead) {
                            p.Detach();
                        }
                        if (Game.IsKeyPressed(recall) && dagger.IsInRangeOf(vehCompPosFront, 10f)) {
                            dagger.Detach();
                        }

                        // recall methods
                        if (Game.IsKeyPressed(recall) && !dagger.IsAttachedTo(Game.Player.Character)) {
                            dagger.FreezePosition = false;
                            dagger.Detach();
                            dagger.ApplyForce((dagger.Position - Game.Player.Character.Position) * -2f);
                        }
                        if (Game.IsKeyPressed(recall) && dagger.IsInRangeOf(Game.Player.Character.Position, 1.5f)) {
                            dagger.AttachTo(Game.Player.Character, Game.Player.Character.GetBoneIndex(Bone.PH_R_Hand),
                                new Vector3(0.05f, -0.02f, -0.03f),
                                new Vector3(118f, 0f, 0f)
                                );
                        }

                        if (Game.IsControlJustPressed(2, GTA.Control.ScriptRB) && !keyUp) {
                            r = 1;
                            keyUp = true;
                        }
                        if (Game.IsControlJustReleased(2, GTA.Control.ScriptRB)) {
                            r = 0;
                            keyUp = false;
                        }

                        if (r == 1 && !dagger.IsAttachedTo(Game.Player.Character)) {
                            dagger.FreezePosition = false;
                            dagger.Detach();
                            dagger.ApplyForce((dagger.Position - Game.Player.Character.Position) * -2f);
                        }
                        if (r == 1 && dagger.IsInRangeOf(Game.Player.Character.Position, 1.5f)) {
                            dagger.AttachTo(Game.Player.Character, Game.Player.Character.GetBoneIndex(Bone.PH_R_Hand),
                                new Vector3(0.05f, -0.02f, -0.03f),
                                new Vector3(118f, 0f, 0f)
                                );
                        }

                        // shove + 1 hit KO
                        if (Game.Player.Character.IsTouching(p)) {
                            p.ApplyForce(p.Position - Game.Player.Character.Position);
                        }
                        if (Game.Player.Character.IsInMeleeCombat) {
                            Ped a = Game.Player.Character.GetMeleeTarget();
                            if (a.HasBeenDamagedBy(Game.Player.Character)) {
                                a.ApplyDamage(300);
                            }
                        }

                        // power selections
                        if (powerMode < -1 || powerMode > 5) {
                            powerMode = 0;
                        }
                        if (powerMode == -1) {
                            powerMode = 5;
                        }

                        if (Game.IsKeyPressed(powerModeDown) && !keyUp) {
                            keyUp = true;
                            powerMode--;
                        }
                        if (Game.IsKeyPressed(powerModeUp) && !keyUp) {
                            keyUp = true;
                            powerMode++;
                        }

                        if (Game.CurrentInputMode == InputMode.GamePad) {
                            if (Game.IsControlJustPressed(2, GTA.Control.ScriptPadRight) && !keyUp) {
                                keyUp = true;
                                powerMode++;
                            }
                            if (Game.IsControlJustReleased(2, GTA.Control.ScriptPadRight)) {
                                keyUp = false;
                            }
                            if (Game.IsControlJustPressed(2, GTA.Control.ScriptPadLeft) && !keyUp) {
                                keyUp = true;
                                powerMode--;
                            }
                            if (Game.IsControlJustReleased(2, GTA.Control.ScriptPadLeft)) {
                                keyUp = false;
                            }
                        }

                        // passive
                        if (Game.IsKeyPressed(passive) && !keyUp) {
                            keyUp = true;
                            passiveOn = !passiveOn;
                        }

                        if (Game.IsControlJustPressed(2, GTA.Control.LookBehind) && !keyUp) {
                            keyUp = true;
                            passiveOn = !passiveOn;
                        }
                        if (Game.IsControlJustReleased(2, GTA.Control.LookBehind)) {
                            keyUp = false;
                        }


                        if (passiveOn) {
                            UI.DrawTexture("scripts\\Dagger Files\\magazine.png", 0, 0, 100, new Point(285, 650), new Size(25, 25), 2f, Color.White);
                            if (p.IsInCombatAgainst(Game.Player.Character)) {
                                p.Weapons.Current.Ammo = 0;
                                p.Weapons.Current.AmmoInClip = 0;
                            }
                        }

                        // kill
                        if (powerMode == 0) {
                            UI.DrawTexture("scripts\\Dagger Files\\skull.png", 0, 0, 100, new Point(200, 600), new Size(130, 100), 2f, Color.White);

                            if (dagger.IsInRangeOf(p.Position, 1.5f) && attached == true) {
                                Game.DisableControlThisFrame(1, GTA.Control.Attack);
                                if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                                    p.Kill();
                                    attached = false;
                                }
                            }
                        }

                        // attach to objects
                        if (powerMode == 1) {
                            UI.DrawTexture("scripts\\Dagger Files\\attach.png", 0, 0, 100, new Point(200, 600), new Size(130, 100), 2f, Color.White);

                            if (dagger.IsInRangeOf(p.Position, 1.5f) && dagger.IsInRangeOf(Game.Player.Character.Position, 1.5f) && attached == true) {
                                Game.DisableControlThisFrame(1, GTA.Control.Attack);
                                if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                                    dagger.Detach();
                                    dagger.FreezePosition = true;

                                    // Left door
                                    if (Game.Player.Character.IsInRangeOf(vehCompPosLeft, 1.5f)) {
                                        dagger.AttachTo(vehiclesToTarget, vehiclesToTarget.GetBoneIndex("handle_dside_f"),
                                            new Vector3(0, 0, 0),
                                            new Vector3(0, -90, 0)
                                        );
                                    }

                                    // Right door
                                    if (Game.Player.Character.IsInRangeOf(vehCompPosRight, 1.5f)) {
                                        dagger.AttachTo(vehiclesToTarget, vehiclesToTarget.GetBoneIndex("handle_pside_f"),
                                            new Vector3(0, 0, 0),
                                            new Vector3(0, 90, 0)
                                        );
                                    }

                                    // Front
                                    if (Game.Player.Character.IsInRangeOf(vehCompPosFront, 1.5f)) {
                                        dagger.AttachTo(vehiclesToTarget, vehiclesToTarget.GetBoneIndex("bonnet"),
                                            new Vector3(0, 1, 0),
                                            new Vector3(0, 0, 0)
                                        );
                                    }

                                    // Back
                                    if (Game.Player.Character.IsInRangeOf(vehCompPosBack, 1.5f)) {
                                        dagger.AttachTo(vehiclesToTarget, vehiclesToTarget.GetBoneIndex("boot"),
                                            new Vector3(0, -0.5f, 0),
                                            new Vector3(0, 0, 0)
                                        );
                                    }
                                }
                            }
                        } else {
                            dagger.FreezePosition = false;
                        }

                        // throw
                        if (powerMode == 2) {
                            UI.DrawTexture("scripts\\Dagger Files\\throw.png", 0, 0, 100, new Point(200, 600), new Size(130, 100), 2f, Color.White);

                            if (dagger.IsInRangeOf(p.Position, 1.5f) && dagger.IsInRangeOf(Game.Player.Character.Position, 1.5f) && attached == true) {
                                Game.DisableControlThisFrame(1, GTA.Control.Attack);
                                if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                                    Game.Player.Character.Task.PlayAnimation("weapons@projectile@grenade_str",
                                        "throw_m_fb_forward", 8.0f, -5.0f, 600, AnimationFlags.None, 0f);
                                    p.Detach();
                                    attached = false;
                                    p.ApplyForce(Vector3.RelativeTop * 25 + GameplayCamera.Direction * 35);
                                }
                            }
                        }

                        // lightning
                        if (powerMode == 3) {
                            UI.DrawTexture("scripts\\Dagger Files\\lightning.png", 0, 0, 100, new Point(200, 600), new Size(130, 100), 2f, Color.White);
                            Game.DisableControlThisFrame(1, GTA.Control.Attack);

                            if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                                foreach (Ped x in nearbyPeds) {
                                    if (x.IsAlive && x != Game.Player.Character) {
                                        World.ShootBullet(dagger.Position, x.Position, Game.Player.Character, "WEAPON_STUNGUN", 1, 10);

                                        if (x.IsBeingStunned) {
                                            if (Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "core")) {
                                                Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");

                                                Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "sp_foundry_sparks", x,
                                                    0, 0, 0.06,
                                                     90f, 0, 0,
                                                     0.05f, 0, 0, 0);
                                            } else {
                                                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "core");
                                            }
                                        }
                                    }
                                }

                                foreach (Vehicle v in nearbyVehs) {
                                    if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                                        if (v.IsAlive) {
                                            v.StartAlarm();
                                            v.ApplyForce(Vector3.WorldDown * 0.1f);
                                            v.EngineRunning = false;

                                            if (Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "core")) {
                                                Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");

                                                Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "sp_foundry_sparks", v,
                                                    0, 0, 0.06,
                                                     90f, 0, 0,
                                                     0.05f, 0, 0, 0);
                                            } else {
                                                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "core");
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // drop and or flee
                        if (powerMode == 4) {
                            UI.DrawTexture("scripts\\Dagger Files\\flee.png", 0, 0, 100, new Point(200, 600), new Size(130, 100), 2f, Color.White);
                            Game.DisableControlThisFrame(1, GTA.Control.Attack);

                            if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                                foreach (Ped z in nearbyPeds) {
                                    if (z != Game.Player.Character) {
                                        z.Weapons.Drop();
                                        z.Task.ReactAndFlee(Game.Player.Character);
                                    }
                                }
                            }
                        }

                        // nuke
                        if (powerMode == 5) {
                            UI.DrawTexture("scripts\\Dagger Files\\nuke.png", 0, 0, 100, new Point(200, 600), new Size(130, 100), 2f, Color.White);
                            Game.DisableControlThisFrame(1, GTA.Control.Attack);

                            if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {

                                foreach (Ped x in nearbyPeds) {
                                    if (x != Game.Player.Character && x.IsAlive) {
                                        World.AddExplosion(x.Position, ExplosionType.Grenade, 2f, 0f);
                                    }
                                }

                                foreach (Vehicle v in nearbyVehs) {
                                    if (v.IsAlive) {
                                        World.AddExplosion(v.Position, ExplosionType.Grenade, 2f, 0f);
                                    }
                                }

                                foreach (Entity q in nearbyStuff) {
                                    if (q != Game.Player.Character) {
                                        q.ApplyForce(q.Position - dagger.Position);
                                    }
                                }
                            }
                        }
                    }

                    // Vehicle target
                    foreach (Vehicle v in vehicles) {
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

                        distCenter = v.Position.DistanceTo(GameplayCamera.Position);
                        comparePositionCenter = GameplayCamera.Position + GameplayCamera.Direction * distCenter;

                        if (vehCompPosFront.DistanceTo(comparePositionFront) < 0.5 && v.IsAlive ||
                            vehCompPosBack.DistanceTo(comparePositionBack) < 0.5 && v.IsAlive ||
                            vehCompPosLeft.DistanceTo(comparePositionLeft) < 0.5 && v.IsAlive ||
                            vehCompPosRight.DistanceTo(comparePositionRight) < 0.5 && v.IsAlive ||
                            v.Position.DistanceTo(comparePositionCenter) < 0.5 && v.IsAlive) {

                            // throw anim
                            if (Game.IsDisabledControlPressed(2, GTA.Control.Aim) && dagger.IsAttachedTo(Game.Player.Character) && !Game.Player.Character.IsGettingUp && !Game.Player.Character.IsInVehicle()) {
                                Game.Player.Character.Task.PlayAnimation("weapons@projectile@grenade_str",
                                    "throw_m_fb_forward", 8.0f, -5.0f, 600, AnimationFlags.None, 0f);
                            }

                            // target vehicle
                            if (Game.IsDisabledControlPressed(2, GTA.Control.Aim)) {
                                dagger.Detach();
                                dagger.ApplyForce((v.Position - dagger.Position) * 3f);

                                if (dagger.IsInRangeOf(v.Position, 3f)) {
                                    dagger.AttachTo(v, v.GetBoneIndex("engine"),
                                        new Vector3(0, 0.5f, 0.6f),
                                        new Vector3(0, -180, 0)
                                        );
                                }

                                if (!dagger.IsAttachedTo(v)) {
                                    World.DrawMarker(MarkerType.UpsideDownCone, (v.Position + new Vector3(0f, 0f, 2f)), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f),
                                        new Vector3(0.4f, 0.4f, 0.4f), Color.Orange, false, false, 0, false, "", "", false);
                                }
                            } else {
                                World.DrawMarker(MarkerType.UpsideDownCone, (v.Position + new Vector3(0f, 0f, 2f)), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f),
                                    new Vector3(0.4f, 0.4f, 0.4f), Color.White, false, false, 0, false, "", "", false);
                            }
                        }

                        // amplified powers (on vehs)
                        if (dagger.IsAttachedTo(v)) {
                            UI.DrawTexture("scripts\\Dagger Files\\amplified.png", 0, 0, 100, new Point(200, 600), new Size(130, 100), 2f, Color.White);

                            // kill
                            if (powerMode == 0) {
                                Ped p = v.GetPedOnSeat(VehicleSeat.Passenger);
                                Ped w = v.GetPedOnSeat(VehicleSeat.LeftRear);
                                Ped r = v.GetPedOnSeat(VehicleSeat.RightRear);
                                if (v.Driver.IsAlive || p.IsAlive || w.IsAlive || r.IsAlive) {
                                    Game.DisableControlThisFrame(1, GTA.Control.Attack);
                                }

                                if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                                    v.Driver.Kill();
                                    p.Kill();
                                    w.Kill();
                                    r.Kill();

                                    v.SmashWindow(VehicleWindow.BackLeftWindow);
                                    v.SmashWindow(VehicleWindow.BackRightWindow);
                                    v.SmashWindow(VehicleWindow.FrontLeftWindow);
                                    v.SmashWindow(VehicleWindow.FrontRightWindow);
                                }
                            }

                            // throw
                            if (powerMode == 2) {
                                Game.DisableControlThisFrame(1, GTA.Control.Attack);
                                if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                                    v.ApplyForce(GameplayCamera.Direction * 20f);
                                }
                            }

                            // lightning
                            if (powerMode == 3) {
                                if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                                    v.EngineHealth = -10;
                                    v.LightsOn = true;
                                    v.OpenDoor(VehicleDoor.FrontLeftDoor, true, false);
                                    v.OpenDoor(VehicleDoor.BackLeftDoor, true, false);
                                    v.OpenDoor(VehicleDoor.FrontRightDoor, true, false);
                                    v.OpenDoor(VehicleDoor.BackRightDoor, true, false);
                                }
                            }

                            // flee
                            if (powerMode == 4) {
                                if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                                    if (!v.IsTireBurst(0) & !v.IsTireBurst(1) & !v.IsTireBurst(2) & !v.IsTireBurst(3) & !v.IsTireBurst(4) & !v.IsTireBurst(5) & !v.IsTireBurst(6) & !v.IsTireBurst(7)) {
                                        v.BurstTire(0);
                                        v.BurstTire(1);
                                        v.BurstTire(2);
                                        v.BurstTire(3);
                                        v.BurstTire(4);
                                        v.BurstTire(5);
                                        v.BurstTire(6);
                                        v.BurstTire(7);
                                    }

                                    v.SmashWindow(VehicleWindow.BackLeftWindow);
                                    v.SmashWindow(VehicleWindow.BackRightWindow);
                                    v.SmashWindow(VehicleWindow.FrontLeftWindow);
                                    v.SmashWindow(VehicleWindow.FrontRightWindow);
                                    v.BreakDoor(VehicleDoor.BackLeftDoor);
                                    v.BreakDoor(VehicleDoor.FrontLeftDoor);
                                    v.BreakDoor(VehicleDoor.BackRightDoor);
                                    v.BreakDoor(VehicleDoor.FrontRightDoor);
                                    v.BreakDoor(VehicleDoor.Hood);
                                    v.BreakDoor(VehicleDoor.Trunk);
                                }
                            }
                        }
                    }
                } catch (Exception fuck) { }

                // ptfx (on dagger)
                if (powerMode == 3 & Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                    if (Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "core")) {
                        Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");

                        Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "sp_foundry_sparks", dagger,
                            0, 0, 0.06,
                             90f, 0, 0,
                             0.05f, 0, 0, 0);
                    } else {
                        Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "core");
                    }
                }
                if (powerMode == 5 & Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                    if (Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "core")) {
                        Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");

                        Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "exp_grd_boat_lod", dagger,
                            0, 0, 0.06,
                             0f, 0f, 0,
                             0.6f, 0, 0, 0);

                    } else {
                        Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "core");
                    }
                }

                if (powerMode == 4 & Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                    if (Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "scr_carsteal4")) {
                        Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "scr_carsteal4");
                        Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_COLOUR, 255.0f, 0, 0);

                        Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "scr_carsteal4_wheel_burnout", dagger,
                            0, 0, 2f,
                             0f, 0f, 0,
                             2f, 0, 0, 0);

                    } else {
                        Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_carsteal4");
                    }
                }

                // sounds
                if (Game.IsDisabledControlJustPressed(1, GTA.Control.Attack)) {
                    // kill
                    if (powerMode == 0) {
                        NAudio.Wave.WaveFileReader neck = new NAudio.Wave.WaveFileReader(@"scripts\Dagger Files\neck.wav");
                        NAudio.Wave.WaveChannel32 waveAt = new NAudio.Wave.WaveChannel32(neck);
                        NAudio.Wave.DirectSoundOut output = new NAudio.Wave.DirectSoundOut();

                        output.Init(waveAt);
                        output.Play();
                        waveAt.Volume = 0.30f;
                    }

                    // attach
                    if (powerMode == 1) {
                        NAudio.Wave.WaveFileReader attach = new NAudio.Wave.WaveFileReader(@"scripts\Dagger Files\attach.wav");
                        NAudio.Wave.WaveChannel32 waveAt3 = new NAudio.Wave.WaveChannel32(attach);
                        NAudio.Wave.DirectSoundOut output2 = new NAudio.Wave.DirectSoundOut();

                        output2.Init(waveAt3);
                        output2.Play();
                        waveAt3.Volume = 0.30f;
                    }

                    // throw
                    if (powerMode == 2) {
                        NAudio.Wave.WaveFileReader throww = new NAudio.Wave.WaveFileReader(@"scripts\Dagger Files\throw.wav");
                        NAudio.Wave.WaveChannel32 waveAt3 = new NAudio.Wave.WaveChannel32(throww);
                        NAudio.Wave.DirectSoundOut output3 = new NAudio.Wave.DirectSoundOut();

                        output3.Init(waveAt3);
                        output3.Play();
                        waveAt3.Volume = 0.30f;
                    }

                    // lightning
                    if (powerMode == 3) {
                        NAudio.Wave.WaveFileReader lightning = new NAudio.Wave.WaveFileReader(@"scripts\Dagger Files\lightning.wav");
                        NAudio.Wave.WaveChannel32 waveAt4 = new NAudio.Wave.WaveChannel32(lightning);
                        NAudio.Wave.DirectSoundOut output4 = new NAudio.Wave.DirectSoundOut();

                        output4.Init(waveAt4);
                        output4.Play();
                        waveAt4.Volume = 1f;
                    }

                    // flee
                    if (powerMode == 4) {
                        NAudio.Wave.WaveFileReader flee = new NAudio.Wave.WaveFileReader(@"scripts\Dagger Files\flee.wav");
                        NAudio.Wave.WaveChannel32 waveAt5 = new NAudio.Wave.WaveChannel32(flee);
                        NAudio.Wave.DirectSoundOut output5 = new NAudio.Wave.DirectSoundOut();

                        output5.Init(waveAt5);
                        output5.Play();
                        waveAt5.Volume = 0.30f;
                    }

                    //nuke
                    if (powerMode == 5) {
                        NAudio.Wave.WaveFileReader nuke = new NAudio.Wave.WaveFileReader(@"scripts\Dagger Files\nuke.wav");
                        NAudio.Wave.WaveChannel32 waveAt5 = new NAudio.Wave.WaveChannel32(nuke);
                        NAudio.Wave.DirectSoundOut output5 = new NAudio.Wave.DirectSoundOut();

                        output5.Init(waveAt5);
                        output5.Play();
                        waveAt5.Volume = 0.10f;
                    }
                }
            } else {
                Game.Player.SetRunSpeedMultThisFrame(1.0f);
            }
        }

        // Anti-key spam
        private void up(object sender, KeyEventArgs e) {
            if (e.KeyCode == powerModeDown || e.KeyCode == passive || e.KeyCode == powerModeUp || e.KeyCode == Keys.Enter) {
                keyUp = false;
            }
        }
    }
}
