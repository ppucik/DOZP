using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Text;
using System.ComponentModel;

namespace Comdat.DOZP.Core
{
    /// <summary>
    /// A data reader read fixed length records 
    /// </summary>
    public abstract class FixedLengthDataReader : StreamDataReader
    {
        protected FixedLengthDataReader(StreamReader streamReader)
            : base(streamReader)
        { }

        /// <summary>
        /// Reads the stream and creates a FixedLengthDataRecord record.
        /// </summary>
        /// <param name="detectionCharsAlreadyRead">An array of any characters of the record that have been read in the record type detection process.</param>
        /// <param name="fullRecordLength">The entire length of the record including the detectionChars length.</param>
        /// <param name="fieldPositions">An array of starting positions for fields in the record.</param>
        /// <param name="types">An array containing the type of each field.</param>
        /// <returns>A FixedLengthDataRecord object.</returns>
        protected FixedLengthDataRecord ReadFixedLengthRecord(char[] detectionCharsAlreadyRead, int fullRecordLength, int[] fieldPositions, Type[] types, string[] columnNames)
        {
            char[] record = new char[fullRecordLength];
            //insert at the beginning
            detectionCharsAlreadyRead.CopyTo(record, 0);

            //read the rest of the record.
            int stillToRead = fullRecordLength - detectionCharsAlreadyRead.Length;
            if (stillToRead > 0)
            {
                int charsRead = StreamReader.Read(record, detectionCharsAlreadyRead.Length, stillToRead);
                if (stillToRead > charsRead)
                    if (detectionCharsAlreadyRead.Length > 0)
                        throw new EndOfStreamException("Unable to read the entire record");
                    else
                        return null;
            }
            return new FixedLengthDataRecord(record, fieldPositions, types, columnNames, StreamReader.CurrentEncoding);
        }

        /// <summary>
        /// Reads the stream and creates a FixedLengthDataRecord record.
        /// Types default to System.String.
        /// </summary>
        /// <param name="detectionCharsAlreadyRead">An array of any characters of the record that have been read in the record type detection process.</param>
        /// <param name="fullRecordLength">The entire length of the record including the detectionChars length.</param>
        /// <param name="fieldPositions">An array of starting positions for fields in the record.</param>
        /// <returns>A FixedLengthDataRecord object.</returns>
        protected FixedLengthDataRecord ReadFixedLengthRecord(char[] detectionCharsAlreadyRead, int fullRecordLength, int[] fieldPositions, string[] columnNames)
        {
            Type[] types = new Type[fieldPositions.Length];
            Type stringType = typeof(string);
            for (int k = 0; k < types.Length; k++)
            {
                types[k] = stringType;
            }

            return ReadFixedLengthRecord(detectionCharsAlreadyRead, fullRecordLength, fieldPositions, types, columnNames);
        }

        /// <summary>
        /// A basic DataRecord to handle fixed length records with fields at specific locations.
        /// Typically this class should be inherited for each record type you intend to support.
        /// </summary>
        public class FixedLengthDataRecord : IDataRecord
        {
            private object[] _values;
            private Encoding _encoding;
            private string[] _columnNames;
            private Type[] _columnTypes;
            private ArrayList _nameArray = null;

            public FixedLengthDataRecord(char[] record, int[] fieldPositions, Type[] types, string[] columnNames, Encoding sourceEncoding)
            {
                _values = new object[fieldPositions.Length];
                _columnNames = columnNames;
                _encoding = sourceEncoding;
                _columnTypes = types;

                //Step thru each field
                for (int k = 0; k < fieldPositions.Length - 1; k++)
                {
                    int fieldPosition = fieldPositions[k];
                    int fieldLength = fieldPositions[k + 1] - fieldPosition;

                    ExtractValue(k, fieldPosition, fieldLength, record);
                }
                //add the last one
                int fieldPositionLast = fieldPositions[fieldPositions.Length - 1];
                int fieldLengthLast = record.Length - fieldPositionLast;

                ExtractValue(fieldPositions.Length - 1, fieldPositionLast, fieldLengthLast, record);
            }

            private void ExtractValue(int ordinal, int position, int length, char[] record)
            {
                //Extract the value
                string fieldValue = new string(record, position, length);

                if (fieldValue.Trim().Length == 0)
                {
                    _values[ordinal] = null;
                }
                else
                {
                    //Convert the type
                    TypeConverter converter = TypeDescriptor.GetConverter(_columnTypes[ordinal]);
                    _values[ordinal] = converter.ConvertFromString(fieldValue);
                }
            }

            #region IDataRecord Members

            public int GetInt32(int i)
            {
                return Convert.ToInt32(_values[i]);
            }

            public object this[string name]
            {
                get
                {
                    if (_nameArray == null)
                        _nameArray = new ArrayList(_columnNames);

                    return _values[_nameArray.IndexOf(name)];
                }
            }

            public object this[int i]
            {
                get
                {
                    return _values[i];
                }
            }

            public object GetValue(int i)
            {
                return _values[i];
            }

            public bool IsDBNull(int i)
            {
                return _values[i] == null;
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException("");
            }

            public byte GetByte(int i)
            {
                return Convert.ToByte(_values[i]);
            }

            public Type GetFieldType(int i)
            {
                return _columnTypes[i];
            }

