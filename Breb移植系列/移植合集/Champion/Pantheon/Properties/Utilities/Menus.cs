using EloBuddy.SDK.Menu.Values;
using ExorSDK.Utilities;

namespace ExorSDK.Champions.Pantheon
{
    /// <summary>
    ///     The menu class.
    /// </summary>
    internal class Menus
    {
        /// <summary>
        ///     Sets the menu.
        /// </summary>
        public static void Initialize()
        {
            /// <summary>
            ///     Sets the menu for the Q.
            /// </summary>
            Vars.QMenu = Vars.Menu.AddSubMenu("ʹ��Q:");
            {
                Vars.QMenu.Add("combo", new CheckBox("����"));
                Vars.QMenu.Add("killsteal", new CheckBox("��ͷ"));
                Vars.QMenu.Add("harass", new Slider("ɧ�� / ������� >= x%", 50, 10, 101));
                Vars.QMenu.Add("jungleclear", new Slider("��Ұ / ������� >= x%", 50, 10, 101));
            }

            /// <summary>
            ///     Sets the menu for the W.
            /// </summary>
            Vars.WMenu = Vars.Menu.AddSubMenu("ʹ��W:");
            {
                Vars.WMenu.Add("combo", new CheckBox("����"));
                Vars.WMenu.Add("killsteal", new CheckBox("��ͷ"));
                Vars.WMenu.Add("interrupter", new CheckBox("���ܴ��"));
            }

            /// <summary>
            ///     Sets the menu for the E.
            /// </summary>
            Vars.EMenu = Vars.Menu.AddSubMenu("ʹ��E:");
            {
                Vars.EMenu.Add("combo", new CheckBox("����"));
                Vars.EMenu.Add("clear", new Slider("���� /������� >= x%", 50, 10, 101));
            }

            /// <summary>
            ///     Sets the drawings menu.
            /// </summary>
            Vars.DrawingsMenu = Vars.Menu.AddSubMenu("��Ȧ");
            {
                Vars.DrawingsMenu.Add("q", new CheckBox("Q ��Χ"));
                Vars.DrawingsMenu.Add("w", new CheckBox("W ��Χ"));
                Vars.DrawingsMenu.Add("e", new CheckBox("E ��Χ"));
            }
        }
    }
}