using System;
using System.Drawing;
using System.Windows.Forms;
namespace MCGalaxy.Gui.Popups
{
    internal sealed partial class TokenSelector : Form
    {
        public string Token;
        public TokenSelector(string title)
        {
            InitializeComponent();
            Text = title;
            SuspendLayout();
            foreach (ChatToken token in ChatTokens.Standard)
            {
                MakeButton(token);
            }
            foreach (ChatToken token in ChatTokens.Custom)
            {
                MakeButton(token);
            }
            UpdateBaseLayout();
            ResumeLayout(false);
        }
        void TokenSelector_Load(object sender, EventArgs e) => GuiUtils.SetIcon(this);
        int index = 0;
        void MakeButton(ChatToken token)
        {
            int row = index / 9, col = index % 9;
            index++;
            Button btn = new()
            {
                Location = new(9 + row * 110, 7 + col * 40),
                Size = new(110, 40),
                Name = "b" + index,
                TabIndex = index
            };
            toolTip.SetToolTip(btn, token.Description);
            btn.Text = token.Trigger;
            btn.Click += delegate
            {
                Token = token.Trigger;
                DialogResult = DialogResult.OK;
                Close();
            };
            btn.Margin = new(0);
            btn.UseMnemonic = false;
            btn.UseVisualStyleBackColor = false;
            btn.Font = new("Microsoft Sans Serif", 9.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Controls.Add(btn);
        }
        void UpdateBaseLayout()
        {
            int rows = index / 9;
            if ((index % 9) != 0)
            {
                rows++;
            }
            int x;
            if ((rows & 1) == 0)
            {
                x = rows * 110 / 2 - (100 / 2);
            }
            else
            {
                x = (rows / 2 * 110) + 5;
            }
            btnCancel.Location = new(8 + x, 12 + 40 * 9);
            ClientSize = new(18 + 110 * rows, 47 + 40 * 9);
        }
    }
}