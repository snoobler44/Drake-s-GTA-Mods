using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using RDR2;
using RDR2.Native;

namespace Drake_s_Red_Dead_Mod_Menu {
    public class Mod_Menu : Script {
        bool menuToggle = false;
        bool keyUp = false;

        string[] scriptLocationPath;
        string printScriptName;
        string one;

        int selection;
        int subMenuInt;
        int menuSelection;
        int subMenuSelection;
        int fileCount;

        static string scriptNameChecker;

        public Mod_Menu() {
            this.Tick += menuTick;
            this.KeyUp += up;

            isModDownloaded();
        }

        void isModDownloaded() {
            var scriptPath = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.dmods", SearchOption.AllDirectories);
            foreach (string scripts in scriptPath) {
                if (File.Exists(scripts)) {
                    TextReader reader = new StreamReader(scripts);
                    printScriptName += reader.ReadToEnd();
                }
            }
            try {
                string[] splitter = printScriptName.Split('/');
                for (int i = 0; i < splitter.Length; i++) {
                    if (i == 0) {
                        one = splitter[i];
                    } else if (i == 1) {
                        //integers[1] = splitter[i];
                    } else if (i == 2) {
                        //integers[2] = splitter[i];
                    }
                }
            } catch { Exception please; }
        }

        public static int modActivator() {
            if (scriptNameChecker == "Force Sensitive Arthur") {
                return 0;
            } else {
                return -1;
            }
        }

