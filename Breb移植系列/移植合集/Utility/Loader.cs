using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

namespace PortAIO.Utility
{
    class Loader
    {
        public static bool VCursor { get { return Miscc["VCursor"].Cast<CheckBox>().CurrentValue; } }
        public static bool limitedShat { get { return Miscc["limitedShat"].Cast<CheckBox>().CurrentValue; } }
        public static bool autoLevel { get { return Miscc["autoLevel"].Cast<CheckBox>().CurrentValue; } }
        public static bool chatLogger { get { return Miscc["chatLogger"].Cast<CheckBox>().CurrentValue; } }
        public static bool autoFF { get { return Miscc["autoFF"].Cast<CheckBox>().CurrentValue; } }
        public static bool urfSpell { get { return Miscc["urfSpell"].Cast<CheckBox>().CurrentValue; } }
        public static bool pastingSharp { get { return Miscc["pastingSharp"].Cast<CheckBox>().CurrentValue; } }
        public static bool autoJungle { get { return Miscc["autoJungle"].Cast<CheckBox>().CurrentValue; } }


        public static bool useActivator { get { return Miscc["activator"].Cast<CheckBox>().CurrentValue; } }
        public static bool sdkPredictioner { get { return Miscc["sdkPredictioner"].Cast<CheckBox>().CurrentValue; } }
        public static bool cheat { get { return Miscc["cheat"].Cast<CheckBox>().CurrentValue; } }
        public static bool autoSharp { get { return Miscc["autoSharp"].Cast<CheckBox>().CurrentValue; } }
        public static bool useTracker { get { return Miscc["tracker"].Cast<CheckBox>().CurrentValue; } }
        public static bool useRecall { get { return Miscc["recall"].Cast<CheckBox>().CurrentValue; } }
        public static bool useSkin { get { return Miscc["skin"].Cast<CheckBox>().CurrentValue; } }
        public static bool champOnly { get { return Miscc["champ"].Cast<CheckBox>().CurrentValue; } }
        public static bool utilOnly { get { return Miscc["util"].Cast<CheckBox>().CurrentValue; } }
        public static bool evade { get { return Miscc["evade"].Cast<CheckBox>().CurrentValue; } }
        public static bool godTracker { get { return Miscc["godTracker"].Cast<CheckBox>().CurrentValue; } }
        public static bool ping { get { return Miscc["ping"].Cast<CheckBox>().CurrentValue; } }
        public static bool human { get { return Miscc["human"].Cast<CheckBox>().CurrentValue; } }
        public static int soraka { get { return Miscc["soraka"].Cast<ComboBox>().CurrentValue; } }
        public static int poppy { get { return Miscc["poppy"].Cast<ComboBox>().CurrentValue; } }
        public static int kogmaw { get { return Miscc["kogmaw"].Cast<ComboBox>().CurrentValue; } }
        public static int lux { get { return Miscc["lux"].Cast<ComboBox>().CurrentValue; } }
        public static int leesin { get { return Miscc["leesin"].Cast<ComboBox>().CurrentValue; } }
        public static int leblanc { get { return Miscc["leblanc"].Cast<ComboBox>().CurrentValue; } }
        public static bool bubba { get { return Miscc["bubba"].Cast<CheckBox>().CurrentValue; } }
        public static int kalista { get { return Miscc["kalista"].Cast<ComboBox>().CurrentValue; } }
        public static bool gank { get { return Miscc["gank"].Cast<CheckBox>().CurrentValue; } }
        public static int diana { get { return Miscc["diana"].Cast<ComboBox>().CurrentValue; } }
        public static int ryze { get { return Miscc["ryze"].Cast<ComboBox>().CurrentValue; } }
        public static int draven { get { return Miscc["draven"].Cast<ComboBox>().CurrentValue; } }
        public static int cait { get { return Miscc["cait"].Cast<ComboBox>().CurrentValue; } }
        public static bool intro { get { return Miscc["intro"].Cast<CheckBox>().CurrentValue; } }
        public static int twitch { get { return Miscc["twitch"].Cast<ComboBox>().CurrentValue; } }
        public static int nidalee { get { return Miscc["nidalee"].Cast<ComboBox>().CurrentValue; } }
        public static int morgana { get { return Miscc["morgana"].Cast<ComboBox>().CurrentValue; } }
        public static int twistedfate { get { return Miscc["twistedfate"].Cast<ComboBox>().CurrentValue; } }
        public static int sona { get { return Miscc["sona"].Cast<ComboBox>().CurrentValue; } }
        public static int shaco { get { return Miscc["shaco"].Cast<ComboBox>().CurrentValue; } }
        public static int sion { get { return Miscc["sion"].Cast<ComboBox>().CurrentValue; } }
        public static int trundle { get { return Miscc["trundle"].Cast<ComboBox>().CurrentValue; } }
        public static int lucian { get { return Miscc["lucian"].Cast<ComboBox>().CurrentValue; } }
        public static int ashe { get { return Miscc["ashe"].Cast<ComboBox>().CurrentValue; } }
        public static int vayne { get { return Miscc["vayne"].Cast<ComboBox>().CurrentValue; } }
        public static int quinn { get { return Miscc["quinn"].Cast<ComboBox>().CurrentValue; } }
        public static int jayce { get { return Miscc["jayce"].Cast<ComboBox>().CurrentValue; } }
        public static int yasuo { get { return Miscc["yasuo"].Cast<ComboBox>().CurrentValue; } }
        public static int katarina { get { return Miscc["katarina"].Cast<ComboBox>().CurrentValue; } }
        public static int xerath { get { return Miscc["xerath"].Cast<ComboBox>().CurrentValue; } }
        public static int gragas { get { return Miscc["gragas"].Cast<ComboBox>().CurrentValue; } }
        public static int gangplank { get { return Miscc["gangplank"].Cast<ComboBox>().CurrentValue; } }
        public static int ezreal { get { return Miscc["ezreal"].Cast<ComboBox>().CurrentValue; } }
        public static int brand { get { return Miscc["brand"].Cast<ComboBox>().CurrentValue; } }
        public static int blitzcrank { get { return Miscc["blitzcrank"].Cast<ComboBox>().CurrentValue; } }
        public static int corki { get { return Miscc["corki"].Cast<ComboBox>().CurrentValue; } }
        public static int darius { get { return Miscc["darius"].Cast<ComboBox>().CurrentValue; } }
        public static int evelynn { get { return Miscc["evelynn"].Cast<ComboBox>().CurrentValue; } }
        public static int jhin { get { return Miscc["jhin"].Cast<ComboBox>().CurrentValue; } }
        public static int jax { get { return Miscc["jax"].Cast<ComboBox>().CurrentValue; } }
        public static int kindred { get { return Miscc["kindred"].Cast<ComboBox>().CurrentValue; } }
        public static int kayle { get { return Miscc["kayle"].Cast<ComboBox>().CurrentValue; } }
        public static int ekko { get { return Miscc["ekko"].Cast<ComboBox>().CurrentValue; } }
        public static int rumble { get { return Miscc["rumble"].Cast<ComboBox>().CurrentValue; } }
        public static int riven { get { return Miscc["riven"].Cast<ComboBox>().CurrentValue; } }
        public static int graves { get { return Miscc["graves"].Cast<ComboBox>().CurrentValue; } }
        public static int ahri { get { return Miscc["ahri"].Cast<ComboBox>().CurrentValue; } }
        public static bool banwards { get { return Miscc["banwards"].Cast<CheckBox>().CurrentValue; } }
        public static bool antialistar { get { return Miscc["antialistar"].Cast<CheckBox>().CurrentValue; } }
        public static bool traptrack { get { return Miscc["traptrack"].Cast<CheckBox>().CurrentValue; } }
        public static int elise { get { return Miscc["elise"].Cast<ComboBox>().CurrentValue; } }
        public static int rengar { get { return Miscc["rengar"].Cast<ComboBox>().CurrentValue; } }
        public static int zed { get { return Miscc["zed"].Cast<ComboBox>().CurrentValue; } }
        public static int reksai { get { return Miscc["reksai"].Cast<ComboBox>().CurrentValue; } }
        public static int volibear { get { return Miscc["volibear"].Cast<ComboBox>().CurrentValue; } }
        public static int anivia { get { return Miscc["anivia"].Cast<ComboBox>().CurrentValue; } }
        public static int taliyah { get { return Miscc["taliyah"].Cast<ComboBox>().CurrentValue; } }
        public static int janna { get { return Miscc["janna"].Cast<ComboBox>().CurrentValue; } }
        public static int irelia { get { return Miscc["irelia"].Cast<ComboBox>().CurrentValue; } }
        public static int sivir { get { return Miscc["sivir"].Cast<ComboBox>().CurrentValue; } }
        public static int jarvan { get { return Miscc["jarvan"].Cast<ComboBox>().CurrentValue; } }
        public static int braum { get { return Miscc["braum"].Cast<ComboBox>().CurrentValue; } }
        public static int karma { get { return Miscc["karma"].Cast<ComboBox>().CurrentValue; } }
        public static int teemo { get { return Miscc["teemo"].Cast<ComboBox>().CurrentValue; } }
        public static int evadeCB { get { return Miscc["evadeCB"].Cast<ComboBox>().CurrentValue; } }
        public static int aramCB { get { return Miscc["aramCB"].Cast<ComboBox>().CurrentValue; } }


