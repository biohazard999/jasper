using System;
using System.Data;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
    using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurableIncoming : SqlServerAccess, IDurableIncoming
    {
        private readonly SqlServerDurableStorageSession _session;
        private readonly SqlServerSettings _settings;
        private readonly string _findAtLargeEnvelopesSql;

        public SqlServerDurableIncoming(SqlServerDurableStorageSession session, SqlServerSettings settings, JasperOptions options)
        {
            _session = session;
            _settings = settings;
            _findAtLargeEnvelopesSql =
                $"select top {options.Retries.RecoveryBatchSize} body from {settings.SchemaName}.{IncomingTable} where owner_id = {TransportConstants.AnyNode} and status = '{TransportConstants.Incoming}'";
        }

        public Task<Envelope[]> LoadPageOfLocallyOwned()
        {
            return _session.CreateCommand(_findAtLargeEnvelopesSql)
                .ExecuteToEnvelopes();
        }

        public Task Reassign(int ownerId, Envelope[] incoming)
        {
            var cmd = _session.CreateCommand($"{_settings.SchemaName}.uspMarkIncomingOwnership");
            cmd.CommandType = CommandType.StoredProcedure;
            var list = cmd.Parameters.AddWithValue("IDLIST", SqlServerEnvelopePersistence.BuildIdTable(incoming));
            list.SqlDbType = SqlDbType.Structured;
            list.TypeName = $"{_settings.SchemaName}.EnvelopeIdList";
            cmd.Parameters.AddWithValue("owner", ownerId).SqlDbType = SqlDbType.Int;

            return cmd.ExecuteNonQueryAsync();
        }
    }
}
