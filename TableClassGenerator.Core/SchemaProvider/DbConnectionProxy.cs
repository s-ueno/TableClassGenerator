using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;
using uEN.Core;
using uEN.Core.Data;

namespace TableClassGenerator.Core
{
    [Export(typeof(BizProxyFactory))]
    [ExportMetadata(uEN.Repository.Priority, 1)]
    public class DbConnectionProxyFactory : BizProxyFactory
    {
        public override RealProxy CreateProxy(MarshalByRefObject target)
        {
            return new DbConnectionProxy(target);
        }
    }

    public class DbConnectionProxy : BizProxy
    {
        public DbConnectionProxy(MarshalByRefObject target)
            : base(target)
        {
        }
        protected DbConnectionHelper DbHelper
        {
            get
            {
                if (_dbHelper == null)
                    _dbHelper = DbConnectionRepository.CreateDbHelper();
                return _dbHelper;
            }
        }
        private DbConnectionHelper _dbHelper;

        protected override void TraceMethodStart(System.Runtime.Remoting.Messaging.IMethodCallMessage callMessage)
        {
            base.TraceMethodStart(callMessage);
            DbHelper.Open();
            DbHelper.BeginTransaction();
        }
        protected override void TraceMethodEnd(System.Runtime.Remoting.Messaging.IMethodReturnMessage msg)
        {
            DbHelper.Commit();
            DbHelper.Close();
            DbHelper.Dispose();
            base.TraceMethodEnd(msg);

        }
        protected override void TraceMethodError(System.Runtime.Remoting.Messaging.IMethodReturnMessage msg, Exception ex)
        {
            base.TraceMethodError(msg, ex);
            DbHelper.Rollback();
            DbHelper.Close();
            DbHelper.Dispose();
        }
    }

}
