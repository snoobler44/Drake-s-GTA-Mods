using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using GTA;
using GTA.Native;

namespace Drake_s_Mod_Menu {
    public class Mod_Menu : Script {
        bool menuToggle = false;
        bool keyUp = false;
        bool keyUpController = false;

        string[] scriptPath;
        string printScriptName;

        string one;
        string two;
        string three;

        int selection;
        int subMenuInt;
        int menuSelection;
        int subMenuSelection;
        int fileCount;

        static string scriptNameChecker;

        public Mod_Menu() {
            this.Tick += customMainMenu;
            this.Tick += controllerSupport;
            this.KeyUp += up;

            isModDownloaded();
        }

        void isModDownloaded() {
            // first we find the .dmods files
            var scriptPath = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.dmods", SearchOption.AllDirectories);
            // now we slap em in an array
            foreach (string scripts in scriptPath) {
                if (File.Exists(scripts)) {
                    // read whats inside these files (the names of the scripts)
                    TextReader reader = new StreamReader(scripts);
                    printScriptName += reader.ReadToEnd();
                }
            }

            // splits the printscriptname string into multiple strings
            string[] splitter = printScriptName.Split('/');
            
            for (int i = 0; i < splitter.Length; i++) {
                if (i == 0) {
                    one = splitter[i];
                }
                if (i == 1) {
                    two = splitter[i];
                }
                if (i == 2) {
                    three = splitter[i];
                }
            }
        }

        public static int modActivator() {
            if (scriptNameChecker == "Cicada Dagger") {
                return 1;
            }

            if (scriptNameChecker == "Whistle Spear") {
                return 2;
            }

            if (scriptNameChecker == "DSF Vehicle Switch") {
                return 3;
            }
            return 0;
        }

