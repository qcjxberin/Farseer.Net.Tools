using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using FS.Utils;
using FS.Utils.Common;
using Microsoft.Win32;

namespace Farseer.Net.Tools.SqlLog
{
    public static class Common
    {
        private const string RegeditPath = @"Software\Farseer\Tools\SqlLog";
        public const string SqlLogPathKey = @"SqlLogPath";
        public const string SqlExceptionLogPathKey = @"SqlExceptionLogPath";
        public const string RunExceptionLogPathKey = @"RunExceptionLogPath";
        private static readonly Dictionary<string, string> DicLogPath = new Dictionary<string, string>();

        static Common()
        {
            DicLogPath.Add(SqlLogPathKey, LoadRegeditValue(SqlLogPathKey));
            DicLogPath.Add(SqlExceptionLogPathKey, LoadRegeditValue(SqlExceptionLogPathKey));
            DicLogPath.Add(RunExceptionLogPathKey, LoadRegeditValue(RunExceptionLogPathKey));
            DicLogPath.Add("vs", "");
        }

        /// <summary>
        /// 打开VS
        /// </summary>
        public static void OpenVs(string filePath, int lineNo)
        {
            var isHaveVs = Process.GetProcessesByName("devenv").Length > 0;

            #region 注册表打开
            try
            {
                // 先获取VS安装路径
                if (String.IsNullOrWhiteSpace(DicLogPath["vs"]))
                {
                    // 通过注册表，找到VS安装路径
                    var regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio");
                    if (regKey != null)
                    {
                        // 找到末尾带_Config名字的Key项
                        var lstSubKey = regKey.GetSubKeyNames().Where(o => o.EndsWith("_Config")).OrderByDescending(o => ConvertHelper.ConvertType(o.Split('.')[0], 0));

                        foreach (var openSubKey in lstSubKey.Select(subKey => regKey.OpenSubKey(subKey + @"\Setup\VS\")).Where(openSubKey => openSubKey != null))
                        {
                            // 找到安装路径
                            DicLogPath["vs"] = openSubKey.GetValue("EnvironmentPath").ToString();
                            if (File.Exists(DicLogPath["vs"])) break;
                            DicLogPath["vs"] = String.Empty;
                        }
                    }
                }

                // 用VS打开，并定位行号（不存在进程时）
                if (File.Exists(DicLogPath["vs"]) && !isHaveVs) { Process.Start(DicLogPath["vs"], filePath + " /command  \"Edit.GoTo " + lineNo + "\""); return; }

                // 用注册表的安装程序打开（已存在进程时）
                if (File.Exists(DicLogPath["vs"])) { Process.Start(DicLogPath["vs"], "/Edit " + filePath + " /command  \"Edit.GoTo " + lineNo + "\""); }
                // 无法用注册表找到VS程序时，直接打开源程序文件
                else { Process.Start(filePath); }

                // 下面用键盘操作定位
                Thread.Sleep(isHaveVs ? 1000 : 4000);//开起程序后等待
                // Ctrl + g
                SendKeys.SendWait("^g");
                Thread.Sleep(100);
                // 清除现有行数
                SendKeys.SendWait("{BACKSPACE}");
                // 输入行号
                foreach (var c in lineNo.ToString())
                {
                    SendKeys.Send(c.ToString());
                    Thread.Sleep(100);
                }
                // 回车
                SendKeys.SendWait("~");
                SendKeys.SendWait("+{END}");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            #endregion
        }
        /// <summary>
        /// 读取日志
        /// </summary>
        public static List<TEntity> LoadLog<TEntity>(string key)
        {
            if (!File.Exists(DicLogPath[key])) { MessageBox.Show("日志文件不存在！"); return new List<TEntity>(); }
            try
            {
                return Serialize.Load<List<TEntity>>(DicLogPath[key], "") ?? new List<TEntity>();
            }
            catch
            {
                //Serialize.Save(new List<TEntity>(), DicLogPath[key], ""); 
                MessageBox.Show("日志文件有错误，请重新打开！");
                return new List<TEntity>();
            }
        }

        /// <summary>
        /// 清除日志
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static bool ClearLog<TEntity>(string key)
        {
            if (MessageBox.Show("确定要清除日志吗？", "询问", MessageBoxButtons.YesNo) == DialogResult.No) { return false; }
            Serialize.Save(new List<TEntity>(), DicLogPath[key], "");
            return true;
        }

        /// <summary>
        /// 打开文件窗口，保存日志路径到注册表
        /// </summary>
        /// <param name="regeditKey">注册表KEY</param>
        /// <param name="path">日志文件路径</param>
        public static bool OpenLoad(string regeditKey)
        {
            var open = new OpenFileDialog();
            open.FileName = DicLogPath[regeditKey];
            if (open.ShowDialog() == DialogResult.OK)
            {
                DicLogPath[regeditKey] = open.FileName;
                Registry.CurrentUser.CreateSubKey(RegeditPath).SetValue(regeditKey, DicLogPath[regeditKey]);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 读取注册表路径
        /// </summary>
        /// <param name="regeditKey"></param>
        /// <returns></returns>
        public static string LoadRegeditValue(string regeditKey)
        {
            return (Registry.CurrentUser.CreateSubKey(RegeditPath).GetValue(regeditKey) ?? "").ToString();
        }
    }
}
