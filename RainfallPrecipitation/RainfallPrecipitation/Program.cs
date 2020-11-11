using RainfallPrecipitation.Utilities;
using RainfalPrecipitation.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainfallPrecipitation
{
    class Program
    {
        static string sqldbConn;
        static string[] inputfilestoprocess;
        static string destinationTableName;

        static void Main(string[] args)
        {
            string mdffilepath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName+ "\\RainfalPrecipitation.Utilities\\RainfallReadingDB.mdf";
            sqldbConn = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename="+ mdffilepath + ";Integrated Security=True";
             inputfilestoprocess = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\..\\..\\datafiles");
            destinationTableName = "Reading";
            process();
        }

        static void process()
        {
            try
            {
                IPrecipitationData p = new PrecipitationData(inputfilestoprocess);
                DataTable dt = p.parseInputFile();

                IDAL sqldal = new DAL(sqldbConn);
                sqldal.bulkSQLInsert(dt,destinationTableName);
            }
            catch(Exception ex)
            {

            }
        }
    }
}
