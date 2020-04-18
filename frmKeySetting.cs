using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NugetPackageManager
{
    public partial class frmKeySetting : Form
    {
        public frmKeySetting()
        {
            InitializeComponent();
        }       

        private void frmKeySetting_Load(object sender, EventArgs e)
        {
            this.LoadSetting();
        }

        private void LoadSetting()
        {
            Setting setting = SettingManager.Setting;

            if(setting!=null)
            {
                this.txtKeyFilePath.Text = setting.KeyFilePath;
                this.txtKeyItemDelimiter.Text = setting.KeyItemDelimiter;

                foreach(KeyPattern keyPattern in setting.KeyPatterns)
                {
                    this.dgvKeyPattern.Rows.Add(keyPattern.KeyName, keyPattern.Pattern);
                }
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.FileName = "";

            if(this.openFileDialog1.ShowDialog()== DialogResult.OK)
            {
                this.txtKeyFilePath.Text = this.openFileDialog1.FileName;
            }
        }      

        private void btnRemove_Click(object sender, EventArgs e)
        {
            int count = this.dgvKeyPattern.SelectedRows.Count;
            if (count == 0)
            {
                MessageBox.Show("Please select a row first.");
                return;
            }

            this.dgvKeyPattern.Rows.RemoveAt(this.dgvKeyPattern.SelectedRows[0].Index);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.dgvKeyPattern.Rows.Clear();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Setting setting = new Setting();

            setting.KeyFilePath = this.txtKeyFilePath.Text.Trim();
            setting.KeyItemDelimiter = this.txtKeyItemDelimiter.Text;

            foreach(DataGridViewRow row in this.dgvKeyPattern.Rows)
            {
                string keyName = row.Cells["KeyName"].Value?.ToString();
                string pattern = row.Cells["Pattern"].Value?.ToString();

                if(!string.IsNullOrEmpty(keyName) && !string.IsNullOrEmpty(pattern))
                {
                    if (setting.KeyPatterns.Any(item=>item.KeyName.ToLower().Trim() == keyName.ToLower().Trim()))
                    {
                        MessageBox.Show($"Duplicated Key Name:{keyName}");
                        return;
                    }
                    else
                    {
                        setting.KeyPatterns.Add(new KeyPattern() { KeyName = keyName.Trim(), Pattern = pattern.Trim() });
                    }                    
                }
            }           

            SettingManager.SaveSetting(setting);

            MessageBox.Show("Saved successfully.");

            this.DialogResult = DialogResult.OK;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