        private void customMainMenu(object sender, EventArgs e) {
            if (Game.IsKeyPressed(Keys.F7) && !keyUp || 
                Game.IsControlPressed(2, GTA.Control.ScriptPadRight) && !keyUpController && Game.IsControlPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                menuToggle = !menuToggle;
                scriptPath = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dmods", SearchOption.AllDirectories);
                fileCount = scriptPath.Count();

                keyUp = true;
                keyUpController = true;
            }

            // menuToggle + menuSelection (pages)
            if (Game.IsKeyPressed(Keys.Back) && menuSelection == 0 && !keyUp || Game.IsKeyPressed(Keys.NumPad0) && menuSelection == 0 && !keyUp 
                || Game.IsControlJustPressed(2, GTA.Control.ScriptRRight) && menuSelection == 0 && !keyUpController) {
                menuToggle = false;

                keyUp = true;
                keyUpController = true;
            }
            if (Game.IsKeyPressed(Keys.Back) && menuSelection != 0 && !keyUp || Game.IsKeyPressed(Keys.NumPad0) && menuSelection != 0 && !keyUp 
                || Game.IsControlJustPressed(2, GTA.Control.ScriptRRight) && menuSelection != 0 && !keyUpController) {
                menuSelection = 0;

                keyUp = true;
                keyUpController = true;
            }

            // changing the selection int by arrow keys
            if (selection > fileCount) {
                selection = 0;
            }
            if (selection < 0) {
                selection = fileCount;
            }
            if (Game.IsKeyPressed(Keys.Up) && fileCount != 1 && !keyUp && menuSelection == 0 ||
                Game.IsControlJustPressed(2, GTA.Control.ScriptPadUp) && menuSelection == 0 && !keyUpController) {
                selection--;

                keyUp = true;
                keyUpController = true;
            }
            if (Game.IsKeyPressed(Keys.Down) && fileCount != 1 && !keyUp && menuSelection == 0 ||
                Game.IsControlJustPressed(2, GTA.Control.ScriptPadDown) && menuSelection == 0 && !keyUpController) {
                selection++;

                keyUp = true;
                keyUpController = true;
            }

            // subMenu selection int
            if (subMenuInt > 2) {
                subMenuInt = 0;
            }
            if (subMenuInt < 0) {
                subMenuInt = 2;
            }
            if (Game.IsKeyPressed(Keys.Up) && !keyUp && subMenuSelection != 0 ||
                Game.IsControlJustPressed(2, GTA.Control.ScriptPadUp) && subMenuSelection != 0 && !keyUpController) {
                subMenuInt--;

                keyUp = true;
                keyUpController = true;
            }
            if (Game.IsKeyPressed(Keys.Down) && !keyUp && subMenuSelection != 0 ||
                Game.IsControlJustPressed(2, GTA.Control.ScriptPadDown) && subMenuSelection != 0 && !keyUpController) {
                subMenuInt++;

                keyUp = true;
                keyUpController = true;
            }


            // menus stuff
            if (menuToggle) {
                // disable phone when in menu
                Game.DisableControlThisFrame(2, GTA.Control.Phone);
                Game.DisableControlThisFrame(2, GTA.Control.MeleeAttackLight);

                // gray background frame
                Function.Call(Hash.DRAW_RECT, 0.1, 0.12, 0.18, 0.2, 160, 160, 160, 230);

                // black header frame
                Function.Call(Hash.DRAW_RECT, 0.1, 0.035, 0.18, 0.035, 0, 0, 0, 245);

                // header + buttons
                if (menuSelection == 0) {
                    UIText Header = new UIText("Drake's Script Menu", new Point(35, 10), 0.7f, Color.White, GTA.Font.HouseScript, false, false, false);
                    Header.Draw();

                    UIText Option1 = new UIText(one, new Point(123, 38), 0.5f, Color.White, GTA.Font.ChaletComprimeCologne, true, false, true);
                    Option1.Draw();

                    UIText Option2 = new UIText(two, new Point(123, 60), 0.5f, Color.White, GTA.Font.ChaletComprimeCologne, true, false, true);
                    Option2.Draw();

                    UIText Option3 = new UIText(three, new Point(123, 82), 0.5f, Color.White, GTA.Font.ChaletComprimeCologne, true, false, true);
                    Option3.Draw();
                    
                    // version
                    UIText Version = new UIText("v1.1", new Point(228, 140), 0.45f, Color.White, GTA.Font.HouseScript, true);
                    Version.Draw();
                }

                if (menuSelection == 4) {
                    UIText Header = new UIText("Change log", new Point(77, 10), 0.65f, Color.White, GTA.Font.HouseScript, false, false, true);
                    Header.Draw();

                    UIText version1dot0 = new UIText("v1.0", new Point(20, 40), 0.4f, Color.White, GTA.Font.HouseScript, false, false, true);
                    version1dot0.Draw();

                    UIText version1dot1 = new UIText("v1.1", new Point(20, 57), 0.4f, Color.White, GTA.Font.HouseScript, false, false, true);
                    version1dot1.Draw();

                    UIText onedot01 = new UIText("-Initial Release", new Point(42, 38), 0.45f, Color.White, GTA.Font.ChaletComprimeCologne, false, false, true);
                    onedot01.Draw();

                    UIText onedot11 = new UIText("-Added cosmetic stuffs", new Point(42, 56), 0.45f, Color.White, GTA.Font.ChaletComprimeCologne, false, false, true);
                    onedot11.Draw();

                    UIText onedot12 = new UIText("-New version menu", new Point(42, 70), 0.45f, Color.White, GTA.Font.ChaletComprimeCologne, false, false, true);
                    onedot12.Draw();
                }

                // selection bar version (red)
                if (selection == 3 && menuSelection == 0) {
                    Function.Call(Hash.DRAW_RECT, 0.178, 0.208, 0.019, 0.022, 175, 0, 0, 245);
                    Function.Call(Hash.DRAW_RECT, 0.169, 0.208, 0.0030, 0.022, 0, 0, 0, 245);

                    if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                        Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                        menuSelection = 4;

                        keyUp = true;
                        keyUpController = true;
                    }
                }

                // selection bar 0 (red)
                if (selection == 0 && menuSelection == 0) {
                    Function.Call(Hash.DRAW_RECT, 0.1, 0.070, 0.18, 0.035, 175, 0, 0, 245);
                    Function.Call(Hash.DRAW_RECT, 0.012, 0.070, 0.0035, 0.035, 0, 0, 0, 245);

                    if (Game.IsKeyPressed(Keys.Enter) && !keyUp 
                        || Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                        menuSelection = 1;

                        keyUp = true;
                        keyUpController = true;
                    }
                }

                // selection bar 1 (red)
                if (selection == 1 && menuSelection == 0) {
                    Function.Call(Hash.DRAW_RECT, 0.1, 0.1, 0.18, 0.035, 175, 0, 0, 245);
                    Function.Call(Hash.DRAW_RECT, 0.012, 0.1, 0.0035, 0.035, 0, 0, 0, 245);

                    if (Game.IsKeyPressed(Keys.Enter) && !keyUp || 
                        Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                        menuSelection = 2;

                        keyUp = true;
                        keyUpController = true;
                    }
                }

                // selection bar 2 (red)
                if (fileCount >= 3 && selection == 2 && menuSelection == 0) {
                    Function.Call(Hash.DRAW_RECT, 0.1, 0.130, 0.18, 0.035, 175, 0, 0, 245);
                    Function.Call(Hash.DRAW_RECT, 0.012, 0.130, 0.0035, 0.035, 0, 0, 0, 245);

                    if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                        Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                        menuSelection = 3;

                        keyUp = true;
                        keyUpController = true;
                    }
                }

