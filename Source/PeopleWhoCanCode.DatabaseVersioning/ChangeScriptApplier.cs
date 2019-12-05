using System;
using System.Collections.Generic;
using System.Reflection;
using System.Transactions;
using log4net;
using PeopleWhoCanCode.DatabaseVersioning.Models;

namespace PeopleWhoCanCode.DatabaseVersioning
{
    public class ChangeScriptApplier
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDbProvider _provider;

        public ChangeScriptApplier(IDbProvider provider)
        {
            _provider = provider;
        }

        public void Apply(IEnumerable<ChangeScript> changeScripts)
        {
            foreach (var changeScript in changeScripts)
            {
                Apply(changeScript);
            }
        }

        private void Apply(ChangeScript changeScript)
        {
            Exception exception = null;

            using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                try
                {
                    _provider.ApplyChangeScript(changeScript);
                    Log.InfoFormat("Database script #{0} of version {1} has been applied.",
                        changeScript.Number,
                        changeScript.Version);
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            var changeLogRecord = new ChangeLogRecord(changeScript, exception);

            using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                _provider.DeleteChangeLogRecord(changeLogRecord);
                _provider.InsertChangeLogRecord(changeLogRecord);

                transaction.Complete();
            }

            if (!changeLogRecord.IsSuccessful)
            {
                _provider.Disconnect();
                throw changeLogRecord.Exception;
            }
        }
    }
}