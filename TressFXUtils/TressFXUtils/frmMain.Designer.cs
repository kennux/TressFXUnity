namespace TressFXUtils
{
    partial class frmMain
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
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.menu_file = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_loadHair = new System.Windows.Forms.ToolStripMenuItem();
            this.convertaseFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveHairMeshesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSelectedHairMeshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_hair = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_recalc_uvs = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lbl_t_vertices = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_vertices = new System.Windows.Forms.ToolStripStatusLabel();
            this.grp_meshes = new System.Windows.Forms.GroupBox();
            this.list_meshes = new System.Windows.Forms.ListBox();
            this.grp_hair_info = new System.Windows.Forms.GroupBox();
            this.btn_detail_save = new System.Windows.Forms.Button();
            this.txt_follow_hair_radius_around_guide_hair = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txt_follow_hair_per_guide_hair = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chk_both_ends_immovable = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.translation_z = new System.Windows.Forms.TextBox();
            this.translation_y = new System.Windows.Forms.TextBox();
            this.translation_x = new System.Windows.Forms.TextBox();
            this.lbl_translation = new System.Windows.Forms.Label();
            this.rotation_z = new System.Windows.Forms.TextBox();
            this.rotation_y = new System.Windows.Forms.TextBox();
            this.rotation_x = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lbl_hair_vertices = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.grp_meshes.SuspendLayout();
            this.grp_hair_info.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_file,
            this.menu_hair});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(648, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // menu_file
            // 
            this.menu_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_loadHair,
            this.convertaseFileToolStripMenuItem,
            this.saveHairMeshesToolStripMenuItem,
            this.saveSelectedHairMeshToolStripMenuItem});
            this.menu_file.Name = "menu_file";
            this.menu_file.Size = new System.Drawing.Size(37, 20);
            this.menu_file.Text = "File";
            // 
            // menu_loadHair
            // 
            this.menu_loadHair.Name = "menu_loadHair";
            this.menu_loadHair.Size = new System.Drawing.Size(199, 22);
            this.menu_loadHair.Text = "Load Hair";
            this.menu_loadHair.Click += new System.EventHandler(this.menu_loadHair_Click);
            // 
            // convertaseFileToolStripMenuItem
            // 
            this.convertaseFileToolStripMenuItem.Name = "convertaseFileToolStripMenuItem";
            this.convertaseFileToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.convertaseFileToolStripMenuItem.Text = "Import .ase file";
            this.convertaseFileToolStripMenuItem.Click += new System.EventHandler(this.convertaseFileToolStripMenuItem_Click);
            // 
            // saveHairMeshesToolStripMenuItem
            // 
            this.saveHairMeshesToolStripMenuItem.Name = "saveHairMeshesToolStripMenuItem";
            this.saveHairMeshesToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.saveHairMeshesToolStripMenuItem.Text = "Save Hair Meshes";
            this.saveHairMeshesToolStripMenuItem.Click += new System.EventHandler(this.saveHairMeshesToolStripMenuItem_Click);
            // 
            // saveSelectedHairMeshToolStripMenuItem
            // 
            this.saveSelectedHairMeshToolStripMenuItem.Name = "saveSelectedHairMeshToolStripMenuItem";
            this.saveSelectedHairMeshToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.saveSelectedHairMeshToolStripMenuItem.Text = "Save selected hair mesh";
            this.saveSelectedHairMeshToolStripMenuItem.Click += new System.EventHandler(this.saveSelectedHairMeshToolStripMenuItem_Click);
            // 
            // menu_hair
            // 
            this.menu_hair.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_recalc_uvs});
            this.menu_hair.Name = "menu_hair";
            this.menu_hair.Size = new System.Drawing.Size(41, 20);
            this.menu_hair.Text = "Hair";
            // 
            // menu_recalc_uvs
            // 
            this.menu_recalc_uvs.Name = "menu_recalc_uvs";
            this.menu_recalc_uvs.Size = new System.Drawing.Size(236, 22);
            this.menu_recalc_uvs.Text = "Recalculate UVs (experimental)";
            this.menu_recalc_uvs.Click += new System.EventHandler(this.menu_recalc_uvs_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbl_t_vertices,
            this.lbl_vertices});
            this.statusStrip.Location = new System.Drawing.Point(0, 337);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(648, 22);
            this.statusStrip.TabIndex = 1;
            // 
            // lbl_t_vertices
            // 
            this.lbl_t_vertices.Name = "lbl_t_vertices";
            this.lbl_t_vertices.Size = new System.Drawing.Size(54, 17);
            this.lbl_t_vertices.Text = "Vertices: ";
            // 
            // lbl_vertices
            // 
            this.lbl_vertices.Name = "lbl_vertices";
            this.lbl_vertices.Size = new System.Drawing.Size(0, 17);
            // 
            // grp_meshes
            // 
            this.grp_meshes.Controls.Add(this.list_meshes);
            this.grp_meshes.Location = new System.Drawing.Point(12, 27);
            this.grp_meshes.Name = "grp_meshes";
            this.grp_meshes.Size = new System.Drawing.Size(200, 298);
            this.grp_meshes.TabIndex = 2;
            this.grp_meshes.TabStop = false;
            this.grp_meshes.Text = "Meshes";
            // 
            // list_meshes
            // 
            this.list_meshes.FormattingEnabled = true;
            this.list_meshes.Location = new System.Drawing.Point(6, 16);
            this.list_meshes.Name = "list_meshes";
            this.list_meshes.Size = new System.Drawing.Size(188, 277);
            this.list_meshes.TabIndex = 0;
            this.list_meshes.SelectedIndexChanged += new System.EventHandler(this.list_meshes_SelectedIndexChanged);
            // 
            // grp_hair_info
            // 
            this.grp_hair_info.Controls.Add(this.btn_detail_save);
            this.grp_hair_info.Controls.Add(this.txt_follow_hair_radius_around_guide_hair);
            this.grp_hair_info.Controls.Add(this.label5);
            this.grp_hair_info.Controls.Add(this.txt_follow_hair_per_guide_hair);
            this.grp_hair_info.Controls.Add(this.label4);
            this.grp_hair_info.Controls.Add(this.chk_both_ends_immovable);
            this.grp_hair_info.Controls.Add(this.label3);
            this.grp_hair_info.Controls.Add(this.translation_z);
            this.grp_hair_info.Controls.Add(this.translation_y);
            this.grp_hair_info.Controls.Add(this.translation_x);
            this.grp_hair_info.Controls.Add(this.lbl_translation);
            this.grp_hair_info.Controls.Add(this.rotation_z);
            this.grp_hair_info.Controls.Add(this.rotation_y);
            this.grp_hair_info.Controls.Add(this.rotation_x);
            this.grp_hair_info.Controls.Add(this.label2);
            this.grp_hair_info.Controls.Add(this.lbl_hair_vertices);
            this.grp_hair_info.Controls.Add(this.label1);
            this.grp_hair_info.Location = new System.Drawing.Point(222, 27);
            this.grp_hair_info.Name = "grp_hair_info";
            this.grp_hair_info.Size = new System.Drawing.Size(414, 298);
            this.grp_hair_info.TabIndex = 3;
            this.grp_hair_info.TabStop = false;
            this.grp_hair_info.Text = "Hair Info";
            // 
            // btn_detail_save
            // 
            this.btn_detail_save.Location = new System.Drawing.Point(9, 167);
            this.btn_detail_save.Name = "btn_detail_save";
            this.btn_detail_save.Size = new System.Drawing.Size(387, 23);
            this.btn_detail_save.TabIndex = 16;
            this.btn_detail_save.Text = "Save";
            this.btn_detail_save.UseVisualStyleBackColor = true;
            this.btn_detail_save.Click += new System.EventHandler(this.btn_detail_save_Click);
            // 
            // txt_follow_hair_radius_around_guide_hair
            // 
            this.txt_follow_hair_radius_around_guide_hair.Location = new System.Drawing.Point(194, 141);
            this.txt_follow_hair_radius_around_guide_hair.Name = "txt_follow_hair_radius_around_guide_hair";
            this.txt_follow_hair_radius_around_guide_hair.Size = new System.Drawing.Size(51, 20);
            this.txt_follow_hair_radius_around_guide_hair.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 141);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(179, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Follow hair radius around guide hair: ";
            // 
            // txt_follow_hair_per_guide_hair
            // 
            this.txt_follow_hair_per_guide_hair.Location = new System.Drawing.Point(194, 116);
            this.txt_follow_hair_per_guide_hair.Name = "txt_follow_hair_per_guide_hair";
            this.txt_follow_hair_per_guide_hair.Size = new System.Drawing.Size(51, 20);
            this.txt_follow_hair_per_guide_hair.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(135, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Follow hairs per guide hair: ";
            // 
            // chk_both_ends_immovable
            // 
            this.chk_both_ends_immovable.AutoSize = true;
            this.chk_both_ends_immovable.Location = new System.Drawing.Point(194, 94);
            this.chk_both_ends_immovable.Name = "chk_both_ends_immovable";
            this.chk_both_ends_immovable.Size = new System.Drawing.Size(15, 14);
            this.chk_both_ends_immovable.TabIndex = 11;
            this.chk_both_ends_immovable.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(114, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Both ends immovable: ";
            // 
            // translation_z
            // 
            this.translation_z.Location = new System.Drawing.Point(251, 67);
            this.translation_z.Name = "translation_z";
            this.translation_z.Size = new System.Drawing.Size(51, 20);
            this.translation_z.TabIndex = 9;
            // 
            // translation_y
            // 
            this.translation_y.Location = new System.Drawing.Point(194, 67);
            this.translation_y.Name = "translation_y";
            this.translation_y.Size = new System.Drawing.Size(51, 20);
            this.translation_y.TabIndex = 8;
            // 
            // translation_x
            // 
            this.translation_x.Location = new System.Drawing.Point(133, 67);
            this.translation_x.Name = "translation_x";
            this.translation_x.Size = new System.Drawing.Size(51, 20);
            this.translation_x.TabIndex = 7;
            // 
            // lbl_translation
            // 
            this.lbl_translation.AutoSize = true;
            this.lbl_translation.Location = new System.Drawing.Point(6, 67);
            this.lbl_translation.Name = "lbl_translation";
            this.lbl_translation.Size = new System.Drawing.Size(65, 13);
            this.lbl_translation.TabIndex = 6;
            this.lbl_translation.Text = "Translation: ";
            // 
            // rotation_z
            // 
            this.rotation_z.Location = new System.Drawing.Point(251, 41);
            this.rotation_z.Name = "rotation_z";
            this.rotation_z.Size = new System.Drawing.Size(51, 20);
            this.rotation_z.TabIndex = 5;
            // 
            // rotation_y
            // 
            this.rotation_y.Location = new System.Drawing.Point(194, 41);
            this.rotation_y.Name = "rotation_y";
            this.rotation_y.Size = new System.Drawing.Size(51, 20);
            this.rotation_y.TabIndex = 4;
            // 
            // rotation_x
            // 
            this.rotation_x.Location = new System.Drawing.Point(133, 41);
            this.rotation_x.Name = "rotation_x";
            this.rotation_x.Size = new System.Drawing.Size(51, 20);
            this.rotation_x.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Rotation: ";
            // 
            // lbl_hair_vertices
            // 
            this.lbl_hair_vertices.AutoSize = true;
            this.lbl_hair_vertices.Location = new System.Drawing.Point(63, 16);
            this.lbl_hair_vertices.Name = "lbl_hair_vertices";
            this.lbl_hair_vertices.Size = new System.Drawing.Size(115, 13);
            this.lbl_hair_vertices.TabIndex = 1;
            this.lbl_hair_vertices.Text = "                                    ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Vertices: ";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 359);
            this.Controls.Add(this.grp_hair_info);
            this.Controls.Add(this.grp_meshes);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "frmMain";
            this.Text = "TressFX Utils";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.grp_meshes.ResumeLayout(false);
            this.grp_hair_info.ResumeLayout(false);
            this.grp_hair_info.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menu_file;
        private System.Windows.Forms.ToolStripMenuItem menu_loadHair;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lbl_t_vertices;
        private System.Windows.Forms.ToolStripStatusLabel lbl_vertices;
        private System.Windows.Forms.ToolStripMenuItem menu_hair;
        private System.Windows.Forms.ToolStripMenuItem menu_recalc_uvs;
        private System.Windows.Forms.ToolStripMenuItem convertaseFileToolStripMenuItem;
        private System.Windows.Forms.GroupBox grp_meshes;
        private System.Windows.Forms.ToolStripMenuItem saveHairMeshesToolStripMenuItem;
        private System.Windows.Forms.ListBox list_meshes;
        private System.Windows.Forms.GroupBox grp_hair_info;
        private System.Windows.Forms.Label lbl_hair_vertices;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox rotation_z;
        private System.Windows.Forms.TextBox rotation_y;
        private System.Windows.Forms.TextBox rotation_x;
        private System.Windows.Forms.TextBox translation_z;
        private System.Windows.Forms.TextBox translation_y;
        private System.Windows.Forms.TextBox translation_x;
        private System.Windows.Forms.Label lbl_translation;
        private System.Windows.Forms.CheckBox chk_both_ends_immovable;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txt_follow_hair_radius_around_guide_hair;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txt_follow_hair_per_guide_hair;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btn_detail_save;
        private System.Windows.Forms.ToolStripMenuItem saveSelectedHairMeshToolStripMenuItem;
    }
}

