using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdDB
{
    public class Row
    {
        Dictionary<string, object> dbfield = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public object this[string key]
        {
            get { return dbfield[key]; }
            set { dbfield[key] = value; }
        }

        public object this[int index]
        {
            get
            {
                if (index > dbfield.Keys.Count)
                    return "erreur: " + index.ToString() + " indice de liste hors des limites";

                int i = 0;
                string key = string.Empty;
                foreach (var row in dbfield)
                {
                    key = row.Key;
                    if (i == index)
                        break;
                    i++;
                }

                return dbfield[key];
            }
        }

        //retourne la valeur pour un champ donné
        public object Value(string key)
        {
            return dbfield[key];
        }

        public bool ContainsKey(string key)
        {
            if (dbfield.ContainsKey(key))
                return true;
            return false;
        }

        //modifie une cle avec une nouvelle valeur
        internal void Add(string key, object NewValue)
        {
            dbfield[key] = NewValue;
        }

        public void Update(string key, object NewValue)
        {
            if (dbfield.Keys.Contains(key)) { dbfield.Remove(key); }
            dbfield[key] = NewValue;
        }

        internal Dictionary<string, object>.KeyCollection Keys()
        {
            return dbfield.Keys;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return dbfield.GetEnumerator();
        }
    }

    public class FirebirdDB
    {
        
		private string CS = "";
        private FbTransaction fbt;
        private FbDataReader dr;
        public FbConnection db = new FbConnection();
        public FbCommand dc;
        public int RowCount;
        public string ConnexionString = string.Empty;

        public FirebirdDB(string s)
        {
            ConnexionString = s;
            Connect(ConnexionString);
        }

        private void Connect(string s)
        {
            CS = ParseConnexionString(s);
            db.ConnectionString = CS;
            db.Open();
            fbt = db.BeginTransaction();
        }

        public FbCommand Query(string Query)
        {
            RowCount = 0;
            dc = new FbCommand(Query, db, fbt);
            if (Query.Trim().Substring(0, 6).ToUpper() != "SELECT")
            {
                RowCount = dc.ExecuteNonQuery();
                CommitAndOpen();
            }
            return dc;
        }

        public int GetRowCount()
        {
            return RowCount;
        }

        public DataTable ToGrid()
        {
            FbDataAdapter da = new FbDataAdapter();
            da.SelectCommand = dc;
            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            da.Fill(table);
            return table;
        }

        public IEnumerable<Row> FetchAll()
        {
            if (dc.CommandText.Trim().Substring(0, 6).ToUpper() != "SELECT")
            {
                Row row = new Row();
                row.Add("COUNT", RowCount);
                yield return row;
            }

            dr = dc.ExecuteReader();
            while (dr.Read())
            {
                var row = new Row();
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    row.Add(dr.GetName(i).ToLower().ToString(), dr.GetValue(i));
                    RowCount++;
                }
                yield return row;
            }
            CommitAndOpen();
        }

        
        public void Close()
        {
            fbt.Commit();
            db.Close();
        }
               

        private void CommitAndOpen()
        {
            fbt.Commit();
            fbt = db.BeginTransaction();
        }

		
        private string ParseConnexionString(string s)
        {
            string New = "";
            try
            {
                string[] S = s.Split('@');

                string NewS2 = "";
                int i = 0;
                foreach (char c in S[1].ToCharArray())
                {
                    if (c != ':')
                        NewS2 += c.ToString();
                    else
                    {
                        if (i > 0)
                            NewS2 += "#";
                        else
                        {
                            NewS2 += c.ToString();
                            i++;
                        }
                    }
                }

                string[] S1 = S[0].Split(':');
                string[] S2 = NewS2.Split(':');

                New = "User=" + S1[0] + ";" +
                      "Password=" + S1[1] + ";" +
                      "Database=" + S2[1].Replace("#", ":") + ";" +
                      "DataSource=" + S2[0];
            }
            catch { }            
            return New;
        }
    }
}
