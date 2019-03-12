using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.SqlServer.Util;
using Jasper.Util;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurableOutgoing : SqlServerAccess, IDurableOutgoing
    {
        private readonly SqlServerDurableStorageSession _session;
        private readonly SqlServerSettings _settings;
        private readonly string _findUniqueDestinations;
        private readonly string _findOutgoingEnvelopesSql;
        private readonly string _deleteOutgoingSql;

        public SqlServerDurableOutgoing(SqlServerDurableStorageSession session, SqlServerSettings settings, JasperOptions options)
        {
            _session = session;
            _settings = settings;
            _findUniqueDestinations =
                $"select distinct destination from {settings.SchemaName}.{OutgoingTable}";
            _findOutgoingEnvelopesSql =
                $"select top {options.Retries.RecoveryBatchSize} body from {settings.SchemaName}.{OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = @destination";
            _deleteOutgoingSql =
                $"delete from {settings.SchemaName}.{OutgoingTable} where owner_id = :owner and destination = @destination";
        }

        public Task<Envelope[]> Load(Uri destination)
        {
            return _session.CreateCommand(_findOutgoingEnvelopesSql)
                .With("destination", destination.ToString(), SqlDbType.VarChar)
                .ExecuteToEnvelopes();
        }

        public Task Reassign(int ownerId, Envelope[] outgoing)
        {
            var cmd = _session.CreateCommand($"{_settings.SchemaName}.uspMarkOutgoingOwnership");
            cmd.CommandType = CommandType.StoredProcedure;
            var list = cmd.Parameters.AddWithValue("IDLIST", SqlServerEnvelopePersistence.BuildIdTable(outgoing));
            list.SqlDbType = SqlDbType.Structured;
            list.TypeName = $"{_settings.SchemaName}.EnvelopeIdList";
            cmd.Parameters.AddWithValue("owner", ownerId).SqlDbType = SqlDbType.Int;

            return cmd.ExecuteNonQueryAsync();
        }

        public Task DeleteByDestination(Uri destination)
        {
            return _session.CreateCommand(_deleteOutgoingSql)
                .With("destination", destination.ToString(), SqlDbType.VarChar)
                .With("owner", TransportConstants.AnyNode, SqlDbType.Int).ExecuteNonQueryAsync();
        }

        public Task Delete(Envelope[] outgoing)
        {
            var cmd = _session.CreateCommand($"{_settings.SchemaName}.uspDeleteOutgoingEnvelopes");
            cmd.CommandType = CommandType.StoredProcedure;
            var list = cmd.Parameters.AddWithValue("IDLIST", SqlServerEnvelopePersistence.BuildIdTable(outgoing));
            list.SqlDbType = SqlDbType.Structured;
            list.TypeName = $"{_settings.SchemaName}.EnvelopeIdList";

            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<Uri[]> FindAllDestinations()
        {
            var list = new List<Uri>();

            var cmd = _session.CreateCommand(_findUniqueDestinations);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var text = await reader.GetFieldValueAsync<string>(0);
                    list.Add(text.ToUri());
                }
            }

            return list.ToArray();
        }
    }
}
