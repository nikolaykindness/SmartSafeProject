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

            string connectionString = $@"XpoProvider=Firebird;DataSource=localhost;User=;Password=;Database={projectDirectory}\PYGG.FDB;ServerType=1;Charset=NONE";

            //FbConnection connecting = new FbConnection($@"User = admin; Password = admin; Database = {projectDirectory}\SMARTSAFE.FDB");
            FbConnection connecting = new FbConnection(connStr);
            connecting.Open();
        }
    }
}
