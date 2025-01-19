using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common {
    public abstract partial class RevenantLoggerPSCmdlet : RevenantLoggerBase {

        //internal static Bootstrap GetBootstrapper(SessionState SessionState) {
        //    Bootstrap returnObject;
        //    var existingBootstrapper = SessionState.PSVariable.GetValue("__RevenantLoggerExistingBootstrap", null);
        //    if ((null == existingBootstrapper)) {
        //        returnObject = new Bootstrap(SessionState);
        //        return returnObject;
        //    }
        //    else {
        //        returnObject = (Bootstrap)existingBootstrapper;
        //        return returnObject;
        //    }
        //}

        //internal static Configuration GetExistingOrNewConfig(SessionState SessionState) {
        //    Configuration returnObject;
        //    var existingConfigVariable = SessionState.PSVariable.GetValue("__RevenantLoggerExistingConfig", null);
        //    if ((null == existingConfigVariable)) {
        //        returnObject = new Configuration();
        //        return returnObject;
        //    }
        //    else {
        //        returnObject = (Configuration)existingConfigVariable;
        //        return returnObject;
        //    }
        //}

        internal static TObject GetExistingPSVariable<TObject>(SessionState SessionState, string psVariable) where TObject : class {
            //var existingObject = SessionState.PSVariable.Get(psVariable);
            var existingObject = SessionState.PSVariable.GetValue(psVariable, null);

            return (null == existingObject) ? null : (TObject)existingObject;
            //if (null == existingObject) {
            //    return null;
            //}
            //return (TObject)existingObject;
        }
    }
}
