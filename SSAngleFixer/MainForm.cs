// Utility for restoring Xbox 360 DVD security sectors after the 
// angles have been set to defaults in ss_sector_range.exe
// Requires the dump's original Xbox Backup Creator log 

// Code included is herby in the public domain
// No restrictions on its use or redistribution

// By Hadzz

using System.Globalization;

namespace ReplayTableFixer
{
    public partial class MainForm : Form
    {
        SS ssObj = new();
        public MainForm()
        {
            InitializeComponent();
            groupBoxSS.AllowDrop = true;
        }

        private void loadSS(string filePath)
        {
            // open up the ss in the helper class 
            if (ssObj.LoadSS(filePath))
            {
                // grab the angles
                byte[] r5 = ssObj.GetResponseRT(5);
                byte[] r6 = ssObj.GetResponseRT(6);
                byte[] r7 = ssObj.GetResponseRT(7);
                byte[] r8 = ssObj.GetResponseRT(8);

                ssFilePath.Text = filePath;
                ssFilePath.SelectionStart = filePath.Length;
                // show angles as hex
                rtResponse5.Text = BitConverter.ToString(r5).Replace("-", string.Empty);
                rtResponse6.Text = BitConverter.ToString(r6).Replace("-", string.Empty);
                rtResponse7.Text = BitConverter.ToString(r7).Replace("-", string.Empty);
                rtResponse8.Text = BitConverter.ToString(r8).Replace("-", string.Empty);

                // angles are two bytes but byte order is reversed
                // so flip bytes and save to an int
                uint r5Angle1 = ((uint)r5[1] << 8) | (uint)r5[0];
                uint r5Angle2 = ((uint)r5[4] << 8) | (uint)r5[3];
                uint r6Angle1 = ((uint)r6[1] << 8) | (uint)r6[0];
                uint r6Angle2 = ((uint)r6[4] << 8) | (uint)r6[3];
                uint r7Angle1 = ((uint)r7[1] << 8) | (uint)r7[0];
                uint r7Angle2 = ((uint)r7[4] << 8) | (uint)r7[3];
                uint r8Angle1 = ((uint)r8[1] << 8) | (uint)r8[0];
                uint r8Angle2 = ((uint)r8[4] << 8) | (uint)r8[3];

                // show angles as degrees in decimal
                angle1_5.Text = r5Angle1.ToString();
                angle2_5.Text = r5Angle2.ToString();
                angle1_6.Text = r6Angle1.ToString();
                angle2_6.Text = r6Angle2.ToString();
                angle1_7.Text = r7Angle1.ToString();
                angle2_7.Text = r7Angle2.ToString();
                angle1_8.Text = r8Angle1.ToString();
                angle2_8.Text = r8Angle2.ToString();

                // while this isn't a guaranteed way to know if its modified, it'll work most of the time
                if (r5Angle1 == 1 && r6Angle1 == 91 && r7Angle1 == 181 && r8Angle1 == 271)
                {
                    lbAltered.Text = "Possible modification detected, load a log.";
                    lbAltered.ForeColor = Color.Blue;
                }
                else
                {
                    lbAltered.Text = "Modification not detected, don't patch.";
                    lbAltered.ForeColor = Color.Green;
                }
                clearXBCGroupBox();
                enableXBCGroupBox();
            }
            else
            {
                lbAltered.Text = "Error: '" + Path.GetFileName(filePath) + "' isn't a valid SS.";
                lbAltered.ForeColor = Color.Red;
                clearXBCGroupBox();
                disableXBCGroupBox();
                clearSSGroupBox();
            }
            btnOverwrite.Enabled = false;
            btnSaveAs.Enabled = false;
        }