        public static Menu Miscc;

        private static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        public static List<string> RandomUltChampsList = new List<string>(new[] { "Ezreal", "Jinx", "Ashe", "Draven", "Gangplank", "Ziggs", "Lux", "Xerath" });
        public static List<string> BaseUltList = new List<string>(new[] { "Jinx", "Ashe", "Draven", "Ezreal", "Karthus"});
        public static List<string> Champion = new List<string>(new[] {
            "Soraka", // 0
            "KogMaw", // 1
            "LeeSin", // 2
            "Kalista", // 3
            "Diana", // 4
            "Caitlyn", // 5
            "Twitch", // 6
            "Nidalee", // 7
            "Lucian", // 8
            "Ashe", // 9
            "Vayne", // 10
            "Jayce", // 11
            "Yasuo", // 12
            "Katarina", // 13
            "Xerath", // 14
            "Gragas", // 15
            "Draven", // 16
            "Ezreal", // 17
            "Brand", // 18
            "Blitzcrank", //19
            "Corki", // 20
            "Darius", // 21
            "Evelynn", // 22
            "Jhin", //23
            "Kindred", // 24
            "Lux", //25
            "Morgana", //26
            "Quinn", //27
            "TwistedFate", // 28
            "Kayle", //29
            "Jax", // 30
            "Sion", // 31
            "Ryze", //32
            "Sona", // 33
            "Trundle", // 34
            "Gangplank", //35
            "Poppy", // 36
            "Shaco", // 37
            "Leblanc", // 38
            "Ekko", // 39
            "Rumble", // 40
            "Riven", // 41
            "Graves", // 42
            "Elise", // 43
            "Rengar", //44
            "Zed", //45
            "Ahri", //46
            "RekSai", //47
            "Volibear", //48
            "Anivia", //49
            "Taliyah", //50
            "Janna", //51
            "Irelia", //52
            "Sivir", //53
            "JarvanIV", // 54
            "Braum", //55
            "Karma", //56
            "Teemo", //57
        });

