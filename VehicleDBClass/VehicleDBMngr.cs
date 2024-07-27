using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;


namespace VehicleDBClass
{
    public class VehicleDBMngr
    {
        static string ConnectionString;
        static public SqlConnection conn = new SqlConnection();
        static List<string> data = new List<string>();
        static public SqlCommand cmd;
        static public object locobj = new object();
        public static void InitializeDB()
        {
            lock (locobj)
            {
                ////ConnectionString = 
                //ConnectionString = System.Configuration.ConfigurationSettings.AppSettings["ConnectionString"];
                //ConnectionString = System.AppDomain.CurrentDomain.BaseDirectory + "GPSTrackingVehicleData.mdf";
                ////conn.ConnectionString = ConnectionString;
                //conn.ConnectionString = "Data Source=.\\SQLEXPRESS;AttachDbFilename=" + ConnectionString + ";Integrated Security=True;User Instance=True";
                //["ConnectionString"].ToString();
                //conn.ConnectionString = @"Data Source=122.175.32.16\SQLEXPRESS;Initial Catalog=GPSTrackingVehicleData;Integrated Security=True";
                //conn.ConnectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=C:\ASnTech's projects\Sagar\My Works\VehicleTracking 0.0.7\VehicleTracking\VehicleDB.mdf;Integrated Security=True;User Instance=True";
                //conn.ConnectionString = @" Data Source=.\SQLEXPRESS;AttachDbFilename=C:\ASnTech's projects\Sagar\My Works\VehicleTracking 0.0.9\VehicleTracking\VehicleDB.mdf;Integrated Security=True;User Instance=True";
                //conn.ConnectionString = @"Data Source=67.225.191.182;Initial Catalog=vehicle_tracking;Persist Security Info=True;User ID=asntech;Password=@sntec#";
                //conn.ConnectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=C:\ASnTech's projects\GPSGPRS Server - 310 & 380 v12\GPRSGPSServer\GPSTrackingVehicleData.mdf;Integrated Security=True;User Instance=True";
                //conn.ConnectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=C:\ASnTech Projects\GPS VT Server\GPSGPRS Server - 310 & 380 v19 for testing all deices\GPRSGPSServer\GPSTrackingVehicleData.mdf;Integrated Security=True;User Instance=True";
                //conn.ConnectionString = @"Data Source=122.175.32.16\SQLEXPRESS;Initial Catalog=vehicle_tracking;User ID=asntech;Password=9849992010";
                //conn.ConnectionString = @"Data Source=122.175.32.16\SQLEXPRESS;Initial Catalog=vehicle_tracking;User ID=asntech;Password=9849992010";
                //conn.ConnectionString = @" Data Source=208.91.198.174;User ID=vehicletracking_asntech;Password=9849992010";
                conn.ConnectionString = @" Data Source=103.1.173.166\SQLEXPRESS;Initial Catalog=vehicle_tracking;User ID=vehicletracking_asntech;Password=asntech@12345";
                //Data Source=208.91.198.174;User ID=vehicletracking_asntech;Password=
            }
        }
        public static bool checkDBConn()
        {
            lock (locobj)
        {
            try
            {
                conn.Open();
                conn.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            }
        }

        public static DataSet SelectQuery(SqlCommand _cmd)
        {
            lock (locobj)
            {
                try
                {
                    DataSet ds = new DataSet();
                    cmd = _cmd;
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter sda = new SqlDataAdapter();
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
        public static void InitializeASnTechDB()
        {
            lock (locobj)
            {
                ////ConnectionString = 
                //ConnectionString = System.Configuration.ConfigurationSettings.AppSettings["ConnectionString"];
                //ConnectionString = System.AppDomain.CurrentDomain.BaseDirectory + "VehicleDB.mdf";
                //conn.ConnectionString = "Data Source=.\\SQLEXPRESS;AttachDbFilename=" + ConnectionString + ";Integrated Security=True;User Instance=True";
                //["ConnectionString"].ToString();
                //conn.ConnectionString = @"Data Source=122.175.32.16\SQLEXPRESS;Initial Catalog=GPSTrackingVehicleData;Integrated Security=True";
                //conn.ConnectionString = @"Data Source=67.227.237.47;Initial Catalog=vehicle_tracking;Persist Security Info=True;User ID=asntech;Password=@sntec#";
                //conn.ConnectionString = @"Data Source=67.227.237.47;Initial Catalog=vehicle_tracking;User ID=sagar;Password=asntech";
                //conn.ConnectionString = @" Data Source=208.91.198.174;User ID=vehicletracking_asntech;Password=9849992010";
                conn.ConnectionString = @" Data Source=103.1.173.166\SQLEXPRESS;Initial Catalog=vehicle_tracking;User ID=vehicletracking_asntech;Password=asntech@12345";
            }
        }
        public static bool Insert(string TableName,string[] fields,string[] Values)
        {
            lock (locobj)
            {
                string formFields = "";
                string formFieldsNames = "";
                cmd = new SqlCommand();
                SqlParameter param;
                try
                {
                    if (fields.Count() == Values.Count())
                    {
                        for (int i = 0; i < fields.Count(); i++)
                        {
                            formFields += fields[i] + ",";
                            formFieldsNames += fields[i].Split('@')[1] + ",";
                            param = new SqlParameter(fields[i], Values[i]);
                            cmd.Parameters.Add(param);
                        }
                        formFields = formFields.Substring(0, formFields.LastIndexOf(","));
                        formFieldsNames = formFieldsNames.Substring(0, formFieldsNames.LastIndexOf(","));
                        cmd.CommandText = "insert into " + TableName + "(" + formFieldsNames + ") values (" + formFields + ")";
                        cmd.Connection = conn;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                    return false;
                }
                catch (SqlException exe)
                {
                    conn.Close();
                    throw new ApplicationException(exe.ErrorCode.ToString());
                }
            }
        }

        public static bool Insert(string TableName, string[] fields, string[] Values,DateTime[] DValues)
        {
            lock (locobj)
            {
                string formFields = "";
                string formFieldsNames = "";
                cmd = new SqlCommand();
                SqlParameter param;
                try
                {
                    if (fields.Count() == Values.Count() + DValues.Count())
                    {
                        for (int i = 0; i < fields.Count(); i++)
                        {
                            formFields += fields[i] + ",";
                            formFieldsNames += fields[i].Split('@')[1] + ",";
                            if (i < Values.Length)
                                param = new SqlParameter(fields[i], Values[i]);
                            else
                                param = new SqlParameter(fields[i], DValues[i - Values.Length]);

                            cmd.Parameters.Add(param);
                        }
                        formFields = formFields.Substring(0, formFields.LastIndexOf(","));
                        formFieldsNames = formFieldsNames.Substring(0, formFieldsNames.LastIndexOf(","));
                        cmd.CommandText = "insert into " + TableName + "(" + formFieldsNames + ") values (" + formFields + ")";
                        cmd.Connection = conn;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                    return false;
                }
                catch (Exception exe)
                {
                    conn.Close();
                    throw new ApplicationException(exe.Message);
                }
            }
        }

        public static bool Insert(SqlCommand _cmd)
        {
            lock (locobj)
            {

                try
                {
                    cmd = _cmd;
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();

                }
                catch
                {
                    conn.Close();
                    return false;
                }
                return true;
            }
        }
        //public static List<string> SelectQuery(string table, string feildstogetdata)
        //{
        //    SqlDataReader dr = null;
        //    try
        //    {
        //        data.Clear();
        //        int count = 0;
        //        string frameString = "";
        //        cmd = new SqlCommand("select " + feildstogetdata + " from " + table, conn);
        //        conn.Open();
        //        dr = cmd.ExecuteReader();

        //        while (dr.Read())
        //        {

        //            count = dr.FieldCount;
        //            for (int i = 0; i < count; i++)
        //            {
        //                frameString += dr[i].ToString() + ",";
        //            }
        //            data.Add(frameString);
        //            frameString = "";
        //        }
        //        conn.Close();
        //        dr.Close();
        //    }
        //    catch (Exception exe)
        //    {
        //        if (dr != null)
        //            dr.Close();
        //        conn.Close();
        //        throw new ApplicationException(exe.Message);
        //    }
        //    return data;
        //}

        public static DataSet SelectQuery(string table, string feildstogetdata)
        {
            lock (locobj)
            {
                try
        {
            DataSet ds = new DataSet();
            cmd = new SqlCommand("select " + feildstogetdata + " from " + table, conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            SqlDataAdapter sda = new SqlDataAdapter();
            sda.SelectCommand = cmd;
            sda.Fill(ds, "Table");
            cmd.Connection.Close();
            return ds;
        }
                catch (Exception ex)
                {
                    cmd.Connection.Close();
                    throw new ApplicationException(ex.Message);
                }
            }
        }

        public static List<string> SelectQuery(string table, string feildstogetdata, string condition)
        {
            lock (locobj)
        {
            SqlDataReader dr = null;
            try
            {
                data.Clear();
                int count = 0;
                string frameString = "";
                cmd = new SqlCommand("select " + feildstogetdata + " from " + table + " where " + condition, conn);
                conn.Open();
                dr = cmd.ExecuteReader();

                while (dr.Read())
                {

                        count = dr.FieldCount;
                        for (int i = 0; i < count; i++)
                        {
                            frameString += dr[i].ToString() + ",";
                        }
                        data.Add(frameString);
                        frameString = "";
                    }
                    conn.Close();
                    dr.Close();
                }
                catch (Exception exe)
                {
                    if (dr != null)
                        dr.Close();
                    conn.Close();
                    throw new ApplicationException(exe.Message);
                }
                return data;
            }
        }

        public static DataSet SelectQuery(string table, string columnNames, string[] conFieldNames,string[] ConFieldValues,string[] Operators)
        {
            lock (locobj)
            {
                try
                {
                    string ConditionStr = "";
                    cmd = new SqlCommand();
                    SqlParameter param;
                    for (int j = 0; j < conFieldNames.Count(); j++)
                    {
                        ConditionStr += conFieldNames[j] + Operators[j];
                        param = new SqlParameter(conFieldNames[j].Split('=')[1], ConFieldValues[j]);
                        cmd.Parameters.Add(param);
                    }
                    DataSet ds = new DataSet();
                    //SqlDataAdapter sda = new SqlDataAdapter("select " + columnNames + " from " + table + " where " + condition, conn);
                    //sda.
                    cmd.CommandText = "select " + columnNames + " from " + table + " where " + ConditionStr;
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter sda = new SqlDataAdapter();
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

        public static DataSet SelectQuery(string table, string columnNames, string[] conFieldNames, string[] ConFieldValues, DateTime[] conDateValues, string[] Operators)
        {
            lock (locobj)
            {
                try
                {
                    string ConditionStr = "";
                    cmd = new SqlCommand();
                    SqlParameter param;
                    for (int j = 0; j < conFieldNames.Count(); j++)
                    {
                        ConditionStr += conFieldNames[j] + Operators[j];
                        param = new SqlParameter(conFieldNames[j].Split('=')[1], ConFieldValues[j]);
                        cmd.Parameters.Add(param);
                    }
                    int conCount = conFieldNames.Count();
                    int confieldvaluecount = ConFieldValues.Count();
                    for (int j = 0; j < conCount; j++)
                    {
                        ConditionStr += conFieldNames[j] + " " + Operators[j] + " ";
                        if (j < confieldvaluecount)
                            param = new SqlParameter(conFieldNames[j].Split('=')[1], ConFieldValues[j]);
                        else
                            param = new SqlParameter(conFieldNames[j].Split('=')[1], conDateValues[j - confieldvaluecount]);

                        cmd.Parameters.Add(param);
                    }

                    DataSet ds = new DataSet();
                    //SqlDataAdapter sda = new SqlDataAdapter("select " + columnNames + " from " + table + " where " + condition, conn);
                    //sda.
                    cmd.CommandText = "select " + columnNames + " from " + table + " where " + ConditionStr;
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    SqlDataAdapter sda = new SqlDataAdapter();
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
        public static bool insertVehicleData(string TableName, string feildsseparatedbycomma)
        {
            lock (locobj)
            {
                try
                {
                    cmd = new SqlCommand("insert into " + TableName + " values(" + feildsseparatedbycomma + ")", conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (SqlException exe)
                {
                    conn.Close();
                    throw new ApplicationException(exe.ErrorCode.ToString());
                }
                return true;
            }
        }

        public static bool Update(string table, string Updatestring, string condition)
        {
            lock (locobj)
            {
            bool flag = false;
            try
            {
                cmd = new SqlCommand("update " + table + " set " + Updatestring + " where " + condition, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                flag = true;
            }
            catch (Exception ex)
            {
                flag = false;
                conn.Close();
                throw new AccessViolationException(ex.Message);
            }
            return flag;
            }

        }

        public static bool Update(string table, string[] fieldNames, string[] fieldValues, string[] conFieldNames, string[] conFieldValues, string[] Operators)
        {
            lock (locobj)
        {
            string UpdateString = "";
            string ConditionStr = "";
            cmd = new SqlCommand();
            SqlParameter param;
            try
            {
                if (fieldNames.Count() == fieldValues.Count()&&conFieldNames.Count()==conFieldValues.Count())
                {
                    for (int i = 0; i < fieldNames.Count(); i++)
                    {
                        UpdateString += fieldNames[i] + ", ";
                        param = new SqlParameter(fieldNames[i].Split('=')[1], fieldValues[i]);
                        cmd.Parameters.Add(param);
                    }
                    UpdateString = UpdateString.Substring(0, UpdateString.LastIndexOf(","));

                    for (int j = 0; j < conFieldNames.Count(); j++)
                    {
                        ConditionStr += conFieldNames[j] +" "+ Operators[j]+" ";
                        param = new SqlParameter(conFieldNames[j].Split('=')[1], conFieldValues[j]);
                        cmd.Parameters.Add(param);
                    }

                        cmd.CommandText = "update " + table + " set " + UpdateString + " where " + ConditionStr;
                        cmd.Connection = conn;
                        conn.Open();
                       int aaa= cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                    return false;
                }
                catch (SqlException exe)
                {
                    conn.Close();
                    throw new ApplicationException(exe.ErrorCode.ToString());
                }
            }
        }

        public static bool Update(string table, string[] fieldNames, string[] fieldValues,DateTime[] DateValues, string[] conFieldNames, string[] conFieldValues,DateTime[] conDateValues, string[] Operators)
        {
            lock (locobj)
        {
            string UpdateString = "";
            string ConditionStr = "";
            cmd = new SqlCommand();
            SqlParameter param;
            try
            {
                if (fieldNames.Count() == (fieldValues.Count()+ DateValues.Count())&& conFieldNames.Count() ==( conFieldValues.Count()+conDateValues.Count()))
                {
                    for (int i = 0; i < fieldNames.Count(); i++)
                    {
                        UpdateString += fieldNames[i] + ", ";
                        if (i < fieldValues.Count())
                            param = new SqlParameter(fieldNames[i].Split('=')[1], fieldValues[i]);
                        else
                            param = new SqlParameter(fieldNames[i].Split('=')[1], DateValues[i - fieldValues.Count()]);
                        cmd.Parameters.Add(param);
                    }
                    UpdateString = UpdateString.Substring(0, UpdateString.LastIndexOf(","));
                    int conCount = conFieldNames.Count();
                    int confieldvaluecount = conFieldValues.Count();
                    for (int j = 0; j < conCount; j++)
                    {
                        ConditionStr += conFieldNames[j] + " " + Operators[j] + " ";
                        if(j<confieldvaluecount)
                        param = new SqlParameter(conFieldNames[j].Split('=')[1], conFieldValues[j]);
                        else
                            param = new SqlParameter(conFieldNames[j].Split('=')[1], conDateValues[j - confieldvaluecount]);

                        cmd.Parameters.Add(param);
                    }

                    cmd.CommandText = "update " + table + " set " + UpdateString + " where " + ConditionStr;
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return true;
                }
                return false;
            }
            catch (Exception exe)
            {
                conn.Close();
                throw new ApplicationException(exe.ToString());
            }
            }
        }

        public static bool Update(SqlCommand cmd)
        {
            lock (locobj)
            {
                try
                {
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception exe)
                {
                    conn.Close();
                    throw new ApplicationException(exe.ToString());
                }
                return true;
            }
        }

        public static bool Update(string table, string[] fieldNames, string[] fieldValues, DateTime[] DateValues, string[] conFieldNames, string[] conFieldValues,  string[] Operators)
        {
            lock (locobj)
        {
            string UpdateString = "";
            string ConditionStr = "";
            cmd = new SqlCommand();
            SqlParameter param;
            try
            {
                if (fieldNames.Count() == (fieldValues.Count() + DateValues.Count()) && conFieldNames.Count() == (conFieldValues.Count()))
                {
                    for (int i = 0; i < fieldNames.Count(); i++)
                    {
                        UpdateString += fieldNames[i] + ", ";
                        if (i < fieldValues.Count())
                            param = new SqlParameter(fieldNames[i].Split('=')[1], fieldValues[i]);
                        else
                            param = new SqlParameter(fieldNames[i].Split('=')[1], DateValues[i - fieldValues.Count()]);
                        cmd.Parameters.Add(param);
                    }
                    UpdateString = UpdateString.Substring(0, UpdateString.LastIndexOf(","));
                    int conCount = conFieldNames.Count();
                    int confieldvaluecount = conFieldValues.Count();
                    for (int j = 0; j < conCount; j++)
                    {
                        ConditionStr += conFieldNames[j] + " " + Operators[j] + " ";
                        param = new SqlParameter(conFieldNames[j].Split('=')[1], conFieldValues[j]);
                        cmd.Parameters.Add(param);
                    }

                        cmd.CommandText = "update " + table + " set " + UpdateString + " where " + ConditionStr;
                        cmd.Connection = conn;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                    return false;
                }
                catch (Exception exe)
                {
                    conn.Close();
                    throw new ApplicationException(exe.ToString());
                }
            }
        }

        public static bool Delete(string table, string condition)
        {
            lock (locobj)
        {
            bool flag = false;
            try
            {
                cmd = new SqlCommand("delete from " + table + " where " + condition, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                flag = true;

                }
                catch (Exception ex)
                {
                    flag = false;
                    conn.Close();
                    throw new ApplicationException(ex.Message);
                }
                return flag;
            }
        }

        public static bool Delete(SqlCommand cmd)
        {
            lock (locobj)
            {
                bool flag = false;
                try
                {
                    cmd.Connection = conn;
             conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    flag = true;

                }
                catch (Exception ex)
                {
                    flag = false;
                    conn.Close();
                    throw new ApplicationException(ex.Message);
                }
                return flag;
            }
        }
        public static DataSet GetReports(string table, string columnNames, string condition)
        {
            lock (locobj)
            {
                try
                {
                    DataSet ds = new DataSet();
                    SqlDataAdapter sda = new SqlDataAdapter("select " + columnNames + " from " + table + " where " + condition, conn);
                    conn.Open();
                    sda.Fill(ds, table);
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

        
        public static DataSet GetReports(string table, string columnNames)
        {
            lock (locobj)
            {
                try
                {
                    DataSet ds = new DataSet();
                    cmd = new SqlCommand("select " + columnNames + " from " + table, conn);
                    //SqlDataAdapter sda = new SqlDataAdapter("select " + columnNames + " from " + table , conn);
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                        ds = DataReaderToDataSet(dr);
                    //sda.Fill(ds, table);
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

        ///    <summary>
        ///    Converts a SqlDataReader to a DataSet
        ///    <param name='reader'>
        /// SqlDataReader to convert.</param>
        ///    <returns>
        /// DataSet filled with the contents of the reader.</returns>
        ///    </summary>
        public static DataSet DataReaderToDataSet(SqlDataReader reader)
        {
            DataSet dataSet = new DataSet();
            do
            {
                // Create new data table

                DataTable schemaTable = reader.GetSchemaTable();
                DataTable dataTable = new DataTable();

                if (schemaTable != null)
                {
                    // A query returning records was executed

                    for (int i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        DataRow dataRow = schemaTable.Rows[i];
                        // Create a column name that is unique in the data table
                        string columnName = (string)dataRow["ColumnName"]; //+ "<C" + i + "/>";
                        // Add the column definition to the data table
                        DataColumn column = new DataColumn(columnName, (Type)dataRow["DataType"]);
                        dataTable.Columns.Add(column);
                    }

                    dataSet.Tables.Add(dataTable);

                    // Fill the data table we just created

                    while (reader.Read())
                    {
                        DataRow dataRow = dataTable.NewRow();

                        for (int i = 0; i < reader.FieldCount; i++)
                            dataRow[i] = reader.GetValue(i);

                        dataTable.Rows.Add(dataRow);
                    }
                }
                else
                {
                    // No records were returned

                    DataColumn column = new DataColumn("RowsAffected");
                    dataTable.Columns.Add(column);
                    dataSet.Tables.Add(dataTable);
                    DataRow dataRow = dataTable.NewRow();
                    dataRow[0] = reader.RecordsAffected;
                    dataTable.Rows.Add(dataRow);
                }
            }
            while (reader.NextResult());
            return dataSet;
        }



    }
}
