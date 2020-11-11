using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainfalPrecipitation.Utilities
{
    public interface IDAL : IDisposable
    {
        /// <summary>
        /// Bulk operation to SQL db
        /// </summary>
        /// <param name="dtToInsert"></param>
        /// <param name="destinationTableName"></param>
        void bulkSQLInsert(DataTable dtToInsert, string destinationTableName);
    }

    public class DAL : IDAL
    {
        private string _connStr;
        #region SQL Bulk update

        public DAL(string connDbstr)
        {
            _connStr = connDbstr;
        }

        public void bulkSQLInsert(DataTable dtToInsert, string destinationTableName)
        {
            try
            {
                using (SqlConnection cnx = new SqlConnection(_connStr))
                {
                    cnx.Open();
                    using (SqlTransaction tran = cnx.BeginTransaction())
                    {

                        using (SqlBulkCopy bulkcopy = new SqlBulkCopy(cnx, SqlBulkCopyOptions.Default, tran))
                        {
                            bulkcopy.BulkCopyTimeout = 660;
                            bulkcopy.DestinationTableName = destinationTableName;
                            bulkcopy.WriteToServer(dtToInsert);
                        }

                        tran.Commit();

                    }
                    cnx.Close();
                }
            }
            catch(Exception ex)
            {
                // log
            }
           
        }
      

        #endregion
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DAL() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

     
        #endregion

    }
}
