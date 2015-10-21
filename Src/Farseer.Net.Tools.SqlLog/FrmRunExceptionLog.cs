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
    public partial class FrmRunExceptionLog : Form
    {
        private List<RunExceptionLogEntity> _sqlRecordList;

        public FrmRunExceptionLog()
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
            _sqlRecordList = Common.LoadLog<RunExceptionLogEntity>(Common.RunExceptionLogPathKey);
            _sqlRecordList = _sqlRecordList.OrderByDescending(o => o.CreateAt.ToString("yy-MM-dd HH:mm")).ToList();

            foreach (var result in _sqlRecordList.GroupBy(o => o.CreateAt.ToString("yy-MM-dd"))) { if (result.Key != null) { coxDate.Items.Add(result.Key); } }
            foreach (var result in _sqlRecordList.GroupBy(o => o.MethodName)) { if (result.Key != null) { coxMethodName.Items.Add(result.Key); } }

            btnSelect.PerformClick();
        }


        private void Clear()
        {
            _sqlRecordList = new List<RunExceptionLogEntity>();
            dgv.Rows.Clear();
            coxDate.Items.Clear();
            coxDate.Items.Add("全部");
            coxDate.SelectedIndex = 0;

            coxMethodName.Items.Clear();
            coxMethodName.Items.Add("全部");
            coxMethodName.SelectedIndex = 0;

        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var selectDate = coxDate.SelectedItem.ToString();
            var selectethodName = coxMethodName.SelectedItem.ToString();
            dgv.Rows.Clear();
            dgv.Visible = false;
            Task.Factory.StartNew(() =>
            {
                var selectSqlRecordList = _sqlRecordList;
                if (selectDate != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.CreateAt.ToString("yy-MM-dd") == selectDate); }
                if (selectethodName != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.MethodName == selectethodName); }
                if (selectethodName != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.MethodName == selectethodName); }

                // 加载表
                foreach (var sqlRecordEntity in selectSqlRecordList)
                {
                    var sb = new StringBuilder();
                    var entity = sqlRecordEntity;
                    Invoke((EventHandler)delegate
                    {
                        dgv.Rows.Add(entity.CreateAt, entity.MethodName, entity.LineNo, entity.Message);
                    });
                }
                Invoke((EventHandler)delegate
                {
                    toolStripStatusLabel2.Text = selectSqlRecordList.Count.ToString("n0");
                    dgv.Visible = true;
                });
            });
        }

        private void dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) { return; }
            if (dgv.CurrentRow.Index == -1) { return; }
            var currentSqlRecord = _sqlRecordList.Find(o => o.CreateAt == ConvertHelper.ConvertType(dgv.CurrentRow.Cells[0].Value, DateTime.Now));
            if (currentSqlRecord == null) { return; }
            textBox1.Text = currentSqlRecord.CreateAt.ToString();
            textBox3.Text = currentSqlRecord.MethodName;
            textBox4.Text = currentSqlRecord.LineNo.ToString();
            textBox8.Text = currentSqlRecord.FileName;
            textBox5.Text = currentSqlRecord.Message;
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
            if (Common.ClearLog<RunExceptionLogEntity>(Common.RunExceptionLogPathKey)) { btnRefresh.PerformClick(); }
        }

        private void menExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menOpenLog_Click(object sender, EventArgs e)
        {
            if (Common.OpenLoad(Common.RunExceptionLogPathKey)) { btnRefresh.PerformClick(); }
        }

        private void coxDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ToolStripComboBox)sender).Items.Count < 2) { return; }
            btnSelect.PerformClick();
        }
    }
}