        private void loadLog(string filePath)
        {
            txtLogPath.Text = filePath;
            txtLogPath.SelectionStart = filePath.Length;

            var lines = File.ReadAllLines(filePath);
            bool found = false;

            /*
             * Example of replay table from XBC log
             * Angles are lines 7-10 on the right half of drive response
             * 
            RT CID MOD DATA          Drive Response
            -- --  --  ------------- -------------------
            03 3E  00  04A9B0 04B9AF 731C9129 10BE733C00    
            01 4B  00  20D7F0 20E7EF 09F11BE0 1174E69800    
            03 E2  00  DF85F0 DF95EF D7F8FCAA AFF4662A00    
            01 AC  00  FB5B70 FB6B6F 8F19A1BB C29BF21B00    
            07 79  00  04A9B0 04B72F A87725FD 0200000200    
            05 C9  00  20D7F0 20E56F 616CC226 5B00005B00    
            07 F1  00  DF85F0 DF936F 65967796 B50000B500    
            05 15  00  FB5B70 FB68EF F2710A4C 0E01000E01    
            E0 29  00  169261 C92791 00000000 0000000000    
            --------------------------------------------
            */

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "RT CID MOD DATA          Drive Response")
                {
                    // make sure theres enough lines for rt to be valid
                    if (lines.Length < i+11) {
                        lblAltered_XBC.Text = "Error: Log's replay table size is wrong.";
                        lblAltered_XBC.ForeColor = Color.Red;
                        clearXBCGroupBox();
                        return;
                    }
                    // save all drive responses into a single string
                    // used to make sure this is the correct log
                    string fullRT = String.Empty;
                    for (var j = i + 2; j < i + 11; j++)
                    {
                        if (lines[j].Length == 48)
                        {
                            fullRT += lines[j].Substring(25, 8);
                            fullRT += lines[j].Substring(34, 10);
                        }
                    }
                    // Check if RT is all valid hex chars
                    if (!SS.IsHex(fullRT))
                    {
                        lblAltered_XBC.Text = "Error: Non-hex values found in log's replay table.";
                        lblAltered_XBC.ForeColor = Color.Red;
                        clearXBCGroupBox();
                        return;
                    }
                    else if (fullRT.Length != 162)
                    {
                        lblAltered_XBC.Text = "Error: Log's replay table size is wrong.";
                        lblAltered_XBC.ForeColor = Color.Red;
                        clearXBCGroupBox();
                        return;
                    }
                    // validate log by comparing to SS (excluding angles)
                    else if (ssObj.compareRestOfRT(fullRT))
                    {
                        found = true;
                        var strRTResponse5 = lines[i + 6].Substring(25, 19).Replace(" ", "");
                        var strRTResponse6 = lines[i + 7].Substring(25, 19).Replace(" ", "");
                        var strRTResponse7 = lines[i + 8].Substring(25, 19).Replace(" ", "");
                        var strRTResponse8 = lines[i + 9].Substring(25, 19).Replace(" ", "");

                        // show angles as hex in the form
                        rtResponse5_XBC.Text = strRTResponse5.Substring(8, 10);
                        rtResponse6_XBC.Text = strRTResponse6.Substring(8, 10);
                        rtResponse7_XBC.Text = strRTResponse7.Substring(8, 10);
                        rtResponse8_XBC.Text = strRTResponse8.Substring(8, 10);

                        byte[] r5 = ConvertHexStringToByteArray(strRTResponse5.Substring(8, 10));
                        byte[] r6 = ConvertHexStringToByteArray(strRTResponse6.Substring(8, 10));
                        byte[] r7 = ConvertHexStringToByteArray(strRTResponse7.Substring(8, 10));
                        byte[] r8 = ConvertHexStringToByteArray(strRTResponse8.Substring(8, 10));

                        // angles are two bytes but byte order is reversed
                        // so flip bytes and save to an int
                        uint r5Angle1 = ((uint)r5[1] << 8) | (uint)r5[0];
                        uint r5Angle2 = ((uint)r5[4] << 8) | (uint)r5[3];
                        uint r6Angle1 = ((uint)r6[1] << 8) | (uint)r6[0];
                        uint r6Angle2 = ((uint)r6[4] << 8) | (uint)r6[3];
                        uint r7Angle1 = ((uint)r7[1] << 8) | (uint)r7[0];
                        uint r7Angle2 = ((uint)r7[4] << 8) | (uint)r7[3];
                        uint r8Angle1 = ((uint)r8[1] << 8) | (uint)r8[0];
                        uint r8Angle2 = ((uint)r8[4] << 8) | (uint)r8[3];

                        // show angles as degrees in decimal
                        angle1_5_XBC.Text = r5Angle1.ToString();
                        angle2_5_XBC.Text = r5Angle2.ToString();
                        angle1_6_XBC.Text = r6Angle1.ToString();
                        angle2_6_XBC.Text = r6Angle2.ToString();
                        angle1_7_XBC.Text = r7Angle1.ToString();
                        angle2_7_XBC.Text = r7Angle2.ToString();
                        angle1_8_XBC.Text = r8Angle1.ToString();
                        angle2_8_XBC.Text = r8Angle2.ToString();

                        lblAltered_XBC.Text = "SS data found from '" + Path.GetFileName(filePath) + "'.";
                        lblAltered_XBC.ForeColor = Color.Green;

                        btnReplaceAngles.Enabled = true;
                        break;
                    }
                    else
                    {
                        lblAltered_XBC.Text = "Error: Data in log does not match the SS file. Is it the right one?";
                        lblAltered_XBC.ForeColor = Color.Red;
                        clearXBCGroupBox();
                        return;
                    }
                }
            }

