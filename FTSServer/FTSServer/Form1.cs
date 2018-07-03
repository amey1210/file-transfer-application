using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;

namespace FTSServer
{
    public partial class Form1 : Form
    {

        private ArrayList nSockets;
        public Form1()
        {
            InitializeComponent();
        }
        string folderPath = @"C:\";
        string extractPath = "";
        int na;
        public void listenerThread()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 8080);
            tcpListener.Start();
            while (true)
            {
                Socket handlerSocket = tcpListener.AcceptSocket();
                if (handlerSocket.Connected)
                {
                    IPEndPoint remoteIP = (IPEndPoint)handlerSocket.RemoteEndPoint;
                    string ipaddress = remoteIP.Address.ToString();
                    int port = remoteIP.Port;
                    Control.CheckForIllegalCrossThreadCalls = false;
                    DialogResult dialogResult = MessageBox.Show("Accept file from client"+ipaddress+"?", "Warning", MessageBoxButtons.YesNo);
                    if ( dialogResult == DialogResult.Yes )
                    {
                        na = DGV.Rows.Add();
                        DGV.Rows[na].Cells[0].Value = ipaddress;
                        DGV.Rows[na].Cells[1].Value = port.ToString();
                        DGV.Rows[na].Cells[2].Value = DateTime.Now.ToString("HH:mm:ss");
                        DGV.Rows[na].Cells[3].Value = "Connected";
                        lock (this)
                        {
                            nSockets.Add(handlerSocket);
                        }
                        ThreadStart thdstHandler = new
                        ThreadStart(handlerThread);
                        Thread thdHandler = new Thread(thdstHandler);
                        thdHandler.IsBackground = true;
                        thdHandler.Start();
                    }
                    if( dialogResult == DialogResult.No)
                    {

                    }
                }
            }
        }
        public void handlerThread()
        {
            string fileName = string.Empty;
            string[] n = new string[50];
            char[] splitter = { '.' };
            Socket handlerSocket = (Socket)nSockets[nSockets.Count - 1];
            NetworkStream networkStream = new NetworkStream(handlerSocket);
            int thisRead = 0;
            int blockSize = 1024;
            Byte[] dataByte = new Byte[blockSize];
            DGV.Rows[na].Cells[3].Value = "Ongoing";
            try
            {
                lock (this)
                {
                    int receivedBytesLen = handlerSocket.Receive(dataByte);
                    int fileNameLen = BitConverter.ToInt32(dataByte, 0);
                    fileName = Encoding.ASCII.GetString(dataByte, 4, fileNameLen);
                    //MessageBox.Show(folderPath + fileName);
                    DGV.Rows[na].Cells[4].Value = fileName.ToString();
                    Stream fileStream = File.OpenWrite(folderPath + fileName);
                    fileStream.Write(dataByte, 4 + fileNameLen, (1024 - (4 + fileNameLen)));
                    while (true)
                    {
                        thisRead = networkStream.Read(dataByte, 0, blockSize);
                        fileStream.Write(dataByte, 0, thisRead);
                        if (thisRead == 0)
                            break;
                    }
                    fileStream.Close();
                    n = fileName.Split(splitter);
                    if (n.Last() == "zip")
                    {
                        ZipFile.ExtractToDirectory(folderPath + fileName, extractPath);
                        File.Delete(folderPath + fileName);
                    }
                }
                DGV.Rows[na].Cells[3].Value = "Compeletd";
                handlerSocket = null;
            }catch
            {

            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            //Choosing Target Location
            FolderBrowserDialog op = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                folderPath = folderBrowserDialog1.SelectedPath;
                extractPath = folderPath;
                folderPath=folderPath+@"\";
                txtPath.Text = folderPath;
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if( txtPath.Text == "" )
            {
                MessageBox.Show("Choose Download Path");
                txtPath.Focus();
            }
            else
            {
                //Satrting sever
                Status.Text = "Enabled";
                IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
                lblStatus.Text = IPHost.AddressList[0].ToString();
                nSockets = new ArrayList();
                Thread thdListener = new Thread(new ThreadStart(listenerThread));
                thdListener.IsBackground = true;
                thdListener.Start();
            }
        }
    }
}
