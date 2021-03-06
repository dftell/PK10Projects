﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PK10CorePress;
using System.Threading;
using System.IO;
using System.Net;
using ExchangeLib;

namespace PK10Server
{
    public partial class MainForm : Form
    {
        ExpectList ViewDataList ;
        PK10ExpectReader er = new PK10ExpectReader();
        int NewestExpectNo = 0;
        public int InputExpect;
        GlobalClass gobj;
        public MainForm()
        {
            //GlobalClass gc = new GlobalClass();
            InitializeComponent();
            gobj = new GlobalClass();
            
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitListView(listView_forSerial, "名");
            InitListView(listView_ForCar, "车");

            InitListView(listView_TXFFCData);
            InitListView(listView_PK10Data);
            
            this.timer_For_NewestData.Interval = 5000;
            //this.timer_For_NewestData_Tick(null, null);

            this.timer_For_CurrTime.Enabled = true;
            this.timer_For_CurrTime.Interval = 1000;
            this.timer_For_CurrTime_Tick(null, null);

            this.timer_For_getHtmlData.Enabled = false;
            this.timer_For_getHtmlData.Interval = 60000;
            //this.timer_For_getHtmlData_Tick(null, null);
            ReLoad();
            ////ViewDataList = er.ReadNewestData(DateTime.Now.AddDays(-1));
            ////RefreshGrid();
            RefreshNewestData();
        }

        public void ReLoad()
        {
            if (InputExpect > 0)
            {
                this.txt_NewestExpect.Text = (this.InputExpect - 1).ToString();
                this.btn_AddExpectNo_Click(null, null);
            }
            else
            {
                ViewDataList = er.ReadNewestData(DateTime.Today.AddDays(-1));
            }
        }

        void StartReceiveDataService()
        {
            this.timer_For_getHtmlData.Enabled = true;
            this.timer_For_getHtmlData_Tick(null, null);
        }
        void StopReceiveService()
        {
            this.timer_For_getHtmlData.Enabled = false;
        }

        void StartRefreshDataService()
        {
            this.timer_For_NewestData.Enabled = true;
            this.timer_For_NewestData_Tick(null, null);
        }

        void EndRefreshDataService()
        {
            this.timer_For_NewestData.Enabled = false;
        }

        public void RefreshGrid()
        {
            RefreshSerialData(this.listView_forSerial, true,this.gobj.MutliColMinTimes);
            RefreshSerialData(this.listView_ForCar, false, this.gobj.MutliColMinTimes);
        }


