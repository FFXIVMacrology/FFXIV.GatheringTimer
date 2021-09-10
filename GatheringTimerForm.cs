using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using System.IO;
using System.Reflection;
using System.Xml;
using GatheringTimer.Data;
using System.Threading.Tasks;


[assembly: AssemblyProduct("GatheringTimer")]
[assembly: AssemblyTitle("GatheringTimer")]
[assembly: AssemblyDescription("GatheringTimer")]
[assembly: AssemblyCompany("ErinnerMO")]
[assembly: AssemblyVersion("0.0.0.1")]
[assembly: AssemblyCopyright("Copyright © 2019-2021 ErinnerMO")]
namespace GatheringTimer
{
    public class GatheringTimerForm : UserControl, IActPluginV1
    {
        private IContainer components;
        #region Designer Created Code (Avoid editing)

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.searchLabel = new System.Windows.Forms.Label();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.textBoxLogger = new System.Windows.Forms.TextBox();
            this.syncData = new System.Windows.Forms.Button();
            this.itemList = new System.Windows.Forms.ListBox();
            this.tabControlGlobal = new System.Windows.Forms.TabControl();
            this.pointTab = new System.Windows.Forms.TabPage();
            this.gatheringPointVIew = new System.Windows.Forms.DataGridView();
            this.gatheringPointVIewColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.eorzeaTimer = new System.Windows.Forms.DataGridView();
            this.eorzeaTimerColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.eorzeaTimerColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.eorzeaTimerColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.eorzeaTimerColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.detailList = new System.Windows.Forms.ListBox();
            this.configTab = new System.Windows.Forms.TabPage();
            this.eorzeaTimerMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControlGlobal.SuspendLayout();
            this.pointTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gatheringPointVIew)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.eorzeaTimer)).BeginInit();
            this.configTab.SuspendLayout();
            this.eorzeaTimerMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // searchLabel
            // 
            this.searchLabel.AutoSize = true;
            this.searchLabel.Location = new System.Drawing.Point(25, 14);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(77, 12);
            this.searchLabel.TabIndex = 0;
            this.searchLabel.Text = "查询采集物品";
            // 
            // searchBox
            // 
            this.searchBox.Location = new System.Drawing.Point(27, 39);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(475, 21);
            this.searchBox.TabIndex = 1;
            this.searchBox.TextChanged += new System.EventHandler(this.SearchBox_TextChanged);
            // 
            // textBoxLogger
            // 
            this.textBoxLogger.Location = new System.Drawing.Point(7, 365);
            this.textBoxLogger.Multiline = true;
            this.textBoxLogger.Name = "textBoxLogger";
            this.textBoxLogger.ReadOnly = true;
            this.textBoxLogger.Size = new System.Drawing.Size(786, 82);
            this.textBoxLogger.TabIndex = 2;
            // 
            // syncData
            // 
            this.syncData.Location = new System.Drawing.Point(24, 18);
            this.syncData.Name = "syncData";
            this.syncData.Size = new System.Drawing.Size(134, 27);
            this.syncData.TabIndex = 6;
            this.syncData.Text = "从第三方同步数据";
            this.syncData.UseVisualStyleBackColor = true;
            this.syncData.Click += new System.EventHandler(this.SyncData_Click);
            // 
            // itemList
            // 
            this.itemList.FormattingEnabled = true;
            this.itemList.ItemHeight = 12;
            this.itemList.Location = new System.Drawing.Point(27, 66);
            this.itemList.Name = "itemList";
            this.itemList.Size = new System.Drawing.Size(151, 148);
            this.itemList.TabIndex = 7;
            this.itemList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ItemList_MouseDoubleClick);
            // 
            // tabControlGlobal
            // 
            this.tabControlGlobal.Controls.Add(this.pointTab);
            this.tabControlGlobal.Controls.Add(this.configTab);
            this.tabControlGlobal.Location = new System.Drawing.Point(3, 3);
            this.tabControlGlobal.Name = "tabControlGlobal";
            this.tabControlGlobal.SelectedIndex = 0;
            this.tabControlGlobal.Size = new System.Drawing.Size(794, 360);
            this.tabControlGlobal.TabIndex = 8;
            // 
            // pointTab
            // 
            this.pointTab.Controls.Add(this.gatheringPointVIew);
            this.pointTab.Controls.Add(this.eorzeaTimer);
            this.pointTab.Controls.Add(this.detailList);
            this.pointTab.Controls.Add(this.searchBox);
            this.pointTab.Controls.Add(this.itemList);
            this.pointTab.Controls.Add(this.searchLabel);
            this.pointTab.Location = new System.Drawing.Point(4, 22);
            this.pointTab.Name = "pointTab";
            this.pointTab.Padding = new System.Windows.Forms.Padding(3);
            this.pointTab.Size = new System.Drawing.Size(786, 334);
            this.pointTab.TabIndex = 0;
            this.pointTab.Text = "采集点";
            this.pointTab.UseVisualStyleBackColor = true;
            // 
            // gatheringPointVIew
            // 
            this.gatheringPointVIew.AllowUserToAddRows = false;
            this.gatheringPointVIew.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gatheringPointVIew.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gatheringPointVIew.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.gatheringPointVIewColumn1});
            this.gatheringPointVIew.Location = new System.Drawing.Point(508, 39);
            this.gatheringPointVIew.Name = "gatheringPointVIew";
            this.gatheringPointVIew.RowTemplate.Height = 23;
            this.gatheringPointVIew.Size = new System.Drawing.Size(272, 281);
            this.gatheringPointVIew.TabIndex = 9;
            // 
            // gatheringPointVIewColumn1
            // 
            this.gatheringPointVIewColumn1.HeaderText = "Location";
            this.gatheringPointVIewColumn1.Name = "gatheringPointVIewColumn1";
            this.gatheringPointVIewColumn1.ReadOnly = true;
            // 
            // eorzeaTimer
            // 
            this.eorzeaTimer.AllowUserToAddRows = false;
            this.eorzeaTimer.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.eorzeaTimer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.eorzeaTimer.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.eorzeaTimerColumn1,
            this.eorzeaTimerColumn2,
            this.eorzeaTimerColumn3,
            this.eorzeaTimerColumn4});
            this.eorzeaTimer.Location = new System.Drawing.Point(27, 217);
            this.eorzeaTimer.Name = "eorzeaTimer";
            this.eorzeaTimer.ReadOnly = true;
            this.eorzeaTimer.RowTemplate.Height = 23;
            this.eorzeaTimer.Size = new System.Drawing.Size(475, 103);
            this.eorzeaTimer.TabIndex = 1;
            this.eorzeaTimer.ContextMenuStrip = this.eorzeaTimerMenuStrip;
            this.eorzeaTimer.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.EorzeaTimer_MouseDoubleClick);
            // 
            // eorzeaTimerColumn1
            // 
            this.eorzeaTimerColumn1.HeaderText = "ID";
            this.eorzeaTimerColumn1.Name = "eorzeaTimerColumn1";
            this.eorzeaTimerColumn1.ReadOnly = true;
            // 
            // eorzeaTimerColumn2
            // 
            this.eorzeaTimerColumn2.HeaderText = "Name";
            this.eorzeaTimerColumn2.Name = "eorzeaTimerColumn2";
            this.eorzeaTimerColumn2.ReadOnly = true;
            // 
            // eorzeaTimerColumn3
            // 
            this.eorzeaTimerColumn3.HeaderText = "NextLocalTime";
            this.eorzeaTimerColumn3.Name = "eorzeaTimerColumn3";
            this.eorzeaTimerColumn3.ReadOnly = true;
            // 
            // eorzeaTimerColumn4
            // 
            this.eorzeaTimerColumn4.HeaderText = "NextEorzeaTime";
            this.eorzeaTimerColumn4.Name = "eorzeaTimerColumn4";
            this.eorzeaTimerColumn4.ReadOnly = true;
            // 
            // detailList
            // 
            this.detailList.FormattingEnabled = true;
            this.detailList.ItemHeight = 12;
            this.detailList.Location = new System.Drawing.Point(184, 65);
            this.detailList.Name = "detailList";
            this.detailList.Size = new System.Drawing.Size(318, 148);
            this.detailList.TabIndex = 8;
            this.detailList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.DetailList_MouseDoubleClick);
            // 
            // configTab
            // 
            this.configTab.Controls.Add(this.syncData);
            this.configTab.Location = new System.Drawing.Point(4, 22);
            this.configTab.Name = "configTab";
            this.configTab.Padding = new System.Windows.Forms.Padding(3);
            this.configTab.Size = new System.Drawing.Size(786, 334);
            this.configTab.TabIndex = 1;
            this.configTab.Text = "设置";
            this.configTab.UseVisualStyleBackColor = true;
            // 
            // eorzeaTimerMenuStrip
            // 
            this.eorzeaTimerMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem});
            this.eorzeaTimerMenuStrip.Name = "eorzeaTimerMenuStrip";
            this.eorzeaTimerMenuStrip.Size = new System.Drawing.Size(181, 48);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.EorzeaTimer_DeleteClick);
            // 
            // GatheringTimerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxLogger);
            this.Controls.Add(this.tabControlGlobal);
            this.Name = "GatheringTimerForm";
            this.Size = new System.Drawing.Size(800, 450);
            this.Load += new System.EventHandler(this.GatheringTimerForm_Load);
            this.tabControlGlobal.ResumeLayout(false);
            this.pointTab.ResumeLayout(false);
            this.pointTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gatheringPointVIew)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.eorzeaTimer)).EndInit();
            this.configTab.ResumeLayout(false);
            this.eorzeaTimerMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox searchBox;
        private TextBox textBoxLogger;
        private Button syncData;
        private ListBox itemList;
        private TabControl tabControlGlobal;
        private TabPage pointTab;
        private TabPage configTab;
        private ListBox detailList;
        private DataGridView eorzeaTimer;
        private DataGridViewTextBoxColumn eorzeaTimerColumn1;
        private DataGridViewTextBoxColumn eorzeaTimerColumn2;
        private DataGridViewTextBoxColumn eorzeaTimerColumn3;
        private DataGridViewTextBoxColumn eorzeaTimerColumn4;
        private DataGridViewTextBoxColumn gatheringPointVIewColumn1;
        private DataGridView gatheringPointVIew;
        private ContextMenuStrip eorzeaTimerMenuStrip;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Label searchLabel;

        #endregion
        public GatheringTimerForm()
        {
            InitializeComponent();
        }

        Label lblStatus;    // The status label that appears in ACT's Plugin tab
        readonly string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\GatheringTimer.config.xml");
        SettingsSerializer xmlSettings;

        #region IActPluginV1 Members
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            lblStatus = pluginStatusText;   // Hand the status label's reference to our local var
            pluginScreenSpace.Controls.Add(this);   // Add this UserControl to the tab ACT provides
            this.Dock = DockStyle.Fill; // Expand the UserControl to fill the tab's client space
            xmlSettings = new SettingsSerializer(this); // Create a new settings serializer and pass it this instance
            LoadSettings();

            // Create some sort of parsing event handler.  After the "+=" hit TAB twice and the code will be generated for you.
            ActGlobals.oFormActMain.AfterCombatAction += new CombatActionDelegate(OFormActMain_AfterCombatAction);

            lblStatus.Text = "Plugin Started";
        }
        public void DeInitPlugin()
        {
            // Unsubscribe from any events you listen to when exiting!
            ActGlobals.oFormActMain.AfterCombatAction -= OFormActMain_AfterCombatAction;

            SaveSettings();
            lblStatus.Text = "Plugin Exited";
        }
        #endregion

        void OFormActMain_AfterCombatAction(bool isImport, CombatActionEventArgs actionInfo)
        {
            throw new NotImplementedException();
        }

        void LoadSettings()
        {
            // Add any controls you want to save the state of
            xmlSettings.AddControlSetting(searchBox.Name, searchBox);

            if (File.Exists(settingsFile))
            {
                FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlTextReader xReader = new XmlTextReader(fs);

                try
                {
                    while (xReader.Read())
                    {
                        if (xReader.NodeType == XmlNodeType.Element)
                        {
                            if (xReader.LocalName == "SettingsSerializer")
                            {
                                xmlSettings.ImportFromXml(xReader);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Error loading settings: " + ex.Message;
                }
                xReader.Close();
            }
        }
        void SaveSettings()
        {
            FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                Indentation = 1,
                IndentChar = '\t'
            };
            xWriter.WriteStartDocument(true);
            xWriter.WriteStartElement("Config");    // <Config>
            xWriter.WriteStartElement("SettingsSerializer");    // <Config><SettingsSerializer>
            xmlSettings.ExportToXml(xWriter);   // Fill the SettingsSerializer XML
            xWriter.WriteEndElement();  // </SettingsSerializer>
            xWriter.WriteEndElement();  // </Config>
            xWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
            xWriter.Flush();    // Flush the file buffer to disk
            xWriter.Close();
        }

        private void GatheringTimerForm_Load(object sender, EventArgs e)
        {
            Logger.SetTextBox(this.textBoxLogger);
        }

        private bool syncDataStatus = true;
        public object syncDataLock = new object();
        private async void SyncData_Click(object sender, EventArgs e)
        {
            if (syncDataStatus)
            {
                lock (syncDataLock)
                {
                    syncDataStatus = false;
                }
                Logger.Info("Sync Start");
                try
                {
                    await DataManagement.Sync();
                }
                finally
                {
                    Logger.Info("Sync Finish");
                    lock (syncDataLock)
                    {
                        syncDataStatus = true;
                    }

                }
            }
        }

        private async void SearchBox_TextChanged(object sender, EventArgs e)
        {
            if ("" == searchBox.Text)
            {
                this.itemList.Items.Clear();
            }
            else
            {
                await GatheringTimerMain.GetItems(this, searchBox.Text);
            }

        }

        public void ItemList_SetContent<T>(List<T> itemList)
        {
            this.itemList.Items.Clear();
            if (null != itemList)
            {
                foreach (T item in itemList)
                {
                    this.itemList.Items.Add(item);
                }
                this.itemList.ValueMember = "ID";
                this.itemList.DisplayMember = "Name_chs";
            }
        }

        private async void ItemList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.itemList.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                int itemID = int.Parse(this.itemList.Items[index].GetType().GetProperty("ID").GetValue(this.itemList.Items[index]).ToString());
                await GatheringTimerMain.GetItemDetail(this, itemID);
            }
        }

        public void DetailList_SetContent(Data.Model.DisplayVo.Item item)
        {
            this.detailList.Items.Clear();
            if (null != detailList)
            {
                List<Data.Model.DisplayVo.GatheringPointBase> gatheringPointBases = default;

                if (null != item.GatheringItem)
                {
                    gatheringPointBases = item.GatheringItem.GatheringPointBases;

                }

                if (null != item.SpearfishingItem)
                {
                    gatheringPointBases = item.SpearfishingItem.GatheringPointBases;

                }

                if (null != gatheringPointBases)
                {

                    foreach (Data.Model.DisplayVo.GatheringPointBase gatheringPointBase in gatheringPointBases)
                    {
                        if (gatheringPointBase.GatheringPointBaseExtension != null && gatheringPointBase.GatheringPoint != null)
                        {
                            this.detailList.Items.Add(gatheringPointBase);
                            this.detailList.ValueMember = "ID";
                            this.detailList.DisplayMember = "Description_chs";
                        }

                    }


                }
            }
        }

        private async void DetailList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.detailList.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                int gatheringPointBaseID = int.Parse(this.detailList.Items[index].GetType().GetProperty("ID").GetValue(this.detailList.Items[index]).ToString());
                Data.Model.DisplayVo.GatheringPointBase gatheringPointBase = await Service.GetGatheringPointBaseDetail(gatheringPointBaseID);
                if (gatheringPointBase != null && gatheringPointBase.TimeConditionExtension != null)
                {
                    await GatheringTimerMain.CreateTimer(this, gatheringPointBaseID);
                }
                await GatheringTimerMain.GetGatheringPointBaseDetail(this, gatheringPointBaseID);
            }
        }

        public void EorzeaTimer_SetContent(List<Timer.EorzeaTimer> eorzeaTimers)
        {
            this.eorzeaTimer.Rows.Clear();
            foreach (Timer.EorzeaTimer eorzeaTimer in eorzeaTimers)
            {
                DataGridViewRow dataGridViewRow = new DataGridViewRow();
                dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell() { Value = eorzeaTimer.Id });
                dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell() { Value = eorzeaTimer.Name });
                dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell() { Value = eorzeaTimer.NextLocalTime });
                dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell() { Value = eorzeaTimer.NextEorzeaTime });
                this.eorzeaTimer.Rows.Add(dataGridViewRow);
            }

        }

        private async void EorzeaTimer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            object gatheringPointBaseID = this.eorzeaTimer.CurrentRow.Cells[0].Value;
            await GatheringTimerMain.GetGatheringPointBaseDetail(this, (int)gatheringPointBaseID);
        }

        private async void EorzeaTimer_DeleteClick(object sender, EventArgs e)
        {
            if (this.eorzeaTimer.SelectedRows.Count > 0) {
                foreach(DataGridViewRow selectRow in this.eorzeaTimer.SelectedRows) {
                    int gatheringPointBaseId = (int)selectRow.Cells[0].Value;
                    Timer.TimerManagement.Stop(gatheringPointBaseId);
                    this.eorzeaTimer.Rows.Remove(eorzeaTimer.SelectedRows[0]);
                }
            }
        }

        public async void GatheringPointVIew_SetContent(Data.Model.DisplayVo.GatheringPointBase gatheringPointBase)
        {
            this.gatheringPointVIew.Rows.Clear();
            this.gatheringPointVIewColumn1.HeaderText = gatheringPointBase.Description_chs;
            for (int i = 0; i < 8; i++) {
                DataGridViewRow dataGridViewRow = new DataGridViewRow();
                var gatheringPointBaseExtension = gatheringPointBase.GatheringPointBaseExtension.GetType().GetProperty("Item" + i).GetValue(gatheringPointBase.GatheringPointBaseExtension);

                dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell() { Value =
                    gatheringPointBaseExtension == null?
                    "" :
                    gatheringPointBaseExtension.GetType().GetProperty("Name_chs").GetValue(gatheringPointBaseExtension)
                });
                this.gatheringPointVIew.Rows.Add(dataGridViewRow);

            }
        }



    }
}
