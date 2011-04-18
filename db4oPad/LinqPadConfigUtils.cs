using Gamlor.Db4oPad.Utils;
using LINQPad.Extensibility.DataContext;

namespace Gamlor.Db4oPad
{
    static class LinqPadConfigUtils
    {
        public const string WriteAccessFlag = "WriteAccess";

        internal static bool HasWriteAccess(IConnectionInfo cxInfo)
        {
            return cxInfo.DriverData.Element(WriteAccessFlag)
                .AsMaybe().Convert(e => "true".Equals(e.Value)).GetValue(false);
        }
    }
}