                // sub menu stuff
                if (menuSelection == 1) {
                    subMenuSelection = 1;

                    UIText Header = new UIText(one, new Point(125, 10), 0.7f, Color.White, GTA.Font.HouseScript, true, false, true);
                    Header.Draw();

                    UIText Option1 = new UIText("Activate Script", new Point(123, 38), 0.5f, Color.White, GTA.Font.ChaletComprimeCologne, true, false, true);
                    Option1.Draw();

                    UIText Option2 = new UIText("Deactivate Script", new Point(123, 60), 0.5f, Color.White, GTA.Font.ChaletComprimeCologne, true, false, true);
                    Option2.Draw();

                    UIText Option3 = new UIText("Open Config File", new Point(123, 80), 0.5f, Color.White, GTA.Font.ChaletComprimeCologne, true, false, true);
                    Option3.Draw();

                    if (subMenuSelection == 1) {
                        // subMenuSelection bar 0 (red)
                        // activate 
                        if (subMenuInt == 0) {
                            Function.Call(Hash.DRAW_RECT, 0.1, 0.070, 0.18, 0.035, 175, 0, 0, 245);
                            Function.Call(Hash.DRAW_RECT, 0.012, 0.070, 0.0035, 0.035, 0, 0, 0, 245);

                            if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                                Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                                scriptNameChecker = one;

                                keyUp = true;
                            }
                        }
                        // deactivate
                        if (subMenuInt == 1) {
                            Function.Call(Hash.DRAW_RECT, 0.1, 0.1, 0.18, 0.035, 175, 0, 0, 245);
                            Function.Call(Hash.DRAW_RECT, 0.012, 0.1, 0.0035, 0.035, 0, 0, 0, 245);

                            if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                                Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                                scriptNameChecker = "";

                                keyUp = true;
                            }
                        }
                        // config
                        if (subMenuInt == 2) {
                            Function.Call(Hash.DRAW_RECT, 0.1, 0.13, 0.18, 0.035, 175, 0, 0, 245);
                            Function.Call(Hash.DRAW_RECT, 0.012, 0.130, 0.0035, 0.035, 0, 0, 0, 245);

                            if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                                Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                                Process.Start(Directory.GetCurrentDirectory() + "\\scripts\\" + one + ".ini");

                                keyUp = true;
                            }
                        }
                    }
                } else {
                    subMenuSelection = 0;
                }

                if (menuSelection == 2) {
                    subMenuSelection = 2;

                    UIText Header = new UIText(two, new Point(125, 10), 0.7f, Color.White, GTA.Font.HouseScript, true, false, true);
                    Header.Draw();

                    UIText Option1 = new UIText("Activate Script", new Point(123, 38), 0.5f, Color.White, GTA.Font.ChaletComprimeCologne, true, false, true);
                    Option1.Draw();

                    UIText Option2 = new UIText("Deactivate Script", new Point(123, 60), 0.5f, Color.White, GTA.Font.ChaletComprimeCologne, true, false, true);
                    Option2.Draw();

                    UIText Option3 = new UIText("Open Config File", new Point(123, 80), 0.5f, Color.White, GTA.Font.ChaletComprimeCologne, true, false, true);
                    Option3.Draw();

                    if (subMenuSelection == 2) {
                        // subMenuSelection bar 0 (red)
                        if (subMenuInt == 0) {
                            Function.Call(Hash.DRAW_RECT, 0.1, 0.070, 0.18, 0.035, 175, 0, 0, 245);
                            Function.Call(Hash.DRAW_RECT, 0.012, 0.070, 0.0035, 0.035, 0, 0, 0, 245);

                            if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                                Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                                scriptNameChecker = two; 

                                keyUp = true;
                            }
                        }
                        if (subMenuInt == 1) {
                            Function.Call(Hash.DRAW_RECT, 0.1, 0.1, 0.18, 0.035, 175, 0, 0, 245);
                            Function.Call(Hash.DRAW_RECT, 0.012, 0.1, 0.0035, 0.035, 0, 0, 0, 245);

                            if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                                Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                                scriptNameChecker = "";

                                keyUp = true;
                            }
                        }
                        if (subMenuInt == 2) {
                            Function.Call(Hash.DRAW_RECT, 0.1, 0.13, 0.18, 0.035, 175, 0, 0, 245);
                            Function.Call(Hash.DRAW_RECT, 0.012, 0.130, 0.0035, 0.035, 0, 0, 0, 245);

                            if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                                Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                                Process.Start(Directory.GetCurrentDirectory() + "\\scripts\\" + two + ".ini");

                                keyUp = true;
                            }
                        }
                    }
                }

                if (menuSelection == 3) {
                    subMenuSelection = 3;

                    UIText Header = new UIText(three, new Point(125, 10), 0.7f, Color.White, GTA.Font.HouseScript, true, false, true);
                    Header.Draw();

                    UIText Option1 = new UIText("Activate Script", new Point(123, 38), 0.5f, Color.White, GTA.Font.ChaletComprimeCologne, true, false, true);
                    Option1.Draw();

                    UIText Option2 = new UIText("Deactivate Script", new Point(123, 60), 0.5f, Color.White, GTA.Font.ChaletComprimeCologne, true, false, true);
                    Option2.Draw();

                    UIText Option3 = new UIText("Open Config File", new Point(123, 80), 0.5f, Color.White, GTA.Font.ChaletComprimeCologne, true, false, true);
                    Option3.Draw();

                    if (subMenuSelection == 3) {
                        // subMenuSelection bar 0 (red)
                        if (subMenuInt == 0) {
                            Function.Call(Hash.DRAW_RECT, 0.1, 0.070, 0.18, 0.035, 175, 0, 0, 245);
                            Function.Call(Hash.DRAW_RECT, 0.012, 0.070, 0.0035, 0.035, 0, 0, 0, 245);

                            if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                                Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                                scriptNameChecker = three;

                                keyUp = true;
                            }
                        }
                        if (subMenuInt == 1) {
                            Function.Call(Hash.DRAW_RECT, 0.1, 0.1, 0.18, 0.035, 175, 0, 0, 245);
                            Function.Call(Hash.DRAW_RECT, 0.012, 0.1, 0.0035, 0.035, 0, 0, 0, 245);

                            if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                                Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                                scriptNameChecker = "";

                                keyUp = true;
                            }
                        }
                        if (subMenuInt == 2) {
                            Function.Call(Hash.DRAW_RECT, 0.1, 0.13, 0.18, 0.035, 175, 0, 0, 245);
                            Function.Call(Hash.DRAW_RECT, 0.012, 0.130, 0.0035, 0.035, 0, 0, 0, 245);

                            if (Game.IsKeyPressed(Keys.Enter) && !keyUp ||
                                Game.IsControlJustPressed(2, GTA.Control.ScriptRDown) && !keyUpController) {
                                Process.Start(Directory.GetCurrentDirectory() + "\\scripts\\" + three + ".ini");

                                keyUp = true;
                            }
                        }
                    }
                }
            }
        }

        private void controllerSupport(object sender, EventArgs e) {
            if (Game.CurrentInputMode == InputMode.GamePad) {
                // Open / Close Menu buttons + back button
                if (Game.IsControlJustReleased(2, GTA.Control.ScriptPadRight) && Game.IsControlJustReleased(2, GTA.Control.ScriptRDown)) {
                    keyUpController = false;
                }
                if (Game.IsControlJustReleased(2, GTA.Control.ScriptRRight)) {
                    keyUpController = false;
                }

                // Up / Down buttons
                if (Game.IsControlJustReleased(2, GTA.Control.ScriptPadUp)) {
                    keyUpController = false;
                }
                if (Game.IsControlJustReleased(2, GTA.Control.ScriptPadDown)) {
                    keyUpController = false;
                }

                // Select button
                if (Game.IsControlJustReleased(2, GTA.Control.ScriptRDown)) {
                    keyUpController = false;
                }
            }
        }

        // Anti-key spam
        private void up(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.F7 || e.KeyCode == Keys.NumPad2 || e.KeyCode == Keys.NumPad0 || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Back) {
                keyUp = false;
            }
        }
    }
}