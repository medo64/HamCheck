using System.Drawing;

namespace HamCheck {
    internal static class Helper {

        public static Color GetColor(ExamElement element) {
            switch (element.Number) {
                case 2: return Color.FromArgb(48, Color.Green);
                case 3: return Color.FromArgb(96, Color.Yellow);
                case 4: return Color.FromArgb(128, Color.Pink);
            }
            return SystemColors.Control;
        }

    }
}