        public static void Menu()
        {
            Miscc = MainMenu.AddMenu("PortAIO Misc", "berbsicmisc");
            Miscc.Add("intro", new CheckBox("Load Intro?", true));
            Miscc.AddSeparator();
            Miscc.AddGroupLabel("Champion Changes");
            if (Champion.Contains(ObjectManager.Player.ChampionName))
            {
                if (Player.ChampionName.Equals(Champion[0]))
                {
                    Miscc.Add("soraka", new ComboBox("Use addon for Soraka : ", 0, "Sophie Soraka", "ChallengerSeries"));
                }
                if (Player.ChampionName.Equals(Champion[1]))
                {
                    Miscc.Add("kogmaw", new ComboBox("Use addon for Kog'Maw : ", 0, "Sharpshooter", "ChallengerSeries", "OKTW"));
                }
                if (Player.ChampionName.Equals(Champion[2]))
                {
                    Miscc.Add("leesin", new ComboBox("Use addon for Lee Sin : ", 0, "ValvraveSharp", "El Lee Sin : Reborn", "FreshBooster"));
                }
                if (Player.ChampionName.Equals(Champion[3]))
                {
                    Miscc.Add("kalista", new ComboBox("Use addon for Kalista : ", 0, "iKalista", "iKalista - Reborn", "ChallengerSeries"));
                }
                if (Player.ChampionName.Equals(Champion[4]))
                {
                    Miscc.Add("diana", new ComboBox("Use addon for Diana : ", 0, "ElDiana", "Nechrito Diana"));
                }
                if (Player.ChampionName.Equals(Champion[5]))
                {
                    Miscc.Add("cait", new ComboBox("Use addon for Caitlyn : ", 0, "OKTW", "ExorSeries : AIO"));
                }
                if (Player.ChampionName.Equals(Champion[6]))
                {
                    Miscc.Add("twitch", new ComboBox("Use addon for Twitch : ", 0, "OKTW", "Nechrito Twitch", "iTwitch"));
                }
                if (Player.ChampionName.Equals(Champion[7]))
                {
                    Miscc.Add("nidalee", new ComboBox("Use addon for Nidalee : ", 0, "Kurisu", "Nechrito"));
                }
                if (Player.ChampionName.Equals(Champion[8]))
                {
                    Miscc.Add("lucian", new ComboBox("Use addon for Lucian : ", 0, "LCS Lucian", "ChallengerSeries", "iLucian"));
                }
                if (Player.ChampionName.Equals(Champion[9]))
                {
                    Miscc.Add("ashe", new ComboBox("Use addon for Ashe : ", 0, "OKTW", "ChallengerSeries"));
                }
                if (Player.ChampionName.Equals(Champion[10]))
                {
                    Miscc.Add("vayne", new ComboBox("Use addon for Vayne : ", 0, "ChallengerVayne", "VayneHunterReborn", "hi im gosu", "hVayne SDK"));
                }
                if (Player.ChampionName.Equals(Champion[11]))
                {
                    Miscc.Add("jayce", new ComboBox("Use addon for Jayce : ", 0, "OKTW", "Hoe's Jayce"));
                }
                if (Player.ChampionName.Equals(Champion[12]))
                {
                    Miscc.Add("yasuo", new ComboBox("Use addon for Yasuo : ", 0, "ValvraveSharp", "YasuoPro", "GosuMechanics"));
                }
                if (Player.ChampionName.Equals(Champion[13]))
                {
                    Miscc.Add("katarina", new ComboBox("Use addon for Katarina : ", 0, "Staberina", "e.Motion Katarina"));
                }
                if (Player.ChampionName.Equals(Champion[14]))
                {
                    Miscc.Add("xerath", new ComboBox("Use addon for Xerath : ", 0, "OKTW", "ElXerath"));
                }
                if (Player.ChampionName.Equals(Champion[15]))
                {
                    Miscc.Add("gragas", new ComboBox("Use addon for Gragas : ", 0, "Drunk Carry", "Nechrito"));
                }
                if (Player.ChampionName.Equals(Champion[16]))
                {
                    Miscc.Add("draven", new ComboBox("Use addon for Draven : ", 0, "Sharp Shooter/Exor", "Tyler1"));
                }
                if (Player.ChampionName.Equals(Champion[17]))
                {
                    Miscc.Add("ezreal", new ComboBox("Use addon for Ezreal : ", 0, "OKTW", "iDzEzreal"));
                }
                if (Player.ChampionName.Equals(Champion[18]))
                {
                    Miscc.Add("brand", new ComboBox("Use addon for Brand : ", 0, "TheBrand", "OKTW"));
                }
                if (Player.ChampionName.Equals(Champion[19]))
                {
                    Miscc.Add("blitzcrank", new ComboBox("Use addon for Blitzcrank : ", 0, "FreshBooster", "OKTW", "KurisuBlitz"));
                }
                if (Player.ChampionName.Equals(Champion[20]))
                {
                    Miscc.Add("corki", new ComboBox("Use addon for Corki : ", 0, "ElCorki", "OKTW", "D-Corki"));
                }
                if (Player.ChampionName.Equals(Champion[21]))
                {
                    Miscc.Add("darius", new ComboBox("Use addon for Darius : ", 0, "ExoryAIO", "OKTW"));
                }
                if (Player.ChampionName.Equals(Champion[22]))
                {
                    Miscc.Add("evelynn", new ComboBox("Use addon for Evelynn : ", 0, "Evelynn#", "OKTW"));
                }
                if (Player.ChampionName.Equals(Champion[23]))
                {
                    Miscc.Add("jhin", new ComboBox("Use addon for Jhin : ", 0, "Jhin Virtuoso", "OKTW", "hJhin"));
                }
                if (Player.ChampionName.Equals(Champion[24]))
                {
                    Miscc.Add("kindred", new ComboBox("Use addon for Kindred : ", 0, "Kindred Yin Yang", "OKTW"));
                }
                if (Player.ChampionName.Equals(Champion[25]))
                {
                    Miscc.Add("lux", new ComboBox("Use addon for Lux : ", 0, "MoonLux", "OKTW"));
                }
                if (Player.ChampionName.Equals(Champion[26]))
                {
                    Miscc.Add("morgana", new ComboBox("Use addon for Morgana : ", 0, "Kurisu Morgana", "OKTW"));
                }
                if (Player.ChampionName.Equals(Champion[27]))
                {
                    Miscc.Add("quinn", new ComboBox("Use addon for Quinn : ", 0, "GFuel Quinn", "OKTW"));
                }
                if (Player.ChampionName.Equals(Champion[28]))
                {
                    Miscc.Add("twistedfate", new ComboBox("Use addon for TwistedFate : ", 0, "Esk0r", "OKTW"));
                }
                if (Player.ChampionName.Equals(Champion[29]))
                {
                    Miscc.Add("kayle", new ComboBox("Use addon for Kayle : ", 0, "SephKayle", "OKTW"));
                }
                if (Player.ChampionName.Equals(Champion[30]))
                {
                    Miscc.Add("jax", new ComboBox("Use addon for Jax : ", 0, "xQx Jax", "NoobJaxReloaded"));
                }
                if (Player.ChampionName.Equals(Champion[31]))
                {
                    Miscc.Add("sion", new ComboBox("Use addon for Sion : ", 0, "UnderratedAIO", "SimpleSion"));
                }
                if (Player.ChampionName.Equals(Champion[32]))
                {
                    Miscc.Add("ryze", new ComboBox("Use addon for Ryze : ", 0, "ExoryAIO", "ElEasy Ryze", "SluttyRyze", "Arcane Ryze"));
                }
                if (Player.ChampionName.Equals(Champion[33]))
                {
                    Miscc.Add("sona", new ComboBox("Use addon for Sona : ", 0, "vSupport", "ElEasy Sona"));
                }
                if (Player.ChampionName.Equals(Champion[34]))
                {
                    Miscc.Add("trundle", new ComboBox("Use addon for Trundle : ", 0, "ElTrundle", "FastTrundle"));
                }
                if (Player.ChampionName.Equals(Champion[35]))
                {
                    Miscc.Add("gangplank", new ComboBox("Use addon for GangPlank : ", 0, "UnderratedAIO"));
                }
                if (Player.ChampionName.Equals(Champion[36]))
                {
                    Miscc.Add("poppy", new ComboBox("Use addon for Poppy : ", 0, "UnderratedAIO", "BadaoKingdom"));
                }
                if (Player.ChampionName.Equals(Champion[37]))
                {
                    Miscc.Add("shaco", new ComboBox("Use addon for Shaco : ", 0, "UnderratedAIO", "ChewyMoon's Shaco"));
                }
                if (Player.ChampionName.Equals(Champion[38]))
                {
                    Miscc.Add("leblanc", new ComboBox("Use addon for LeBlanc : ", 0, "PopBlanc", "xQx LeBlanc", "FreshBooster"));
                }
                if (Player.ChampionName.Equals(Champion[39]))
                {
                    Miscc.Add("Ekko", new ComboBox("Use addon for Ekko : ", 0, "OKTW", "ElEkko", "EkkoGod"));
                }
                if (Player.ChampionName.Equals(Champion[40]))
                {
                    Miscc.Add("Rumble", new ComboBox("Use addon for Rumble : ", 0, "Underrated Rumble", "ElRumble"));
                }
                if (Player.ChampionName.Equals(Champion[41]))
                {
                    Miscc.Add("Riven", new ComboBox("Use addon for Riven : ", 0, "NechritoRiven", "Heaven Strike Riven", "KurisuRiven"));
                }
                if (Player.ChampionName.Equals(Champion[42]))
                {
                    Miscc.Add("Graves", new ComboBox("Use addon for Graves : ", 0, "OKTW", "D-Graves"));
                }
                if (Player.ChampionName.Equals(Champion[43]))
                {
                    Miscc.Add("Elise", new ComboBox("Use addon for Elise : ", 0, "GFuel Elise", "D-Elise"));
                }
                if (Player.ChampionName.Equals(Champion[44]))
                {
                    Miscc.Add("rengar", new ComboBox("Use addon for Rengar : ", 0, "ElRengar", "D-Rengar", "PrideStalker"));
                }
                if (Player.ChampionName.Equals(Champion[45]))
                {
                    Miscc.Add("zed", new ComboBox("Use addon for Zed : ", 0, "ValvraveSharp", "Ze-D is Back", "iDZed"));
                }
                if (Player.ChampionName.Equals(Champion[46]))
                {
                    Miscc.Add("ahri", new ComboBox("Use addon for Ahri : ", 0, "OKTW", "AhriSharp"));
                }
                if (Player.ChampionName.Equals(Champion[47]))
                {
                    Miscc.Add("reksai", new ComboBox("Use addon for RekSai : ", 0, "D-RekSai", "HeavenStrike"));
                }
                if (Player.ChampionName.Equals(Champion[48]))
                {
                    Miscc.Add("volibear", new ComboBox("Use addon for VoliBear : ", 0, "Underrated Voli", "VoliPower"));
                }
                if (Player.ChampionName.Equals(Champion[49]))
                {
                    Miscc.Add("anivia", new ComboBox("Use addon for Anivia : ", 0, "OKTW", "ExorAnivia"));
                }
                if (Player.ChampionName.Equals(Champion[50]))
                {
                    Miscc.Add("taliyah", new ComboBox("Use addon for Taliyah : ", 0, "Taliyah", "TophSharp"));
                }
                if (Player.ChampionName.Equals(Champion[51]))
                {
                    Miscc.Add("janna", new ComboBox("Use addon for Janna : ", 0, "LCS Janna", "FreshBooster"));
                }
                if (Player.ChampionName.Equals(Champion[52]))
                {
                    Miscc.Add("irelia", new ComboBox("Use addon for Irelia : ", 0, "ChallengerSeries", "IreliaGOD", "Irelia II"));
                }
                if (Player.ChampionName.Equals(Champion[53]))
                {
                    Miscc.Add("sivir", new ComboBox("Use addon for Sivir : ", 0, "OKTW", "ExorAIO SDK"));
                }
                if (Player.ChampionName.Equals(Champion[54]))
                {
                    Miscc.Add("jarvan", new ComboBox("Use addon for Jarvan : ", 0, "BrianSharp", "D_Jarvan"));
                }
                if (Player.ChampionName.Equals(Champion[55]))
                {
                    Miscc.Add("braum", new ComboBox("Use addon for Braum : ", 0, "OKTW", "FreshBooster"));
                }
                if (Player.ChampionName.Equals(Champion[56]))
                {
                    Miscc.Add("karma", new ComboBox("Use addon for Karma : ", 0, "Spirit Karma", "Esk0r Karma"));
                }
                if (Player.ChampionName.Equals(Champion[57]))
                {
                    Miscc.Add("teemo", new ComboBox("Use addon for Teemo : ", 0, "Sharpshooter", "Swiftly Teemo"));
                }
            }
            else
            {
                Miscc.AddLabel("This champion is not supported for these feature.");
            }
            Miscc.AddSeparator();
            Miscc.AddGroupLabel("选择加载内容");
            Miscc.AddLabel("更改以下内容，请按 F5 重新载入 >>");
            Miscc.Add("champ", new CheckBox("英雄模式? (只载入英雄脚本)", false));
            Miscc.Add("util", new CheckBox("功能模式? (只载入功能脚本)", false));
            Miscc.AddSeparator();
            Miscc.Add("activator", new CheckBox("载入 El活化剂?"));
            Miscc.Add("tracker", new CheckBox("载入 Nabb计时器?"));
            Miscc.Add("recall", new CheckBox("载入 回城计时?"));
            Miscc.Add("skin", new CheckBox("开启换肤?"));
            Miscc.AddSeparator();
            Miscc.Add("evade", new CheckBox("开启 躲避?", false));
            Miscc.Add("godTracker", new CheckBox("载入 野区计时（El活化剂中有）?", false));
            Miscc.Add("ping", new CheckBox("载入 信号管理器（玩家信号）?", false));
            Miscc.Add("human", new CheckBox("载入 人性化?", false));
            Miscc.AddSeparator();
            Miscc.Add("gank", new CheckBox("载入 Gank提示?", false));
            Miscc.Add("cheat", new CheckBox("开启 辅助探测器?", false));
            Miscc.Add("banwards", new CheckBox("开启 Sebby 人性化?", false));
            Miscc.Add("antialistar", new CheckBox("开启 防牛头冲撞?", true));
            Miscc.AddSeparator();
            Miscc.Add("autoSharp", new CheckBox("开启Auto Sharp(挂机)?", false));
            Miscc.Add("sdkPredictioner", new CheckBox("开启 SDK预判者?", true));
            Miscc.Add("traptrack", new CheckBox("开启 陷阱计时?", false));
            Miscc.Add("limitedShat", new CheckBox("开启 LimitedShat?", false));
            Miscc.AddSeparator();
            Miscc.Add("autoLevel", new CheckBox("开启 自动加点?", false));
            Miscc.Add("chatLogger", new CheckBox("开启 聊天记录?", false));
            Miscc.Add("autoFF", new CheckBox("开启 自动投降?", false));
            Miscc.Add("urfSpell", new CheckBox("开启 阿福快打技能狂放?", false));
            Miscc.AddSeparator();
            Miscc.Add("pastingSharp", new CheckBox("开启 PastingSharp?", false));
            //Miscc.Add("VCursor", new CheckBox("Enable VCursor?", false));
            Miscc.Add("autoJungle", new CheckBox("开启 爱台湾自动打野?", false));
            Miscc.AddSeparator();
            Miscc.AddGroupLabel("功能切换 :");
            Miscc.Add("evadeCB", new ComboBox("躲避切换?", 0, "ezEvade", "Evade#"));
            Miscc.Add("aramCB", new ComboBox("大乱斗挂机切换?", 0, "AutoSharp", "AramDETFull"));

            /*
            Miscc.Add("stream", new CheckBox("Enable StreamBuddy?", false));
            public static bool stream { get { return Miscc["stream"].Cast<CheckBox>().CurrentValue; } }
            public static bool randomUlt { get { return Miscc["randomUlt"].Cast<CheckBox>().CurrentValue; } }
            public static bool baseUlt { get { return Miscc["baseUlt"].Cast<CheckBox>().CurrentValue; } }

            if (RandomUltChampsList.Contains(ObjectManager.Player.ChampionName))
            {
                Miscc.Add("randomUlt", new CheckBox("Enable Random Ult?", false));
            }

            if (BaseUltList.Contains(ObjectManager.Player.ChampionName))
            {
                Miscc.Add("baseUlt", new CheckBox("Enable Base Ult?", false));
            }
            */
        }
    }

    }