using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TressFXUtils.TressFX;
using TressFXUtils.Util;
using System.Numerics;

namespace TressFXUtils
{
    public partial class frmMain : Form
    {
        #region Singleton
        private static frmMain instance;

        public static frmMain GetInstance()
        {
            return instance;
        }
        #endregion

        public Dictionary<string, TressFXHair> hairMeshes;

        /// <summary>
        /// Initializes the form and sets the singleton pattern.
        /// </summary>
        public frmMain()
        {
            // GUI Init
            InitializeComponent();
            this.grp_hair_info.Visible = false;

            // Logic init
            instance = this;
            this.hairMeshes = new Dictionary<string, TressFXHair>();
        }

        /// <summary>
        /// Updates all hair related gui informations.
        /// Call this after you changed anything of hairMeshes.
        /// </summary>
        private void UpdateHairGuiInfos()
        {
            int vertexCount = 0;

            // Update list
            list_meshes.Items.Clear();
            
            foreach (KeyValuePair<string, TressFXHair> hairEntry in this.hairMeshes)
            {
                vertexCount += hairEntry.Value.vertexCount;
                list_meshes.Items.Add(hairEntry.Key);
            }
            
            
            this.lbl_vertices.Text = vertexCount + "";
        }

        /// <summary>
        /// Load hair menu strip item.
        /// Gets used for... loading hair...
        /// Seems like captain obvious hit again :D
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu_loadHair_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open TressFX Hairfile";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filename = System.IO.Path.GetFileName(ofd.FileName);
                TressFXHair hair = new TressFXHair();
                hair.LoadTressFXFile(ofd.FileName);
                hairMeshes.Add(Path.GetFileNameWithoutExtension(ofd.FileName), hair);

                this.UpdateHairGuiInfos();
            }
        }

        /// <summary>
        /// UV-Recalculate menu strip item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu_recalc_uvs_Click(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, TressFXHair> hairEntry in this.hairMeshes)
            {
                hairEntry.Value.RecalculateUVs();
            }
            MessageBox.Show("UVs Recalculated!");
        }

        /// <summary>
        /// Converts an .ase file to multiple tressfx meshes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void convertaseFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Open .ase file";
                ofd.Filter = "ASE File (*.ase)|";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    this.hairMeshes = AseConverter.ConvertAse(ofd.FileName);
                    this.UpdateHairGuiInfos();
                }
                else
                    return;
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Couldn't convert ase file: \r\n" + ex.Message + "!\r\n" + ex.StackTrace);
                return;
            }
        }

        private void saveHairMeshesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Please set the tressfx hair file names prefix.");
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save TressFX Hair";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // Export hair
                string filename = sfd.FileName;

                int i = 0;
                foreach (KeyValuePair<string, TressFXHair> hairEntry in this.hairMeshes)
                {
                    hairEntry.Value.SaveHair(filename + "_" + i + ".tfx");

                    i++;
                }
            }
        }

        private void list_meshes_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateHairDetail();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private TressFXHair GetCurrentSelectedHair()
        {
            // Get TressFX Hair
            if (this.hairMeshes.ContainsKey(this.list_meshes.SelectedItem.ToString()))
                return this.hairMeshes[this.list_meshes.SelectedItem.ToString()];

            return null;
        }

        /// <summary>
        /// Updates the hair detail group box elements.
        /// </summary>
        private void UpdateHairDetail()
        {
            TressFXHair hair = this.GetCurrentSelectedHair();

            if (hair != null)
            {
                this.grp_hair_info.Visible = true;

                // Update statistics / info group box
                this.lbl_hair_vertices.Text = hair.vertexCount + "";
                this.rotation_x.Text = hair.rotation.X + "";
                this.rotation_y.Text = hair.rotation.Y + "";
                this.rotation_z.Text = hair.rotation.Z + "";

                this.translation_x.Text = hair.translation.X + "";
                this.translation_y.Text = hair.translation.Y + "";
                this.translation_z.Text = hair.translation.Z + "";

                this.chk_both_ends_immovable.Checked = hair.bothEndsImmovable;
                this.txt_follow_hair_per_guide_hair.Text = hair.numFollowHairsPerGuideHair + "";
                this.txt_follow_hair_radius_around_guide_hair.Text = hair.maxRadiusAroundGuideHair + "";
            }
        }

        private void btn_detail_save_Click(object sender, EventArgs e)
        {
            TressFXHair hair = this.GetCurrentSelectedHair();

            if (hair != null)
            {
                // Update hair info
                hair.rotation = new Vector3(ParseUtils.ParseFloat(rotation_x.Text), ParseUtils.ParseFloat(rotation_y.Text), ParseUtils.ParseFloat(rotation_z.Text));
                hair.translation = new Vector3(ParseUtils.ParseFloat(translation_x.Text), ParseUtils.ParseFloat(translation_y.Text), ParseUtils.ParseFloat(translation_z.Text));

                hair.bothEndsImmovable = this.chk_both_ends_immovable.Checked;
                hair.numFollowHairsPerGuideHair = int.Parse(this.txt_follow_hair_per_guide_hair.Text);
                hair.maxRadiusAroundGuideHair = ParseUtils.ParseFloat(this.txt_follow_hair_radius_around_guide_hair.Text);

                // Update hair detail info
                this.UpdateHairDetail();
            }
        }

        /// <summary>
        /// Saves the currently selected hair mesh.
        /// This function will open an savefiledialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveSelectedHairMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TressFXHair hair = this.GetCurrentSelectedHair();

            if (hair != null)
            {
                // Call save file dialog
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = "tfx";
                sfd.FileName = hair.filename;
                sfd.AddExtension = false;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    hair.SaveHair(sfd.FileName);
                }
            }
        }
    }
}
