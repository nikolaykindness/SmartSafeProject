using System.IO;
using System;
using System.Windows;
using FirebirdSql.Data.FirebirdClient;

namespace SmartSafeProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            FbConnectionStringBuilder bld = new FbConnectionStringBuilder();
            //bld.Charset = "NONE";
            //bld.DataSource = "localhost";
            bld.Database = $@"{projectDirectory}\SMARTSAFE.FDB";
            bld.UserID = "";
            bld.Password = "";
            bld.WireCrypt = FbWireCrypt.Enabled;
            string connStr = bld.ConnectionString;

            string connectionString = $@"User=sysdba;Password=masterkey;Database={projectDirectory}\SMARTSAFE.FDB;DataSource=127.0.0.1;Port=3050;Dialect=3;Charset=NONE;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;";

            //FbConnection connecting = new FbConnection($@"User = admin; Password = admin; Database = {projectDirectory}\SMARTSAFE.FDB");
            FbConnection connecting = new FbConnection(connectionString);
            connecting.Open();
        }
    }
}
