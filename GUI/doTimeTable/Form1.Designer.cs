namespace doTimeTable
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing)
            {
                foreach (Form2 form in myForm2s)
                {
                    if (form != null)
                    {
                        form.Dispose();
                    }
                }
            }

            Save_geometry();
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAnonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.log_window = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editCreateToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.daysHoursToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.subjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.classesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.teacherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.teachertoClassToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.classtoSubjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewRosterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.doTimeTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installNewVersionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editCreateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.daysHoursToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.subjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.classesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.teacherToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.teachertoClassToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.classtoSubjectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.configurationToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.findRosterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewRosterToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.importRegistrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.editToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.installNewVersionToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.exportAnonToolStripMenuItem,
            this.importToolStripMenuItem,
            this.toolStripSeparator1,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.newToolStripMenuItem.Text = "New...";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.ExportToolStripMenuItem_Click);
            // 
            // exportAnonToolStripMenuItem
            // 
            this.exportAnonToolStripMenuItem.Name = "exportAnonToolStripMenuItem";
            this.exportAnonToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.exportAnonToolStripMenuItem.Text = "Export anonymised";
            this.exportAnonToolStripMenuItem.Click += new System.EventHandler(this.ExportAnonToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.ImportToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(172, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.CloseToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.log_window});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // log_window
            // 
            this.log_window.Checked = true;
            this.log_window.CheckState = System.Windows.Forms.CheckState.Checked;
            this.log_window.Name = "log_window";
            this.log_window.Size = new System.Drawing.Size(94, 22);
            this.log_window.Text = "Log";
            this.log_window.Click += new System.EventHandler(this.Log_window_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem2,
            this.deleteToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.editCreateToolStripMenuItem1,
            this.runToolStripMenuItem,
            this.viewRosterToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.editToolStripMenuItem.Text = "Project";
            // 
            // newToolStripMenuItem2
            // 
            this.newToolStripMenuItem2.Name = "newToolStripMenuItem2";
            this.newToolStripMenuItem2.Size = new System.Drawing.Size(133, 22);
            this.newToolStripMenuItem2.Text = "New...";
            this.newToolStripMenuItem2.Click += new System.EventHandler(this.NewToolStripMenuItem2_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.CopyToolStripMenuItem_Click);
            // 
            // editCreateToolStripMenuItem1
            // 
            this.editCreateToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.daysHoursToolStripMenuItem,
            this.toolStripSeparator6,
            this.subjectsToolStripMenuItem,
            this.classesToolStripMenuItem,
            this.teacherToolStripMenuItem,
            this.toolStripSeparator2,
            this.teachertoClassToolStripMenuItem,
            this.classtoSubjectToolStripMenuItem,
            this.toolStripSeparator4,
            this.configurationToolStripMenuItem});
            this.editCreateToolStripMenuItem1.Name = "editCreateToolStripMenuItem1";
            this.editCreateToolStripMenuItem1.Size = new System.Drawing.Size(133, 22);
            this.editCreateToolStripMenuItem1.Text = "Edit/Create";
            // 
            // daysHoursToolStripMenuItem
            // 
            this.daysHoursToolStripMenuItem.Name = "daysHoursToolStripMenuItem";
            this.daysHoursToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.daysHoursToolStripMenuItem.Text = "DaysHours";
            this.daysHoursToolStripMenuItem.Click += new System.EventHandler(this.DaysHoursToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(160, 6);
            // 
            // subjectsToolStripMenuItem
            // 
            this.subjectsToolStripMenuItem.Name = "subjectsToolStripMenuItem";
            this.subjectsToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.subjectsToolStripMenuItem.Text = "Subjects";
            this.subjectsToolStripMenuItem.ToolTipText = "Select a single project";
            this.subjectsToolStripMenuItem.Click += new System.EventHandler(this.SubjectsToolStripMenuItem_Click);
            // 
            // classesToolStripMenuItem
            // 
            this.classesToolStripMenuItem.Name = "classesToolStripMenuItem";
            this.classesToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.classesToolStripMenuItem.Text = "Classes";
            this.classesToolStripMenuItem.Click += new System.EventHandler(this.ClassesToolStripMenuItem_Click);
            // 
            // teacherToolStripMenuItem
            // 
            this.teacherToolStripMenuItem.Name = "teacherToolStripMenuItem";
            this.teacherToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.teacherToolStripMenuItem.Text = "Teacher";
            this.teacherToolStripMenuItem.Click += new System.EventHandler(this.TeacherToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(160, 6);
            // 
            // teachertoClassToolStripMenuItem
            // 
            this.teachertoClassToolStripMenuItem.Name = "teachertoClassToolStripMenuItem";
            this.teachertoClassToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.teachertoClassToolStripMenuItem.Text = "Teacher-to-Class";
            this.teachertoClassToolStripMenuItem.Click += new System.EventHandler(this.TeachertoClassToolStripMenuItem_Click);
            // 
            // classtoSubjectToolStripMenuItem
            // 
            this.classtoSubjectToolStripMenuItem.Name = "classtoSubjectToolStripMenuItem";
            this.classtoSubjectToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.classtoSubjectToolStripMenuItem.Text = "Class-to-Subject";
            this.classtoSubjectToolStripMenuItem.Click += new System.EventHandler(this.ClasstoSubjectToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(160, 6);
            // 
            // configurationToolStripMenuItem
            // 
            this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            this.configurationToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.configurationToolStripMenuItem.Text = "Configuration";
            this.configurationToolStripMenuItem.Click += new System.EventHandler(this.ConfigurationToolStripMenuItem_Click);
            // 
            // runToolStripMenuItem
            // 
            this.runToolStripMenuItem.Name = "runToolStripMenuItem";
            this.runToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.runToolStripMenuItem.Text = "Find roster";
            this.runToolStripMenuItem.Click += new System.EventHandler(this.RunToolStripMenuItem_Click);
            // 
            // viewRosterToolStripMenuItem
            // 
            this.viewRosterToolStripMenuItem.Name = "viewRosterToolStripMenuItem";
            this.viewRosterToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.viewRosterToolStripMenuItem.Text = "View roster";
            this.viewRosterToolStripMenuItem.Click += new System.EventHandler(this.ViewRosterToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.doTimeTableToolStripMenuItem,
            this.importRegistrationToolStripMenuItem});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // doTimeTableToolStripMenuItem
            // 
            this.doTimeTableToolStripMenuItem.Name = "doTimeTableToolStripMenuItem";
            this.doTimeTableToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.doTimeTableToolStripMenuItem.Text = "doTimeTable";
            this.doTimeTableToolStripMenuItem.Click += new System.EventHandler(this.DoTimeTableToolStripMenuItem_Click);
            // 
            // installNewVersionToolStripMenuItem
            // 
            this.installNewVersionToolStripMenuItem.Name = "installNewVersionToolStripMenuItem";
            this.installNewVersionToolStripMenuItem.Size = new System.Drawing.Size(118, 20);
            this.installNewVersionToolStripMenuItem.Text = "Install New version";
            this.installNewVersionToolStripMenuItem.Click += new System.EventHandler(this.InstallNewVersionToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dataGridView1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 426);
            this.panel1.TabIndex = 1;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(800, 426);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellValueChanged);
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.DataGridView1_SelectionChanged);
            this.dataGridView1.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.DataGridView1_SortCompare);
            this.dataGridView1.DoubleClick += new System.EventHandler(this.DataGridView1_DoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem1,
            this.deleteToolStripMenuItem1,
            this.copyToolStripMenuItem1,
            this.editCreateToolStripMenuItem,
            this.findRosterToolStripMenuItem,
            this.viewRosterToolStripMenuItem1});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(134, 136);
            // 
            // newToolStripMenuItem1
            // 
            this.newToolStripMenuItem1.Name = "newToolStripMenuItem1";
            this.newToolStripMenuItem1.Size = new System.Drawing.Size(133, 22);
            this.newToolStripMenuItem1.Text = "New";
            this.newToolStripMenuItem1.Click += new System.EventHandler(this.NewToolStripMenuItem1_Click);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(133, 22);
            this.deleteToolStripMenuItem1.Text = "Delete";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.DeleteToolStripMenuItem1_Click);
            // 
            // copyToolStripMenuItem1
            // 
            this.copyToolStripMenuItem1.Name = "copyToolStripMenuItem1";
            this.copyToolStripMenuItem1.Size = new System.Drawing.Size(133, 22);
            this.copyToolStripMenuItem1.Text = "Copy";
            this.copyToolStripMenuItem1.Click += new System.EventHandler(this.CopyToolStripMenuItem1_Click);
            // 
            // editCreateToolStripMenuItem
            // 
            this.editCreateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.daysHoursToolStripMenuItem1,
            this.toolStripSeparator7,
            this.subjectToolStripMenuItem,
            this.classesToolStripMenuItem1,
            this.teacherToolStripMenuItem1,
            this.toolStripSeparator3,
            this.teachertoClassToolStripMenuItem1,
            this.classtoSubjectToolStripMenuItem1,
            this.toolStripSeparator5,
            this.configurationToolStripMenuItem1});
            this.editCreateToolStripMenuItem.Name = "editCreateToolStripMenuItem";
            this.editCreateToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.editCreateToolStripMenuItem.Text = "Edit/Create";
            // 
            // daysHoursToolStripMenuItem1
            // 
            this.daysHoursToolStripMenuItem1.Name = "daysHoursToolStripMenuItem1";
            this.daysHoursToolStripMenuItem1.Size = new System.Drawing.Size(163, 22);
            this.daysHoursToolStripMenuItem1.Text = "DaysHours";
            this.daysHoursToolStripMenuItem1.Click += new System.EventHandler(this.DaysHoursToolStripMenuItem1_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(160, 6);
            // 
            // subjectToolStripMenuItem
            // 
            this.subjectToolStripMenuItem.Name = "subjectToolStripMenuItem";
            this.subjectToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.subjectToolStripMenuItem.Text = "Subjects";
            this.subjectToolStripMenuItem.ToolTipText = "Select a single project";
            this.subjectToolStripMenuItem.Click += new System.EventHandler(this.SubjectToolStripMenuItem_Click);
            // 
            // classesToolStripMenuItem1
            // 
            this.classesToolStripMenuItem1.Name = "classesToolStripMenuItem1";
            this.classesToolStripMenuItem1.Size = new System.Drawing.Size(163, 22);
            this.classesToolStripMenuItem1.Text = "Classes";
            this.classesToolStripMenuItem1.Click += new System.EventHandler(this.ClassesToolStripMenuItem1_Click);
            // 
            // teacherToolStripMenuItem1
            // 
            this.teacherToolStripMenuItem1.Name = "teacherToolStripMenuItem1";
            this.teacherToolStripMenuItem1.Size = new System.Drawing.Size(163, 22);
            this.teacherToolStripMenuItem1.Text = "Teacher";
            this.teacherToolStripMenuItem1.Click += new System.EventHandler(this.TeacherToolStripMenuItem1_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(160, 6);
            // 
            // teachertoClassToolStripMenuItem1
            // 
            this.teachertoClassToolStripMenuItem1.Name = "teachertoClassToolStripMenuItem1";
            this.teachertoClassToolStripMenuItem1.Size = new System.Drawing.Size(163, 22);
            this.teachertoClassToolStripMenuItem1.Text = "Teacher-to-Class";
            this.teachertoClassToolStripMenuItem1.Click += new System.EventHandler(this.TeachertoClassToolStripMenuItem1_Click);
            // 
            // classtoSubjectToolStripMenuItem1
            // 
            this.classtoSubjectToolStripMenuItem1.Name = "classtoSubjectToolStripMenuItem1";
            this.classtoSubjectToolStripMenuItem1.Size = new System.Drawing.Size(163, 22);
            this.classtoSubjectToolStripMenuItem1.Text = "Class-to-Subject";
            this.classtoSubjectToolStripMenuItem1.Click += new System.EventHandler(this.ClasstoSubjectToolStripMenuItem1_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(160, 6);
            // 
            // configurationToolStripMenuItem1
            // 
            this.configurationToolStripMenuItem1.Name = "configurationToolStripMenuItem1";
            this.configurationToolStripMenuItem1.Size = new System.Drawing.Size(163, 22);
            this.configurationToolStripMenuItem1.Text = "Configuration";
            this.configurationToolStripMenuItem1.Click += new System.EventHandler(this.ConfigurationToolStripMenuItem1_Click);
            // 
            // findRosterToolStripMenuItem
            // 
            this.findRosterToolStripMenuItem.Name = "findRosterToolStripMenuItem";
            this.findRosterToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.findRosterToolStripMenuItem.Text = "Find roster";
            this.findRosterToolStripMenuItem.Click += new System.EventHandler(this.FindRosterToolStripMenuItem_Click);
            // 
            // viewRosterToolStripMenuItem1
            // 
            this.viewRosterToolStripMenuItem1.Name = "viewRosterToolStripMenuItem1";
            this.viewRosterToolStripMenuItem1.Size = new System.Drawing.Size(133, 22);
            this.viewRosterToolStripMenuItem1.Text = "View roster";
            this.viewRosterToolStripMenuItem1.Click += new System.EventHandler(this.ViewRosterToolStripMenuItem1_Click);
            // 
            // importRegistrationToolStripMenuItem
            // 
            this.importRegistrationToolStripMenuItem.Name = "importRegistrationToolStripMenuItem";
            this.importRegistrationToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.importRegistrationToolStripMenuItem.Text = "Import registration key";
            this.importRegistrationToolStripMenuItem.Click += new System.EventHandler(this.ImportRegistrationToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.LocationChanged += new System.EventHandler(this.Form1_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem log_window;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem editCreateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem subjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editCreateToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem subjectsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem classesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem classesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem daysHoursToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem daysHoursToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem teacherToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem teacherToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem teachertoClassToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem teachertoClassToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem classtoSubjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem classtoSubjectToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem runToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findRosterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem viewRosterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewRosterToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem exportAnonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem doTimeTableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem installNewVersionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importRegistrationToolStripMenuItem;
    }
}

