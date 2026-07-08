using System.Collections.Generic;
using System.Linq;
using Age.Format;

namespace Age.Recipients
{
    public static class DefaultIdentity
    {
        /// <summary>
        /// Attempts to unwrap a file key from any of the provided stanzas.
        /// The default implementation iterates stanzas one at a time, returning
        /// the first successful unwrap. Replace for batch-based identity protocols
        /// (e.g. plugin identities) that need to see the full stanza list at once.
        /// </summary>
        public static byte[] Unwrap(IIdentity identity, IReadOnlyList<Stanza> stanzas) =>
            stanzas.Select(identity.Unwrap).OfType<byte[]>().FirstOrDefault();
    }
}
