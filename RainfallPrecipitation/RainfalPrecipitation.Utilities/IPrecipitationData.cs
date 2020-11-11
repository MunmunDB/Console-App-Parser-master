using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainfallPrecipitation.Utilities
{
    public interface IPrecipitationData
    {

        /// <summary>
        /// parsing logic
        /// parallelism is demostrated for a section of logic in this module
        /// </summary>
        /// <returns></returns>
        DataTable parseInputFile();
    }
    
}