        private void menuTick(object sender, EventArgs e) {
            if (Game.IsKeyPressed(Keys.F7) && !keyUp ||
                Game.IsControlPressed(2, RDR2.Control.OpenSatchelMenu) && !Game.IsKeyPressed(Keys.B) && 
                Game.IsControlPressed(2, RDR2.Control.Cover) && !Game.IsKeyPressed(Keys.Q)) {
                menuToggle = !menuToggle;
                scriptLocationPath = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dmods", SearchOption.AllDirectories);
                fileCount = scriptLocationPath.Count();

                keyUp = true;
            }

            // menuToggle + menuSelection (pages)
            if (Game.IsKeyPressed(Keys.Back) && menuSelection == 0 && !keyUp || Game.IsKeyPressed(Keys.NumPad0) && menuSelection == 0 && !keyUp
                || Game.IsControlJustPressed(2, RDR2.Control.MeleeAttack) && !Game.IsKeyPressed(Keys.F) && menuSelection == 0) {
                menuToggle = false;

                keyUp = true;
            }
            if (Game.IsKeyPressed(Keys.Back) && menuSelection != 0 && !keyUp || Game.IsKeyPressed(Keys.NumPad0) && menuSelection != 0 && !keyUp
                || Game.IsControlJustPressed(2, RDR2.Control.MeleeAttack) && !Game.IsKeyPressed(Keys.F) && menuSelection != 0) {
                menuSelection = 0;

                keyUp = true;
            }

            // changing the selection int by arrow keys
            if (selection > fileCount) {
                selection = 0;
            }
            if (selection < 0) {
                selection = fileCount;
            }

            if (Game.IsKeyPressed(Keys.Up) && fileCount != 0 && !keyUp && menuSelection == 0 ||
                Game.IsControlJustPressed(2, RDR2.Control.FrontendUp) && !Game.IsKeyPressed(Keys.H) && menuSelection == 0) {
                selection--;

                

                keyUp = true;
            }
            if (Game.IsKeyPressed(Keys.Down) && fileCount != 0 && !keyUp && menuSelection == 0 ||
                Game.IsControlJustPressed(2, RDR2.Control.FrontendDown) && menuSelection == 0) {
                selection++;

                keyUp = true;
            }

            // subMenu selection int
            if (subMenuInt > 2) {
                subMenuInt = 0;
            }
            if (subMenuInt < 0) {
                subMenuInt = 2;
            }
            if (Game.IsKeyPressed(Keys.Up) && !keyUp && subMenuSelection != 0 ||
                Game.IsControlJustPressed(2, RDR2.Control.FrontendUp) && subMenuSelection != 0) {
                subMenuInt--;

                keyUp = true;
            }
            if (Game.IsKeyPressed(Keys.Down) && !keyUp && subMenuSelection != 0 ||
                Game.IsControlJustPressed(2, RDR2.Control.FrontendDown) && subMenuSelection != 0) {
                subMenuInt++;

                keyUp = true;
            }

            // menus stuff
            if (menuToggle) {
                Game.DisableControlThisFrame(2, RDR2.Control.MeleeAttack);
                Game.DisableControlThisFrame(2, RDR2.Control.Whistle);

                if (Game.IsControlPressed(2, RDR2.Control.Map) || Game.IsControlPressed(2, RDR2.Control.FrontendPause)) {
                    menuToggle = false;
                }

                RDR2.UI.Sprite background = new RDR2.UI.Sprite("menu_textures", "translate_bg_1a", new Size(390, 255), new PointF(-5, 5), Color.Black, 0f, false); background.Draw();

                // header + buttons
                if (menuSelection == 0) {
                    string font = string.Format("<b> important </b> {0}", "bore");

                    RDR2.UI.TextElement Header = new RDR2.UI.TextElement("Drake's Script Menu" + font, new Point(32, 14) , 0.55f, Color.White, RDR2.UI.Alignment.Left, false, false, 0f); Header.Draw();

                    RDR2.UI.TextElement Option1 = new RDR2.UI.TextElement("bottom text ", new Point(35, 57), 0.45f, Color.White, RDR2.UI.Alignment.Center, false, false, 0f); Option1.Draw();

                    // version
                    RDR2.UI.TextElement Version = new RDR2.UI.TextElement("v1.0", new Point(325, 213), 0.28f, Color.White, RDR2.UI.Alignment.Center, false, false, 0); Version.Draw();
                }

                // selection bar version (red)
                if (selection == 0 && menuSelection == 0) {
                    RDR2.UI.Sprite cross = new RDR2.UI.Sprite("menu_textures", "cross", new Size(20, 20), new PointF(300, 213), Color.Red, 0f, false); cross.Draw();

                    if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                        Game.IsControlJustPressed(2, RDR2.Control.FrontendAccept)) {
                        menuSelection = 4;

                        keyUp = true;
                    }
                }

                // selection bar 1 (red)
                if (selection == 1 && menuSelection == 0) {
                    RDR2.UI.Sprite cross = new RDR2.UI.Sprite("menu_textures", "cross", new Size(25, 25), new PointF(315, 63), Color.Red, 0f, false); cross.Draw();

                    if (Game.IsKeyPressed(Keys.Enter) && !keyUp
                        || Game.IsControlJustPressed(2, RDR2.Control.FrontendAccept)) {
                        menuSelection = 1;

                        keyUp = true;
                    }
                }

                // sub menu stuff
                if (menuSelection == 1) {
                    subMenuSelection = 1;

                    RDR2.UI.TextElement Header = new RDR2.UI.TextElement(one, new Point(32, 16), 0.55f, Color.White, RDR2.UI.Alignment.Center, false, false, 0f); Header.Draw();

                    RDR2.UI.TextElement Option1 = new RDR2.UI.TextElement("Activate Script", new Point(35, 58), 0.43f, Color.White, RDR2.UI.Alignment.Center, false, false, 0f); Option1.Draw();

                    RDR2.UI.TextElement Option2 = new RDR2.UI.TextElement("Deactivate Script", new Point(35, 92), 0.43f, Color.White, RDR2.UI.Alignment.Center, false, false, 0f); Option2.Draw();

                    RDR2.UI.TextElement Option3 = new RDR2.UI.TextElement("Open Config File", new Point(35, 124), 0.43f, Color.White, RDR2.UI.Alignment.Center, false, false, 0f); Option3.Draw();

                    if (subMenuSelection == 1) {
                        // subMenuSelection bar 0 (red)
                        // activate 
                        if (subMenuInt == 0) {
                            RDR2.UI.Sprite cross = new RDR2.UI.Sprite("menu_textures", "cross", new Size(25, 25), new PointF(315, 63), Color.Red, 0f, false); cross.Draw();

                            if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                                Game.IsControlJustPressed(2, RDR2.Control.FrontendAccept)) {
                                scriptNameChecker = one;

                                keyUp = true;
                            }
                        }
                        // deactivate
                        if (subMenuInt == 1) {
                            RDR2.UI.Sprite cross = new RDR2.UI.Sprite("menu_textures", "cross", new Size(25, 25), new PointF(315, 95), Color.Red, 0f, false); cross.Draw();

                            if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                                Game.IsControlJustPressed(2, RDR2.Control.FrontendAccept)) {
                                scriptNameChecker = "";

                                keyUp = true;
                            }
                        }
                        // config
                        if (subMenuInt == 2) {
                            RDR2.UI.Sprite cross = new RDR2.UI.Sprite("menu_textures", "cross", new Size(25, 25), new PointF(315, 128), Color.Red, 0f, false); cross.Draw();

                            if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                                Game.IsControlJustPressed(2, RDR2.Control.FrontendAccept)) {
                                Process.Start(Directory.GetCurrentDirectory() + "\\scripts\\" + one + ".ini");

                                keyUp = true;
                            }
                        }
                    }
                } else {
                    subMenuSelection = 0;
                }

                if (menuSelection == 4) {
                    RDR2.UI.TextElement Header = new RDR2.UI.TextElement("Change log", new Point(90, 15), 0.58f, Color.White, RDR2.UI.Alignment.Center, false, false, 0f); Header.Draw();

                    RDR2.UI.TextElement version1dot0 = new RDR2.UI.TextElement("v1.0", new Point(20, 55), 0.35f, Color.White, RDR2.UI.Alignment.Center, false, false, 0f); version1dot0.Draw();

                    RDR2.UI.TextElement onedot01 = new RDR2.UI.TextElement("-Initial Release", new Point(42, 75), 0.30f, Color.White, RDR2.UI.Alignment.Center, false, false, 0f); onedot01.Draw();
                }
            }
        }

        private void up(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.F7 || e.KeyCode == Keys.NumPad2 || e.KeyCode == Keys.NumPad0 
                || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down 
                || e.KeyCode == Keys.Back) {
                keyUp = false;
            }
        }
    }
}