            public decimal GetDecimal(int i)
            {
                return Convert.ToDecimal(_values[i]);
            }

            public int GetValues(object[] values)
            {
                _values.CopyTo(values, 0);
                return (values.Length > _values.Length) ? _values.Length : values.Length;
            }

            public string GetName(int i)
            {
                return _columnNames[i];
            }

            public int FieldCount
            {
                get
                {
                    return _values.Length;
                }
            }

            public long GetInt64(int i)
            {
                return Convert.ToInt64(_values[i]);
            }

            public double GetDouble(int i)
            {
                return Convert.ToDouble(_values[i]);
            }

            public bool GetBoolean(int i)
            {
                return Convert.ToBoolean(_values[i]);
            }

            public Guid GetGuid(int i)
            {
                return new Guid(Convert.ToString(_values[i]));
            }

            public DateTime GetDateTime(int i)
            {
                return Convert.ToDateTime(_values[i]);
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
                return Convert.ToSingle(_values[i]);
            }

            public IDataReader GetData(int i)
            {
                if (i < 0 || i > this.FieldCount - 1)
                    throw new IndexOutOfRangeException();
                return null;
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException("");
            }

            public string GetString(int i)
            {
                return Convert.ToString(_values[i]);
            }

            public char GetChar(int i)
            {
                return Convert.ToChar(_values[i]);
            }

            public short GetInt16(int i)
            {
                return Convert.ToInt16(_values[i]);
            }

            #endregion
        }

        /// <summary>
        /// A marker class for implementations to inherit from.
        /// </summary>
        public abstract class AbstractFixedLengthRecordMarker : IDataRecord
        {
            private FixedLengthDataRecord _currentDataRecord;

            protected AbstractFixedLengthRecordMarker(FixedLengthDataRecord dataRecord)
            {
                _currentDataRecord = dataRecord;
            }

            #region IDataRecord Members

            public int GetInt32(int i)
            {
                return _currentDataRecord.GetInt32(i);
            }

            public virtual object this[string name]
            {
                get
                {
                    return _currentDataRecord[name];
                }
            }

            public virtual object this[int i]
            {
                get
                {
                    return _currentDataRecord[i];
                }
            }

            public object GetValue(int i)
            {
                return _currentDataRecord.GetValue(i);
            }

            /// <summary>
            /// Whether the specified field is set to null.
            /// </summary>
            /// <param name="i">The column index.</param>
            /// <returns></returns>
            public bool IsDBNull(int i)
            {
                return _currentDataRecord.IsDBNull(i);
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                return _currentDataRecord.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
            }

            public byte GetByte(int i)
            {
                return _currentDataRecord.GetByte(i);
            }

            public Type GetFieldType(int i)
            {
                return _currentDataRecord.GetFieldType(i);
            }

            public decimal GetDecimal(int i)
            {
                return _currentDataRecord.GetDecimal(i);
            }

            public int GetValues(object[] values)
            {
                return _currentDataRecord.GetValues(values);
            }

            /// <summary>
            /// Gets the name for the field.
            /// </summary>
            /// <param name="i">The field index.</param>
            /// <returns>The name of the field.</returns>
            public string GetName(int i)
            {
                return _currentDataRecord.GetName(i);
            }

            public int FieldCount
            {
                get
                {
                    return _currentDataRecord.FieldCount;
                }
            }

            public long GetInt64(int i)
            {
                return _currentDataRecord.GetInt64(i);
            }

            public double GetDouble(int i)
            {
                return _currentDataRecord.GetDouble(i);
            }

            public bool GetBoolean(int i)
            {
                return _currentDataRecord.GetBoolean(i);
            }

            public Guid GetGuid(int i)
            {
                return _currentDataRecord.GetGuid(i);
            }

            public DateTime GetDateTime(int i)
            {
                return _currentDataRecord.GetDateTime(i);
            }

            public int GetOrdinal(string name)
            {
                return _currentDataRecord.GetOrdinal(name);
            }

            public string GetDataTypeName(int i)
            {
                return _currentDataRecord.GetDataTypeName(i);
            }

            public float GetFloat(int i)
            {
                return _currentDataRecord.GetFloat(i);
            }

            public IDataReader GetData(int i)
            {
                return _currentDataRecord.GetData(i);
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                return _currentDataRecord.GetChars(i, fieldoffset, buffer, bufferoffset, length);
            }

            public string GetString(int i)
            {
                return _currentDataRecord.GetString(i);
            }

            public char GetChar(int i)
            {
                return _currentDataRecord.GetChar(i);
            }

            public short GetInt16(int i)
            {
                return _currentDataRecord.GetInt16(i);
            }

            #endregion
        }

        /// <summary>
        /// An ignorable record marker.
        /// </summary>
        public class FluffRecord : AbstractFixedLengthRecordMarker
        {
            private static string[] _columnNames = new string[] { "fluff" };
            private static int[] _columnPositions = new int[] { 0 };
            private static Type[] _columnTypes = new Type[] { typeof(System.String) };

            public FluffRecord(FixedLengthDataRecord dataRecord)
                : base(dataRecord)
            { }

            public static string[] ColumnNames
            {
                get { return _columnNames; }
            }

            public static int[] ColumnPositions
            {
                get { return _columnPositions; }
            }

            public static Type[] ColumnTypes
            {
                get { return _columnTypes; }
            }
        }
    }
}
