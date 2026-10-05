using System;
using System.Drawing;
using System.Windows.Forms;

namespace Integral.App
{
    /// <summary>Окно справки: поиск, список разделов и текст выбранного раздела с подсветкой найденного.</summary>
    public sealed class HelpForm : Form
    {
        private readonly TextBox _search;
        private readonly TreeView _tree;
        private readonly RichTextBox _text;
        private readonly Font _fTitle = new Font("Tahoma", 14f, FontStyle.Bold);
        private readonly Font _fHead = new Font("Tahoma", 11f, FontStyle.Bold);
        private readonly Font _fBody = new Font("Tahoma", 10f);
        private readonly Font _fCode = new Font("Courier New", 10f);

        public HelpForm()
        {
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = Ui.BaseFont();
            Text = "Справка";
            Icon = IconFactory.AppIcon();
            ClientSize = new Size(880, 560);
            MinimumSize = new Size(560, 340);
            StartPosition = FormStartPosition.CenterParent;
            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1 };
            _search = new TextBox { Dock = DockStyle.Top, BackColor = Color.White };
            _tree = new TreeView { Dock = DockStyle.Fill, HideSelection = false, BorderStyle = BorderStyle.None, ShowLines = false, ShowRootLines = false, ItemHeight = 24 };
            _text = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.White, BorderStyle = BorderStyle.None };
            var pad = new Panel { Dock = DockStyle.Top, Height = Ui.GapUnrelated };
            split.Panel1.Controls.Add(_tree);
            split.Panel1.Controls.Add(pad);
            split.Panel1.Controls.Add(_search);
            split.Panel1.Padding = new Padding(Ui.GapUnrelated);
            split.Panel2.Padding = new Padding(Ui.GapUnrelated * 2, Ui.GapUnrelated, Ui.GapUnrelated, Ui.GapUnrelated);
            split.Panel2.BackColor = Color.White;
            split.Panel2.Controls.Add(_text);
            Controls.Add(split);
            split.SplitterDistance = 260;
            Ui.SetCue(_search, "Поиск по справке");
            _search.TextChanged += (s, e) => Filter();
            _tree.AfterSelect += (s, e) => { if (e.Node.Tag is int) Render((int)e.Node.Tag); };
            Filter();
        }

        /// <summary>Открывает раздел справки по ключу (см. <see cref="HelpContent"/>).</summary>
        public void ShowTopic(string key)
        {
            if (_search.Text.Length > 0) _search.Text = "";
            int index = HelpContent.IndexOf(key);
            foreach (TreeNode n in _tree.Nodes)
                if (n.Tag is int && (int)n.Tag == index) { _tree.SelectedNode = n; Render(index); return; }
        }

        /// <summary>Оставляет в списке разделы, где встречается текст поиска.</summary>
        private void Filter()
        {
            string q = _search.Text.Trim();
            _tree.BeginUpdate();
            _tree.Nodes.Clear();
            for (int i = 0; i < HelpContent.Sections.Count; i++)
                if (q.Length == 0 || HelpContent.Sections[i].PlainText.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                    _tree.Nodes.Add(new TreeNode(HelpContent.Sections[i].Title) { Tag = i });
            if (_tree.Nodes.Count == 0) _tree.Nodes.Add(new TreeNode("Ничего не найдено") { ForeColor = SystemColors.GrayText });
            _tree.EndUpdate();
            if (_tree.Nodes[0].Tag is int) _tree.SelectedNode = _tree.Nodes[0];
            else _text.Clear();
        }

        private void Put(string text, Font font, Color color, bool bullet = false, int indent = 0)
        {
            _text.SelectionStart = _text.TextLength;
            _text.SelectionLength = 0;
            _text.SelectionFont = font;
            _text.SelectionColor = color;
            _text.SelectionBullet = bullet;
            _text.SelectionIndent = indent;
            _text.SelectedText = text;
        }

        /// <summary>Выводит раздел: заголовок, подзаголовки, списки, примеры; подсвечивает текст поиска.</summary>
        private void Render(int index)
        {
            HelpSection s = HelpContent.Sections[index];
            _text.Clear();
            Put(s.Title + "\n\n", _fTitle, Ui.Ink);
            foreach (string line in s.Lines)
            {
                if (line.StartsWith("## ", StringComparison.Ordinal)) Put("\n" + line.Substring(3) + "\n", _fHead, Ui.Accent);
                else if (line.StartsWith("- ", StringComparison.Ordinal)) Put(line.Substring(2) + "\n", _fBody, Color.Black, true, 8);
                else if (line.StartsWith("    ", StringComparison.Ordinal)) Put(line.Substring(4) + "\n", _fCode, Ui.Ink, false, 16);
                else Put(line + "\n", _fBody, Color.Black);
            }
            Highlight(_search.Text.Trim());
            _text.Select(0, 0);
            _text.ScrollToCaret();
        }

        private void Highlight(string q)
        {
            if (q.Length == 0) return;
            int pos = 0;
            while ((pos = _text.Text.IndexOf(q, pos, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                _text.Select(pos, q.Length);
                _text.SelectionBackColor = Color.FromArgb(254, 240, 138);
                pos += q.Length;
            }
        }
    }
}
