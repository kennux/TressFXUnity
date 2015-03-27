namespace TressFXUtils
{
    partial class frmSimplification
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbl_minvertices = new System.Windows.Forms.Label();
            this.lbl_t_minvertices = new System.Windows.Forms.Label();
            this.lbl_maxvertices = new System.Windows.Forms.Label();
            this.lbl_t_maxvertices = new System.Windows.Forms.Label();
            this.grp_uniforming = new System.Windows.Forms.GroupBox();
            this.btn_uniformstrands = new System.Windows.Forms.Button();
            this.txt_uniforming_vertexcount = new System.Windows.Forms.TextBox();
            this.lbl_uniform_vertex_count = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.grp_uniforming.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lbl_minvertices);
            this.groupBox1.Controls.Add(this.lbl_t_minvertices);
            this.groupBox1.Controls.Add(this.lbl_maxvertices);
            this.groupBox1.Controls.Add(this.lbl_t_maxvertices);
            this.groupBox1.Location = new System.Drawing.Point(12, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(273, 62);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Statistics";
            // 
            // lbl_minvertices
            // 
            this.lbl_minvertices.AutoSize = true;
            this.lbl_minvertices.Location = new System.Drawing.Point(159, 39);
            this.lbl_minvertices.Name = "lbl_minvertices";
            this.lbl_minvertices.Size = new System.Drawing.Size(13, 13);
            this.lbl_minvertices.TabIndex = 4;
            this.lbl_minvertices.Text = "0";
            // 
            // lbl_t_minvertices
            // 
            this.lbl_t_minvertices.AutoSize = true;
            this.lbl_t_minvertices.Location = new System.Drawing.Point(6, 39);
            this.lbl_t_minvertices.Name = "lbl_t_minvertices";
            this.lbl_t_minvertices.Size = new System.Drawing.Size(144, 13);
            this.lbl_t_minvertices.TabIndex = 3;
            this.lbl_t_minvertices.Text = "Minimum vertices per strand: ";
            // 
            // lbl_maxvertices
            // 
            this.lbl_maxvertices.AutoSize = true;
            this.lbl_maxvertices.Location = new System.Drawing.Point(159, 16);
            this.lbl_maxvertices.Name = "lbl_maxvertices";
            this.lbl_maxvertices.Size = new System.Drawing.Size(13, 13);
            this.lbl_maxvertices.TabIndex = 2;
            this.lbl_maxvertices.Text = "0";
            // 
            // lbl_t_maxvertices
            // 
            this.lbl_t_maxvertices.AutoSize = true;
            this.lbl_t_maxvertices.Location = new System.Drawing.Point(6, 16);
            this.lbl_t_maxvertices.Name = "lbl_t_maxvertices";
            this.lbl_t_maxvertices.Size = new System.Drawing.Size(147, 13);
            this.lbl_t_maxvertices.TabIndex = 1;
            this.lbl_t_maxvertices.Text = "Maximum vertices per strand: ";
            // 
            // grp_uniforming
            // 
            this.grp_uniforming.Controls.Add(this.btn_uniformstrands);
            this.grp_uniforming.Controls.Add(this.txt_uniforming_vertexcount);
            this.grp_uniforming.Controls.Add(this.lbl_uniform_vertex_count);
            this.grp_uniforming.Location = new System.Drawing.Point(12, 80);
            this.grp_uniforming.Name = "grp_uniforming";
            this.grp_uniforming.Size = new System.Drawing.Size(273, 81);
            this.grp_uniforming.TabIndex = 2;
            this.grp_uniforming.TabStop = false;
            this.grp_uniforming.Text = "Strand uniforming (Simplification)";
            // 
            // btn_uniformstrands
            // 
            this.btn_uniformstrands.Location = new System.Drawing.Point(9, 43);
            this.btn_uniformstrands.Name = "btn_uniformstrands";
            this.btn_uniformstrands.Size = new System.Drawing.Size(258, 23);
            this.btn_uniformstrands.TabIndex = 2;
            this.btn_uniformstrands.Text = "Simplificate";
            this.btn_uniformstrands.UseVisualStyleBackColor = true;
            this.btn_uniformstrands.Click += new System.EventHandler(this.btn_uniformstrands_Click);
            // 
            // txt_uniforming_vertexcount
            // 
            this.txt_uniforming_vertexcount.Location = new System.Drawing.Point(137, 17);
            this.txt_uniforming_vertexcount.Name = "txt_uniforming_vertexcount";
            this.txt_uniforming_vertexcount.Size = new System.Drawing.Size(130, 20);
            this.txt_uniforming_vertexcount.TabIndex = 1;
            // 
            // lbl_uniform_vertex_count
            // 
            this.lbl_uniform_vertex_count.AutoSize = true;
            this.lbl_uniform_vertex_count.Location = new System.Drawing.Point(6, 20);
            this.lbl_uniform_vertex_count.Name = "lbl_uniform_vertex_count";
            this.lbl_uniform_vertex_count.Size = new System.Drawing.Size(125, 13);
            this.lbl_uniform_vertex_count.TabIndex = 0;
            this.lbl_uniform_vertex_count.Text = "Uniform to Vertex Count: ";
            // 
            // frmSimplification
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 173);
            this.Controls.Add(this.grp_uniforming);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmSimplification";
            this.Text = "TressFX Utils - Hair Simplification";
            this.Load += new System.EventHandler(this.frmSimplification_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grp_uniforming.ResumeLayout(false);
            this.grp_uniforming.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lbl_minvertices;
        private System.Windows.Forms.Label lbl_t_minvertices;
        private System.Windows.Forms.Label lbl_maxvertices;
        private System.Windows.Forms.Label lbl_t_maxvertices;
        private System.Windows.Forms.GroupBox grp_uniforming;
        private System.Windows.Forms.Button btn_uniformstrands;
        private System.Windows.Forms.TextBox txt_uniforming_vertexcount;
        private System.Windows.Forms.Label lbl_uniform_vertex_count;
    }
}