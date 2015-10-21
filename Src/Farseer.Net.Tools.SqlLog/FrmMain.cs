using System;
using System.Windows.Forms;

namespace Farseer.Net.Tools.SqlLog
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            AddForm(new FrmSqlLog() { TopLevel = false, FormBorderStyle = FormBorderStyle.None });
            AddForm(new FrmSqlExceptionLog() { TopLevel = false, FormBorderStyle = FormBorderStyle.None });
            AddForm(new FrmRunExceptionLog() { TopLevel = false, FormBorderStyle = FormBorderStyle.None });
        }

        /// <summary>
        /// 向TabPages添加控件
        /// </summary>
        /// <param name="frm">新窗口</param>
        private void AddForm(Form frm)
        {
            tabMain.TabPages.Add(frm.Text);
            tabMain.TabPages[tabMain.TabPages.Count - 1].Controls.Add(frm);
            frm.Size = tabMain.TabPages[tabMain.TabPages.Count - 1].Size;
            frm.Show();
            tabMain.TabPages[tabMain.SelectedIndex].Resize += (sender1, e1) =>
            {
                ((Form)((TabPage)sender1).Controls[0]).Size = ((TabPage)sender1).Size;
            };
        }

        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            ((Form)(tabMain.TabPages[tabMain.SelectedIndex]).Controls[0]).Size = tabMain.TabPages[tabMain.SelectedIndex].Size;
        }
    }
}