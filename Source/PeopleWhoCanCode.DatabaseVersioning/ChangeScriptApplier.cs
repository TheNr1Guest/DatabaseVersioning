using System;
using System.Collections.Generic;
using System.Transactions;
using Microsoft.Extensions.Logging;
using PeopleWhoCanCode.DatabaseVersioning.Models;

namespace PeopleWhoCanCode.DatabaseVersioning
{
    public class ChangeScriptApplier
    {
        private readonly IDbProvider _provider;
        private readonly ILogger<ChangeScriptApplier> _logger;

        public ChangeScriptApplier(IDbProvider provider, ILogger<ChangeScriptApplier> logger)
        {
            _provider = provider;
            _logger = logger;
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
                    _logger.LogInformation($"Database script #{changeScript.Number} of version {changeScript.Version} has been applied.");
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