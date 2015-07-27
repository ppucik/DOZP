using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Text;

namespace Comdat.DOZP.Core
{
    /// <summary>
    /// A datareader for character delimited files (eg. CSV) with line breaks separating records.
    /// Caters for 1st row having column headings or column headings array supplied.
    /// Can take an array of System.Type objects to specify column types.
    /// </summary>
    public class CharDelimitedDataReader : StreamDataReader
    {
        private char[] _delimiter;
        private bool _hasColumnHeadings;
        private string[] _columnHeadings;
        private Type[] _columnTypes = null;

        public CharDelimitedDataReader(StreamReader streamReader, char[] delimiter, bool hasColumnHeadings)
            : base(streamReader)
        {
            _delimiter = delimiter;
            _hasColumnHeadings = hasColumnHeadings;

            if (_hasColumnHeadings)
            {
                string heading = StreamReader.ReadLine();
                _columnHeadings = heading.Split(_delimiter);
            }
        }

        public CharDelimitedDataReader(StreamReader streamReader, char[] delimiter, bool hasColumnHeadings, Type[] columnTypes)
            : base(streamReader)
        {
            _delimiter = delimiter;
            _hasColumnHeadings = hasColumnHeadings;
            _columnTypes = columnTypes;

            if (_hasColumnHeadings)
            {
                string heading = StreamReader.ReadLine();
                _columnHeadings = heading.Split(_delimiter);
            }
        }

        public CharDelimitedDataReader(StreamReader streamReader, char[] delimiter, string[] columnHeadings, Type[] columnTypes)
            : base(streamReader)
        {
            _delimiter = delimiter;
            _hasColumnHeadings = true;
            _columnTypes = columnTypes;
            _columnHeadings = columnHeadings;
        }

        public override IDataRecord GetNextDataRecord()
        {
            string record = StreamReader.ReadLine();
            if (record == null) return null;
            string[] columnValues = record.Split(_delimiter);

            //Determine if we should create some auto-headings
            if (this.RecordsAffected == -1)
            {
                if (!_hasColumnHeadings)
                {
                    _columnHeadings = new string[columnValues.Length];
                    for (int k = 0; k < columnValues.Length; k++)
                    {
                        _columnHeadings[k] = "Column" + k.ToString();
                    }
                }

                if (_columnTypes == null)
                {
                    _columnTypes = new Type[columnValues.Length];
                    Type stringType = typeof(String);
                    for (int k = 0; k < columnValues.Length; k++)
                    {
                        _columnTypes[k] = stringType;
                    }
                }
            }

            return new CharacterDelimitedDataRecord(columnValues, _columnHeadings, _columnTypes, StreamReader.CurrentEncoding);
        }

        public class CharacterDelimitedDataRecord : IDataRecord
        {
            private string[] _columnValues;
            private string[] _columnNames;
            private Type[] _columnTypes;
            private ArrayList _nameArray = null;
            private Encoding _encoding;

            public CharacterDelimitedDataRecord(string[] columnValues, string[] columnNames, Type[] columnTypes, Encoding sourceEncoding)
            {
                _columnValues = columnValues;
                _columnNames = columnNames;
                _columnTypes = columnTypes;
                _encoding = sourceEncoding;
            }

            #region IDataRecord Members

            public int GetInt32(int i)
            {
                return Convert.ToInt32(_columnValues[i]);
            }

            public object this[string name]
            {
                get
                {
                    if (_nameArray == null)
                        _nameArray = new ArrayList(_columnNames);

                    return _columnValues[_nameArray.IndexOf(name)];
                }
            }

            object System.Data.IDataRecord.this[int i]
            {
                get
                {
                    return _columnValues[i];
                }
            }

            public object GetValue(int i)
            {
                return _columnValues[i];
            }

            public bool IsDBNull(int i)
            {
                if (i < 0 || i > this.FieldCount - 1)
                    throw new IndexOutOfRangeException();
                return false;
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                return (int)_encoding.GetBytes(_columnValues[i], (int)fieldOffset, length, buffer, bufferoffset);
            }

            public byte GetByte(int i)
            {
                return Convert.ToByte(_columnValues[i]);
            }

            public Type GetFieldType(int i)
            {
                return _columnTypes[i];
            }

            public decimal GetDecimal(int i)
            {
                return Convert.ToDecimal(_columnValues[i]);
            }

            public int GetValues(object[] values)
            {
                _columnValues.CopyTo(values, 0);
                return (values.Length > _columnValues.Length) ? _columnValues.Length : values.Length;
            }

            public string GetName(int i)
            {
                return _columnNames[i];
            }

            public int FieldCount
            {
                get
                {
                    return _columnValues.Length;
                }
            }

            public long GetInt64(int i)
            {
                return Convert.ToInt64(_columnValues[i]);
            }

            public double GetDouble(int i)
            {
                return Convert.ToDouble(_columnValues[i]);
            }

            public bool GetBoolean(int i)
            {
                return Convert.ToBoolean(_columnValues[i]);
            }

            public Guid GetGuid(int i)
            {
                return new Guid(_columnValues[i]);
            }

            public DateTime GetDateTime(int i)
            {
                return Convert.ToDateTime(_columnValues[i]);
            }

            public int GetOrdinal(string name)
            {
                if (_nameArray == null)
                    _nameArray = new ArrayList(_columnNames);

                return _nameArray.IndexOf(name);
            }

            public string GetDataTypeName(int i)
            {
                return this.GetFieldType(i).FullName;
            }

            public float GetFloat(int i)
            {
                return Convert.ToSingle(_columnValues[i]);
            }

            public IDataReader GetData(int i)
            {
                if (i < 0 || i > this.FieldCount - 1)
                    throw new IndexOutOfRangeException();
                return null;
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                char[] chars = _columnValues[i].ToCharArray((int)fieldoffset, length);
                chars.CopyTo(buffer, bufferoffset);

                return (chars.Length < length) ? chars.Length : length;
            }

            public string GetString(int i)
            {
                return _columnValues[i];
            }

            public char GetChar(int i)
            {
                return Convert.ToChar(_columnValues[i]);
            }

            public short GetInt16(int i)
            {
                return Convert.ToInt16(_columnValues[i]);
            }

            #endregion
        }
    }
}
