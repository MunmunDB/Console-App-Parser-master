using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainfallPrecipitation.Utilities
{
    public class PrecipitationData : IPrecipitationData
    {
        #region private members


        string[] filepaths;
        string matchingXYtext = "Grid-ref=";
        string matchingYearstext = "Years=";
        enum matchingmonths { Jan, Feb, March, April, May, June, July, Aug, Sept, Oct, Nov, Dec };

        List<measures> parsedlistfromfile;

        class measures
        {
            public int X { get; set; }
            public int Y { set; get; }
            public int startyear { get; set; }
            public int endyear { get; set; }
            public int[,] infomatrix { get; set; }

        }
        
         ConcurrentBag<Rainfallrecord> parsedRecordListbeforeDBinsert;
        DataTable parsedRecordListbeforeDBinsertDT;

        void initialse(string[] inputfilepath)
        {
            filepaths = inputfilepath;// Directory.GetFiles(Directory.GetCurrentDirectory() + "\\..\\..\\datafiles");
            parsedlistfromfile = new List<measures>();
            parsedRecordListbeforeDBinsert = new ConcurrentBag<Rainfallrecord>();
            //parsedRecordListbeforeDBinsertDT = new DataTable("Rainfallrecord");
            //parsedRecordListbeforeDBinsertDT.Columns.Add("X", typeof(int));
            //parsedRecordListbeforeDBinsertDT.Columns.Add("Y", typeof(int));
            //parsedRecordListbeforeDBinsertDT.Columns.Add("monthOn", typeof(int));
            //parsedRecordListbeforeDBinsertDT.Columns.Add("yearOn", typeof(int));
            //parsedRecordListbeforeDBinsertDT.Columns.Add("record", typeof(int));
            //    parsedRecordListbeforeDBinsertDT.Columns.Add("recorddate", typeof(int));
        }
        /// <summary>
        /// parsing algorithm from file to custom list named parsedRecordListbeforeDBinsert
        /// </summary>
        void parseFile()
        {

            if (filepaths.Count() > 0)
            {

                string line;
                try
                {
                    foreach (string file in filepaths)
                    {
                        StreamReader filestream = new StreamReader(file);

                        parsedlistfromfile = new List<measures>();
                        int startyear = 0;
                        int endyear = 0;
                        while ((line = filestream.ReadLine()) != null)
                        {

                            if (line.Contains(matchingYearstext))
                            {
                                int.TryParse(line.Substring(line.IndexOf(matchingYearstext) + matchingYearstext.Length, 4), out startyear);
                                int.TryParse(line.Substring(line.IndexOf(matchingYearstext) + matchingYearstext.Length + 4 + 1, 4), out endyear);

                            }
                            if (line.Contains(matchingXYtext))
                            {
                                var obj = new measures() { startyear = startyear, endyear = endyear };
                                int xValue = 0;
                                int yValue = 0;
                                var xytext = line.Substring(line.IndexOf(matchingXYtext) + matchingXYtext.Length).Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                int.TryParse(xytext[0], out xValue);
                                int.TryParse(xytext[1], out yValue);
                                obj.X = xValue;
                                obj.Y = yValue;

                                if (xValue > 0 && yValue > 0)
                                {
                                    int[,] xyValue = new int[12, 12];
                                    for (int i = 0; i < 12; i++)
                                    {
                                        line = filestream.ReadLine();
                                        if (line == null)
                                            break;
                                        var readingLine = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                        int j = 0;
                                        foreach (string s in readingLine)
                                        {
                                            int noval = 0;
                                            int.TryParse(s, out noval);
                                            xyValue[i, j++] = noval;
                                        }
                                    }
                                    obj.infomatrix = xyValue;
                                    parsedlistfromfile.Add(obj);
                                }
                            }
                        }

                        if (file != null)
                            filestream.Close();

                    }

                }
                catch (Exception ex)
                {

                }

            }
        }

        /// <summary>
        /// pivoting or conservion of custom list into collection objects which is to be inserted to db
        /// this method uses parallelism to loop through a collection 
        /// </summary>
        void convertToDTforDBOperation()
        {
            if (parsedlistfromfile != null & parsedlistfromfile.Count > 0)
            {
                // to monitor the processing time
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                #region Parallel processing of data for conversion

                Parallel.ForEach(parsedlistfromfile, (item, index) =>
                {
                    for (int r = 0; r < item.infomatrix.GetLength(0); r++)
                    {
                        for (int c = 0; c < item.infomatrix.GetLength(1); c++)
                        {
                            //Note: Parallel processing is not working on datatable. its is getting runtime error as datatable is corrupted.
                            //DataRow dr = parsedRecordListbeforeDBinsertDT.NewRow();
                            //dr["X"] = item.X;
                            //dr["Y"] = item.Y;
                            //dr["monthOn"] = r + 1;
                            //dr["yearOn"] = item.startyear +c;
                            //dr["record"] = item.infomatrix[r, c];
                            // dr["recorddate"]= new DateTime(item.startyear,r+1,1);
                            //parsedRecordListbeforeDBinsertDT.Rows.Add(dr);

                            parsedRecordListbeforeDBinsert.Add(new Rainfallrecord()
                            { X = item.X, Y = item.Y, monthOn = c + 1, yearOn = item.startyear + r, record = item.infomatrix[r, c]
                            , recorddate= new DateTime(item.startyear+r,c+1,1)
                            });
                        }
                    }
                });

                #endregion
                sw.Stop();

                Console.WriteLine(sw.ElapsedMilliseconds);

                //convert list to db
                parsedRecordListbeforeDBinsertDT = ConvertToDataTable(parsedRecordListbeforeDBinsert.ToList());



                #region using for loop of data for conversion
                /*
                sw.Reset();
                sw.Start();
                foreach (var record in parsedlistfromfile)
                {

                    for(int r=0;r<record.infomatrix.GetLength(0);r++)
                    {
                        for(int c=0; c<record.infomatrix.GetLength(1);c++)
                        {
                            parsedRecordListbeforeDBinsert.Add(new Rainfallrecord()
                            { X = record.X, Y = record.Y, monthOn = r + 1, yearOn = int.Parse(record.startyear) + c, record= record.infomatrix[r,c],  recorddate= new DateTime(record.startyear,r+1,1) });
                        }
                    }

                }

                

                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);

                */
                #endregion
            }
        }

        DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                   row[prop.Name] = prop.GetValue(item)?? DBNull.Value ;
                }
                if(!row.ItemArray.Contains(DBNull.Value))
                    table.Rows.Add(row);
            }
            return table;
        }

        #endregion

        public PrecipitationData(string[] filepaths)
        {
            initialse(filepaths);
        }
        public DataTable parseInputFile()
        {
            try
            {
                parseFile();
                convertToDTforDBOperation();


            }
            catch (IOException)
            {
                //log

            }
            catch (Exception ex)
            {

            }
            return parsedRecordListbeforeDBinsertDT;
        }




       
    }

}
