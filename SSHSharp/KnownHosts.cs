using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SSHSharp.Transport;

namespace SSHSharp
{
    /// <summary>
    /// Searches an OpenSSH-style known-host file for a given host, and returns all
    /// matching keys. This is used to implement host-key verification, as well as
    /// to determine what key a user prefers to use for a given host.
    /// 
    /// This is used internally by SSHSharp, and will never need to be used directly
    /// by consumers of the library.
    /// </summary>
    public class KnownHosts
    {

        #region static methods

        /// <summary>
        /// Searches all known host files (see KnownHosts.hostfiles) for all keys
        /// of the given host. Returns an array of keys found.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Key[] SearchFor(string host, SessionOptions options)
        {
            return SearchIn(Hostfiles(options), host);
        }

        /// <summary>
        /// Search for all known keys for the given host, in every file given in
        /// the +files+ array. Returns the list of keys.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static Key[] SearchIn(string[] files, string host)
        {
            return files.SelectMany(file => new KnownHosts(file).KeysFor(host)).ToArray();
        }

        /// <summary>
        /// Looks in the given +options+ hash for the :user_known_hosts_file and
        /// :global_known_hosts_file keys, and returns an array of all known
        /// hosts files. If the :user_known_hosts_file key is not set, the
        /// default is returned (~/.ssh/known_hosts and ~/.ssh/known_hosts2). If
        /// :global_known_hosts_file is not set, the default is used
        /// (/etc/ssh/known_hosts and /etc/ssh/known_hosts2).
        /// 
        /// If you only want the user known host files, you can pass :user as
        /// the second option.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="which"></param>
        /// <returns></returns>
        public static string[] Hostfiles(SessionOptions options, WhichHostfiles which = WhichHostfiles.All)
        {
            var files = new List<string>();
            if (which == WhichHostfiles.All || which == WhichHostfiles.User)
            {
                if (options.UserKnownHostsFile != null)
                    files.Add(options.UserKnownHostsFile);
                else
                {
                    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    files.Add(home + "/.ssh/known_hosts");
                    files.Add(home + "/.ssh/known_hosts2");
                }
            }

            if (which == WhichHostfiles.All || which == WhichHostfiles.Global)
            {
                if (options.GlobalKnownHostsFile != null)
                    files.Add(options.GlobalKnownHostsFile);
                else
                {
                    files.Add("/etc/ssh/known_hosts");
                    files.Add("/etc/ssh/known_hosts2");
                }
            }

            return files.ToArray();
        }

      

        /// <summary>
        /// Looks in all user known host files (see KnownHosts.hostfiles) and tries to
        /// add an entry for the given host and key to the first file it is able
        /// to.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="key"></param>
        /// <param name="options"></param>
        public static void Add(string host, Key key, SessionOptions options = null)
        {
            if(options == null)
                options = new SessionOptions();

            foreach(var file in Hostfiles(options,  WhichHostfiles.User) )
            {
                 try
                 {
                     new KnownHosts(file).Add(host, key);
                     return;
                 }
                 catch
                 {
                 }
            }
        }

        public enum WhichHostfiles
        {
            All,
            User,
            Global
        }

        #endregion





        /// <summary>
        /// The host-key file name that this KnownHosts instance will use to search
        /// for keys.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Instantiate a new KnownHosts instance that will search the given known-hosts
        /// file. The path is expanded file File.expand_path.
        /// </summary>
        public KnownHosts(string source)
        {
            this.Source = Path.GetFullPath(source); 
        }

        /// <summary>
        /// Returns an array of all keys that are known to be associatd with the
        /// given host. The +host+ parameter is either the domain name or ip address
        /// of the host, or both (comma-separated). Additionally, if a non-standard
        /// port is being used, it may be specified by putting the host (or ip, or
        /// both) in square brackets, and appending the port outside the brackets
        /// after a colon. Possible formats for +host+, then, are;
        /// 
        ///   "net.ssh.test"
        ///   "1.2.3.4"
        ///   "net.ssh.test,1.2.3.4"
        ///   "[net.ssh.test]:5555"
        ///   "[1,2,3,4]:5555"
        ///   "[net.ssh.test]:5555,[1.2.3.4]:5555
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public Key[] KeysFor(string host)
        {
            var keys = new List<Key>();
            var entries = host.Split(',');
            using (var file = new StreamReader(File.OpenRead(this.Source)))
            {
                if (!file.BaseStream.CanRead)
                    return keys.ToArray();

                var scanner = new StringScanner();

                string line;
                while ((line = file.ReadLine()) != null)
                {
                    scanner.Text = line;

                    scanner.Skip(@"\s*");
                    if (scanner.IsMatch("$|#"))
                        continue;

                    var hostlist = scanner.Scan(@"\S+").Split(',');
                    if (!entries.All(entry => hostlist.Contains(entry)))
                        continue;

                    scanner.Skip(@"\s*");
                    var type = scanner.Scan(@"\S+");
                    if (!new[] {"ssh-rsa", "ssh-dss"}.Contains(type))
                        continue;

                    scanner.Skip(@"\s*");
                    var blob = Encoding.Default.GetString(Convert.FromBase64String(scanner.Rest));
                    keys.Add(new Buffer(blob).ReadKey());
                }
            }
            return keys.ToArray();
        }

        /// <summary>
        /// Tries to append an entry to the current source file for the given host
        /// and key. If it is unable to (because the file is not writable, for
        /// instance), an exception will be raised.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="key"></param>
        public void Add(string host, Key key)
        {
            using(var file = new StreamWriter(File.Open(this.Source,  FileMode.Append)))
            {
                var buffer = new Buffer().WriteKey(key).ToString();
                var b64 = Convert.ToBase64String(Encoding.Default.GetBytes(buffer));
                var blob = new Regex(@"\s").Replace(b64, string.Empty);
                file.WriteLine(string.Format("#{0} #{1} #{2}", host, key.SshType, blob));
            }
        }
    }
}
