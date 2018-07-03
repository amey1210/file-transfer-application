using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO.Compression;


namespace FTSclient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string shortFileName;
        OpenFileDialog op;
        String folderPath;
        string zippath="";
        string[] n = new string[50];
        char[] splitter = { '\\' }; 
        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            lblstatus.Text = "Not connected....";
            //To check whether folder or file is going to send 
            if (CheckFolder.Checked)
            {
                FolderBrowserDialog op = new FolderBrowserDialog();
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    //Conevrting Folder to zip file to Transfer 
                    folderPath = folderBrowserDialog1.SelectedPath;
                    zippath = folderPath + ".zip";
                    ZipFile.CreateFromDirectory(folderPath, zippath, CompressionLevel.Fastest, true);
                    txtFile.Text = zippath;
                    n = zippath.Split(splitter);
                    shortFileName = n.Last();
                }
            }
            else
            {
                op = new OpenFileDialog();
                op.ShowDialog();
                txtFile.Text = op.FileName;
                shortFileName = op.SafeFileName;
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (txtIPAddress.Text == "")
            {
                MessageBox.Show("Enter IP Address of Reciver.");
                txtIPAddress.Focus();
            }
            else if (txtFile.Text == "")
            {
                MessageBox.Show("Choose File/Folder from system.");
                txtFile.Focus();
            }
            else
            {
                //Sending file to given IP Address
                lblstatus.Text = "Connected to Server....";
                byte[] fileNameByte = Encoding.ASCII.GetBytes(shortFileName);
                byte[] fileData = File.ReadAllBytes(txtFile.Text);
                byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];
                byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);
                fileNameLen.CopyTo(clientData, 0);
                fileNameByte.CopyTo(clientData, 4);
                fileData.CopyTo(clientData, 4 + fileNameByte.Length);
                TcpClient clientSocket = new TcpClient(txtIPAddress.Text, 8080);
                NetworkStream networkStream = clientSocket.GetStream();
                lblstatus.Text = "Transfering Data....";
                networkStream.Write(clientData, 0, clientData.GetLength(0));
                networkStream.Close();
                if (zippath != "")
                {
                    File.Delete(zippath);
                    zippath = "";
                }
                lblstatus.Text = "Data Tansfer Done....";
                Clear();
            }
        }
        public void Clear()
        {
            //Clearing textboxes
            CheckFolder.Checked = false;
            txtFile.Text = "";
      
        }
        //Deleteing Zip file after using 
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if( zippath != "" )
            {
                File.Delete(zippath);
            }
        }
    }
}
