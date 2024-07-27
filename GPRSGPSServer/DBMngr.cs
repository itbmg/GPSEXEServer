using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using MySql.Data.MySqlClient;
namespace GPRSGPSServer
{
    public class DBMgr
    {
        string ConnectionString;
        public MySqlConnection conn = new MySqlConnection();
        List<string> data = new List<string>();
        //  public string UserName = "";
        object obj_lock = new object();

        public DBMgr()
        {
           // conn.ConnectionString = @" SERVER=49.50.65.160;DATABASE=vyshnavi_newgps;UID=root;PASSWORD=Vyshnavi@123;";
           // conn.ConnectionString = @"SERVER=49.50.79.13;DATABASE=vyshnavi_newgps;UID=vyshnavi_root;PASSWORD=Vyshnavi@123;";
            conn.ConnectionString = @"SERVER=182.18.162.51;DATABASE=vyshnavi_newgps;UID=admin;PASSWORD=CK!uM$Btz2#4c$;";
        }



        public bool insert(MySqlCommand _cmd)
        {
            lock (conn)
            {
                MySqlCommand cmd = _cmd;
                try
                {

                    cmd.Connection = conn;
                    if (cmd.Connection.State == ConnectionState.Open)
                        cmd.Connection.Close();
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();

                    return true;
                }
                catch (Exception ex)
                {
                    cmd.Connection.Close();
                    throw new ApplicationException(ex.Message);
                }
            }
            //}
        }

        public long insertScalar(MySqlCommand _cmd)
        {
            long sno = 0;
            MySqlCommand cmd = _cmd;
            try
            {
                cmd = _cmd;
              
                    cmd.Connection = conn;
                    if (cmd.Connection.State == ConnectionState.Open)
                        cmd.Connection.Close();
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    sno = cmd.LastInsertedId;
                    cmd.Connection.Close();

                return sno;
            }
            catch (Exception ex)
            {
                cmd.Connection.Close();
                throw new ApplicationException(ex.Message);
            }
            //}
        }

        public DataSet SelectQuery(MySqlCommand _cmd)
        {
            lock (conn)
            {
                MySqlCommand cmd = _cmd;


              
                    try
                    {
                        DataSet ds = new DataSet();
                        cmd.Connection = conn;
                        conn.Open();
                        //cmd.ExecuteNonQuery();
                        MySqlDataAdapter sda = new MySqlDataAdapter();
                        sda.SelectCommand = cmd;
                        sda.Fill(ds, "Table");
                        conn.Close();
                        return ds;
                    }
                    catch (Exception ex)
                    {
                        conn.Close();
                        throw new ApplicationException(ex.Message);
                    }
              
            }
        }

        public int Update(MySqlCommand _cmd)
        {
            MySqlCommand cmd = _cmd;
            lock (conn)
            {
                try
                {
                    int i = 0;
                   
                        cmd.Connection = conn;
                        if (cmd.Connection.State == ConnectionState.Open)
                            cmd.Connection.Close();
                        cmd.Connection.Open();
                        i = cmd.ExecuteNonQuery();
                        cmd.Connection.Close();
                        return i;
                }
                catch (Exception ex)
                {
                    cmd.Connection.Close();
                    throw new ApplicationException(ex.Message);
                }
            }
        }

        public void Delete(MySqlCommand _cmd)
        {
            lock (conn)
            {
                MySqlCommand cmd = _cmd;
                try
                {
                    cmd = _cmd;
                    cmd.Connection = conn;
                    if (cmd.Connection.State == ConnectionState.Open)
                        cmd.Connection.Close();
                    cmd.Connection.Open();
                    int ii = cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                catch (Exception ex)
                {
                    cmd.Connection.Close();
                    throw new ApplicationException(ex.Message);
                }
            }
        }
        public static DateTime GetTime(MySqlConnection conn)
        {

            DataSet ds = new DataSet();
            DateTime dt = DateTime.Now;
            MySqlCommand cmd = new MySqlCommand("SELECT NOW()");
            cmd.Connection = conn;
            if (cmd.Connection.State == ConnectionState.Open)
            {
                cmd.Connection.Close();
            }
            conn.Open();
            //cmd.ExecuteNonQuery();
            MySqlDataAdapter sda = new MySqlDataAdapter();
            sda.SelectCommand = cmd;
            sda.Fill(ds, "Table");
            conn.Close();
            if (ds.Tables[0].Rows.Count > 0)
            {
                dt = (DateTime)ds.Tables[0].Rows[0][0];
            }
            return dt;

        }

        internal bool checkDBConn()
        {
            throw new NotImplementedException();
        }
    }
}