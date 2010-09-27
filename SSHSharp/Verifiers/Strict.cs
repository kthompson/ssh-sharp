using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSHSharp.Verifiers
{
    /// <summary>
    /// Does a strict host verification, looking the server up in the known
    /// host files to see if a key has already been seen for this server. If this
    /// server does not appear in any host file, this will silently add the
    /// server. If the server does appear at least once, but the key given does
    /// not match any known for the server, an exception will be raised (HostKeyMismatch).
    /// Otherwise, this returns true.
    /// </summary>
    public class Strict : IHostKeyVerifier
    {
        public virtual bool Verify(HostKeyVerificationData arguments)
        {
            var options = arguments.Session.Options;
            var host = options.HostKeyAlias ?? arguments.Session.HostAsString();
            var matches = KnownHosts.SearchFor(host, arguments.Session.Options);

            /* we've never seen this host before, so just automatically add the key.
             * not the most secure option (since the first hit might be the one that
             * is hacked), but since almost nobody actually compares the key
             * fingerprint, this is a reasonable compromise between usability and
             * security.
             */
            if (matches.Length == 0)
            {
                var ip = arguments.Session.Peer.IPAddress;
                KnownHosts.Add(host, arguments.Key, arguments.Session.Options);
                return true;
            }
            // If we found any matches, check to see that the key type and
            // blob also match.

            var found =
                matches.Any(key => key.SshType == arguments.Key.SshType && key.ToBlob() == arguments.Key.ToBlob());

            //If a match was found, return true. Otherwise, raise an exception
            //indicating that the key was not recognized.

            return found || ProcessCacheMiss(host, arguments);
        }

        private static bool ProcessCacheMiss(string host, HostKeyVerificationData args)
        {
            var exception =
                new HostKeyMismatchException(
                    string.Format("fingerprint {0} does not match for {1}", args.Fingerprint, host));

            exception.VerificationData = args;
            exception.Callback = () => KnownHosts.Add(host, args.Key, args.Session.Options);
            throw exception;
        }
    }
}
