using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorPrinter
{
    public partial class MainForm : Form
    {
        private readonly BindingList<string> messages = new BindingList<string>();
        private readonly SoundPlayer soundToPlayPlayer = new SoundPlayer();

        private string soundToPlay
        {
            set
            {
                soundToPlayPlayer.SoundLocation = value;
                openFileToPlayDialog.FileName = value;
                Properties.Settings.Default.soundToPlay = value;
                Properties.Settings.Default.Save();
            }
            get { return this.soundToPlayPlayer.SoundLocation; }
        }

        public MainForm()
        {
            InitializeComponent();

            soundToPlay = (string)Properties.Settings.Default.soundToPlay;
            Debug.WriteLine(Properties.Settings.Default);
            Debug.WriteLine(Properties.Settings.Default.printersToWatch);
            if (Properties.Settings.Default.printersToWatch == null)
            {
                Properties.Settings.Default.printersToWatch = new System.Collections.Specialized.StringCollection();
            }
        }

        private void Form_Load(object sender, EventArgs e)
        {
            foreach (PrinterQueueWatch.PrinterInformation pi in new PrinterQueueWatch.PrinterInformationCollection())
            {
                checkedListBox1.Items.Add(pi.PrinterName, Properties.Settings.Default.printersToWatch.Contains(pi.PrinterName));
            }
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string deviceName = (string)checkedListBox1.Items[e.Index];
            if (e.NewValue == CheckState.Checked)
            {
                this.printerMonitorComponent.AddPrinter(deviceName);
                Properties.Settings.Default.printersToWatch.Add(deviceName);
                this.messages.Add($"Added printer \"{deviceName}\" to the watch list.");
            }
            else
            {
                this.printerMonitorComponent.RemovePrinter((string)checkedListBox1.Items[e.Index]);
                Properties.Settings.Default.printersToWatch.Remove(deviceName);
                this.messages.Add($"Removed printer \"{deviceName}\" from the watch list.");
            }
            Properties.Settings.Default.Save();
        }

        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.printerMonitorComponent.Disconnect();
            Properties.Settings.Default.Save();
        }

        delegate void printerMonitorComponent_JobAddedDelegate(object sender, PrinterQueueWatch.PrintJobEventArgs e);
        private void printerMonitorComponent_JobAdded(object sender, PrinterQueueWatch.PrintJobEventArgs e)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke((printerMonitorComponent_JobAddedDelegate)delegate (object origSender, PrinterQueueWatch.PrintJobEventArgs args)
                {
                    printerMonitorComponent_JobAdded(origSender, args);
                },
                new object[] { sender, e }
                );
                return;
            }

            try
            {
                soundToPlayPlayer.Play();
            }
            catch (System.InvalidOperationException ex)
            {
                this.messages.Add($"Could not playback file \"{soundToPlay}\" - exception \"{ex.Message}\"");
            }
            this.messages.Add(e.PrintJob.Document);
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void soundWählenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileToPlayDialog.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            soundToPlay = openFileToPlayDialog.FileName;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            listBox1.DataSource = this.messages;
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = this.WindowState == FormWindowState.Normal ? FormWindowState.Minimized : FormWindowState.Normal;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            this.ShowInTaskbar = this.WindowState != FormWindowState.Minimized;
        }
    }
}
