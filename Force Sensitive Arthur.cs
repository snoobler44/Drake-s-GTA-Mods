using RDR2;
using RDR2.Math;
using RDR2.Native;
using System;
using System.Windows.Forms;

namespace Force_Sensitive_Arthur {
    public class Force_Sensitive_Arthur: Script {
        ScriptSettings config;
        Keys usePower;

        int powerInt;
        int forcePushLevel = 1;

        bool modActive = false;
        bool gripActive = false;
        bool keyup = false;

        float distX, distY, distZ;
        float forceGripLevelFactor;
        float forcePushLevelFactor;
        float forceStompLevelFactor;
        float forcePullLevelFactor;

        RaycastResult ray;

        private void modActivate() {

        }

        public Force_Sensitive_Arthur() {
            Interval = 10;
            this.Tick += tick;
            this.Tick += forceGrip;
            this.Tick += forcePush;
            this.Tick += forcePull;
            this.Tick += forceFocus;
            this.KeyUp += keyUp;
            this.KeyDown += onKeyDown;
            this.KeyDown += powerSwitch;

            config = ScriptSettings.Load("scripts\\Force Sensitive Arthur.ini");
            usePower = config.GetValue("Options", "usePower", usePower);
        }

        private void tick(object sender, EventArgs e) {
            if (modActive) {
                Game.DisableControlThisFrame(1, RDR2.Control.LookBehind);
                RDR2.UI.Screen.ShowSubtitle("" + gripActive);

                RDR2.UI.CustomSprite customSprite = new RDR2.UI.CustomSprite("scripts\\punjabi.png", new System.Drawing.SizeF(10, 10), new System.Drawing.PointF(50, 50));
                customSprite.Draw();
            }
        }

        private void powerSwitch(object sender, KeyEventArgs e) {
            if (powerInt > 2) {
                powerInt = 0;
            } else if (powerInt < 0) {
                powerInt = 2;
            }
            if (e.KeyCode == Keys.Z) {
                powerInt--;
                gripActive = false;
            } else if(e.KeyCode == Keys.C) {
                powerInt++;
                gripActive = false;
            }
        }

        private void forceGrip(object sender, EventArgs e) {
            if (modActive && powerInt == 0) {
                try {
                    Vector3 cam = new Vector3((GameplayCamera.Direction.X * distX), (GameplayCamera.Direction.Y * distY), (GameplayCamera.Direction.Z * distZ + 1f));
                    Vector3 pedPos = Game.Player.Character.Position + cam;
                    
                    if (Game.IsKeyPressed(usePower) && !gripActive && !keyup) {
                        ray = World.CrosshairRaycast(5000f, IntersectOptions.Everything, Game.Player.Character);
                        keyup = true;
                    } else if (Game.IsKeyPressed(usePower) && gripActive && !keyup) {
                        ray = World.Raycast(GameplayCamera.Position, GameplayCamera.Direction + new Vector3(10, 10, 10), IntersectOptions.Everything, Game.Player.Character);
                        gripActive = false;
                        keyup = true;
                    }

                    if (ray.HitEntity.EntityType == EntityType.Ped || ray.HitEntity.EntityType == EntityType.Prop) {
                        distX = 2.5f; distY = 2.5f; distZ = 2.5f;
                    } else if(ray.HitEntity.EntityType == EntityType.Vehicle) {
                        distX = 6.0f; distY = 6.0f; distZ = 1.5f;
                    }

                    if (ray.HitEntity != Game.Player.Character) {
                        // 100f temp value, level system later
                        ray.HitEntity.ApplyForce((pedPos - ray.HitEntity.Position) * 100f);
                        if (ray.HitEntity.IsInAir) {
                            gripActive = true;
                            Function.Call(Hash.SET_PED_TO_RAGDOLL, ray.HitEntity, 5000, 5000, 0, false, false, false);
                        }
                    }
                } catch { Exception STOP; }
            }
        }

        private void forcePush(object sender, EventArgs e) {
            if (modActive && powerInt == 1) {
                try {
                    // force stomp & levels
                    if (forcePushLevel == 1) {
                        forcePushLevelFactor = 20f;
                    } else if (forcePushLevel == 2) {
                        forcePushLevelFactor = 30f;
                    } else if (forcePushLevel == 3) {
                        forcePushLevelFactor = 40f;
                        forceStompLevelFactor = 10f;
                    } else if (forcePushLevel == 4) {
                        forcePushLevelFactor = 50f;
                        forceStompLevelFactor = 20f;
                    } else if (forcePushLevel == 5) {
                        forcePushLevelFactor = 70f;
                        forceStompLevelFactor = 40f;
                    }

                    // temp float value, implement level system later
                    if (Game.IsKeyPressed(usePower) && !keyup && ray.HitEntity != Game.Player.Character) {
                        ray = World.CrosshairRaycast(5000f, IntersectOptions.Everything, Game.Player.Character);
                        Function.Call(Hash.SET_PED_TO_RAGDOLL, ray.HitEntity, 5000, 5000, 0, false, false, false);
                        ray.HitEntity.ApplyForce(GameplayCamera.Direction * forcePushLevelFactor);

                        if (forcePushLevel >= 3) {
                            ray.HitEntity.ApplyForce(Vector3.WorldUp * forceStompLevelFactor);
                        }

                        keyup = true;
                    } 
                } catch { Exception no; }
            }
        }

        private void forcePull(object sender, EventArgs e) {
            if (modActive && powerInt == 2) {
                try {
                    // temp float value, implement level system later
                    forcePullLevelFactor = 10f;
                    if (Game.IsKeyPressed(usePower) && !keyup && ray.HitEntity != Game.Player.Character) {
                        ray = World.CrosshairRaycast(5000f, IntersectOptions.Everything, Game.Player.Character);
                        Function.Call(Hash.SET_PED_TO_RAGDOLL, ray.HitEntity, 5000, 5000, 0, false, false, false);
                        ray.HitEntity.ApplyForce(GameplayCamera.Direction * -forcePullLevelFactor);

                        keyup = true;
                    }


                } catch { Exception idc; }
            }
        }

        private void forceFocus(object sender, EventArgs e) {

        }

        // temp on/off button
        private void onKeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.NumPad0) {
                modActive = !modActive;
                RDR2.UI.Screen.ShowSubtitle("YEE " + modActive);
            }
        }

        private void keyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == usePower) {
                keyup = false;
            }
        }
    }
}
