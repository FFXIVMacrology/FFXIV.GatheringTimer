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

[assembly: AssemblyTitle("GatheringTimer")]
[assembly: AssemblyDescription("GatheringTimer")]
[assembly: AssemblyCompany("ErinnerMO")]
[assembly: AssemblyVersion("0.0.0.1")]

namespace GatheringTimer
{
    public class GatheringTimerForm : UserControl, IActPluginV1
    {
        #region Designer Created Code (Avoid editing)
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private readonly System.ComponentModel.IContainer components = null;

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
            this.searchLabel = new System.Windows.Forms.Label();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.textBoxLogger = new System.Windows.Forms.TextBox();
            this.syncData = new System.Windows.Forms.Button();
            this.itemList = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // searchLabel
            // 
            this.searchLabel.AutoSize = true;
            this.searchLabel.Location = new System.Drawing.Point(3, 0);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(53, 12);
            this.searchLabel.TabIndex = 0;
            this.searchLabel.Text = "查询相关";
            // 
            // searchBox
            // 
            this.searchBox.Location = new System.Drawing.Point(6, 15);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(431, 21);
            this.searchBox.TabIndex = 1;
            this.searchBox.TextChanged += new System.EventHandler(this.SearchBox_TextChanged);
            // 
            // textBoxLogger
            // 
            this.textBoxLogger.Location = new System.Drawing.Point(20, 239);
            this.textBoxLogger.Multiline = true;
            this.textBoxLogger.Name = "textBoxLogger";
            this.textBoxLogger.ReadOnly = true;
            this.textBoxLogger.Size = new System.Drawing.Size(647, 80);
            this.textBoxLogger.TabIndex = 2;
            // 
            // syncData
            // 
            this.syncData.Location = new System.Drawing.Point(600, 45);
            this.syncData.Name = "syncData";
            this.syncData.Size = new System.Drawing.Size(67, 20);
            this.syncData.TabIndex = 6;
            this.syncData.Text = "syncData";
            this.syncData.UseVisualStyleBackColor = true;
            this.syncData.Click += new System.EventHandler(this.SyncData_Click);
            // 
            // itemList
            // 
            this.itemList.FormattingEnabled = true;
            this.itemList.ItemHeight = 12;
            this.itemList.Location = new System.Drawing.Point(6, 42);
            this.itemList.Name = "itemList";
            this.itemList.Size = new System.Drawing.Size(431, 172);
            this.itemList.TabIndex = 7;
            this.itemList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ItemList_MouseDoubleClick);
            this.itemList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ItemList_MouseDoubleClick);
            // 
            // GatheringTimerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.itemList);
            this.Controls.Add(this.syncData);
            this.Controls.Add(this.textBoxLogger);
            this.Controls.Add(this.searchBox);
            this.Controls.Add(this.searchLabel);
            this.Name = "GatheringTimerForm";
            this.Size = new System.Drawing.Size(686, 354);
            this.Load += new System.EventHandler(this.GatheringTimerForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox searchBox;
        private TextBox textBoxLogger;
        private Button syncData;
        private ListBox itemList;
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
                await GatheringTimer.GetItem(this, searchBox.Text);
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

        private void ItemList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.itemList.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                MessageBox.Show(this.itemList.Items[index].GetType().GetProperty("ID").GetValue(this.itemList.Items[index]).ToString());
            }
        }

    }

}
