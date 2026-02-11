/*
    Copyright 2015-2024 MCGalaxy
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
namespace MCGalaxy.SQL
{
    /// <summary> Abstracts iterating over the results from executing a SQL command </summary>
    public class ISqlReader : ISqlRecord, IDisposable
    {
        SQLiteCommand _command;
        SQLiteStatement stmt;
        int readState, columns;
        public int rowsAffected;
        string[] fieldNames;
        internal ISqlReader(SQLiteCommand cmd) => _command = cmd;
        public void Dispose() => Close();
        public void Close()
        {
            _command = null;
            stmt = null;
            fieldNames = null;
        }
        void CheckClosed()
        {
            if (_command == null)
                throw new InvalidOperationException("DataReader has been closed");
            SQLiteConnection.Check(_command.conn);
        }
        void VerifyForGet()
        {
            CheckClosed();
            if (readState != 0) throw new InvalidOperationException("No current row");
        }
        TypeAffinity GetAffinity(int i)
        {
            VerifyForGet();
            return stmt.ColumnAffinity(i);
        }
        public override bool GetBoolean(int i) => GetInt32(i) != 0;
        public override byte[] GetBytes(int i) => GetAffinity(i) == TypeAffinity.Blob ? stmt.GetBytes(i) : throw new InvalidCastException();
        public override DateTime GetDateTime(int i) => GetAffinity(i) == TypeAffinity.Text ? stmt.GetDateTime(i) : throw new NotSupportedException();
        public override double GetDouble(int i) => GetAffinity(i) == TypeAffinity.Int64 || GetAffinity(i) == TypeAffinity.Double ? stmt.GetDouble(i) : throw new NotSupportedException();
        public override int GetInt32(int i) => GetAffinity(i) == TypeAffinity.Int64 ? stmt.GetInt32(i) : throw new InvalidCastException();
        public override long GetInt64(int i) => GetAffinity(i) == TypeAffinity.Int64 ? stmt.GetInt64(i) : throw new InvalidCastException();
        public override string GetString(int i) => stmt.GetText(i);
        public override bool IsDBNull(int i) => GetAffinity(i) == TypeAffinity.Null;
        public override object GetValue(int i) => stmt.GetValue(i, GetAffinity(i));
        public override string GetStringValue(int col) => GetString(col);
        public override string DumpValue(int col) => GetAffinity(col) == TypeAffinity.Null
                ? "NULL"
                : GetAffinity(col) == TypeAffinity.Text || GetAffinity(col) == TypeAffinity.Blob
                ? SqlUtils.QuoteString(GetString(col))
                : GetString(col);
        public override string GetName(int i) => stmt.ColumnName(i);
        public override int GetOrdinal(string name)
        {
            VerifyForGet();
            fieldNames ??= new string[columns];
            for (int i = 0; i < columns; i++)
            {
                string field = fieldNames[i];
                if (field == null)
                {
                    field = stmt.ColumnName(i);
                    fieldNames[i] = field;
                }
                if (name.Equals(field, StringComparison.OrdinalIgnoreCase)) return i;
            }
            return -1;
        }
        public override int FieldCount => columns;
        public bool NextResult()
        {
            CheckClosed();
            while (true)
            {
                stmt = _command.NextStatement();
                readState = 1;
                if (stmt == null) return false;
                columns = stmt.ColumnCount();
                if (stmt.Step())
                {
                    readState = -1;
                }
                else if (columns == 0)
                {
                    rowsAffected += stmt.conn.Changes;
                    continue;
                }
                else
                {
                }
                fieldNames = null;
                return true;
            }
        }
        public bool Read()
        {
            CheckClosed();
            if (readState == -1)
            {
                readState = 0; return true;
            }
            else if (readState == 0)
            { 
                if (stmt.Step()) return true;
                readState = 1;
            }
            return false;
        }
    }
    public abstract class ISqlRecord
    {
        public abstract int FieldCount { get; }
        public abstract string GetName(int i);
        public abstract int GetOrdinal(string name);
        public abstract byte[] GetBytes(int i);
        public abstract bool GetBoolean(int i);
        public abstract int GetInt32(int i);
        public abstract long GetInt64(int i);
        public abstract double GetDouble(int i);
        public abstract string GetString(int i);
        public abstract DateTime GetDateTime(int i);
        public abstract bool IsDBNull(int i);
        public abstract object GetValue(int i);
        public abstract string GetStringValue(int col);
        public abstract string DumpValue(int col);
        public string GetText(int col) => IsDBNull(col) ? "" : GetString(col);
        public string GetText(string name) => IsDBNull(GetOrdinal(name)) ? "" : GetString(GetOrdinal(name));
        public int GetInt(string name) => IsDBNull(GetOrdinal(name)) ? 0 : GetInt32(GetOrdinal(name));
        public long GetLong(string name) => IsDBNull(GetOrdinal(name)) ? 0 : GetInt64(GetOrdinal(name));
    }
}
