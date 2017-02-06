namespace BrodieTheatre
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listBoxActivities = new System.Windows.Forms.ListBox();
            this.labelHarmonyStatus = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelCurrentActivity = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labelPots = new System.Windows.Forms.Label();
            this.labelTray = new System.Windows.Forms.Label();
            this.trackBarPots = new System.Windows.Forms.TrackBar();
            this.trackBarTray = new System.Windows.Forms.TrackBar();
            this.label9 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.labelPLMstatus = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelKodiStatus = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.labelLastVoiceCommand = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.labelRoomStatus = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.labelKinectStatus = new System.Windows.Forms.Label();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.timerPLMreceive = new System.Windows.Forms.Timer(this.components);
            this.timerTrayTrack = new System.Windows.Forms.Timer(this.components);
            this.timerPotTrack = new System.Windows.Forms.Timer(this.components);
            this.timerCheckPLM = new System.Windows.Forms.Timer(this.components);
            this.timerKodiPoller = new System.Windows.Forms.Timer(this.components);
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.timerClearStatus = new System.Windows.Forms.Timer(this.components);
            this.timerCheckKinect = new System.Windows.Forms.Timer(this.components);
            this.timerSkeletonTracker = new System.Windows.Forms.Timer(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.timerUnoccupiedRoom = new System.Windows.Forms.Timer(this.components);
            this.timerShutdown = new System.Windows.Forms.Timer(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.timerStartLights = new System.Windows.Forms.Timer(this.components);
            this.menuStrip.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPots)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTray)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(697, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // listBoxActivities
            // 
            this.listBoxActivities.FormattingEnabled = true;
            this.listBoxActivities.Location = new System.Drawing.Point(227, 33);
            this.listBoxActivities.Name = "listBoxActivities";
            this.listBoxActivities.Size = new System.Drawing.Size(149, 69);
            this.listBoxActivities.TabIndex = 1;
            this.listBoxActivities.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxActivities_MouseDoubleClick);
            // 
            // labelHarmonyStatus
            // 
            this.labelHarmonyStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelHarmonyStatus.ForeColor = System.Drawing.Color.Maroon;
            this.labelHarmonyStatus.Location = new System.Drawing.Point(90, 33);
            this.labelHarmonyStatus.Name = "labelHarmonyStatus";
            this.labelHarmonyStatus.Size = new System.Drawing.Size(120, 19);
            this.labelHarmonyStatus.TabIndex = 3;
            this.labelHarmonyStatus.Text = "Not Initialized";
            this.labelHarmonyStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Controls.Add(this.labelCurrentActivity);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.listBoxActivities);
            this.groupBox1.Controls.Add(this.labelHarmonyStatus);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox1.Location = new System.Drawing.Point(12, 42);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(392, 113);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(9, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(108, 23);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // labelCurrentActivity
            // 
            this.labelCurrentActivity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelCurrentActivity.Location = new System.Drawing.Point(90, 74);
            this.labelCurrentActivity.Name = "labelCurrentActivity";
            this.labelCurrentActivity.Size = new System.Drawing.Size(120, 19);
            this.labelCurrentActivity.TabIndex = 7;
            this.labelCurrentActivity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Hub Status";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Current Activity";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(224, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Start Activity";
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.White;
            this.groupBox2.Controls.Add(this.labelPots);
            this.groupBox2.Controls.Add(this.labelTray);
            this.groupBox2.Controls.Add(this.trackBarPots);
            this.groupBox2.Controls.Add(this.trackBarTray);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.pictureBox2);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.labelPLMstatus);
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox2.Location = new System.Drawing.Point(12, 170);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(392, 160);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            // 
            // labelPots
            // 
            this.labelPots.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelPots.Location = new System.Drawing.Point(335, 109);
            this.labelPots.Name = "labelPots";
            this.labelPots.Size = new System.Drawing.Size(35, 23);
            this.labelPots.TabIndex = 22;
            this.labelPots.Text = "0%";
            this.labelPots.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelTray
            // 
            this.labelTray.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelTray.Location = new System.Drawing.Point(335, 64);
            this.labelTray.Name = "labelTray";
            this.labelTray.Size = new System.Drawing.Size(35, 23);
            this.labelTray.TabIndex = 21;
            this.labelTray.Text = "0%";
            this.labelTray.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // trackBarPots
            // 
            this.trackBarPots.BackColor = System.Drawing.SystemColors.Window;
            this.trackBarPots.LargeChange = 1;
            this.trackBarPots.Location = new System.Drawing.Point(90, 110);
            this.trackBarPots.Name = "trackBarPots";
            this.trackBarPots.Size = new System.Drawing.Size(239, 45);
            this.trackBarPots.TabIndex = 20;
            this.trackBarPots.Scroll += new System.EventHandler(this.trackBarPots_Scroll);
            this.trackBarPots.ValueChanged += new System.EventHandler(this.trackBarPots_ValueChanged);
            // 
            // trackBarTray
            // 
            this.trackBarTray.BackColor = System.Drawing.SystemColors.Window;
            this.trackBarTray.LargeChange = 1;
            this.trackBarTray.Location = new System.Drawing.Point(90, 65);
            this.trackBarTray.Name = "trackBarTray";
            this.trackBarTray.Size = new System.Drawing.Size(239, 45);
            this.trackBarTray.TabIndex = 19;
            this.trackBarTray.Scroll += new System.EventHandler(this.trackBarTray_Scroll);
            this.trackBarTray.ValueChanged += new System.EventHandler(this.trackBarTray_ValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(33, 114);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(54, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Pot Lights";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Tray Lights";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(9, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(187, 23);
            this.pictureBox2.TabIndex = 5;
            this.pictureBox2.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 36);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "PLM Status";
            // 
            // labelPLMstatus
            // 
            this.labelPLMstatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelPLMstatus.ForeColor = System.Drawing.Color.Maroon;
            this.labelPLMstatus.Location = new System.Drawing.Point(90, 33);
            this.labelPLMstatus.Name = "labelPLMstatus";
            this.labelPLMstatus.Size = new System.Drawing.Size(120, 19);
            this.labelPLMstatus.TabIndex = 3;
            this.labelPLMstatus.Text = "Not Initialized";
            this.labelPLMstatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.labelKodiStatus);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.pictureBox3);
            this.groupBox3.Location = new System.Drawing.Point(435, 42);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(253, 112);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            // 
            // labelKodiStatus
            // 
            this.labelKodiStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelKodiStatus.Location = new System.Drawing.Point(103, 47);
            this.labelKodiStatus.Name = "labelKodiStatus";
            this.labelKodiStatus.Size = new System.Drawing.Size(120, 19);
            this.labelKodiStatus.TabIndex = 12;
            this.labelKodiStatus.Text = "Stopped";
            this.labelKodiStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(46, 50);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(51, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Playback";
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(10, 0);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(80, 23);
            this.pictureBox3.TabIndex = 10;
            this.pictureBox3.TabStop = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.labelLastVoiceCommand);
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Controls.Add(this.labelRoomStatus);
            this.groupBox4.Controls.Add(this.label12);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.labelKinectStatus);
            this.groupBox4.Controls.Add(this.pictureBox4);
            this.groupBox4.Location = new System.Drawing.Point(436, 170);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(251, 160);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            // 
            // labelLastVoiceCommand
            // 
            this.labelLastVoiceCommand.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelLastVoiceCommand.Location = new System.Drawing.Point(102, 120);
            this.labelLastVoiceCommand.Name = "labelLastVoiceCommand";
            this.labelLastVoiceCommand.Size = new System.Drawing.Size(120, 19);
            this.labelLastVoiceCommand.TabIndex = 16;
            this.labelLastVoiceCommand.Text = "None";
            this.labelLastVoiceCommand.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(19, 123);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(77, 13);
            this.label14.TabIndex = 15;
            this.label14.Text = "Last Command";
            // 
            // labelRoomStatus
            // 
            this.labelRoomStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelRoomStatus.Location = new System.Drawing.Point(102, 80);
            this.labelRoomStatus.Name = "labelRoomStatus";
            this.labelRoomStatus.Size = new System.Drawing.Size(120, 19);
            this.labelRoomStatus.TabIndex = 8;
            this.labelRoomStatus.Text = "Unoccupied";
            this.labelRoomStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelRoomStatus.TextChanged += new System.EventHandler(this.labelRoomStatus_TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(28, 83);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(68, 13);
            this.label12.TabIndex = 14;
            this.label12.Text = "Room Status";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(26, 42);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(70, 13);
            this.label10.TabIndex = 13;
            this.label10.Text = "Kinect Status";
            // 
            // labelKinectStatus
            // 
            this.labelKinectStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelKinectStatus.ForeColor = System.Drawing.Color.Maroon;
            this.labelKinectStatus.Location = new System.Drawing.Point(102, 39);
            this.labelKinectStatus.Name = "labelKinectStatus";
            this.labelKinectStatus.Size = new System.Drawing.Size(120, 19);
            this.labelKinectStatus.TabIndex = 12;
            this.labelKinectStatus.Text = "Disconnected";
            this.labelKinectStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox4.Image")));
            this.pictureBox4.Location = new System.Drawing.Point(10, 0);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(100, 24);
            this.pictureBox4.TabIndex = 11;
            this.pictureBox4.TabStop = false;
            // 
            // timerPLMreceive
            // 
            this.timerPLMreceive.Interval = 350;
            this.timerPLMreceive.Tick += new System.EventHandler(this.timerPLMreceive_Tick);
            // 
            // timerTrayTrack
            // 
            this.timerTrayTrack.Tick += new System.EventHandler(this.timerTrayTrack_Tick);
            // 
            // timerPotTrack
            // 
            this.timerPotTrack.Tick += new System.EventHandler(this.timerPotTrack_Tick);
            // 
            // timerCheckPLM
            // 
            this.timerCheckPLM.Interval = 2000;
            this.timerCheckPLM.Tick += new System.EventHandler(this.timerCheckPLM_Tick);
            // 
            // timerKodiPoller
            // 
            this.timerKodiPoller.Enabled = true;
            this.timerKodiPoller.Tick += new System.EventHandler(this.timerKodiPoller_Tick);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 353);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(697, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 11;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatus
            // 
            this.toolStripStatus.Name = "toolStripStatus";
            this.toolStripStatus.Size = new System.Drawing.Size(0, 17);
            this.toolStripStatus.TextChanged += new System.EventHandler(this.toolStripStatus_TextChanged);
            // 
            // timerClearStatus
            // 
            this.timerClearStatus.Interval = 5000;
            this.timerClearStatus.Tick += new System.EventHandler(this.timerClearStatus_Tick);
            // 
            // timerCheckKinect
            // 
            this.timerCheckKinect.Enabled = true;
            this.timerCheckKinect.Interval = 1000;
            this.timerCheckKinect.Tick += new System.EventHandler(this.timercheckKinect_Tick);
            // 
            // timerSkeletonTracker
            // 
            this.timerSkeletonTracker.Interval = 1000;
            this.timerSkeletonTracker.Tick += new System.EventHandler(this.timerSkeletonTracker_Tick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(395, 327);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 17;
            this.button1.Text = "Enter";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // timerUnoccupiedRoom
            // 
            this.timerUnoccupiedRoom.Interval = 1000;
            this.timerUnoccupiedRoom.Tick += new System.EventHandler(this.timerUnoccupiedRoom_Tick);
            // 
            // timerShutdown
            // 
            this.timerShutdown.Tick += new System.EventHandler(this.timerShutdown_Tick);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(476, 327);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 18;
            this.button2.Text = "Exit";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // timerStartLights
            // 
            this.timerStartLights.Interval = 15000;
            this.timerStartLights.Tick += new System.EventHandler(this.timerStartLights_Tick);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(697, 375);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "Brodie Home Theatre";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPots)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTray)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ListBox listBoxActivities;
        private System.Windows.Forms.Label labelHarmonyStatus;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelCurrentActivity;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labelPLMstatus;
        private System.Windows.Forms.Label labelPots;
        private System.Windows.Forms.Label labelTray;
        private System.Windows.Forms.TrackBar trackBarPots;
        private System.Windows.Forms.TrackBar trackBarTray;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label labelKodiStatus;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Label labelLastVoiceCommand;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label labelRoomStatus;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label labelKinectStatus;
        private System.Windows.Forms.Timer timerPLMreceive;
        private System.Windows.Forms.Timer timerTrayTrack;
        private System.Windows.Forms.Timer timerPotTrack;
        private System.Windows.Forms.Timer timerCheckPLM;
        private System.Windows.Forms.Timer timerKodiPoller;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatus;
        private System.Windows.Forms.Timer timerClearStatus;
        private System.Windows.Forms.Timer timerCheckKinect;
        private System.Windows.Forms.Timer timerSkeletonTracker;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer timerUnoccupiedRoom;
        private System.Windows.Forms.Timer timerShutdown;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Timer timerStartLights;
    }
}