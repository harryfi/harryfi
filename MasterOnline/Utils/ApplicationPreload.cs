using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Utils
{
    public class ApplicationPreload : System.Web.Hosting.IProcessHostPreloadClient
    {
        public void Preload(string[] parameters)
        {
#if (DEBUG || Debug_AWS)

#else
            HangfireBootstrapper.Instance.Start();
#endif
        }
    }
}