        void RefreshSerialData(ListView lv,bool byNo,int minRow)
        {
            ExpectList el = ViewDataList; ;
            ExpectListProcess elp = new ExpectListProcess(el);
            CommCollection sc = elp.getSerialData(180, byNo);
            sc.isByNo = byNo;
            lv.Items.Clear();
            for (int i = minRow-1; i < sc.Table.Rows.Count; i++)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = (i-minRow+1+1).ToString();
                if (byNo)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        lvi.SubItems.Add(sc.Table.Rows[i][j].ToString());
                    }
                }
                else
                {
                    for (int j = 0; j < 10; j++)
                    {
                        lvi.SubItems.Add(sc.Table.Rows[i][string.Format("{0}",(j+1)%10)].ToString());
                    }
                }
                for (int j = 1; j < 9; j++)
                {
                    if (i + 1 == this.gobj.MinTimeForChance(j))
                    {
                        lvi.BackColor = Color.Yellow;
                        break;
                    }
                }
                lv.Items.Add(lvi);
            }
            
        }

        void InitListView(ListView lv, string strHeaderFlag)
        {
            lv.Columns.Clear();
            lv.Columns.Add("次数");
            for (int i = 1; i < 10; i++)
            {
                lv.Columns.Add(string.Format("{0}{1}", i, strHeaderFlag),100);
            }
            lv.Columns.Add(string.Format("{0}{1}", 0, strHeaderFlag),100);
        }

        private void toolStripMenuItem_backTest_Click(object sender, EventArgs e)
        {
            ////BackTestFrm frm = new BackTestFrm();
            ////frm.Show();
            
        }

        private void btn_AddExpectNo_Click(object sender, EventArgs e)
        {
            this.timer_For_NewestData.Enabled = false;
            int NextNo = int.Parse(this.txt_NewestExpect.Text.Trim());
            ViewDataList = er.ReadNewestData(NextNo+1,180,true);
            if (ViewDataList == null || ViewDataList.Count == 0)
                return;
            RefreshGrid();
            RefreshNewestData();
        }

        void RefreshNewestData()
        {
            this.txt_NewestExpect.Text = ViewDataList.LastData.Expect;
            this.txt_NewestOpenCode.Text = ViewDataList.LastData.OpenCode;
            this.txt_NewestOpenTime.Text = ViewDataList.LastData.OpenTime.ToString();
            this.txt_NextExpectNo.Text = string.Format("{0}", int.Parse(ViewDataList.LastData.Expect) + 1);
            FillOrgData(listView_PK10Data, ViewDataList);
        }

        private void btn_Subtract_Click(object sender, EventArgs e)
        {
            this.timer_For_NewestData.Enabled = false;
            int NextNo = int.Parse(this.txt_NewestExpect.Text.Trim());
            ViewDataList = er.ReadNewestData(NextNo - 1, 180,true);
            RefreshGrid();
            RefreshNewestData();
        }

        private void timer_For_NewestData_Tick(object sender, EventArgs e)
        {
            DateTime CurrTime = DateTime.Now;
            ViewDataList = er.ReadNewestData(DateTime.Now.AddDays(-1));
            int CurrExpectNo = int.Parse(ViewDataList.LastData.Expect);
            if (CurrExpectNo > this.NewestExpectNo)
            {
                this.timer_For_NewestData.Interval = 290000;//5分钟以后见
                RefreshGrid();
                RefreshNewestData();
                this.NewestExpectNo = CurrExpectNo;
            }
            else
            {
                this.timer_For_NewestData.Interval = 10000;//10秒后见
            }
        }

        private void timer_For_CurrTime_Tick(object sender, EventArgs e)
        {
            this.lbl_Timer.Text = DateTime.Now.ToString();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void tMI_HistoryBackTest_Click(object sender, EventArgs e)
        {
            
            //BackTestFrm
        }

        private void TSMI_StartSvr_Click(object sender, EventArgs e)
        {
            StartReceiveDataService();
        }

        private void TSMI_StopSvr_Click(object sender, EventArgs e)
        {
            StopReceiveService();
        }

        private void timer_For_getHtmlData_Tick(object sender, EventArgs e)
        {
            int secCnt = DateTime.Now.Second;
            if (secCnt > 10)
            {
                timer_For_getHtmlData.Interval = (7+60 - secCnt) * 1000;
            }
            else if (secCnt <7)
            {
                timer_For_getHtmlData.Interval = (7 - secCnt) * 1000;
            }
            else
            {
                timer_For_getHtmlData.Interval = 60000;
            }
            TXFFC_HtmlDataClass hdc = new TXFFC_HtmlDataClass();
            ExpectList el =  hdc.getExpectList();
            TXFFCExpectReader rd = new TXFFCExpectReader();
            ExpectList currEl = rd.ReadNewestData(DateTime.Now.AddDays(-1));
            rd.SaveNewestData(rd.getNewestData(el, currEl));
            currEl = rd.ReadNewestData(DateTime.Now.AddDays(-1));
            FillOrgData(listView_TXFFCData, currEl);
        }

        void InitListView(ListView lv)
        {
            lv.Columns.Clear();
            lv.Columns.Add("次序", 50);
            lv.Columns.Add("期号",200);
            lv.Columns.Add("开奖号码",200);
            lv.Columns.Add("开奖时间",200);
        }

        void FillOrgData(ListView lv, ExpectList el)
        {
            lv.Items.Clear();
            if (el == null) return;
            int cnt = (int)el.LastData.OpenTime.Subtract(el.FirstData.OpenTime).TotalMinutes + 1;
            for (int i = el.Count - 1;i>=0 ; i--)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = string.Format("{0}", el.Count-i );
                lvi.SubItems.Add(el[i].Expect.ToString());
                lvi.SubItems.Add(el[i].OpenCode.ToString());
                lvi.SubItems.Add(el[i].OpenTime.ToString());
                lv.Items.Add(lvi);
            }
        }

        private void tsmi_getTXFFCHistoryTxtData_Click(object sender, EventArgs e)
        {
            TXFFCExpectReader er = new TXFFCExpectReader();
            TXFFC_HtmlDataClass rder = new TXFFC_HtmlDataClass();
            string strFolder = @"C:\Users\zhouys\Desktop\TXFFC";
            string fileType = "txt";
            
            DirectoryInfo dir = new DirectoryInfo(strFolder);
            FileInfo[] fil = dir.GetFiles();
            foreach (FileInfo f in fil)
            {
                long size = f.Length;
                if (f.FullName.ToLower().Substring(f.FullName.Length - fileType.Length) != fileType.ToLower())
                {
                    continue;//非指定类型跳过
                }
                ExpectList el = rder.getFileData(f.FullName);
                TXFFCExpectReader rd = new TXFFCExpectReader();
                ExpectList currEl = rd.ReadHistory();
                rd.SaveHistoryData(rd.getNewestData(el, currEl));
                //currEl = rd.ReadNewestData(DateTime.Now.AddDays(-1));
                //FillOrgData(listView_TXFFCData, currEl);

            }

            
        }

        private void tsmi_getTXFFCHistoryFromWeb_Click(object sender, EventArgs e)
        {
            TXFFC_HtmlDataClass rder = new TXFFC_HtmlDataClass();
            TXFFCExpectReader er = new TXFFCExpectReader();
            string StrBegDate = "2018-08-25";
            ExpectList el = er.GetMissedData(true, StrBegDate);
            for (int i = 0; i < el.Count; i++)
            {
                ExpectList tmpList = new ExpectList();
                DateTime endT = el[i].OpenTime;
                DateTime begT = el[i].OpenTime.AddMinutes(-1 * el[i].MissedCnt-1);
                DateTime tt = DateTime.Parse(begT.ToShortDateString());
                DateTime begT0 = tt;
                while (tt <= endT)
                {
                    string strTt = tt.ToString("yyyy-MM-dd");
                    for (int j = 1; j <= 29; j++)
                    {
                        ExpectList wlist = rder.getHistoryData(strTt, j);//取到web
                        tmpList = ExpectList.Concat(tmpList, wlist);
                        Thread.Sleep(800);
                    }
                    tt=tt.AddDays(1);
                }
                ExpectList currEl = er.ReadHistory(begT0.ToString(),endT.AddDays(1).ToString());
                er.SaveHistoryData(er.getNewestData(tmpList, currEl));
            }
        }

        private void tsmiStartCalcToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void tsmi_StartRefreshWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartRefreshDataService();
        }

        private void tsmi_StopRefreshWindows_Click(object sender, EventArgs e)
        {
            EndRefreshDataService();
        }

        private void toolStripMenuItem_StragManager_Click(object sender, EventArgs e)
        {
            frm_StragManager frm = new frm_StragManager();
            frm.ShowDialog();
        }

        private void tsmi_RunMonitor_Click(object sender, EventArgs e)
        {
            frm_StragMonitor frm = new frm_StragMonitor();
            frm.Show();
        }

        private void ToolStripMenuItem_StragRunPlan_Click(object sender, EventArgs e)
        {
            //frm_StragPlanSetting frm = new frm_StragPlanSetting();
            frm_CommDBObjectsSetting<StragRunPlanClass> frm = new frm_CommDBObjectsSetting<StragRunPlanClass>();
            frm.OuterList = Program.AllGlobalSetting.AllRunPlannings;
            frm.Show();
        }

        private void tsmi_AssetUnitMgr_Click(object sender, EventArgs e)
        {
            frm_CommDBObjectsSetting<AssetUnitClass> frm = new frm_CommDBObjectsSetting<AssetUnitClass>();
            frm.OuterList = Program.AllGlobalSetting.AllAssetUnits;
            frm.Show();
        }
    }

    
}
