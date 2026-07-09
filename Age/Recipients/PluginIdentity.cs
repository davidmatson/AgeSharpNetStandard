using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Age.Crypto;
using Age.Format;
using Age.Plugin;
using Age.Polyfills;

namespace Age.Recipients
{
    public sealed class PluginIdentity : IIdentity
    {
        readonly string _identity;
        readonly IPluginCallbacks _callbacks;
        readonly string _pluginName;

        public PluginIdentity(string identity, IPluginCallbacks callbacks = null)
        {
            _identity = identity;
            _callbacks = callbacks;
            _pluginName = ExtractPluginName(_identity);
        }

        internal string PluginName => _pluginName;

        public byte[] Unwrap(Stanza stanza) =>
            Unwrap(new[] { stanza });

        public byte[] Unwrap(IReadOnlyList<Stanza> stanzas)
        {
            using (var conn = new PluginConnection(PluginName, "identity-v1"))
            {
                return UnwrapWithConnection(conn, stanzas);
            }
        }

        internal byte[] UnwrapWithConnection(PluginConnection conn, IReadOnlyList<Stanza> stanzas)
        {
            SendUnwrapRequest(conn, stanzas);
            return ReadUnwrapResponse(conn);
        }

        private void SendUnwrapRequest(PluginConnection conn, IReadOnlyList<Stanza> stanzas)
        {
            conn.WriteStanza("add-identity",new[] { _identity }, Array.Empty<byte>());

            for (var i = 0; i < stanzas.Count; i++)
            {
                var s = stanzas[i];

                string[] args = ArrayFactory.Concatenate(new[] { i.ToString(), s.Type }, s.Args.ToArray());
                conn.WriteStanza("recipient-stanza", args, s.Body.ToArray());
            }

            conn.WriteStanza("done", Array.Empty<string>(), Array.Empty<byte>());
        }

        private byte[] ReadUnwrapResponse(PluginConnection conn)
        {
            byte[] result = null;

            while (true)
            {
                var (type, args, body) = ReadNextStanza(conn);

                switch (type)
                {
                    case "file-key":
                        if (args.Length < 1)
                            throw new AgePluginException("file-key stanza missing file index");
                        result = body;
                        conn.WriteStanza("ok", Array.Empty<string>(), Array.Empty<byte>());
                        break;

                    case "error":
                        HandleError(conn, args, body);
                        break;

                    case "done":
                        return result;

                    default:
                        HandleCommonStanza(conn, type, args, body);
                        break;
                }
            }
        }

        private static void HandleError(PluginConnection conn, string[] args, byte[] body)
        {
            if (args.Length > 0 && args[0] == "internal")
                throw new AgePluginException($"plugin internal error: {EncodingExtended.UTF8.GetString(body)}");

            // Identity errors mean this identity doesn't match — return null
            conn.WriteStanza("ok", Array.Empty<string>(), Array.Empty<byte>());
        }

        private static (string Type, string[] Args, byte[] Body) ReadNextStanza(PluginConnection conn)
        {
            var raw = conn.ReadStanza() ?? throw new AgePluginException("unexpected end of plugin output");
            return raw;
        }

        private void HandleCommonStanza(PluginConnection conn, string type, string[] args, byte[] body)
        {
            switch (type)
            {
                case "msg":
                    if (_callbacks != null)
                        _callbacks.DisplayMessage(EncodingExtended.UTF8.GetString(body));
                    conn.WriteStanza("ok", Array.Empty<string>(), Array.Empty<byte>());
                    break;

                case "request-secret":
                    if (_callbacks == null)
                        throw new AgePluginException("plugin requested secret but no callbacks provided");
                    var secret = _callbacks.RequestValue(EncodingExtended.UTF8.GetString(body), true);
                    conn.WriteStanza("ok", Array.Empty<string>(), EncodingExtended.UTF8.GetBytes(secret));
                    break;

                case "confirm":
                    if (_callbacks == null)
                        throw new AgePluginException("plugin requested confirmation but no callbacks provided");
                    HandleConfirm(conn, args, body);
                    break;

                default:
                    conn.WriteStanza("unsupported", Array.Empty<string>(), Array.Empty<byte>());
                    break;
            }
        }

        private void HandleConfirm(PluginConnection conn, string[] args, byte[] body)
        {
            var message = EncodingExtended.UTF8.GetString(body);
            var yes = args.Length > 0 ? EncodingExtended.UTF8.GetString(Base64Unpadded.Decode(args[0].AsSpan())) : "yes";
            var no = args.Length > 1 ? EncodingExtended.UTF8.GetString(Base64Unpadded.Decode(args[1].AsSpan())) : null;
            var confirmed = _callbacks.Confirm(message, yes, no);
            conn.WriteStanza("ok", new[] { confirmed ? "yes" : "no" }, Array.Empty<byte>());
        }

        internal static string ExtractPluginName(string identity)
        {
            // Bech32-decode to get HRP. For "AGE-PLUGIN-YUBIKEY-1...", HRP = "age-plugin-yubikey-", name = HRP[11..^1] = "yubikey"
            var (hrp, _) = Bech32.Decode(identity);

            // skip "age-plugin-" prefix and trailing "-"
            if (!hrp.StartsWith("age-plugin-"))
                throw new FormatException($"invalid plugin identity HRP: {hrp}");

            var name = hrp.AsSpan().Slice(11, hrp.Length - 11 - 1).ToString();
            PluginConnection.ValidatePluginName(name);
            return name;
        }

        public override string ToString() =>
            _identity;
    }
}
