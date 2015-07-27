using System;
using System.IO;
using System.Data;

namespace Comdat.DOZP.Core
{
    /// <summary>
    /// A DataReader for streams, allowing for multiple types of records in a single file. 
    /// </summary>
    /// <remarks>Derived classes are responsible for implementing the IDataRecord objects, and for detecting which record to use.</remarks>
    public abstract class StreamDataReader : IDataReader
    {
        private StreamReader _streamReader;
        private IDataRecord _currentDataRecord;
        private int _recordsAffected = -1;

        protected StreamDataReader(StreamReader streamReader)
        {
            _streamReader = streamReader;
            _currentDataRecord = null;
        }

        /// <summary>
        /// Implementations must determine the type of record and create an appropriate IDataRecord to get values from the record.
        /// The implementation must advance the streamReader to the end of the record, in preparation for the next read.
        /// </summary>
        /// <param name="streamReader"></param>
        /// <returns></returns>
        public abstract IDataRecord GetNextDataRecord();

        /// <summary>
        /// Returns the type of the current IDataRecord.
        /// </summary>
        public System.Type CurrentRecordType
        {
            get
            {
                return _currentDataRecord.GetType();
            }
        }

        /// <summary>
        /// Access to the StreamReader.
        /// </summary>
        protected StreamReader StreamReader
        {
            get
            {
                return _streamReader;
            }
        }

        #region IDataReader Members

        public int RecordsAffected
        {
            get
            {
                return _recordsAffected;
            }
        }

        public bool IsClosed
        {
            get
            {
                return _streamReader == null;
            }
        }

        /// <summary>
        /// Not implemented. 
        /// </summary>
        /// <returns>Always returns false</returns>
        public bool NextResult()
        {
            return false;
        }

        public void Close()
        {
            _currentDataRecord = null;
            if (_streamReader != null)
                _streamReader.Close();
            _streamReader = null;
        }

        public bool Read()
        {
            if (_streamReader == null)
                return false;
            //get the next record.
            _currentDataRecord = this.GetNextDataRecord();

            if (_currentDataRecord != null)
            {
                //Increment the record count
                if (_recordsAffected == -1)
                    _recordsAffected = 1;
                else
                    _recordsAffected++;

                return true;
            }
            else
            {
                return false;
            }
        }

        public int Depth
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <returns>null</returns>
        public DataTable GetSchemaTable()
        {
            return null;
        }

        #endregion

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
            return _currentDataRecord.GetString(i).Trim();
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

        #region IDisposable Members

        public void Dispose()
        {
            this.Close();
        }

        #endregion
    }
}
