using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;

namespace NullMarketManager
{
    
    class Program
    {        
        static void Main(string[] args)
        {

            AccessManager accessManager = new AccessManager();
            Thread accessManagerThread = new Thread(new ThreadStart(accessManager.RunAccessManager));
            accessManagerThread.Start();

            ExportManager exportManager1 = new ExportManager("1DQ", "Jita");
            Thread exportManagerThread1 = new Thread(new ThreadStart(exportManager1.RunExportManager));
            exportManagerThread1.Start();


        }
    }
}
