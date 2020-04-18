using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NugetPackageManager
{
    public partial class frmMain : Form
    {
        private Dictionary<string, string> dictKeys = new Dictionary<string, string>();
        private Setting setting;       

        public frmMain()
        {
            InitializeComponent();

            RichTextBox.CheckForIllegalCrossThreadCalls = false;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.LoadSetting();
        }

        private void LoadSetting()
        {
            this.setting = SettingManager.Setting;

            if (setting != null)
            {
                string keyFilePath = setting.KeyFilePath;

                if (File.Exists(keyFilePath))
                {
                    this.dictKeys.Clear();

                    string[] lines = File.ReadAllLines(keyFilePath);

                    string delimiter = setting.KeyItemDelimiter;

                    if (string.IsNullOrEmpty(delimiter))
                    {
                        delimiter = ":";
                    }

                    foreach (string line in lines)
                    {
                        if (line.Contains(delimiter))
                        {
                            string[] items = line.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

                            if (items.Length == 2)
                            {
                                string keyName = items[0];
                                string keyValue = items[1];

                                if (!this.dictKeys.ContainsKey(keyName))
                                {
                                    this.dictKeys.Add(keyName.Trim(), keyValue.Trim());
                                }
                            }
                        }
                    }
                }

                this.cboKey.Items.Clear();

                this.cboKey.Items.AddRange(this.dictKeys.Keys.ToArray());
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.FileName = "";

            DialogResult result = this.openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.txtFile.Text = this.openFileDialog1.FileName;

                this.SetDefaultKeyByFile(this.txtFile.Text);
            }
        }

        private string GetPackageName(string fileName)
        {
            string[] items = Path.GetFileNameWithoutExtension(fileName).Split('.');

            string packageName = string.Join(".", items.Take(items.Length - 3));

            return packageName;
        }

        private void SetDefaultKeyByFile(string filePath)
        {
            string packageName = this.GetPackageName(Path.GetFileName(filePath));

            string key = this.FindKey(packageName);

            if (!string.IsNullOrEmpty(key))
            {
                this.cboKey.Text = key;
            }
            else
            {
                this.cboKey.SelectedIndex = -1;
            }
        }

        private string FindKey(string packageName)
        {
            IEnumerable<KeyPattern> keyPatterns = this.setting == null ? Enumerable.Empty<KeyPattern>() : setting.KeyPatterns;

            foreach (var kp in this.dictKeys)
            {
                string keyName = kp.Key;

                KeyPattern keyPattern = keyPatterns.FirstOrDefault(item => item.KeyName.ToLower() == keyName.ToLower());

                if (keyPattern != null && !string.IsNullOrEmpty(keyPattern.Pattern))
                {
                    string pattern = keyPattern.Pattern;

                    if (Regex.IsMatch(pattern, "[_a-zA-Z]+"))
                    {
                        if(pattern.EndsWith("*") && !pattern.EndsWith(".*"))
                        {
                            pattern = pattern.Replace("*", ".*");
                        }

                        Regex regex = new Regex(pattern);

                        if (regex.IsMatch(packageName))
                        {
                            return keyName;
                        }
                    }
                }
            }

            return null;
        }

        private async void btnPush_Click(object sender, EventArgs e)
        {
            await Task.Factory.StartNew(() => { this.Execute(CommandType.Push); });
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            await Task.Factory.StartNew(() => { this.Execute(CommandType.Delete); });
        }

        private void Execute(CommandType commandType)
        {
            string filePath = this.txtFile.Text;

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Please select a package file.");
                return;
            }

            string argsName = $"{commandType}Args";

            string argsTemplate = ConfigurationManager.AppSettings[argsName];

            if (string.IsNullOrEmpty(argsTemplate))
            {
                MessageBox.Show($"{argsName} hasn't given a value.");
                return;
            }

            string source = ConfigurationManager.AppSettings["Source"];

            FileInfo file = new FileInfo(filePath);
            string packageFileName = file.Name;

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.Name);

            string packageName = this.GetPackageName(packageFileName);

            string version = fileNameWithoutExt.Replace(packageName, "").Trim('.');

            string keyName = this.cboKey.Text;

            if (string.IsNullOrEmpty(keyName))
            {
                MessageBox.Show("Please select a api key.");
                return;
            }

            string key = this.dictKeys[keyName];

            string args = argsTemplate
                .Replace("{PackageFileName}", packageFileName)
                .Replace("{PackageName}", packageName)
                .Replace("{Version}", version)
                .Replace("{Key}", key)
                .Replace("{Source}", source);

            string command = $"dotnet nuget {commandType.ToString().ToLower()} {args}";

            this.txtMessage.Clear();

            Action<bool> exec = (waitForInput) =>
              {
                  ProcessHelper.ExecuteCommand(file.FullName, command, this.Process_OutputDataReceived, this.Process_ErrorDataReceived, waitForInput);
              };

            if (commandType == CommandType.Delete)
            {
                if (MessageBox.Show("Are you sure to delete this package?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    exec(true);
                }
            }
            else
            {
                exec(false);
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains("(y/N)"))
                {
                    (sender as Process).StandardInput.WriteLine("y");
                }

                bool isError = Regex.IsMatch(e.Data, @"\b(error)\b", RegexOptions.IgnoreCase);

                if(!e.Data.Contains("Microsoft"))
                {
                    this.AppendMessage(isError, e.Data);
                }               
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                this.AppendMessage(true, e.Data);
            }
        }

        private void AppendMessage(bool isError, string message)
        {
            int start = this.txtMessage.Text.Length;

            if (start > 0)
            {
                this.txtMessage.AppendText(Environment.NewLine);
            }

            this.txtMessage.AppendText(message);

            this.txtMessage.Select(start, this.txtMessage.Text.Length - start);
            this.txtMessage.SelectionColor = isError ? Color.Red : Color.Black;

            this.txtMessage.SelectionStart = this.txtMessage.TextLength;
            this.txtMessage.ScrollToCaret();
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            this.SetDropFiles(e);
        }

        private void frmMain_DragOver(object sender, DragEventArgs e)
        {
            this.SetDragEffect(e);
        }

        private void SetDragEffect(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void SetDropFiles(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop.ToString());

                string filePath = filePaths.FirstOrDefault();

                if (!string.IsNullOrEmpty(filePath) && filePath != this.txtFile.Text)
                {
                    this.txtFile.Text = filePath;
                    this.SetDefaultKeyByFile(filePath);
                }
            }
        }

        private void keySettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmKeySetting keySetting = new frmKeySetting();

            if (keySetting.ShowDialog() == DialogResult.OK)
            {
                this.LoadSetting();

                if(string.IsNullOrEmpty(this.cboKey.Text) && !string.IsNullOrEmpty(this.txtFile.Text))
                {
                    this.SetDefaultKeyByFile(this.txtFile.Text);
                }
            }
        }
    }

    public enum CommandType
    {
        None = 0,
        Push = 1,
        Delete = 2
    }
}
