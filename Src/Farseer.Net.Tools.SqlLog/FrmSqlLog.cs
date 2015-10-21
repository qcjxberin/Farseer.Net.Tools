using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FS.Log;
using FS.Utils;
using FS.Utils.Common;

namespace Farseer.Net.Tools.SqlLog
{
    public partial class FrmSqlLog : Form
    {
        private List<SqlLogEntity> _sqlRecordList;

        public FrmSqlLog()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            btnRefresh.PerformClick();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Clear();
            _sqlRecordList = Common.LoadLog<SqlLogEntity>(Common.SqlLogPathKey);
            _sqlRecordList = _sqlRecordList.OrderByDescending(o => o.CreateAt.ToString("yy-MM-dd HH:mm")).ThenByDescending(o => o.UserTime).ThenBy(o => o.Name).ToList();

            foreach (var result in _sqlRecordList.GroupBy(o => o.CreateAt.ToString("yy-MM-dd"))) { if (result.Key != null) { coxDate.Items.Add(result.Key); } }
            foreach (var result in _sqlRecordList.GroupBy(o => o.MethodName)) { if (result.Key != null) { coxMethodName.Items.Add(result.Key); } }
            foreach (var result in _sqlRecordList.GroupBy(o => o.Name)) { if (result.Key != null) { coxName.Items.Add(result.Key); } }
            foreach (var result in _sqlRecordList.GroupBy(o => o.CmdType)) { coxCmdType.Items.Add(result.Key); }

            btnSelect.PerformClick();
        }

        private void Clear()
        {
            _sqlRecordList = new List<SqlLogEntity>();
            dgv.Rows.Clear();
            coxDate.Items.Clear();
            coxDate.Items.Add("全部");
            coxDate.SelectedIndex = 0;

            coxMethodName.Items.Clear();
            coxMethodName.Items.Add("全部");
            coxMethodName.SelectedIndex = 0;

            coxName.Items.Clear();
            coxName.Items.Add("全部");
            coxName.SelectedIndex = 0;

            coxCmdType.Items.Clear();
            coxCmdType.Items.Add("全部");
            coxCmdType.SelectedIndex = 0;

        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var selectDate = coxDate.SelectedItem.ToString();
            var selectethodName = coxMethodName.SelectedItem.ToString();
            var selecName = coxName.SelectedItem.ToString();
            var selectCmdType = coxCmdType.SelectedItem.ToString();
            dgv.Rows.Clear();
            dgv.Visible = false;
            Task.Factory.StartNew(() =>
            {
                var selectSqlRecordList = _sqlRecordList;
                if (selectDate != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.CreateAt.ToString("yy-MM-dd") == selectDate); }
                if (selectethodName != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.MethodName == selectethodName); }
                if (selecName != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.Name == selecName); }
                if (selectCmdType != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.CmdType.ToString() == selectCmdType); }

                // 加载表
                foreach (var sqlRecordEntity in selectSqlRecordList)
                {
                    var sb = new StringBuilder();
                    sqlRecordEntity.SqlParamList.ForEach(o => sb.AppendFormat("{0} = {1} ", o.Name, o.Value));
                    var entity = sqlRecordEntity;
                    Invoke((EventHandler)delegate
                    {
                        dgv.Rows.Add(entity.CreateAt, entity.UserTime, entity.CmdType, entity.MethodName, entity.LineNo, entity.Name, entity.Sql, sb.ToString());
                    });
                }
                Invoke((EventHandler)delegate
                {
                    toolStripStatusLabel2.Text = selectSqlRecordList.Count.ToString("n0");
                    toolStripStatusLabel4.Text = selectSqlRecordList.Sum(o => o.UserTime).ToString("n0") + " ms";
                    dgv.Visible = true;
                });
            });
        }

        private void dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) { return; }
            if (dgv.CurrentRow.Index == -1) { return; }
            var index = dgv.CurrentRow.Index;
            var currentSqlRecord = _sqlRecordList.Find(o => o.CreateAt == ConvertHelper.ConvertType(dgv.CurrentRow.Cells[0].Value, DateTime.Now));
            if (currentSqlRecord == null) { return; }
            textBox1.Text = currentSqlRecord.CreateAt.ToString();
            textBox9.Text = currentSqlRecord.UserTime.ToString();
            textBox3.Text = currentSqlRecord.MethodName;
            textBox4.Text = currentSqlRecord.LineNo.ToString();
            textBox5.Text = currentSqlRecord.Name;
            textBox6.Text = currentSqlRecord.Sql;
            textBox8.Text = currentSqlRecord.FileName;
            textBox2.Text = currentSqlRecord.CmdType.ToString();

            textBox7.Clear();
            currentSqlRecord.SqlParamList.ForEach(o => textBox7.AppendText(string.Format("{0} = {1}\r\n", o.Name, o.Value)));
        }

        private void btnOpenVS_Click(object sender, EventArgs e)
        {
            Common.OpenVs(textBox8.Text, ConvertHelper.ConvertType(textBox4.Text, 0));
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) { return; }
            btnOpenVS_Click(null, null);
        }

        private void menDelLog_Click(object sender, EventArgs e)
        {
            if (Common.ClearLog<SqlLogEntity>(Common.SqlLogPathKey)) { btnRefresh.PerformClick(); }
        }

        private void menExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menOpenLog_Click(object sender, EventArgs e)
        {
            if (Common.OpenLoad(Common.SqlLogPathKey)) { btnRefresh.PerformClick(); }
        }

        private void coxDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ToolStripComboBox)sender).Items.Count < 2) { return; }
            btnSelect.PerformClick();
        }
    }
}
