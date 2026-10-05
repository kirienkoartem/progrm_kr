using System;
using System.Drawing;
using System.Windows.Forms;

namespace Integral.App
{
    /// <summary>Модальное окно «О программе»: сведения об авторе и описание программы.</summary>
    public sealed class AboutForm : Form
    {
        public AboutForm()
        {
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = Ui.BaseFont();
            Text = "О программе";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            

            var pic = new PictureBox { Image = IconFactory.AppImage(48), Location = new Point(16, 16), Size = new Size(48, 48) };
            var title = new Label { Text = AppInfo.Title, Font = new Font("Tahoma", 14f, FontStyle.Bold), Location = new Point(76, 16), Size = new Size(468, 28), ForeColor = Ui.Ink };
            var sub = new Label { Text = "Курсовая работа по дисциплине «Основы программирования»", Location = new Point(76, 44), Size = new Size(468, 20), ForeColor = SystemColors.GrayText };
            Controls.AddRange(new Control[] { pic, title, sub });

            string[,] rows =
            {
                { "Университет", AppInfo.University }, { "Факультет", AppInfo.Faculty }, { "Кафедра", AppInfo.Department },
                { "Группа", AppInfo.Group }, { "Студент", AppInfo.Student }, { "Тема", AppInfo.Topic },
                { "Руководитель", AppInfo.Supervisor }, { "Год", AppInfo.Year },
            };
            int y = 80;
            for (int i = 0; i < rows.GetLength(0); i++)
            {
                int h = Math.Max(Ui.ControlHeight, TextRenderer.MeasureText(rows[i, 1], Font, new Size(420, 1000), TextFormatFlags.WordBreak).Height + 2);
                Controls.Add(new Label { Text = rows[i, 0] + ":", Location = new Point(16, y), Size = new Size(104, Ui.ControlHeight), ForeColor = SystemColors.GrayText });
                Controls.Add(new Label { Text = rows[i, 1], Location = new Point(124, y), Size = new Size(420, h) });
                y += h + Ui.GapRelated;
            }
            var desc = new TextBox
            {
                Text = AppInfo.Description, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = Color.White,
                Location = new Point(16, y + Ui.GapUnrelated), Size = new Size(528, 104),
            };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Size = new Size(96, Ui.ButtonHeight), Location = new Point(448, y + Ui.GapUnrelated + 104 + Ui.GapUnrelated), UseVisualStyleBackColor = true };
            Controls.AddRange(new Control[] { desc, ok });
            ClientSize = new Size(560, ok.Bottom + 16);
            AcceptButton = ok; CancelButton = ok; ActiveControl = ok;
        }
    }
}
