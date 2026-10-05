using System.Drawing;
using System.Windows.Forms;

namespace Integral.App
{
    /// <summary>Окно справки: дерево разделов и текст выбранного раздела.</summary>
    public sealed class HelpForm : Form
    {
        private readonly TreeView _tree;
        private readonly RichTextBox _text;

        public HelpForm()
        {
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = Ui.BaseFont();
            Text = "Справка";
            Icon = IconFactory.AppIcon();
            ClientSize = new Size(820, 520);
            MinimumSize = new Size(520, 320);
            StartPosition = FormStartPosition.CenterParent;
            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 250, FixedPanel = FixedPanel.Panel1 };
            _tree = new TreeView { Dock = DockStyle.Fill, HideSelection = false, BorderStyle = BorderStyle.None, ShowLines = false, ItemHeight = 24 };
            _text = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.White, BorderStyle = BorderStyle.None, Padding = new Padding(8) };
            for (int i = 0; i < HelpContent.Sections.GetLength(0); i++)
                _tree.Nodes.Add(new TreeNode(HelpContent.Sections[i, 0]) { Tag = i });
            _tree.AfterSelect += (s, e) => ShowSection((int)e.Node.Tag);
            split.Panel1.Controls.Add(_tree);
            split.Panel2.Controls.Add(_text);
            Controls.Add(split);
            split.SplitterDistance = 250;
            ShowSection(0);
        }

        /// <summary>Открывает раздел справки по номеру.</summary>
        public void ShowSection(int index)
        {
            if (index < 0 || index >= HelpContent.Sections.GetLength(0)) index = 0;
            if (_tree.SelectedNode == null || (int)_tree.SelectedNode.Tag != index) { _tree.SelectedNode = _tree.Nodes[index]; return; }
            _text.Clear();
            _text.SelectionFont = new Font("Tahoma", 14f, FontStyle.Bold);
            _text.SelectionColor = Ui.Ink;
            _text.AppendText(HelpContent.Sections[index, 0] + "\n\n");
            _text.SelectionFont = new Font("Tahoma", 10f);
            _text.SelectionColor = Color.Black;
            _text.AppendText(HelpContent.Sections[index, 1]);
            _text.Select(0, 0);
        }
    }
}
