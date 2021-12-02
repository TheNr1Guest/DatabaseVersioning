using System;
using System.Collections.Generic;
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

            using (var transaction = _provider.BeginTransaction())
            {
                try
                {
                    _provider.ApplyChangeScript(changeScript);

                    _logger.LogInformation($"Database script #{changeScript.Number} of version {changeScript.Version} has been applied.");

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            var changeLogRecord = new ChangeLogRecord(changeScript, exception);

            using (var transaction = _provider.BeginTransaction())
            {
                _provider.DeleteChangeLogRecord(changeLogRecord);
                _provider.InsertChangeLogRecord(changeLogRecord);

                transaction.Commit();
            }

            if (!changeLogRecord.IsSuccessful)
            {
                _provider.Disconnect();

                throw changeLogRecord.Exception;
            }
        }
    }
}