            if (!found)
            {
                lblAltered_XBC.Text = "Error: Couldn't find SS data from '" + Path.GetFileName(filePath) + "'.";
                lblAltered_XBC.ForeColor = Color.Red;
                clearXBCGroupBox();
            }
        }

        private void btnLoadSS_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.InitialDirectory = "";
            openFileDialog.Filter = "bin files (*.bin)|*.bin|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                loadSS(openFileDialog.FileName);
            }
        }

        private void clearSSGroupBox()
        {
            angle1_5.Text = String.Empty;
            angle2_5.Text = String.Empty;
            angle1_6.Text = String.Empty;
            angle2_6.Text = String.Empty;
            angle1_7.Text = String.Empty;
            angle2_7.Text = String.Empty;
            angle1_8.Text = String.Empty;
            angle2_8.Text = String.Empty;

            rtResponse5.Text = String.Empty;
            rtResponse6.Text = String.Empty;
            rtResponse7.Text = String.Empty;
            rtResponse8.Text = String.Empty;

            ssFilePath.Text = String.Empty;
            btnReplaceAngles.Enabled = false;
            btnOverwrite.Enabled = false;
            btnSaveAs.Enabled = false;
        }

        private void enableXBCGroupBox()
        {
            groupBoxXBCLog.Enabled = true;
            lblAltered_XBC.Text = "Waiting for log file...";
            lblAltered_XBC.ForeColor = Color.Black;
            btnReplaceAngles.Enabled = false;
            groupBoxXBCLog.AllowDrop = true;
        }

        private void disableXBCGroupBox()
        {
            groupBoxXBCLog.Enabled = false;
            btnReplaceAngles.Enabled = false;
            lblAltered_XBC.Text = "Waiting for SS file...";
            lblAltered_XBC.ForeColor = Color.Black;
            groupBoxXBCLog.AllowDrop = false;
        }

        private void clearXBCGroupBox()
        {
            angle1_5_XBC.Text = String.Empty;
            angle2_5_XBC.Text = String.Empty;
            angle1_6_XBC.Text = String.Empty;
            angle2_6_XBC.Text = String.Empty;
            angle1_7_XBC.Text = String.Empty;
            angle2_7_XBC.Text = String.Empty;
            angle1_8_XBC.Text = String.Empty;
            angle2_8_XBC.Text = String.Empty;

            rtResponse5_XBC.Text = String.Empty;
            rtResponse6_XBC.Text = String.Empty;
            rtResponse7_XBC.Text = String.Empty;
            rtResponse8_XBC.Text = String.Empty;

            txtLogPath.Text = String.Empty;
            btnReplaceAngles.Enabled = false;
        }

        private void btnLoadLog_Click(object sender, EventArgs e)
        {
            var filePath = string.Empty;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "log files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    loadLog(filePath);
                }
            }
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

        private void btnReplaceAngles_Click(object sender, EventArgs e)
        {
            rtResponse5.Text = rtResponse5_XBC.Text;
            rtResponse6.Text = rtResponse6_XBC.Text;
            rtResponse7.Text = rtResponse7_XBC.Text;
            rtResponse8.Text = rtResponse8_XBC.Text;

            angle1_5.Text = angle1_5_XBC.Text;
            angle2_5.Text = angle2_5_XBC.Text;
            angle1_6.Text = angle1_6_XBC.Text;
            angle2_6.Text = angle2_6_XBC.Text;
            angle1_7.Text = angle1_7_XBC.Text;
            angle2_7.Text = angle2_7_XBC.Text;
            angle1_8.Text = angle1_8_XBC.Text;
            angle2_8.Text = angle2_8_XBC.Text;

            btnSaveAs.Enabled = true;
            btnOverwrite.Enabled = true;

            lbAltered.Text = "Successfully imported angle data from '" + Path.GetFileName(txtLogPath.Text) +
                "'.\r\nPlease save changes now.";
            lbAltered.ForeColor = Color.Green;
        }

        private void btnOverwrite_Click(object sender, EventArgs e)
        {
            if (ssObj.writeSS(ssFilePath.Text, getRTFromTextBoxes()))
            {
                lbAltered.Text = "SS saved successfully to '" + Path.GetFileName(ssFilePath.Text) + "'.";
                lbAltered.ForeColor = Color.Green;
            }
            else
            {
                lbAltered.Text = "Error: could not save file '" + Path.GetFileName(ssFilePath.Text) + "'.";
                lbAltered.ForeColor = Color.Red;
            }
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "bin files (*.bin)|*.bin|All files (*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = Path.GetFileName(ssFilePath.Text);

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = saveFileDialog1.FileName;
                if (ssObj.writeSS(file, getRTFromTextBoxes()))
                {
                    lbAltered.Text = "SS saved successfully to '" + Path.GetFileName(file) + "'.";
                    lbAltered.ForeColor = Color.Green;
                }
                else
                {
                    lbAltered.Text = "Error: could not save file '" + Path.GetFileName(file) + "'.";
                    lbAltered.ForeColor = Color.Red;
                }
            }
        }

        private string[] getRTFromTextBoxes()
        {
            string[] rtFromTextBoxes = new string[4];
            rtFromTextBoxes[0] = rtResponse5.Text;
            rtFromTextBoxes[1] = rtResponse6.Text;
            rtFromTextBoxes[2] = rtResponse7.Text;
            rtFromTextBoxes[3] = rtResponse8.Text;
            return rtFromTextBoxes;
        }

        private void groupBoxSS_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void groupBoxSS_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data != null)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Any())
                {
                    string filePath = files.First();
                    loadSS(filePath);
                }
            }
        }

        private void groupBoxXBCLog_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void groupBoxXBCLog_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data != null)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Any())
                {
                    string filePath = files.First();
                    loadLog(filePath);
                }
            }
        }
    }
}